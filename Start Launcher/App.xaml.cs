using System.Windows;

namespace StartLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public bool AutoShutdownCancelled { get; set; }
        public static App CurrentApp { get; private set; }

        private System.Windows.Threading.DispatcherTimer timer;

        public void SetTimer(int? secondsToShutdown)
        {
            if (secondsToShutdown.HasValue && timer is null)
            {
                timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = System.TimeSpan.FromSeconds(secondsToShutdown.Value)
                };
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else
            {
                timer?.Stop();
            }
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            if (!AutoShutdownCancelled)
            {
                Shutdown();
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CurrentApp = this;
        }
    }
}
