using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StartLauncher.PersistentSettings
{
    /// <summary>
    /// Class containing basic application settings
    /// </summary>
    public class Settings
    {
        private static readonly string PERSISTENT_FOLDER_PATH = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}\\StartLauncher";
        private static readonly string SETTING_FILE_PATH = $"{PERSISTENT_FOLDER_PATH}\\settings.json";
        private bool launchOnStartup = true;
        public readonly List<StartObjects.StartApplication> startApps = new List<StartObjects.StartApplication>();
        public readonly List<StartObjects.StartUrl> startUrls = new List<StartObjects.StartUrl>();

        /// <summary>
        /// Indicates whether the app should launch on system startup
        /// </summary>
        public bool LaunchOnStartup { get => launchOnStartup; set { launchOnStartup = value; SaveToFile(); StartupLaunch.Switch(value); } }
        /// <summary>
        /// This property is only for JSON serialization and should not be used to get or set the actual list
        /// </summary>
        public List<StartObjects.StartApplication> StartAppsJSONProperty { get => startApps; set { startApps.Clear(); startApps.AddRange(value); } }
        public List<StartObjects.StartUrl> StartUrlsJSONProperty { get => startUrls; set { startUrls.Clear(); startUrls.AddRange(value); } }

        public List<StartObjects.StartObject> GetGetAllStartObjects()
        {
            return startApps.Cast<StartObjects.StartObject>().Concat(startUrls.Cast<StartObjects.StartObject>()).OrderBy(s => s.LaunchOrder).ToList();
        }
        public void AddStartObject(StartObjects.StartObject startObject)
        {
            if (GetGetAllStartObjects().Any(o => o.Location == startObject.Location))
            {
                throw new System.ArgumentException("Object already exists", nameof(startObject));
            }
            if (GetGetAllStartObjects().Count < startObject.LaunchOrder)
            {
                startObject.LaunchOrder = GetGetAllStartObjects().Count + 1;
            }
            else
            {
                foreach (var presentStartObjects in GetGetAllStartObjects().Where(s => s.LaunchOrder >= startObject.LaunchOrder))
                {
                    presentStartObjects.LaunchOrder++;
                }
            }
            startObject.AddListToSettings(this);
            SaveToFile();
        }
        public void RemoveStartObject(int order)
        {
            if (order < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(order));
            }
            startApps.RemoveAll(a => a.LaunchOrder == order);
            startUrls.RemoveAll(a => a.LaunchOrder == order);
            foreach (var apps in GetGetAllStartObjects().Where(l => l.LaunchOrder > order))
            {
                apps.LaunchOrder--;
            }
            SaveToFile();
        }
        public void ReorderStartObject(int oldIndex, int newIndex)
        {
            if (oldIndex < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(oldIndex));
            }
            if (newIndex < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(newIndex));
            }
            var allObjects = GetGetAllStartObjects();
            if (oldIndex > allObjects.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(oldIndex));
            }
            if (newIndex > allObjects.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(newIndex));
            }
            var startObject = allObjects.First(o => o.LaunchOrder == oldIndex);
            if (newIndex < oldIndex)
            {
                foreach (var @object in allObjects.Where(o => o.LaunchOrder >= newIndex && o.LaunchOrder < oldIndex))
                {
                    @object.LaunchOrder++;
                }
            }
            else
            {
                foreach (var @object in allObjects.Where(o => o.LaunchOrder <= newIndex && o.LaunchOrder > oldIndex))
                {
                    @object.LaunchOrder--;
                }
            }
            startObject.LaunchOrder = newIndex;
            SaveToFile();
        }

        /// <summary>
        /// Saves current settings to file
        /// </summary>
        public void SaveToFile()
        {
            if (!Directory.Exists(PERSISTENT_FOLDER_PATH))
            {
                _ = Directory.CreateDirectory(PERSISTENT_FOLDER_PATH);
            }
            var jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = false });
            System.IO.File.WriteAllText(SETTING_FILE_PATH, jsonString);
        }
        /// <summary>
        /// Loads settings from file
        /// </summary>
        /// <returns><see cref="Settings"/> object containing loaded settings</returns>
        public static Settings ReadFromFile()
        {
            if (!File.Exists(SETTING_FILE_PATH))
            {
                throw new FileNotFoundException("Settings file was not found", SETTING_FILE_PATH);
            }
            try
            {
                var settingsJson = File.ReadAllText(SETTING_FILE_PATH);
                var settings = JsonSerializer.Deserialize<Settings>(settingsJson);
                foreach (var appLauncher in settings.startApps.ToList())
                {
                    try
                    {
                        appLauncher.Validate();
                    }
                    catch (System.Exception)
                    {
                        settings.RemoveStartObject(appLauncher.LaunchOrder);//TODO: inform a user instead
                    }
                }
                settings.SaveToFile();//TODO: do somethig so that this isn't necessary
                return settings;
            }
            catch (JsonException)
            {
                throw new FileFormatException("Unable to read settings file");
            }
        }
        /// <summary>
        /// Ensused settings file and directory are created
        /// </summary>
        public static void InitialiseFile()
        {
            if (!Directory.Exists(PERSISTENT_FOLDER_PATH))
            {
                Directory.CreateDirectory(PERSISTENT_FOLDER_PATH);
            }
            if (!File.Exists(SETTING_FILE_PATH))
            {
                var defaultSettings = new Settings();
                File.WriteAllText(SETTING_FILE_PATH, JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = false }));
            }
        }
    }
}
