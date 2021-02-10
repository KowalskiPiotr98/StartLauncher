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
        public override Task<bool> Run() => throw new System.NotImplementedException();
        internal override void AddListToSettings(Settings settings)
        {
            settings.startProcessKills.Add(this);
        }
    }
}
