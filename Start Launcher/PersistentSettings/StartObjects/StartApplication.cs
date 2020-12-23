using System.Linq;

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

        }
        public StartApplication(string location, string userGivenName, int launchOrder) : base(location, userGivenName, launchOrder)
        {

        }

        public override bool Run()
        {
            throw new System.NotImplementedException();
        }

        internal override void AddListToSettings(Settings settings)
        {
            settings.startApps.Add(this);
        }
    }
}
