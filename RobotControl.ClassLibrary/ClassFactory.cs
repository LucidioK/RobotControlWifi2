using RobotControl.ClassLibrary.ImageRecognition;
using RobotControl.ClassLibrary.RobotCommunication;

namespace RobotControl.ClassLibrary
{
    public static class ClassFactory
    {
        public static IRobotCommunication CreateRobotCommunication(string ipAddress) => new RobotCommunication.RobotCommunication(ipAddress);
        public static IImageRecognitionFromCamera CreateImageRecognitionFromCamera(int GPUDeviceId) => new ImageRecognitionFromCamera(GPUDeviceId);
        public static ISimpleMotorControl CreateSimpleMotorControl() => new RobotCommunication.RobotCommunication("");
    }
}
