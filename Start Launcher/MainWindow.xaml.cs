﻿using System.Windows;
using System.Windows.Controls;

namespace Start_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static StartLauncher.PersistentSettings.Settings Settings { get; private set; }
        public MainWindow()
        {
            StartLauncher.PersistentSettings.Settings.InitialiseFile();
            Settings = StartLauncher.PersistentSettings.Settings.ReadFromFile();
            InitializeComponent();
            LaunchOnStartup.IsChecked = Settings.LaunchOnStartup;
        }

        private void LaunchOnStartup_Click(object sender, RoutedEventArgs e)
        {
            var launch = sender as MenuItem;
            Settings.LaunchOnStartup = launch.IsChecked;
        }
    }
}
