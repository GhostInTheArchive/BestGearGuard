using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using VampireCommandFramework;
using System.IO;

namespace BestGearGuard
{
    [BepInPlugin("com.tonpseudo.bestgearguard", "BestGearGuard", "1.0.0")]
    public class Plugin : BasePlugin
    {
        private Harmony _harmony;
        public static Plugin Instance { get; private set; }
        public static ManualLogSource Logger;

        public override void Load()
        {
            Instance = this;
            Logger = Log;

            var configPath = Path.Combine(BepInEx.Paths.ConfigPath, "BestGearGuard");
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            GearGuardSettings.Init(configPath);

            CommandRegistry.RegisterAll();

            _harmony = new Harmony("com.tonpseudo.bestgearguard");
            _harmony.PatchAll();

            Log.LogInfo("BestGearGuard loaded and ready!");
        }

        public override bool Unload()
        {
            _harmony?.UnpatchSelf();
            return base.Unload();
        }
    }
}
