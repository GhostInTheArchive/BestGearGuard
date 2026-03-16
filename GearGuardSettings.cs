using BepInEx.Configuration;
using System.IO;

namespace BestGearGuard
{
    public static class GearGuardSettings
    {
        private static ConfigFile _config;

        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<int> MaxTierDifference;
        public static ConfigEntry<bool> DebuffEnabled;

        public static void Init(string configPath)
        {
            _config = new ConfigFile(Path.Combine(configPath, "gearguard.cfg"), true);

            Enabled = _config.Bind("General", "Enabled", true, "Enable or disable the gear tier enforcement.");
            MaxTierDifference = _config.Bind("General", "MaxTierDifference", 1, "Maximum allowed tier difference between equipped items.");
            DebuffEnabled = _config.Bind("General", "DebuffEnabled", true, "Apply a debuff to players who violate the gear tier rule.");
        }

        public static void Reload() => _config?.Reload();
    }
}