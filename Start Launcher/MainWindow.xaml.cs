using StartLauncher.Utilities;
using System.Text;
using System.Threading.Tasks;
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

        private static bool firstLaunch = true;
        private bool launching = false;
        private object launchingMutex = new object();

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
            _startObjectsManager = new PersistentSettings.StartObjects.StartObjectsManager(Settings);
            _launchProfileManager = new PersistentSettings.LaunchProfiles.LaunchProfileManager(Settings);
            InitializeComponent();
            SetProfileName();
            SetProfileMenuItems();
            App.CurrentApp.SetTimer(Settings.ShutdownTimerSeconds, ShutdownProgressBar, Settings.ShutdownTimerAction, _startObjectsManager);
            LaunchOnStartup.IsChecked = Settings.LaunchOnStartup;
        }

        private void SetProfileMenuItems()
        {
            foreach (var profile in _launchProfileManager.GetAll())
            {
                var menuItem = new MenuItem
                {
                    Header = profile.Name
                };
                menuItem.Click += SetActiveProfileFromMenuItem;
                ProfilesOptions.Items.Add(menuItem);
            }
        }

        private void SetActiveProfileFromMenuItem(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var profile = _launchProfileManager.FindByName(menuItem.Header as string);
            if (profile != null)
            {
                _startObjectsManager.SwitchToProfile(profile);
                SetProfileName();
            }
        }

        private void LaunchOnStartup_Click(object sender, RoutedEventArgs e)
        {
            var launch = sender as MenuItem;
            Settings.LaunchOnStartup = launch.IsChecked;
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            await Launch(exit: true);
        }

        private void ModLaunchApps_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new StartupObjectsWindow(Settings, _startObjectsManager);
            settingsWindow.Show();
            Close();
        }

        private async Task Launch(bool exit)
        {
            lock (launchingMutex)
            {
                if (!launching)
                {
                    launching = true;
                }
                else
                {
                    _startObjectsManager.CancelLaunch();
                    IsEnabled = false;
                    return;
                }
            }
            App.CurrentApp.CancelShutdownTimer();
            SwitchDisplay(launchingDisplay: true);
            var (failed, failedNames) = await _startObjectsManager.LaunchAllInCurrentProfile();
            if (failed)
            {
                MessageBox.Show($"Some programs failed to launch:\n{failedNames}", "Launch failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                
            }
            else
            {
                if (exit)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    SwitchDisplay(launchingDisplay: false);
                }
            }
            lock (launchingMutex)
            {
                launching = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if DEBUG
            PersistentSettings.StartupLaunch.Disable();
#endif
            App.CurrentApp.CancelShutdownTimer();
        }

        private void ShutdownTimerSet_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.CancelShutdownTimer();
            var shutdownTimerWindow = new ShutdownTimerPicker(Settings);
            shutdownTimerWindow.ShowDialog();
            if (shutdownTimerWindow.Confirmed)
            {
                Settings.ShutdownTimerSeconds = shutdownTimerWindow.ShutdownTimerSeconds;
                Settings.ShutdownTimerAction = shutdownTimerWindow.Action;
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
            App.CurrentApp.CancelShutdownTimer();
            _launchProfileManager.PrevProfile(_startObjectsManager);
            SetProfileName();
        }

        private void ProfileUp_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.CancelShutdownTimer();
            _launchProfileManager.NextProfile(_startObjectsManager);
            SetProfileName();
        }

        private void SetProfileName()
        {
            ProfileName.Text = _launchProfileManager.FindById(_startObjectsManager.CurrentProfileId).Name;
        }

        private void window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }

        private void FactorySettings_Click(object sender, RoutedEventArgs e)
        {
            var response = MessageBox.Show("This will delete all of your settings and restore the original state of the application. Do you want to continue?", "Restore factory settings", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (response == MessageBoxResult.Yes)
            {
                PersistentSettings.Settings.RestoreDefaultSettings();
                //Force window reload
                var newWindow = new MainWindow();
                Close();
                newWindow.Show();
            }
        }

        private void ShutdownCancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.CancelShutdownTimer();
        }

        private void UpdatesSettings_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.CancelShutdownTimer();
            var dialog = new Utilities.Updater.UpdaterOptionsWindow(Settings);
            _ = dialog.ShowDialog();
        }

        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            if (firstLaunch)
            {
#pragma warning disable S2696 // Instance members should not write to "static" fields
                firstLaunch = false;
#pragma warning restore S2696 // Instance members should not write to "static" fields
                if (Settings.AutoUpdateCheck)
                {
                    App.CurrentApp.PauseShutdownTimer(true);
                    if (await Utilities.Updater.UpdateChecker.CheckAndInstallUpdatesAsync())
                    {
                        App.CurrentApp.Shutdown();
                    }
                    App.CurrentApp.PauseShutdownTimer(false);
                }
            }
        }

        private void MoveToShellStartup_Click(object sender, RoutedEventArgs e)
        {
            var mbox = MessageBox.Show("This will move all applications from current profile to shell:startup. Are you sure you want to continue?", "shell:startup move", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbox == MessageBoxResult.Yes)
            {
                var mover = new ShellStartupMover(_startObjectsManager);
                mover.MoveAllApplicationsToShellStartup();
            }
        }

        private void MoveFromShellStartup_Click(object sender, RoutedEventArgs e)
        {
            var mbox = MessageBox.Show("This will move all applications from shell:startup to current profile. Are you sure you want to continue?", "shell:startup move", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbox == MessageBoxResult.Yes)
            {
                var mover = new ShellStartupMover(_startObjectsManager);
                mover.MoveAllApplicationsFromShellStartup();
            }
        }
        private async void LaunchNoExit_Click(object sender, RoutedEventArgs e)
        {
            await Launch(exit: false);
        }

        private void SwitchDisplay(bool launchingDisplay)
        {
            if (launchingDisplay)
            {
                LaunchButton.Content = "Cancel";
                LaunchNoExit.IsEnabled = false;
                ProfileDown.IsEnabled = ProfileUp.IsEnabled = false;
            }
            else
            {
                IsEnabled = true;
                LaunchButton.Content = "Launch and exit";
                LaunchNoExit.IsEnabled = true;
                ProfileDown.IsEnabled = ProfileUp.IsEnabled = true;
            }
        }
    }
}
