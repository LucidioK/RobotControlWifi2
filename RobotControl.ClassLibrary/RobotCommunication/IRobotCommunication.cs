using System;
using System.Threading.Tasks;

namespace RobotControl.ClassLibrary.RobotCommunication
{
    public interface IRobotCommunication : IDisposable, ISimpleMotorControl
    {
        Task<string> ReadAsync();
        Task WriteAsync(string s);
        Task StartAsync();
        Task StopMotorsAsync();
        string Read();
        void SetMotors(int l, int r);
        void Write(string s);
        void Start();
        void StopMotors();
        string[] PortNames { get; }
    }
}