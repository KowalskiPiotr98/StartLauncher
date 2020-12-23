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
            if (!System.IO.Directory.Exists(PERSISTENT_FOLDER_PATH))
            {
                System.IO.Directory.CreateDirectory(PERSISTENT_FOLDER_PATH);
            }
            var jsonString = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = false });
            System.IO.File.WriteAllText(SETTING_FILE_PATH, jsonString);
        }
        public static Settings ReadFromFile()
        {
            if (!System.IO.File.Exists(SETTING_FILE_PATH))
            {
                throw new System.IO.FileNotFoundException("Settings file was not found", SETTING_FILE_PATH);
            }
            try
            {
                var settingsJson = System.IO.File.ReadAllText(SETTING_FILE_PATH);
                var settings = System.Text.Json.JsonSerializer.Deserialize<Settings>(settingsJson);
                return settings;
            }
            catch (System.Text.Json.JsonException)
            {
                throw new System.IO.FileFormatException("Unable to read settings file");
            }
        }
        public static void InitialiseFile()
        {
            if (!System.IO.Directory.Exists(PERSISTENT_FOLDER_PATH))
            {
                System.IO.Directory.CreateDirectory(PERSISTENT_FOLDER_PATH);
            }
            if (!System.IO.File.Exists(SETTING_FILE_PATH))
            {
                var defaultSettings = new Settings();
                System.IO.File.WriteAllText(SETTING_FILE_PATH, System.Text.Json.JsonSerializer.Serialize(defaultSettings, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = false }));
            }
        }
    }
}
