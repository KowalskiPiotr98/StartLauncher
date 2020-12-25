using System;
using System.Collections.Generic;
using System.Linq;

namespace StartLauncher.PersistentSettings
{
    public class StoreAppsManager
    {
        private static readonly string STORE_APPS_FOLDER = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Microsoft\\WindowsApps";

        public List<string> Files { get; private set; }

        private readonly string[] _alreadyPresent;

        public StoreAppsManager(IEnumerable<string> alreadyPresent)
        {
            _alreadyPresent = alreadyPresent.Where(f => f.StartsWith(STORE_APPS_FOLDER)).Select(f => f.Split('\\').Last()).ToArray();
            GetStoreExecutables();
        }

        public List<string> GetStoreExecutables()
        {
            Files = System.IO.Directory.GetFiles(STORE_APPS_FOLDER, "*.exe").Select(f => f.Split('\\').Last()).Where(f => !_alreadyPresent.Contains(f)).ToList();
            return Files;
        }

        public StartObjects.StartApplication GetStartApplication(string exeName)
        {
            if (Files.Contains(exeName))
            {
                return new StartObjects.StartApplication($"{STORE_APPS_FOLDER}\\{exeName}", int.MaxValue);
            }
            return null;
        }
    }
}
