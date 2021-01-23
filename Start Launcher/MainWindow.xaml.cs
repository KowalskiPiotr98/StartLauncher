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
        private readonly PersistentSettings.LaunchProfiles.LaunchProfileManager _launchProfileManager;
        public MainWindow()
        {
            PersistentSettings.Settings.InitialiseFile();
            try
            {
                Settings = PersistentSettings.Settings.ReadFromFile();
            }
            catch (System.IO.FileFormatException)
            {
                MessageBox.Show("Settings file was corrupted and will now be reverted to default.", "Start Launcher loading error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Settings = PersistentSettings.Settings.RestoreDefaultSettings();
            }
            App.CurrentApp.SetTimer(Settings.ShutdownTimerSeconds);
            _startObjectsManager = new PersistentSettings.StartObjects.StartObjectsManager(Settings);
            _launchProfileManager = new PersistentSettings.LaunchProfiles.LaunchProfileManager(Settings);
            InitializeComponent();
            SetProfileName();
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
                Application.Current.Shutdown();
            }
        }

        private void ModLaunchApps_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new StartupObjectsWindow(Settings, _startObjectsManager);
            settingsWindow.Show();
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if DEBUG
            PersistentSettings.StartupLaunch.Disable();
#endif
            App.CurrentApp.AutoShutdownCancelled = true;
        }

        private void ShutdownTimerSet_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.AutoShutdownCancelled = true;
            var shutdownTimerWindor = new ShutdownTimerPicker(Settings);
            shutdownTimerWindor.ShowDialog();
            if (shutdownTimerWindor.Confirmed)
            {
                Settings.ShutdownTimerSeconds = shutdownTimerWindor.ShutdownTimerSeconds;
            }
        }

        private void SetLaunchProfiles_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new PersistentSettings.LaunchProfiles.LaunchProfilesEditor(Settings);
            settingsWindow.Show();
            Close();
        }

        private void ProfileDown_Click(object sender, RoutedEventArgs e)
        {
            _launchProfileManager.PrevProfile(_startObjectsManager);
            SetProfileName();
        }

        private void ProfileUp_Click(object sender, RoutedEventArgs e)
        {
            _launchProfileManager.NextProfile(_startObjectsManager);
            SetProfileName();
        }

        private void SetProfileName()
        {
            ProfileName.Text = _launchProfileManager.FindById(_startObjectsManager.CurrentProfileId).Name;
        }
    }
}
