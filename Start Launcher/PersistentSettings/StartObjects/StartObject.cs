using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace StartLauncher.PersistentSettings.StartObjects
{
    /// <summary>
    /// Abstract class defining an object to run on button press
    /// </summary>
    public abstract class StartObject
    {
        /// <summary>
        /// User description for the object
        /// </summary>
        public string UserGivenName { get; set; }
        /// <summary>
        /// Location of the object to run (exe path, url)
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Order in the list
        /// </summary>
        public int LaunchOrder { get; set; }
        public string LaunchPofileId { get; set; }

        public bool WaitForExit { get; set; }
        public int WaitForExitMsTimeout { get; set; }

        public int WaitBeforeLaunchMS { get; set; }
        public int WaitAfterLaunchMS { get; set; }

        public string ProcessWaitName { get; set; }
        public int ProcessWaitTimeoutMS { get; set; }
        public bool ProcessWaitForExit { get; set; }

        public string WaitForIpAddress { get; set; }
        public int WaitForIpPort { get; set; } = 443;
        public int WaitForIpTimeoutMS { get; set; }

        protected StartObject() { }
        protected StartObject(string location, int launchOrder)
        {
            UserGivenName = Location = location;
            if (launchOrder < 1)
            {
                launchOrder = 1;
            }
            LaunchOrder = launchOrder;
        }

        protected StartObject(string location, string userGivenName, int launchOrder)
        {
            Location = location;
            UserGivenName = userGivenName;
            if (launchOrder < 1)
            {
                launchOrder = 1;
            }
            LaunchOrder = launchOrder;
        }
        public override string ToString()
        {
            return UserGivenName;
        }
        /// <summary>
        /// Method running the object
        /// </summary>
        /// <returns>True if object launched successfully, false otherwise</returns>
        public abstract Task<bool> Run();
        internal abstract void AddListToSettings(Settings settings);

        protected void CommonActionsBeforeLaunch()
        {
            if (WaitBeforeLaunchMS > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(WaitBeforeLaunchMS));
            }
            //Wait for process to start/exit
            if (!string.IsNullOrWhiteSpace(ProcessWaitName))
            {
                WaitForProcessLaunch();
            }
            //Wait for port
            if (!string.IsNullOrWhiteSpace(WaitForIpAddress) && IPAddress.TryParse(WaitForIpAddress, out _) && WaitForIpPort > 0 && WaitForIpPort < 65536)
            {
                WaitForIpAddressConnection();
            }
        }
        protected void CommonActionsAfterLaunch()
        {
            if (WaitAfterLaunchMS > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(WaitAfterLaunchMS));
            }
        }

        private void WaitForIpAddressConnection()
        {
            var timer = new Stopwatch();
            timer.Start();
            using var client = new TcpClient();
            var result = client.BeginConnect(WaitForIpAddress, WaitForIpPort, null, null);
            while (true)
            {
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(0.1));
                if (success)
                {
                    if (!client.Connected)
                    {
                        result = client.BeginConnect(WaitForIpAddress, WaitForIpPort, null, null);
                        continue;
                    }
                    return;
                }
                if (WaitForIpTimeoutMS > 0 && timer.ElapsedMilliseconds > WaitForIpTimeoutMS)
                {
                    try
                    {
                        client.EndConnect(result);
                    }
                    catch (Exception)
                    {
                        //Close just for good measure if you can
                    }
                    return;
                }
            }
        }

        private void WaitForProcessLaunch()
        {
            var timer = new Stopwatch();
            timer.Start();
            while (true)
            {
                var processes = Process.GetProcessesByName(ProcessWaitName);
                if ((ProcessWaitForExit && processes.Length == 0) || (!ProcessWaitForExit && processes.Length > 0))
                {
                    break;
                }
                if (ProcessWaitTimeoutMS > 0 && timer.ElapsedMilliseconds > ProcessWaitTimeoutMS)
                {
                    break;
                }
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
        }
    }
}
