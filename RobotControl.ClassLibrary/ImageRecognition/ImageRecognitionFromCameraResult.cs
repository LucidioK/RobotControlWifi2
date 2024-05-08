using System.Text.Json.Serialization;
using System.Drawing;

namespace RobotControl.ClassLibrary.ImageRecognition
{
    public class ImageRecognitionFromCameraResult
    {
        public bool HasData { get; set; } = false;
        public Bitmap Bitmap { get; set; } = new Bitmap(1, 1);
        public float XDeltaProportionFromBitmapCenter { get; set; }
        public string Label { get; set; } = string.Empty;

        [JsonIgnore]
        public required IImageRecognitionFromCamera ImageRecognitionFromCamera { get; set; }
    }
}
