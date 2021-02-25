using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StartLauncher.LaunchObjectsPickers
{
    /// <summary>
    /// Interaction logic for RunningProcessListPickerWindow.xaml
    /// </summary>
    public partial class RunningProcessListPickerWindow : Window
    {
        public PersistentSettings.StartObjects.StartProcessKill ProcessKill { get; set; }
        public Process[] Processes { get; set; }
        public IEnumerable<string> ProcessNames => Processes.Select(p => p.ProcessName).OrderBy(p => p).Distinct();
        public RunningProcessListPickerWindow()
        {
            Processes = Process.GetProcesses();
            InitializeComponent();
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var clicked = sender as ListViewItem;
            var procName = clicked.Content as string;
            if (!string.IsNullOrWhiteSpace(procName))
            {
                ProcessKill = new PersistentSettings.StartObjects.StartProcessKill(procName, 1);
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
