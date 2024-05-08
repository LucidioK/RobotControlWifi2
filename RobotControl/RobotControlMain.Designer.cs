
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Windows.Forms;

namespace RobotControl
{
    partial class RobotControlMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            label1 = new Label();
            txtRobotIPAddress = new TextBox();
            txtCameraRSTLURL = new TextBox();
            label2 = new Label();
            lstLabelsToFind = new ListBox();
            label3 = new Label();
            btnStart = new Button();
            pctImage = new PictureBox();
            toolTip1 = new ToolTip(components);
            chkHasGPU = new CheckBox();
            toolTip2 = new ToolTip(components);
            lblRobotStatus = new Label();
            txtGPUDeviceId = new TextBox();
            label4 = new Label();
            lblTime = new Label();
            webBrowser = new WebView2();
            chkChase = new CheckBox();
            trkL = new TrackBar();
            trkR = new TrackBar();
            label5 = new Label();
            label6 = new Label();
            numL = new NumericUpDown();
            numR = new NumericUpDown();
            btnConnect = new Button();
            btnAhead = new Button();
            btnStop = new Button();
            btnBack = new Button();
            btnRight = new Button();
            btnLeft = new Button();
            ((System.ComponentModel.ISupportInitialize)pctImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webBrowser).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkL).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkR).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numL).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numR).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 22);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(155, 25);
            label1.TabIndex = 0;
            label1.Text = "Robot IP Address:";
            // 
            // txtRobotIPAddress
            // 
            txtRobotIPAddress.Location = new Point(17, 52);
            txtRobotIPAddress.Margin = new Padding(4, 5, 4, 5);
            txtRobotIPAddress.Name = "txtRobotIPAddress";
            txtRobotIPAddress.Size = new Size(197, 31);
            txtRobotIPAddress.TabIndex = 1;
            txtRobotIPAddress.TextChanged += txtRobotIPAddress_TextChanged;
            // 
            // txtCameraRSTLURL
            // 
            txtCameraRSTLURL.Location = new Point(19, 245);
            txtCameraRSTLURL.Margin = new Padding(4, 5, 4, 5);
            txtCameraRSTLURL.Name = "txtCameraRSTLURL";
            txtCameraRSTLURL.Size = new Size(350, 31);
            txtCameraRSTLURL.TabIndex = 3;
            txtCameraRSTLURL.TextChanged += txtCameraRSTLURL_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(19, 215);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(225, 25);
            label2.TabIndex = 2;
            label2.Text = "Camera RSTP or HTTP URL:";
            // 
            // lstLabelsToFind
            // 
            lstLabelsToFind.FormattingEnabled = true;
            lstLabelsToFind.ItemHeight = 25;
            lstLabelsToFind.Location = new Point(19, 340);
            lstLabelsToFind.Margin = new Padding(4, 5, 4, 5);
            lstLabelsToFind.Name = "lstLabelsToFind";
            lstLabelsToFind.SelectionMode = SelectionMode.MultiSimple;
            lstLabelsToFind.Size = new Size(350, 304);
            lstLabelsToFind.TabIndex = 4;
            lstLabelsToFind.SelectedIndexChanged += lstLabelsToFind_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(19, 298);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(121, 25);
            label3.TabIndex = 5;
            label3.Text = "Items to Find:";
            // 
            // btnStart
            // 
            btnStart.Enabled = false;
            btnStart.Location = new Point(187, 647);
            btnStart.Margin = new Padding(4, 5, 4, 5);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(183, 37);
            btnStart.TabIndex = 6;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // pctImage
            // 
            pctImage.BorderStyle = BorderStyle.FixedSingle;
            pctImage.Location = new Point(396, 100);
            pctImage.Margin = new Padding(4, 5, 4, 5);
            pctImage.Name = "pctImage";
            pctImage.Size = new Size(367, 271);
            pctImage.TabIndex = 7;
            pctImage.TabStop = false;
            // 
            // chkHasGPU
            // 
            chkHasGPU.AutoSize = true;
            chkHasGPU.Location = new Point(19, 100);
            chkHasGPU.Margin = new Padding(4, 5, 4, 5);
            chkHasGPU.Name = "chkHasGPU";
            chkHasGPU.Size = new Size(279, 29);
            chkHasGPU.TabIndex = 11;
            chkHasGPU.Text = "This computer has NVidia GPU";
            toolTip1.SetToolTip(chkHasGPU, "Use Find-NVidiaCard.cmd to discover the NVidia GPU.");
            chkHasGPU.UseVisualStyleBackColor = true;
            chkHasGPU.CheckedChanged += chkHasGPU_CheckedChanged;
            // 
            // lblRobotStatus
            // 
            lblRobotStatus.AutoSize = true;
            lblRobotStatus.Location = new Point(569, 57);
            lblRobotStatus.Margin = new Padding(4, 0, 4, 0);
            lblRobotStatus.Name = "lblRobotStatus";
            lblRobotStatus.Size = new Size(128, 25);
            lblRobotStatus.TabIndex = 8;
            lblRobotStatus.Text = "lblRobotStatus";
            // 
            // txtGPUDeviceId
            // 
            txtGPUDeviceId.Location = new Point(19, 172);
            txtGPUDeviceId.Margin = new Padding(4, 5, 4, 5);
            txtGPUDeviceId.Name = "txtGPUDeviceId";
            txtGPUDeviceId.Size = new Size(120, 31);
            txtGPUDeviceId.TabIndex = 10;
            txtGPUDeviceId.TextChanged += txtGPUDeviceId_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(19, 142);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(130, 25);
            label4.TabIndex = 9;
            label4.Text = "GPU Device ID:";
            // 
            // lblTime
            // 
            lblTime.AutoSize = true;
            lblTime.Location = new Point(396, 57);
            lblTime.Margin = new Padding(4, 0, 4, 0);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(16, 25);
            lblTime.TabIndex = 12;
            lblTime.Text = ".";
            // 
            // webBrowser
            // 
            webBrowser.AllowExternalDrop = true;
            webBrowser.CreationProperties = null;
            webBrowser.DefaultBackgroundColor = Color.White;
            webBrowser.Location = new Point(396, 381);
            webBrowser.Margin = new Padding(4, 5, 4, 5);
            webBrowser.Name = "webBrowser";
            webBrowser.Size = new Size(367, 303);
            webBrowser.TabIndex = 0;
            webBrowser.TabStop = false;
            webBrowser.ZoomFactor = 1D;
            // 
            // chkChase
            // 
            chkChase.AutoSize = true;
            chkChase.Location = new Point(19, 652);
            chkChase.Name = "chkChase";
            chkChase.Size = new Size(133, 29);
            chkChase.TabIndex = 13;
            chkChase.Text = "Chase items";
            chkChase.UseVisualStyleBackColor = true;
            // 
            // trkL
            // 
            trkL.Location = new Point(840, 140);
            trkL.Maximum = 255;
            trkL.Name = "trkL";
            trkL.Size = new Size(188, 69);
            trkL.TabIndex = 14;
            trkL.Scroll += trkL_Scroll;
            // 
            // trkR
            // 
            trkR.Location = new Point(840, 215);
            trkR.Maximum = 255;
            trkR.Name = "trkR";
            trkR.Size = new Size(188, 69);
            trkR.TabIndex = 16;
            trkR.TickStyle = TickStyle.TopLeft;
            trkR.Scroll += trkR_Scroll;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(799, 140);
            label5.Name = "label5";
            label5.Size = new Size(20, 25);
            label5.TabIndex = 17;
            label5.Text = "L";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(799, 226);
            label6.Name = "label6";
            label6.Size = new Size(23, 25);
            label6.TabIndex = 18;
            label6.Text = "R";
            // 
            // numL
            // 
            numL.Location = new Point(1022, 140);
            numL.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numL.Minimum = new decimal(new int[] { 255, 0, 0, int.MinValue });
            numL.Name = "numL";
            numL.Size = new Size(77, 31);
            numL.TabIndex = 21;
            numL.TextAlign = HorizontalAlignment.Right;
            // 
            // numR
            // 
            numR.Location = new Point(1022, 220);
            numR.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numR.Minimum = new decimal(new int[] { 255, 0, 0, int.MinValue });
            numR.Name = "numR";
            numR.Size = new Size(77, 31);
            numR.TabIndex = 23;
            numR.TextAlign = HorizontalAlignment.Right;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(222, 45);
            btnConnect.Margin = new Padding(4, 5, 4, 5);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(148, 37);
            btnConnect.TabIndex = 24;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnAhead
            // 
            btnAhead.Location = new Point(866, 287);
            btnAhead.Name = "btnAhead";
            btnAhead.Size = new Size(78, 39);
            btnAhead.TabIndex = 25;
            btnAhead.Text = "Ahead";
            btnAhead.UseVisualStyleBackColor = true;
            btnAhead.Click += btnAhead_ClickAsync;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(866, 332);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(78, 39);
            btnStop.TabIndex = 26;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_ClickAsync;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(866, 377);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(78, 39);
            btnBack.TabIndex = 27;
            btnBack.Text = "Back";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_ClickAsync;
            // 
            // btnRight
            // 
            btnRight.Location = new Point(950, 332);
            btnRight.Name = "btnRight";
            btnRight.Size = new Size(78, 39);
            btnRight.TabIndex = 28;
            btnRight.Text = "Right";
            btnRight.UseVisualStyleBackColor = true;
            btnRight.Click += btnRight_ClickAsync;
            // 
            // btnLeft
            // 
            btnLeft.Location = new Point(782, 332);
            btnLeft.Name = "btnLeft";
            btnLeft.Size = new Size(78, 39);
            btnLeft.TabIndex = 29;
            btnLeft.Text = "Left";
            btnLeft.UseVisualStyleBackColor = true;
            btnLeft.Click += btnLeft_Click;
            // 
            // RobotControlMain
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1123, 715);
            Controls.Add(btnLeft);
            Controls.Add(btnRight);
            Controls.Add(btnBack);
            Controls.Add(btnStop);
            Controls.Add(btnAhead);
            Controls.Add(btnConnect);
            Controls.Add(numR);
            Controls.Add(numL);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(trkR);
            Controls.Add(trkL);
            Controls.Add(chkChase);
            Controls.Add(lblTime);
            Controls.Add(chkHasGPU);
            Controls.Add(txtGPUDeviceId);
            Controls.Add(label4);
            Controls.Add(lblRobotStatus);
            Controls.Add(pctImage);
            Controls.Add(webBrowser);
            Controls.Add(btnStart);
            Controls.Add(label3);
            Controls.Add(lstLabelsToFind);
            Controls.Add(txtCameraRSTLURL);
            Controls.Add(label2);
            Controls.Add(txtRobotIPAddress);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            Name = "RobotControlMain";
            Text = "RobotControlMain";
            Load += RobotControlMain_Load;
            ((System.ComponentModel.ISupportInitialize)pctImage).EndInit();
            ((System.ComponentModel.ISupportInitialize)webBrowser).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkL).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkR).EndInit();
            ((System.ComponentModel.ISupportInitialize)numL).EndInit();
            ((System.ComponentModel.ISupportInitialize)numR).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtRobotIPAddress;
        private TextBox txtCameraRSTLURL;
        private Label label2;
        private ListBox lstLabelsToFind;
        private Label label3;
        private Button btnStart;
        private PictureBox pctImage;
        private ToolTip toolTip1;
        private ToolTip toolTip2;
        private Label lblRobotStatus;
        private TextBox txtGPUDeviceId;
        private Label label4;
        private CheckBox chkHasGPU;
        private Label lblTime;
        private WebView2 webBrowser;
        private CheckBox chkChase;
        private TrackBar trkL;
        private TrackBar trkR;
        private Label label5;
        private Label label6;
        private NumericUpDown numL;
        private NumericUpDown numR;
        private Button btnConnect;
        private Button btnAhead;
        private Button btnStop;
        private Button btnBack;
        private Button btnRight;
        private Button btnLeft;
    }
}