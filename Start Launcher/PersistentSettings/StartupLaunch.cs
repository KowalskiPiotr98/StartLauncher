namespace StartLauncher.PersistentSettings
{
    static class StartupLaunch
    {
        private static readonly string REGISTRY_KEY = "Start Launcher";
        /// <summary>
        /// Enables app start on system boot
        /// </summary>
        public static void Enable()
        {
            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            if (regKey.GetValue(REGISTRY_KEY) is null)
            {
                regKey.SetValue(REGISTRY_KEY, exePath);
            }
        }
        /// <summary>
        /// Disables app start on system boot
        /// </summary>
        public static void Disable()
        {
            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            regKey.DeleteValue(REGISTRY_KEY, false);
        }
        /// <summary>
        /// Switches between enabled and disabled boot startup according to <paramref name="enable"/>
        /// </summary>
        /// <param name="enable">True if boot startup should be enabled, false if disabled</param>
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
