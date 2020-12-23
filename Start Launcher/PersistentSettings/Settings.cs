using System.IO;
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
        /// <summary>
        /// Indicates whether the app should launch on system startup
        /// </summary>
        public bool LaunchOnStartup { get => launchOnStartup; set { launchOnStartup = value; SaveToFile(); StartupLaunch.Switch(value); } }

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
