using Microsoft.Win32;
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
        public ObservableCollection<PersistentSettings.StartObjects.StartObject> StartObjects => new ObservableCollection<PersistentSettings.StartObjects.StartObject>(Settings.GetGetAllStartObjects());
        public StartupObjectsWindow()
        {
            InitializeComponent();
        }
        public StartupObjectsWindow(PersistentSettings.Settings settings)
        {
            Settings = settings;
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

        private void ObjectDelete_Click(object sender, RoutedEventArgs e)
        {
            if (StartAppsListView.SelectedIndex == -1)
            {
                return;
            }
            Settings.RemoveStartObject((StartAppsListView.SelectedItem as PersistentSettings.StartObjects.StartObject).LaunchOrder);
            StartAppsListView.ItemsSource = StartObjects;
            StartAppsListView.Items.Refresh();
            StartAppsListView.SelectedIndex = -1;
        }

        private void OrderUp_Click(object sender, RoutedEventArgs e)
        {
            var selIndex = StartAppsListView.SelectedIndex + 1;
            if (selIndex == 0)
            {
                return;
            }
            try
            {
                Settings.ReorderStartObject(selIndex, selIndex - 1);
                StartAppsListView.ItemsSource = StartObjects;
                StartAppsListView.Items.Refresh();
                StartAppsListView.SelectedIndex = selIndex - 2;
                OrderTextBox.Text = (selIndex - 1).ToString();
                StartAppsListView_SelectionChanged(StartAppsListView, null);
            }
            catch (System.ArgumentException)
            {
                //Reorder failed, quietly return
            }
        }

        private void OrderDown_Click(object sender, RoutedEventArgs e)
        {
            var selIndex = StartAppsListView.SelectedIndex + 1;
            if (selIndex == 0)
            {
                return;
            }
            try
            {
                Settings.ReorderStartObject(selIndex, selIndex + 1);
                StartAppsListView.ItemsSource = StartObjects;
                StartAppsListView.Items.Refresh();
                StartAppsListView.SelectedIndex = selIndex;
                OrderTextBox.Text = (selIndex + 1).ToString();
                StartAppsListView_SelectionChanged(StartAppsListView, null);
            }
            catch (System.ArgumentException)
            {
                //Reorder failed, quietly return
            }
        }

        private void OrderTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            var text = sender as TextBox;
            int selIndex;
            if (!int.TryParse(text.Text, out selIndex))
            {
                text.Text = (StartAppsListView.SelectedIndex + 1).ToString();
                return;
            }
            try
            {
                Settings.ReorderStartObject(StartAppsListView.SelectedIndex + 1, selIndex);
                StartAppsListView.ItemsSource = StartObjects;
                StartAppsListView.Items.Refresh();
                StartAppsListView.SelectedIndex = selIndex - 1;
                OrderTextBox.Text = (selIndex).ToString();
                StartAppsListView_SelectionChanged(StartAppsListView, null);
            }
            catch (System.ArgumentException)
            {
                text.Text = (StartAppsListView.SelectedIndex + 1).ToString();
            }
        }

        private void StartAppsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as ListView;
            if (view.SelectedIndex == -1)
            {
                SettingsPanel.IsEnabled = false;
                return;
            }
            SettingsPanel.IsEnabled = true;
            if (view.SelectedIndex == 0)
            {
                OrderUp.IsEnabled = false;
            }
            else
            {
                OrderUp.IsEnabled = true;
            }
            if (view.SelectedIndex == Settings.GetGetAllStartObjects().Count - 1)
            {
                OrderDown.IsEnabled = false;
            }
            else
            {
                OrderDown.IsEnabled = true;
            }
        }

        private void AddStartApp_Click(object sender, RoutedEventArgs args)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "EXE files (*.exe)|*.exe"
            };
            var res = ofd.ShowDialog();
            if (res.HasValue && res.Value)
            {
                try
                {
                    Settings.AddStartObject(new PersistentSettings.StartObjects.StartApplication(ofd.FileName, int.MaxValue));
                    StartAppsListView.ItemsSource = StartObjects;
                    StartAppsListView.Items.Refresh();
                    StartAppsListView.SelectedIndex = -1;
                }
                catch (System.ArgumentException)
                {
                    MessageBox.Show($"Application already exists on the list", "Unable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AddStartUrl_Click(object sender, RoutedEventArgs e)
        {
            var urlPicker = new UrlPickerWindow();
            urlPicker.ShowDialog();
            if (urlPicker.Confirmed)
            {
                try
                {
                    Settings.AddStartObject(new PersistentSettings.StartObjects.StartUrl(urlPicker.Url, int.MaxValue));
                    StartAppsListView.ItemsSource = StartObjects;
                    StartAppsListView.Items.Refresh();
                    StartAppsListView.SelectedIndex = -1;
                }
                catch (System.ArgumentException)
                {
                    MessageBox.Show($"Unable to add URL", "Unable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
