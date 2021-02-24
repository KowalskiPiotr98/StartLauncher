using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StartLauncher.PersistentSettings.StartObjects
{
    /// <summary>
    /// Application to be started on system boot
    /// </summary>
    public class StartApplication : StartObject
    {
        public StartApplication()
        {

        }
        public StartApplication(string location, int launchOrder) : base(location, location.Split('\\').Last(), launchOrder)
        {
            Validate();
        }
        public StartApplication(string location, string userGivenName, int launchOrder) : base(location, userGivenName, launchOrder)
        {
            Validate();
        }

        public void Validate()
        {
            if (!System.IO.File.Exists(Location))
            {
                throw new System.IO.FileNotFoundException("Application file not found", Location);
            }
            if (!Location.EndsWith(".exe", System.StringComparison.CurrentCultureIgnoreCase))
            {
                throw new System.IO.FileFormatException("Only executable files are allowed");
            }
        }

        public override Task<bool> Run() => Task.Run(() =>
        {
            try
            {
                if (WaitBeforeLaunchMS > 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(WaitBeforeLaunchMS));
                }
                var process = System.Diagnostics.Process.Start(Location);
                if (WaitForExit)
                {
                    process.WaitForExit(WaitForExitMsTimeout == 0 ? -1 : WaitForExitMsTimeout);
                }

                if (WaitAfterLaunchMS > 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(WaitAfterLaunchMS));
                }
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        });

        internal override void AddListToSettings(Settings settings)
        {
            settings.startApps.Add(this);
        }
    }
}
