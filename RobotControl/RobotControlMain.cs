using IronSoftware.Drawing;
using Microsoft.Web.WebView2.Core;
using RobotControl.ClassLibrary;
using RobotControl.ClassLibrary.ImageRecognition;
using RobotControl.ClassLibrary.RobotCommunication;
using System.Text.Json;
using System.Text.RegularExpressions;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace RobotControl
{
    public partial class RobotControlMain : Form
    {
        private IImageRecognitionFromCamera _imageRecognition;
        private readonly Thread _observeWebBrowserThread;
        private readonly string _jsonFileName;
        private readonly Regex _ipAddressPattern = new Regex(@"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
        private readonly Regex _cameraRSTLURLPattern = new Regex(@"rtsp://[^:]+:[^@]+@[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}:[0-9]{1,5}.*$|https?://[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}:[0-9]{1,5}.*$");
        private readonly Regex _gpuDeviceIdPattern = new Regex(@"^[0-9]{1,2}");
        private ISimpleMotorControl _motorControl;
        public RobotControlMain()
        {
            InitializeComponent();
            lstLabelsToFind.Items.AddRange(YOLORecognitionLabels);
            _jsonFileName = Path.Combine(Path.GetTempPath(), nameof(RobotControl) + ".json");
            _observeWebBrowserThread = new Thread(ObserveWebBrowserProc) { Priority = ThreadPriority.AboveNormal };
            fromJSON();
        }

        private void txtRobotIPAddress_TextChanged(object sender, EventArgs e) =>
            validateText(txtRobotIPAddress, _ipAddressPattern);
        private void txtCameraRSTLURL_TextChanged(object sender, EventArgs e) =>
            validateText(txtCameraRSTLURL, _cameraRSTLURLPattern);
        private void txtGPUDeviceId_TextChanged(object sender, EventArgs e)
        {
            if (chkHasGPU.Checked)
            {
                validateText(txtGPUDeviceId, _gpuDeviceIdPattern);
            }
        }

        public static string[] YOLORecognitionLabels => TinyYolo2Labels.Labels;
        private void validateText(TextBox textBox, Regex regex)
        {
            textBox.ForeColor = regex.IsMatch(textBox.Text) ? Color.Black : Color.Red;
            enableStartIfNeeded();
        }

        private void lstLabelsToFind_SelectedIndexChanged(object sender, EventArgs e) =>
            enableStartIfNeeded();

        private void enableStartIfNeeded()
        {
            btnStart.Enabled =
                lstLabelsToFind.SelectedIndices.Count > 0 &&
                txtRobotIPAddress.Text.Any() &&
                txtRobotIPAddress.ForeColor == Color.Black &&
                txtCameraRSTLURL.Text.Any() &&
                txtCameraRSTLURL.ForeColor == Color.Black;
            if (btnStart.Enabled)
            {
                saveToJSON();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _imageRecognition = ClassFactory.CreateImageRecognitionFromCamera(int.Parse(txtGPUDeviceId.Text));

            if (txtCameraRSTLURL.Text.StartsWith("http"))
            {
                webBrowser.Source = new System.Uri(txtCameraRSTLURL.Text, System.UriKind.Absolute);
                _observeWebBrowserThread.Start(this);
            }
        }

        private void ObserveWebBrowserProc(object? obj)
        {
            if (!(obj is RobotControlMain))
            {
                throw new ArgumentException("Window parameter was null, aborting.");
            }
            RobotControlMain thisWindow = (RobotControlMain)obj;
            IList<string> itemsToRecognize = GetItemsToRecognize(thisWindow);
            AnyBitmap bitmap = new AnyBitmap(thisWindow.webBrowser.Width, thisWindow.webBrowser.Height);
            bool showedRecognizedObject = false;
            Pen greenPen = new Pen(Color.Green, 2);
            CoreWebView2CapturePreviewImageFormat imageFormat = CoreWebView2CapturePreviewImageFormat.Png;
            float midX = thisWindow.webBrowser.Width / 2;
            while (true)
            {
                _ = thisWindow.Invoke(async () =>
                    await thisWindow
                        .webBrowser
                        .CoreWebView2
                        .CapturePreviewAsync(imageFormat, bitmap.GetStream())
                );
                showedRecognizedObject = false;
                var imageData = thisWindow._imageRecognition.Recognize(bitmap, itemsToRecognize);
                if (imageData != null && imageData?.Bitmap != null && imageData.HasData)
                {
                    try
                    {
                        thisWindow.Invoke(() =>
                        {
                            thisWindow.lblTime.Text = $"{imageData.Label} {DateTime.Now}";
                            thisWindow.pctImage.Image = new Bitmap(imageData.Bitmap, new Size(thisWindow.pctImage.Width, thisWindow.pctImage.Height));
                            using (var gr = Graphics.FromImage(thisWindow.pctImage.Image))
                            {
                                var x = midX + (imageData.XDeltaProportionFromBitmapCenter * thisWindow.pctImage.Image.Width);
                                gr.DrawLine(greenPen, x, 0, x, thisWindow.pctImage.Image.Height - 1);
                            }
                            showedRecognizedObject = true;
                        });
                    }
                    catch (ObjectDisposedException ex)
                    {
                        // This happens when the form is closed.
                        System.Diagnostics.Debug.WriteLine(ex);
                        break;
                    }
                }

                if (!showedRecognizedObject)
                {
                    thisWindow.Invoke(() =>
                    {
                        this.lblTime.Text = $"NOTHING FOUND {DateTime.Now}";
                        thisWindow.pctImage.Image = new Bitmap(bitmap, new Size(thisWindow.pctImage.Width, thisWindow.pctImage.Height));
                    });
                }
            }
        }

        private static IList<string> GetItemsToRecognize(RobotControlMain? thisWindow)
        {
            var items = new List<string>();
            if (thisWindow != null)
            {
                thisWindow.Invoke(() =>
                {
                    foreach (var selectedItem in thisWindow.lstLabelsToFind.SelectedItems)
                    {
                        if (selectedItem != null)
                        {
                            items.Add(selectedItem.ToString());
                        }
                    }
                });
            }

            return items;
        }

        private void chkHasGPU_CheckedChanged(object sender, EventArgs e) =>
            txtGPUDeviceId.Enabled = chkHasGPU.Checked;

        private void fromJSON()
        {
            if (Path.Exists(_jsonFileName))
            {
                var config = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(_jsonFileName));
                if (config != null)
                {
                    foreach (string controlName in config.Keys)
                    {
                        Control control = Controls[controlName];
                        if (control is TextBox)
                        {
                            ((TextBox)control).Text = config[controlName].ToString();
                        }
                        if (control is CheckBox)
                        {
                            ((CheckBox)control).Checked = bool.Parse(config[controlName].ToString());
                        }
                        if (control is ListBox)
                        {
                            ListBox lb = (ListBox)control;
                            IList<string> strings = JsonSerializer.Deserialize<List<string>>(config[controlName].ToString());
                            var indices = strings.Select(s => Array.FindIndex(YOLORecognitionLabels, l => l == s));
                            foreach (var index in indices)
                            {
                                lb.SelectedIndex = index;
                            }
                        }
                    }
                }
            }
        }

        private void saveToJSON()
        {
            var config = new Dictionary<string, object>();
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    config[control.Name] = ((TextBox)control).Text;
                }
                if (control is CheckBox)
                {
                    config[control.Name] = ((CheckBox)control).Checked;
                }
                if (control is ListBox)
                {
                    config[control.Name] = ((ListBox)control).SelectedItems.Cast<object>().ToList().Select(item => item.ToString()).ToList();
                }
            }

            File.WriteAllText(_jsonFileName, JsonSerializer.Serialize(config));
        }

        private async void RobotControlMain_Load(object sender, EventArgs e) =>
            await webBrowser.EnsureCoreWebView2Async();

        private void trkR_Scroll(object sender, EventArgs e) => updateAssociatedNumericUpDown(trkR, trkR.Value);
        private void trkL_Scroll(object sender, EventArgs e) => updateAssociatedNumericUpDown(trkL, trkL.Value);

        private void updateAssociatedNumericUpDown(TrackBar trackBar, int newValue)
        {
            newValue = Math.Min(Math.Max(newValue, -255), 255);
            trackBar.Value = newValue;
            var name = trackBar.Name;
            var numName = "num" + name.Substring(3);
            Control[] controls = this.Controls.Find(numName, false);
            if (controls != null && controls.Length > 0)
            {
                NumericUpDown num = (NumericUpDown)controls[0];
                num.Value = newValue;
            }
        }

        private void num_Changed()
            => _motorControl?.SetMotorsAsync((int)numL.Value, (int)numR.Value);
        private void btnConnect_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            _motorControl = ClassFactory.CreateSimpleMotorControl();
            txtRobotIPAddress.Text = _motorControl.IPAddress;
            Cursor = Cursors.Default;
        }

        private async void btnLeft_Click(object sender, EventArgs e)
            => await _motorControl?.SetMotorsAsync(-1*(int)numL.Value, (int)numR.Value);

        private async void btnAhead_ClickAsync(object sender, EventArgs e)
            => await _motorControl?.SetMotorsAsync((int)numL.Value, (int)numR.Value);

        private async void btnRight_ClickAsync(object sender, EventArgs e)
            => await _motorControl?.SetMotorsAsync((int)numL.Value, -1*(int)numR.Value);
        private async void btnBack_ClickAsync(object sender, EventArgs e)
            => await _motorControl?.SetMotorsAsync(-1 * (int)numL.Value, -1 * (int)numR.Value);

        private async void btnStop_ClickAsync(object sender, EventArgs e)
            => await _motorControl?.SetMotorsAsync(0, 0);
    }
}