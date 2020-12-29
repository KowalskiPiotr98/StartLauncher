using System;
using System.Collections.Generic;
using System.Linq;

namespace StartLauncher.PersistentSettings.LaunchProfiles
{
    public class LaunchProfileManager
    {
        private readonly Settings _settings;

        public LaunchProfileManager(Settings settings)
        {
            _settings = settings;
        }

        public List<LaunchProfile> GetAll() => _settings.LaunchProfiles;
        public LaunchProfile GetDefault() => FindById(_settings.DefaultLaunchProfile);
        public LaunchProfile FindById(string id)
        {
            return _settings.LaunchProfiles.FirstOrDefault(l => l.Id == id);
        }
        public void Add(string name)
        {
            if (_settings.LaunchProfiles.Any(l => l.Name == name))
            {
                throw new ArgumentException("Launch profile with this name already exists", nameof(name));
            }
            _settings.LaunchProfiles.Add(new LaunchProfile(name));
            _settings.SaveToFile();
        }
        public void MakeDefault(string id)
        {
            if (!_settings.LaunchProfiles.Any(l => l.Id == id))
            {
                throw new ArgumentException("Id not found", nameof(id));
            }
            _settings.DefaultLaunchProfile = id;
            _settings.SaveToFile();
        }
        public void Delete(string id)
        {
            if (!_settings.LaunchProfiles.Any(l => l.Id == id))
            {
                throw new ArgumentException("Id not found", nameof(id));
            }
            if (_settings.DefaultLaunchProfile == id)
            {
                throw new ArgumentException("Unable to delete default launch profile", nameof(id));
            }
            _settings.LaunchProfiles.RemoveAll(l => l.Id == id);
            _settings.SaveToFile();
        }
    }
}
