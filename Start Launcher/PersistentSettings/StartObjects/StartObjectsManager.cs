using System.Collections.Generic;
using System.Linq;

namespace StartLauncher.PersistentSettings.StartObjects
{
    public class StartObjectsManager
    {
        private readonly Settings _settings;

        public StartObjectsManager(Settings settings)
        {
            _settings = settings;
        }

        public List<StartObject> GetGetAllStartObjects()
        {
            return _settings.startApps.Cast<StartObject>().Concat(_settings.startUrls.Cast<StartObject>()).OrderBy(s => s.LaunchOrder).ToList();
        }
        public void AddStartObject(StartObject startObject)
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
            startObject.AddListToSettings(_settings);
            _settings.SaveToFile();
        }
        public void RemoveStartObject(int order)
        {
            if (order < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(order));
            }
            _settings.startApps.RemoveAll(a => a.LaunchOrder == order);
            _settings.startUrls.RemoveAll(a => a.LaunchOrder == order);
            foreach (var apps in GetGetAllStartObjects().Where(l => l.LaunchOrder > order))
            {
                apps.LaunchOrder--;
            }
            _settings.SaveToFile();
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
            _settings.SaveToFile();
        }
    }
}
