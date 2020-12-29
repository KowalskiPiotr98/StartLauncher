using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StartLauncher.PersistentSettings
{
    /// <summary>
    /// Class containing basic application settings
    /// </summary>
    public class Settings
    {
#if RELEASE
        private static readonly string PERSISTENT_FOLDER_PATH = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}\\StartLauncher";
#else
        private static readonly string PERSISTENT_FOLDER_PATH = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}\\StartLauncher-DEBUG";
#endif
        private static readonly string SETTING_FILE_PATH = $"{PERSISTENT_FOLDER_PATH}\\settings.json";
        private bool launchOnStartup = true;
        private int? shutdownTimerSeconds;

        public readonly List<StartObjects.StartApplication> startApps = new List<StartObjects.StartApplication>();
        public readonly List<StartObjects.StartUrl> startUrls = new List<StartObjects.StartUrl>();

        /// <summary>
        /// Indicates whether the app should launch on system startup
        /// </summary>
        public bool LaunchOnStartup { get => launchOnStartup; set { launchOnStartup = value; SaveToFile(); StartupLaunch.Switch(value); } }
        public int? ShutdownTimerSeconds { get => shutdownTimerSeconds; set { shutdownTimerSeconds = value; SaveToFile(); } }
        public List<LaunchProfiles.LaunchProfile> LaunchProfiles { get; set; }
        public string DefaultLaunchProfile { get; set; }
        /// <summary>
        /// This property is only for JSON serialization and should not be used to get or set the actual list
        /// </summary>
        public List<StartObjects.StartApplication> StartAppsJSONProperty { get => startApps; set { startApps.Clear(); startApps.AddRange(value); } }
        public List<StartObjects.StartUrl> StartUrlsJSONProperty { get => startUrls; set { startUrls.Clear(); startUrls.AddRange(value); } }

        [JsonIgnore]
        public bool SkipSavingToFile { get; set; }

        public Settings()
        {
            SkipSavingToFile = true;
        }
        /// <summary>
        /// Saves current settings to file
        /// </summary>
        public void SaveToFile()
        {
            if (SkipSavingToFile)
            {
                return;
            }
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
                settings.SkipSavingToFile = false;
                var startObjectsManager = new StartObjects.StartObjectsManager(settings);
                foreach (var appLauncher in settings.startApps.ToList())
                {
                    try
                    {
                        appLauncher.Validate();
                    }
                    catch (System.Exception)
                    {
                        startObjectsManager.RemoveStartObject(appLauncher.LaunchOrder);//TODO: inform a user instead
                        settings.SaveToFile();
                    }
                }
                var launchProfileManager = new LaunchProfiles.LaunchProfileManager(settings);
                if (settings.LaunchProfiles is null || settings.LaunchProfiles.Count == 0)
                {
                    settings.LaunchProfiles = new List<LaunchProfiles.LaunchProfile>();
                    launchProfileManager.Add("default");
                }
                if (string.IsNullOrEmpty(settings.DefaultLaunchProfile))
                {
                    launchProfileManager.MakeDefault(settings.LaunchProfiles.First().Id);
                }
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
                var defaultSettings = GetDefaultSettings();
                defaultSettings.SaveToFile();
            }
        }

        public static Settings GetDefaultSettings()
        {
            var defaultSettings = new Settings
            {
                launchOnStartup = true
            };
            defaultSettings.SkipSavingToFile = false;
            return defaultSettings;
        }
    }
}
