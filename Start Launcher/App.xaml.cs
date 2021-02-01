using StartLauncher.Utilities;
using System.Windows;

namespace StartLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Version => $"v{Major}.{Minor}.{Patch}";
        public static int Major => 0;
        public static int Minor => 3;
        public static int Patch => 1;

        public static App CurrentApp { get; private set; }

        private ShutdownTimer shutdownTimer;

        public void CancelShutdownTimer()
        {
            shutdownTimer?.Cancel();
        }

        public void PauseShutdownTimer(bool pause = true)
        {
            shutdownTimer?.SetRunState(!pause);
        }

        public void SetTimer(int? secondsToShutdown, Controls.ProgressBarWithText progressBar)
        {
            if (!secondsToShutdown.HasValue)
            {
                shutdownTimer?.Cancel();
                return;
            }
            shutdownTimer = new ShutdownTimer(secondsToShutdown.Value, progressBar, this);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CurrentApp = this;
        }
    }
}
