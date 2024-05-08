using AForge.Video;
using Microsoft.ML;
using Microsoft.ML.Data;
using IronSoftware.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using RobotControl.ClassLibrary.ONNXImplementation;
using SkiaSharp;
using System.Drawing;
using Font = System.Drawing.Font;
using PointF = System.Drawing.PointF;
using Color = System.Drawing.Color;
using SixLabors.ImageSharp;
using BitMiracle.LibTiff.Classic;

namespace RobotControl.ClassLibrary.ImageRecognition
{
    internal class ImageRecognitionFromCamera : IImageRecognitionFromCamera
    {
        private const VideoCaptureAPIs _videoCaptureAPI = VideoCaptureAPIs.FFMPEG;
        private readonly TinyYoloModel _tinyYoloModel;
        private readonly OnnxModelConfigurator _onnxModelConfigurator;
        private readonly OnnxOutputParser _onnxOutputParser;
        private readonly PredictionEngine<ImageInputData, TinyYoloPrediction> _tinyYoloPredictionEngine;
        private VideoCapture _videoCapture;
        private MJPEGStream _mjpegStream;
        private Thread _retrieveFramesFromVideoCaptureThread;
        private Mat _latestFrame = new Mat();
        private AnyBitmap _latestBitmap = null;
        private readonly Mat _emptyFrame = new Mat();
        private UInt128 _successfulRecognitionCount = 0;
        private int _failedRecognitionRetryCount = 0;
        public ImageRecognitionFromCamera(int GPUDeviceId)
        {
            var onnxFilePath = Directory.EnumerateFiles(".", "*.onnx").First();
            if (string.IsNullOrEmpty(onnxFilePath))
            {
                throw new FileNotFoundException($"Could not find any onnx file in the current folder {Directory.GetCurrentDirectory()}");
            }
            _tinyYoloModel = new TinyYoloModel(onnxFilePath);
            _onnxModelConfigurator = new OnnxModelConfigurator(_tinyYoloModel, GPUDeviceId);
            _onnxOutputParser = new OnnxOutputParser(_tinyYoloModel);
            _tinyYoloPredictionEngine = _onnxModelConfigurator.GetMlNetPredictionEngine<TinyYoloPrediction>();
        }

        public bool Open(string rtspOrHttpUrl)
        {
            Task<bool> taskopened;
            switch (rtspOrHttpUrl.Substring(0, 4))
            {
                case "http":
                    taskopened = OpenHTTPCameraAsync(rtspOrHttpUrl);
                    break;
                case "rtsp":
                    taskopened = OpenIPCameraAsync(rtspOrHttpUrl);
                    break;
                default:
                    throw new ArgumentException($"Open can only use either rtsp or http, could not process {rtspOrHttpUrl}");
            }
            taskopened.Wait();
            var opened = taskopened.Result;

            if (!opened)
            {
                throw new ArgumentException($"Could not open camera {rtspOrHttpUrl}");
            }

            return opened;
        }

        public ImageRecognitionFromCameraResult Get(IList<string> labelsOfObjectsToDetect) =>
            Recognize(_latestBitmap, labelsOfObjectsToDetect);

