using System.Windows;
using System.Windows.Controls;

namespace StartLauncher
{
    /// <summary>
    /// Interaction logic for ShutdownTimerPicker.xaml
    /// </summary>
    public partial class ShutdownTimerPicker : Window
    {
        public int? ShutdownTimerSeconds { get; set; }
        public bool Confirmed { get; set; }
        public ShutdownTimerPicker(PersistentSettings.Settings settings)
        {
            ShutdownTimerSeconds = settings.ShutdownTimerSeconds;
            InitializeComponent();
            Enabled.IsChecked = ShutdownTimerSeconds.HasValue;
            SecondsToShutdownText.Text = ShutdownTimerSeconds.HasValue ? ShutdownTimerSeconds.Value.ToString() : "";
            SecondsToShutdownText.IsEnabled = ShutdownTimerSeconds.HasValue;
        }

        private void Enabled_Checked(object sender, RoutedEventArgs e)
        {
            var box = sender as CheckBox;
            SecondsToShutdownText.IsEnabled = box.IsChecked.Value;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!Enabled.IsChecked.Value || string.IsNullOrWhiteSpace(SecondsToShutdownText.Text))
            {
                ShutdownTimerSeconds = null;
            }
            else if (int.TryParse(SecondsToShutdownText.Text, out int seconds))
            {
                ShutdownTimerSeconds = seconds;
            }
            else
            {
                MessageBox.Show("Please provide a valid integer", "Invalid data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Confirmed = true;
            Close();
        }
    }
}
