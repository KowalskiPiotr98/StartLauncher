using System;
using System.Windows;

namespace StartLauncher
{
    /// <summary>
    /// Interaction logic for UrlPicker.xaml
    /// </summary>
    public partial class UrlPickerWindow : Window
    {
        public string Url { get; set; }
        public bool Confirmed { get; set; }
        public UrlPickerWindow()
        {
            InitializeComponent();
        }

        private void ConfirmUrl_Click(object sender, RoutedEventArgs e)
        {
            if (Uri.IsWellFormedUriString(UrlText.Text, UriKind.Absolute))
            {
                Confirmed = true;
                Url = UrlText.Text;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid url");
            }
        }
    }
}