        public ImageRecognitionFromCameraResult Recognize(AnyBitmap bitmap, IList<string> labelsOfObjectsToDetect)
        {
            ImageRecognitionFromCameraResult result = GetEmptyImageRecognitionFromCameraResult();
            if (!FlipImage(bitmap))
            {
                return result;
            }
            result.Bitmap = bitmap;
            float[] labels = Predict(BitmapConverter.ToMat(bitmap));
            List<BoundingBox> filteredBoxes =
                _onnxOutputParser
                    .FilterBoundingBoxes(_onnxOutputParser.ParseOutputs(labels), 5, 0.5f)
                    .Where(b => 
                        labelsOfObjectsToDetect.Any(l => 
                            l.Equals(b.Label, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                    .ToList();
            if (filteredBoxes.Count > 0)
            {
                PopulateResult(result, filteredBoxes);
            }
            _successfulRecognitionCount++;
            _failedRecognitionRetryCount = 0;
            return result;
        }

        public void Dispose()
        {
            _videoCapture?.Dispose();
            _tinyYoloPredictionEngine?.Dispose();
        }

        private static void PopulateResult(ImageRecognitionFromCameraResult result, List<BoundingBox> filteredBoxes)
        {
            var highestConfidence    = filteredBoxes.Select(b => b.Confidence).Max();
            var highestConfidenceBox = filteredBoxes.First(b => b.Confidence == highestConfidence);
            var bbdfb                = BoundingBoxDeltaFromBitmap.FromBitmap(result.Bitmap.Width, result.Bitmap.Height, highestConfidenceBox);
            var dimensions           = HighlightDetectedObject(result.Bitmap, highestConfidenceBox, bbdfb);
            result.HasData           = true;
            result.Label             = dimensions + $", label={highestConfidenceBox.Label}";
            result.XDeltaProportionFromBitmapCenter 
                                     = bbdfb.XDeltaProportionFromBitmapCenter;
        }

        private bool FlipImage(AnyBitmap bitmap)
        {
            if (bitmap is null)
                return false;
            try
            {
                bitmap.RotateFlip(AnyBitmap.RotateMode.None, AnyBitmap.FlipMode.Vertical);
            }
            catch (InvalidImageContentException e)
            {
                if (_successfulRecognitionCount > 0 && _failedRecognitionRetryCount < 8)
                {
                    _failedRecognitionRetryCount++;
                    return false;
                }
                throw e;
            }

            _failedRecognitionRetryCount = 0;
            return true;
        }

        private ImageRecognitionFromCameraResult GetEmptyImageRecognitionFromCameraResult() =>
            new ImageRecognitionFromCameraResult
            {
                HasData = false,
                ImageRecognitionFromCamera = this,
            };

        private void PrintTimeSpans(IList<DateTime> times, IList<string> labels)
        {
            string s = "";
            for (var i = 1; i < times.Count; i++)
            {
                var elapsed = times[i] - times[i - 1];
                s += labels[i] + ":" + (int)elapsed.TotalMicroseconds + " ";
            }
            System.Diagnostics.Debug.WriteLine(s);
        }

        private float[] Predict(Mat frame) =>
            _tinyYoloPredictionEngine
                .Predict(new ImageInputData { Image = MLImage.CreateFromStream(frame.ToMemoryStream()) })
                .PredictedLabels;

        private async Task<bool> OpenIPCameraAsync(string cameraId)
        {
            bool opened;
            _videoCapture = new VideoCapture(cameraId, _videoCaptureAPI);
            
            opened = _videoCapture.Open(cameraId, _videoCaptureAPI);
            if (opened)
            {
                _retrieveFramesFromVideoCaptureThread = new Thread(RetrieveFramesFromVideoCaptureThreadProc) { Priority = ThreadPriority.AboveNormal };
                _retrieveFramesFromVideoCaptureThread.Start();
            }

            return opened;
        }

        private Task<bool> OpenHTTPCameraAsync(string rtspOrHttpUrl)
        {
            _mjpegStream = new MJPEGStream(rtspOrHttpUrl);
            _mjpegStream.NewFrame += NewFrameEventReceived;
            _mjpegStream.Start();
            return Task.FromResult(true);
        }

        private void NewFrameEventReceived(object sender, NewFrameEventArgs e) =>
            _latestBitmap = e.Frame;

        private static Font highlightFont = new Font(FontFamily.GenericMonospace, 15.0f);
        private static Brush highlightTextBrush = new SolidBrush(Color.Yellow);
        private static PointF highlightTextPosition = new PointF(64.0f, 64.0f);
        private static Pen redPen = new Pen(Color.Red, 3);
        private static Pen greenPen = new Pen(Color.Green, 2);
        private static string HighlightDetectedObject(AnyBitmap bitmap, BoundingBox box, BoundingBoxDeltaFromBitmap bbdfb)
        {
            var x = box.Dimensions.X * bbdfb.CorrX;
            var y = box.Dimensions.Y * bbdfb.CorrY;
            var w = box.Dimensions.Width * bbdfb.CorrX;
            var h = box.Dimensions.Height * bbdfb.CorrY;
            highlightTextPosition.X = bitmap.Width - 256;
            var midX = x + w / 2;
            var midY = y + h / 2;
            if (w != 0)
            {
                using (var gr = Graphics.FromImage(bitmap))
                {
                    gr.DrawRectangle(redPen, x, y, w, h);
                    gr.DrawLine(greenPen, 0, midY, bitmap.Width - 1, midY);
                    gr.DrawLine(greenPen, midX, 0, midX, bitmap.Height - 1);
                    gr.DrawString(DateTime.Now.ToString(), highlightFont, highlightTextBrush, highlightTextPosition);
                }
            }

            return $"x:{(int)x}, y:{(int)y}, w:{(int)w}, h:{(int)h}";
        }

        private void RetrieveFramesFromVideoCaptureThreadProc(object obj)
        {
            while (true)
            {
                _latestFrame = _videoCapture.RetrieveMat();
            }
        }


    }
}
