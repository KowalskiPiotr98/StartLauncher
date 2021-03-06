﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StartLauncher.LaunchObjectsPickers
{
    /// <summary>
    /// Interaction logic for StoreAppsPicker.xaml
    /// </summary>
    public partial class StoreAppsPicker : Window
    {
        public PersistentSettings.StoreAppsManager _appsManager { get; }
        public PersistentSettings.StartObjects.StartApplication StartApplication { get; private set; }
        public StoreAppsPicker(PersistentSettings.StartObjects.StartObjectsManager startObjectsManager)
        {
            _appsManager = new PersistentSettings.StoreAppsManager(startObjectsManager.GetAllStartObjects().Where(s => s is PersistentSettings.StartObjects.StartApplication).Select(s => s.Location));
            InitializeComponent();
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            StartApplication = _appsManager.GetStartApplication(item.Content as string);
            if (StartApplication != null)
            {
                Close();
            }
        }
    }
}
