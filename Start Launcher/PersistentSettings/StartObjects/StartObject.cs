namespace StartLauncher.PersistentSettings.StartObjects
{
    /// <summary>
    /// Abstract class defining an object to run on button press
    /// </summary>
    public abstract class StartObject
    {
        /// <summary>
        /// User description for the object
        /// </summary>
        public string UserGivenName { get; set; }
        /// <summary>
        /// Location of the object to run (exe path, url)
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Order in the list
        /// </summary>
        public int LaunchOrder { get; set; }

        protected StartObject() { }
        protected StartObject(string location, int launchOrder)
        {
            UserGivenName = Location = location;
            if (launchOrder < 1)
            {
                launchOrder = 1;
            }
            LaunchOrder = launchOrder;
        }

        protected StartObject(string location, string userGivenName, int launchOrder)
        {
            Location = location;
            UserGivenName = userGivenName;
            if (launchOrder < 1)
            {
                launchOrder = 1;
            }
            LaunchOrder = launchOrder;
        }
        public override string ToString()
        {
            return UserGivenName;
        }
        /// <summary>
        /// Method running the object
        /// </summary>
        /// <returns>True if object launched successfully, false otherwise</returns>
        public abstract bool Run();
        internal abstract void AddListToSettings(Settings settings);
    }
}
