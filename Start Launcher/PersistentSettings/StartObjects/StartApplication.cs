﻿using System.Linq;

namespace StartLauncher.PersistentSettings.StartObjects
{
    /// <summary>
    /// Application to be started on system boot
    /// </summary>
    public class StartApplication : StartObject
    {
        public StartApplication()
        {

        }
        public StartApplication(string location, int launchOrder) : base(location, location.Split('\\').Last(), launchOrder)
        {
            Validate();
        }
        public StartApplication(string location, string userGivenName, int launchOrder) : base(location, userGivenName, launchOrder)
        {
            Validate();
        }

        public void Validate()
        {
            if (!System.IO.File.Exists(Location))
            {
                throw new System.IO.FileNotFoundException("Application file not found", Location);
            }
            if (!Location.EndsWith(".exe"))
            {
                throw new System.IO.FileFormatException("Only executable files are allowed");
            }
        }

        public override bool Run()
        {
            throw new System.NotImplementedException();
        }

        internal override void AddListToSettings(Settings settings)
        {
            settings.startApps.Add(this);
        }
    }
}