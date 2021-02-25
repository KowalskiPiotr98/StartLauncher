using System;
using System.Diagnostics;
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
        }
        protected void CommonActionsAfterLaunch()
        {
            if (WaitAfterLaunchMS > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(WaitAfterLaunchMS));
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
