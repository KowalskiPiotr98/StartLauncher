using StartLauncher.PersistentSettings.StartObjects;
using System.Windows;

namespace StartLauncher.LaunchObjectsPickers
{
    /// <summary>
    /// Interaction logic for StartProcessKillerPickerWindow.xaml
    /// </summary>
    public partial class StartProcessKillerPickerWindow : Window
    {
        private readonly StartObjectsManager _objectsManager;
        public StartProcessKillerPickerWindow(StartObjectsManager objectsManager)
        {
            _objectsManager = objectsManager;
            InitializeComponent();
        }

        private void ProcessNameInput_Click(object sender, RoutedEventArgs e)
        {
            var procName = ProcessNameBox.Text;
            if (string.IsNullOrWhiteSpace(procName))
            {
                _ = MessageBox.Show("Process name cannot be empty", "Invalid process name", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _objectsManager.AddStartObject(new StartProcessKill(procName, 1));
            DialogResult = true;
            Close();
        }

        private void SelectFromRunning_Click(object sender, RoutedEventArgs e)
        {
            var runningProcessesDialog = new RunningProcessListPickerWindow();
            var result = runningProcessesDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                _objectsManager.AddStartObject(runningProcessesDialog.ProcessKill);
                DialogResult = true;
                Close();
            }
        }
    }
}
