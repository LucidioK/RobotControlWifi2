using System.Drawing;
using IronSoftware.Drawing;
namespace RobotControl.ClassLibrary.ImageRecognition
{
    public interface IImageRecognitionFromCamera : IDisposable
    {
        bool Open(string rtspOrHttpUrl);
        ImageRecognitionFromCameraResult Get(IList<string> labelsOfObjectsToDetect);
        ImageRecognitionFromCameraResult Recognize(AnyBitmap bitmap, IList<string> labelsOfObjectsToDetect);
    }
}