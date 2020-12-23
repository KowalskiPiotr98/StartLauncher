using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StartLauncher
{
    /// <summary>
    /// Interaction logic for StartupObjects.xaml
    /// </summary>
    public partial class StartupObjectsWindow : Window
    {
        public PersistentSettings.Settings Settings { get; set; }
        public ObservableCollection<PersistentSettings.StartObjects.StartObject> StartObjects { get; private set; }
        public StartupObjectsWindow()
        {
            InitializeComponent();
        }
        public StartupObjectsWindow(PersistentSettings.Settings settings)
        {
            Settings = settings;
            StartObjects = new ObservableCollection<PersistentSettings.StartObjects.StartObject>(settings.GetGetAllStartObjects());
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void UserGivenNameText_LostFocus(object sender, RoutedEventArgs e)
        {
            var text = sender as TextBox;
            if (string.IsNullOrWhiteSpace(text.Text))
            {
                text.Text = (StartAppsListView.SelectedItem as PersistentSettings.StartObjects.StartObject).UserGivenName;
                return;
            }
            (StartAppsListView.SelectedItem as PersistentSettings.StartObjects.StartObject).UserGivenName = text.Text;
            StartAppsListView.Items.Refresh();
            StartAppsListView.SelectedIndex = -1;
            Settings.SaveToFile();
        }
    }
}
