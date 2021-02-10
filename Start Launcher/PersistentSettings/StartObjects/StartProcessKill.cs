using System.Diagnostics;
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
            return success;
        });
        internal override void AddListToSettings(Settings settings)
        {
            settings.startProcessKills.Add(this);
        }
    }
}
