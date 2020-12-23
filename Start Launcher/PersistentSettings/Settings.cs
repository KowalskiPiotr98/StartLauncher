using System.IO;
using System.Text.Json;

namespace StartLauncher.PersistentSettings
{
    public class Settings
    {
        private static readonly string PERSISTENT_FOLDER_PATH = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}\\StartLauncher";
        private static readonly string SETTING_FILE_PATH = $"{PERSISTENT_FOLDER_PATH}\\settings.json";
        private bool launchOnStartup = true;

        public bool LaunchOnStartup { get => launchOnStartup; set { launchOnStartup = value; SaveToFile(); StartupLaunch.Switch(value); } }
        public void SaveToFile()
        {
            if (!Directory.Exists(PERSISTENT_FOLDER_PATH))
            {
                _ = Directory.CreateDirectory(PERSISTENT_FOLDER_PATH);
            }
            var jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = false });
            System.IO.File.WriteAllText(SETTING_FILE_PATH, jsonString);
        }
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
