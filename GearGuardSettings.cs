using BepInEx.Configuration;
using System.IO;

namespace BestGearGuard
{
    public static class GearGuardSettings
    {
        private static ConfigFile _config;

        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<int> MaxTierDifference;
        public static ConfigEntry<int> MinAmuletTierBelowArmor;
        public static ConfigEntry<bool> DebuffEnabled;

        public static void Init(string configPath)
        {
            _config = new ConfigFile(Path.Combine(configPath, "gearguard.cfg"), true);

            Enabled = _config.Bind(
                "General", "Enabled", true,
                "Enable or disable the gear tier enforcement.");

            MaxTierDifference = _config.Bind(
                "General", "MaxTierDifference", 1,
                "Maximum allowed tier difference between armor pieces, and between weapon/amulet and the armor max tier (upward).");

            MinAmuletTierBelowArmor = _config.Bind(
                "General", "MinAmuletTierBelowArmor", 1,
                "Maximum number of tiers the amulet can be BELOW the armor max tier. " +
                "0 = amulet must match armor max tier exactly (strictest). " +
                "1 = amulet can be 1 tier below. Set to a high number to disable this check.");

            DebuffEnabled = _config.Bind(
                "General", "DebuffEnabled", true,
                "Apply a sun damage debuff to players who violate the gear tier rules.");
        }

        public static void Reload() => _config?.Reload();
    }
}