namespace RobotControl.ClassLibrary.RobotCommunication
{
    public interface ISimpleMotorControl
    {
        Task SetMotorsAsync(int l, int r);
        String IPAddress { get; }
    }
}
