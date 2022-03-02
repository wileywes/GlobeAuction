using System;
using System.Collections.Generic;
using System.Linq;
using GlobeAuction.Models;

namespace GlobeAuction.Helpers
{
    public enum ConfigNames
    {
        TeacherNames
    }

    public class ConfigHelper
    {
        private static Dictionary<string, ConfigProperty> _propertiesByName;
        private ApplicationDbContext db;

        public ConfigHelper(ApplicationDbContext context)
        {
            db = context;
        }

        public List<string> GetTeacherNames()
        {
            var propValue = GetConfigValue(ConfigNames.TeacherNames);

            if (string.IsNullOrEmpty(propValue)) return new List<string>();

            return propValue.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public string GetConfigValue(ConfigNames configName)
        {
            return GetConfigValue(configName.ToString());
        }

        public string GetConfigValue(string configName)
        {
            if (_propertiesByName == null)
            {
                _propertiesByName = db.ConfigProperties.ToDictionary(c => c.PropertyName, c => c);
            }

            if (_propertiesByName.TryGetValue(configName, out var prop))
            {
                return prop.PropertyValue;
            }

            return null;
        }

        public static void ResetCache()
        {
            _propertiesByName = null;
        }
    }
}