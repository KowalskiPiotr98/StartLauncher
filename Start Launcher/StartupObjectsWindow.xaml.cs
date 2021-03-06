﻿using Microsoft.Win32;
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
        private readonly PersistentSettings.StartObjects.StartObjectsManager _startObjectsManager;
        public ObservableCollection<PersistentSettings.StartObjects.StartObject> StartObjects => new ObservableCollection<PersistentSettings.StartObjects.StartObject>(_startObjectsManager.GetAllStartObjects());
        public StartupObjectsWindow()
        {
            InitializeComponent();
        }
        public StartupObjectsWindow(PersistentSettings.Settings settings, PersistentSettings.StartObjects.StartObjectsManager startObjectsManager)
        {
            Settings = settings;
            _startObjectsManager = startObjectsManager;
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
            if (text.IsEnabled)
            {
                SaveCustomObjectName(text);
            }
        }

        private void UserGivenNameText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                SaveCustomObjectName(sender as TextBox);
            }
        }

        private void SaveCustomObjectName(TextBox text)
        {
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
            _startObjectsManager.RemoveStartObject((StartAppsListView.SelectedItem as PersistentSettings.StartObjects.StartObject).LaunchOrder);
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
                _startObjectsManager.ReorderStartObject(selIndex, selIndex - 1);
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
                _startObjectsManager.ReorderStartObject(selIndex, selIndex + 1);
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
                _startObjectsManager.ReorderStartObject(StartAppsListView.SelectedIndex + 1, selIndex);
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
            if (view.SelectedIndex == _startObjectsManager.GetAllStartObjects().Count - 1)
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
                    _startObjectsManager.AddStartObject(new PersistentSettings.StartObjects.StartApplication(ofd.FileName, int.MaxValue));
                    HandleAddingItems();
                }
                catch (System.ArgumentException)
                {
                    MessageBox.Show($"Application already exists on the list", "Unable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (System.IO.FileFormatException)
                {
                    MessageBox.Show($"Only .exe files are allowed", "Unable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (System.IO.FileNotFoundException)
                {
                    MessageBox.Show($"Selected application was not found", "Unable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AddStartUrl_Click(object sender, RoutedEventArgs e)
        {
            var urlPicker = new LaunchObjectsPickers.UrlPickerWindow();
            urlPicker.ShowDialog();
            if (urlPicker.Confirmed)
            {
                try
                {
                    _startObjectsManager.AddStartObject(new PersistentSettings.StartObjects.StartUrl(urlPicker.Url, int.MaxValue));
                    HandleAddingItems();
                }
                catch (System.ArgumentException)
                {
                    MessageBox.Show($"Unable to add URL", "Unable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AddStoreApp_Click(object sender, RoutedEventArgs e)
        {
            var storeAppPicker = new LaunchObjectsPickers.StoreAppsPicker(_startObjectsManager);
            storeAppPicker.ShowDialog();
            if (storeAppPicker.StartApplication != null)
            {
                _startObjectsManager.AddStartObject(storeAppPicker.StartApplication);
                HandleAddingItems();
            }
        }

        private void HandleAddingItems()
        {
            StartAppsListView.ItemsSource = StartObjects;
            StartAppsListView.Items.Refresh();
            StartAppsListView.SelectedIndex = -1;
        }

        private void AddKillProcess_Click(object sender, RoutedEventArgs e)
        {
            var processKillSelect = new LaunchObjectsPickers.StartProcessKillerPickerWindow(_startObjectsManager);
            var result = processKillSelect.ShowDialog();
            if (result.HasValue && result.Value)
            {
                HandleAddingItems();
            }
        }

        private async void WaitForExit_Checked(object sender, RoutedEventArgs e)
        {
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            o.WaitForExit = true;
            _startObjectsManager.SaveChanges();
        }

        private async void WaitForExit_Unchecked(object sender, RoutedEventArgs e)
        {
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            o.WaitForExit = false;
            _startObjectsManager.SaveChanges();
        }

        private async void WaitForExitTimeout_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            if (int.TryParse(textBox.Text, out int timeout))
            {
                o.WaitForExitMsTimeout = timeout;
                _startObjectsManager.SaveChanges();
            }
            else
            {
                MessageBox.Show("Timeout must be a valid number", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Information);
                textBox.Text = o.WaitForExitMsTimeout.ToString();
            }
        }

        private async void WaitBeforeChanged_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            if (int.TryParse(textBox.Text, out int waitTime))
            {
                o.WaitBeforeLaunchMS = waitTime;
                _startObjectsManager.SaveChanges();
            }
            else
            {
                MessageBox.Show("Time must be a valid number", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Information);
                textBox.Text = o.WaitBeforeLaunchMS.ToString();
            }
        }

        private async void WaitAfterChanged_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            if (int.TryParse(textBox.Text, out int waitTime))
            {
                o.WaitAfterLaunchMS = waitTime;
                _startObjectsManager.SaveChanges();
            }
            else
            {
                MessageBox.Show("Time must be a valid number", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Information);
                textBox.Text = o.WaitAfterLaunchMS.ToString();
            }
        }

        private async void ProcessWaitName_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            o.ProcessWaitName = textBox.Text;
            _startObjectsManager.SaveChanges();
        }

        private async void ProcessWaitForExit_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            o.ProcessWaitForExit = checkbox.IsChecked.Value;
            _startObjectsManager.SaveChanges();
        }

        private async void ProcessWaitTimeoutMS_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            if (int.TryParse(textBox.Text, out int timeout))
            {
                o.ProcessWaitTimeoutMS = timeout;
                _startObjectsManager.SaveChanges();
            }
            else
            {
                MessageBox.Show("Time must be a valid number", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Information);
                textBox.Text = o.ProcessWaitTimeoutMS.ToString();
            }
        }

        private async void WaitForIpAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            if (System.Net.IPAddress.TryParse(textBox.Text, out _))
            {
                o.WaitForIpAddress = textBox.Text;
                _startObjectsManager.SaveChanges();
            }
            else
            {
                MessageBox.Show("Invalid IPv4 address", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Information);
                textBox.Text = o.WaitForIpAddress.ToString();
            }
        }

        private async void WaitForIpPort_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            if (int.TryParse(textBox.Text, out int port) && port > 0 && port < 65536)
            {
                o.WaitForIpPort = port;
                _startObjectsManager.SaveChanges();
            }
            else
            {
                MessageBox.Show("Port must be a valid number in range [1,65535]", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Information);
                textBox.Text = o.WaitForIpPort.ToString();
            }
        }

        private async void WaitForIpTimeoutMS_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var o = await _startObjectsManager.GetStartObjectAtIndexAsync(StartAppsListView.SelectedIndex);
            if (o is null)
            {
                return;
            }
            if (int.TryParse(textBox.Text, out int timeout))
            {
                o.WaitForIpTimeoutMS = timeout;
                _startObjectsManager.SaveChanges();
            }
            else
            {
                MessageBox.Show("Time must be a valid number", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Information);
                textBox.Text = o.WaitForIpTimeoutMS.ToString();
            }
        }
    }
}
