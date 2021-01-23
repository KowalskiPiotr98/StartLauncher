using StartLauncher.Utilities;
using System.Windows;

namespace StartLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App CurrentApp { get; private set; }

        private ShutdownTimer shutdownTimer;

        public void CancelShutdownTimer()
        {
            shutdownTimer?.Cancel();
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
