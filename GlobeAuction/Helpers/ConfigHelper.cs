using System;
using System.Collections.Generic;
using System.Linq;
using GlobeAuction.Models;

namespace GlobeAuction.Helpers
{
    public enum ConfigNames
    {
        TeacherNames,
        HomePage_ShowRegisterIcon,
        HomePage_ShowDonateIcon,
        HomePage_ShowCatalogIcon,
        HomePage_ShowBidIcon,
        HomePage_ShowFaqIcon,
        HomePage_ShowStoreIcon,
        HomePage_ShowSponsorsIcon,
        HomePage_ShowShoutOutsIcon,
        HomePage_WhoHtml,
        HomePage_WhenHtml,
        HomePage_WhereHtml,
        HomePage_FooterHtml,
        SponsorLevelsOrdered,
        DonatePage_HeaderNote,
        DonatePage_SubHeaderNote,
        RegisterPage_StoreItemsSectionHeader,
        RegisterPage_StoreItemsSectionDescription
    }

    public static class ConfigHelper
    {
        private static Dictionary<string, ConfigProperty> _propertiesByName;
        private static Lazy<ApplicationDbContext> db => new Lazy<ApplicationDbContext>();

        public static List<string> GetTeacherNames()
        {
            return GetLineSeparatedConfig(ConfigNames.TeacherNames);
        }

        public static List<string> GetLineSeparatedConfig(ConfigNames config)
        {
            var propValue = GetConfigValue(config);

            if (string.IsNullOrEmpty(propValue)) return new List<string>();

            return propValue
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        public static string GetConfigValue(ConfigNames configName)
        {
            return GetConfigValue(configName.ToString());
        }

        public static T GetConfigValue<T>(ConfigNames configName, T defaultValue)
        {
            var configValue = GetConfigValue(configName);

            if (configValue == null) return defaultValue;

            try
            {
                return (T)Convert.ChangeType(configValue, typeof(T));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string GetConfigValue(string configName)
        {
            EnsureConfigLoaded();

            if (_propertiesByName.TryGetValue(configName, out var prop))
            {
                return prop.PropertyValue;
            }

            return null;
        }

        private static void EnsureConfigLoaded()
        {
            if (_propertiesByName == null)
            {
                _propertiesByName = db.Value.ConfigProperties.ToDictionary(c => c.PropertyName, c => c);
            }
        }

        public static void ResetCache()
        {
            _propertiesByName = null;
        }

        public static List<string> GetUnusedConfigName()
        {
            var allNames = Enum.GetNames(typeof(ConfigNames)).ToList(); ;

            EnsureConfigLoaded();

            var usedNamed = _propertiesByName.Keys.ToList();

            return allNames.Except(usedNamed).ToList();
        }
    }
}