using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Concurrent;
using System.Net.Http;

namespace RobotControl.ClassLibrary.RobotCommunication
{
    internal class RobotCommunication : IRobotCommunication, ISimpleMotorControl
    {
        private readonly HttpClient _httpClient;
        private string _ipAddress;
        public RobotCommunication(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) 
            { 
                ipAddress = GetRobotIPAddress();
                if (string.IsNullOrEmpty(ipAddress))
                {
                    throw new ArgumentException("Could not find robot in current network.");
                }
            }
            _httpClient = new HttpClient() { BaseAddress = new Uri($"http://{ipAddress}") };
        }

        public string[] PortNames => new string[] { "" };

        public string IPAddress => _ipAddress;

        public string GetRobotIPAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return string.Empty;
            }

            IPAddress? thisMachinesIp = Dns
                .GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip =>
                    ip.GetAddressBytes().Length == 4 &&
                    ip.AddressFamily == AddressFamily.InterNetwork);
            if (thisMachinesIp == null)
            {
                return string.Empty;
            }
            IPAddress? ip = null;
            byte[] ipBytes = thisMachinesIp.GetAddressBytes();
            var lastByteCandidates = Enumerable.Range(1, 255).Where(i => i != ipBytes[3]).ToList();

            _ = Parallel.ForEach(lastByteCandidates, (i, loopState) =>
            {

                IPAddress toPing = new IPAddress(new byte[] { ipBytes[0], ipBytes[1], ipBytes[2], (byte)i });
                PingReply pingReply = new Ping().Send(toPing, 1000);
                if (pingReply.Status == IPStatus.Success)
                {
                    if (isThisTheRobotsIp(toPing))
                    {
                        Interlocked.Exchange(ref ip, toPing);
                        loopState.Break();
                    }
                }
            });

            _ipAddress = ip != null ? ip.ToString() : string.Empty;
            return _ipAddress;
        }
        public void Dispose() => _httpClient.Dispose();

        public string Read() => throw new NotImplementedException();

        public Task<string> ReadAsync() => throw new NotImplementedException();

        public void SetMotors(int l, int r) => throw new NotImplementedException();

        public Task SetMotorsAsync(int l, int r) => WriteAsync($"M{hex(l)}{hex(r)}");


        public void Start() => throw new NotImplementedException();

        public Task StartAsync() => throw new NotImplementedException();

        public void StopMotors() => throw new NotImplementedException();

        public Task StopMotorsAsync() => WriteAsync("M+00+00");

        public void Write(string s) => throw new NotImplementedException();

        public Task WriteAsync(string s) => _httpClient.GetAsync(s);

        private string hex(int n) => (n >= 0 ? "+" : "-") + Math.Abs(n).ToString("X2");

        private bool isThisTheRobotsIp(IPAddress ip)
        {
            bool itIsTheRobotsIp = false;
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri($"http://{ip}") };
            try
            {
                HttpResponseMessage response = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, ""));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Task<string> t = response.Content.ReadAsStringAsync();
                    t.Wait();
                    string content = t.Result;
                    if (content.Contains("GARY THE SMARTROBOT"))
                    {
                        itIsTheRobotsIp = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

            return itIsTheRobotsIp;
        }
    }
}
