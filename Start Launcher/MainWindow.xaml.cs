using System.Windows;
using System.Windows.Controls;

namespace StartLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static PersistentSettings.Settings Settings { get; private set; }
        public MainWindow()
        {
            PersistentSettings.Settings.InitialiseFile();
            Settings = PersistentSettings.Settings.ReadFromFile();
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
            var button = sender as Button;
            IsEnabled = false;

            //TODO: execute

            System.Environment.Exit(0);
        }
    }
}
