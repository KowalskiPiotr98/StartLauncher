namespace StartLauncher.PersistentSettings.LaunchProfiles
{
    public class LaunchProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public LaunchProfile()
        {

        }
        public LaunchProfile(string name)
        {
            Id = System.Guid.NewGuid().ToString();
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
