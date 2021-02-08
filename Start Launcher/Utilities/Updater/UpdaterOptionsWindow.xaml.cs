using System.Diagnostics;
using System.Windows;

namespace StartLauncher.Utilities.Updater
{
    /// <summary>
    /// Interaction logic for UpdaterOptions.xaml
    /// </summary>
    public partial class UpdaterOptionsWindow : Window
    {
        private static readonly string _githubPageUrl = @"https://github.com/KowalskiPiotr98/StartLauncher";
        private readonly PersistentSettings.Settings _settings;
        public UpdaterOptionsWindow(PersistentSettings.Settings settings)
        {
            _settings = settings;
            InitializeComponent();
            CheckOnBoot.IsChecked = _settings.AutoUpdateCheck;
            VersionString.Text = $"Version: {App.Version}";
        }

        private async void CheckNow_Click(object sender, RoutedEventArgs e)
        {
            var checker = new UpdateChecker();
            bool updateAvailable;
            try
            {
                updateAvailable = await checker.IsUpdateAvailableAsync();
            }
            catch (UpdateException)
            {
                MessageBox.Show("Unable to check for updates", "Error", MessageBoxButton.OK);
                return;
            }
            if (updateAvailable)
            {
                var installNowConfirm = MessageBox.Show("New update is available. Download and install now?", "Update available", MessageBoxButton.YesNo);
                if (installNowConfirm == MessageBoxResult.Yes)
                {
                    if (!await UpdateInstaller.TryStartInstallerAsync(checker))
                    {
                        MessageBox.Show("Unable to install updates", "Error", MessageBoxButton.OK);
                        return;
                    }
                    App.CurrentApp.Shutdown();
                    return;
                }
                else
                {
                    return;
                }
            }
            MessageBox.Show("No updates available", "Update check", MessageBoxButton.OK);
        }

        private void GitHubLink_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = _githubPageUrl,
                UseShellExecute = true
            };
            _ = Process.Start(psi);
        }

        private void CheckOnBoot_Checked(object sender, RoutedEventArgs e)
        {
            _settings.AutoUpdateCheck = true;
            _settings.SaveToFile();
        }

        private void CheckOnBoot_Unchecked(object sender, RoutedEventArgs e)
        {
            _settings.AutoUpdateCheck = false;
            _settings.SaveToFile();
        }
    }
}
