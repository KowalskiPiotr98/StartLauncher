﻿using System.Text;
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
            Settings = PersistentSettings.Settings.ReadFromFile();
            _startObjectsManager = new PersistentSettings.StartObjects.StartObjectsManager(Settings);
            _launchProfileManager = new PersistentSettings.LaunchProfiles.LaunchProfileManager(Settings);
            InitializeComponent();
            SetProfileName();
            App.CurrentApp.SetTimer(Settings.ShutdownTimerSeconds, ShutdownProgressBar);
            LaunchOnStartup.IsChecked = Settings.LaunchOnStartup;
        }

        private void LaunchOnStartup_Click(object sender, RoutedEventArgs e)
        {
            var launch = sender as MenuItem;
            Settings.LaunchOnStartup = launch.IsChecked;
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.CancelShutdownTimer();
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
            App.CurrentApp.CancelShutdownTimer();
        }

        private void ShutdownTimerSet_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.CancelShutdownTimer();
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

        private void ShutdownCancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.CancelShutdownTimer();
        }
    }
}
