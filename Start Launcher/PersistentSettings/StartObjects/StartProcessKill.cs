using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StartLauncher.PersistentSettings.StartObjects
{
    public class StartProcessKill : StartObject
    {
        public StartProcessKill()
        {

        }
        public StartProcessKill(string name, int launchOrder) : base(name, $"Kill: {name}", launchOrder)
        {

        }
        public StartProcessKill(string name, string userGivenName, int launchOrder) : base(name, userGivenName, launchOrder)
        {

        }
        public override Task<bool> Run() => Task.Run(() =>
        {
            var processes = Process.GetProcessesByName(Location);
            bool success = true;
            if (WaitBeforeLaunchMS > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(WaitBeforeLaunchMS));
            }
            foreach (var item in processes)
            {
                try
                {
                    item.Kill(true);
                }
                catch (System.Exception)
                {
                    success = false;
                }
            }
            if (WaitAfterLaunchMS > 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(WaitAfterLaunchMS));
            }
            return success;
        });
        internal override void AddListToSettings(Settings settings)
        {
            settings.startProcessKills.Add(this);
        }
    }
}
