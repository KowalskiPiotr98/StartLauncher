using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace StartLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PersistentSettings.Settings Settings { get; private set; }

        private readonly PersistentSettings.StartObjects.StartObjectsManager _startObjectsManager;
        public MainWindow()
        {
            PersistentSettings.Settings.InitialiseFile();
            Settings = PersistentSettings.Settings.ReadFromFile();
            _startObjectsManager = new PersistentSettings.StartObjects.StartObjectsManager(Settings);
            InitializeComponent();
            LaunchOnStartup.IsChecked = Settings.LaunchOnStartup;
        }

        private void LaunchOnStartup_Click(object sender, RoutedEventArgs e)
        {
            var launch = sender as MenuItem;
            Settings.LaunchOnStartup = launch.IsChecked;
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            Hide();
            var failed = false;
            var failedNames = new StringBuilder();
            foreach (var start in _startObjectsManager.GetGetAllStartObjects())
            {
                if (!await start.Run())
                {
                    failed = true;
                    failedNames.AppendLine(start.UserGivenName);
                }
            }
            if (failed)
            {
                MessageBox.Show($"Some programs failed to launch:\n{failedNames}", "Launch failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsEnabled = true;
                Show();
            }
            else
            {
                System.Environment.Exit(0);
            }
        }

        private void ModLaunchApps_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new StartupObjectsWindow(Settings);
            settingsWindow.Show();
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if DEBUG
            PersistentSettings.StartupLaunch.Disable();
#endif
        }
    }
}
