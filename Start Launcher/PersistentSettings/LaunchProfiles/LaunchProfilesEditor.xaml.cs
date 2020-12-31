using System.Collections.ObjectModel;
using System.Windows;

namespace StartLauncher.PersistentSettings.LaunchProfiles
{
    /// <summary>
    /// Interaction logic for LaunchProfilesEditor.xaml
    /// </summary>
    public partial class LaunchProfilesEditor : Window
    {
        private readonly LaunchProfileManager _manager;

        public ObservableCollection<LaunchProfile> LaunchProfiles { get; set; }
        public LaunchProfilesEditor(Settings settings)
        {
            _manager = new LaunchProfileManager(settings);
            LaunchProfiles = new ObservableCollection<LaunchProfile>(_manager.GetAll());
            InitializeComponent();
            ProfilesListView.SelectedIndex = LaunchProfiles.IndexOf(_manager.GetDefault());
        }

        private void ProfilesListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!DeleteMode.IsChecked.Value)
            {
                var indexOfDefault = LaunchProfiles.IndexOf(_manager.GetDefault());
                if (ProfilesListView.SelectedIndex == indexOfDefault || ProfilesListView.SelectedIndex == -1)
                {
                    return;
                }
                var selectedItem = ProfilesListView.SelectedItem as LaunchProfile;
                try
                {
                    _manager.MakeDefault(selectedItem.Id);
                }
                catch (System.ArgumentException)
                {
                    ProfilesListView.SelectedIndex = indexOfDefault;
                }
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            new MainWindow().Show();
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewNameText.Text))
            {
                return;
            }
            try
            {
                _manager.Add(NewNameText.Text);
                NewNameText.Text = "";
                HandleChange();
            }
            catch (System.ArgumentException)
            {
                MessageBox.Show("Unable to add. Make sure provided name doesn't already exist.", "Unable to add", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void HandleChange()
        {
            LaunchProfiles = new ObservableCollection<LaunchProfile>(_manager.GetAll());
            ProfilesListView.SelectedIndex = DeleteMode.IsChecked.Value ? -1 : LaunchProfiles.IndexOf(_manager.GetDefault());
            ProfilesListView.ItemsSource = LaunchProfiles;
            ProfilesListView.Items.Refresh();
        }

        private void DeleteMode_Checked(object sender, RoutedEventArgs e)
        {
            ProfilesListView.SelectedIndex = -1;
        }

        private void DeleteMode_Unchecked(object sender, RoutedEventArgs e)
        {
            ProfilesListView.SelectedIndex = LaunchProfiles.IndexOf(_manager.GetDefault());
        }

        private void ProfilesListView_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!DeleteMode.IsChecked.Value)
            {
                return;
            }
            var item = ProfilesListView.SelectedItem as LaunchProfile;
            if (item.Id == _manager.GetDefault().Id)
            {
                return;
            }
            _manager.Delete(item.Id);
            HandleChange();
        }
    }
}
