namespace StartLauncher.PersistentSettings
{
    static class StartupLaunch
    {
        private static readonly string REGISTRY_KEY = "Start Launcher";
        public static void Enable()
        {
            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            if (regKey.GetValue(REGISTRY_KEY) is null)
            {
                regKey.SetValue(REGISTRY_KEY, exePath);
            }
        }
        public static void Disable()
        {
            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            regKey.DeleteValue(REGISTRY_KEY, false);
        }
        public static void Switch(bool enable)
        {
            if (enable)
            {
                Enable();
            }
            else
            {
                Disable();
            }
        }
    }
}
