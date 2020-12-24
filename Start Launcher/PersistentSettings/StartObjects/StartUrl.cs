using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StartLauncher.PersistentSettings.StartObjects
{
    public class StartUrl : StartObject
    {
        public StartUrl()
        {

        }
        public StartUrl(string location, int launchOrder) : base(location, location, launchOrder)
        {
            Validate();
        }
        public StartUrl(string location, string userGivenName, int launchOrder) : base(location, userGivenName, launchOrder)
        {
            Validate();
        }

        public void Validate()
        {
            if (!Uri.IsWellFormedUriString(Location, UriKind.Absolute))
            {
                throw new ArgumentException("InvalidUrl");
            }
            if (!Location.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ArgumentException("Only HTTP protocol is allowed");
            }
        }

        public override Task<bool> Run() => Task.Run(() =>
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Location,
                    UseShellExecute = true
                };
                _ = Process.Start(psi);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        });

        internal override void AddListToSettings(Settings settings)
        {
            settings.startUrls.Add(this);
        }
    }
}
