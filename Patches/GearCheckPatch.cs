using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using Unity.Entities;
using BestGearGuard.Services;
using BestGearGuard.Utils;

namespace BestGearGuard.Patches
{
    [HarmonyPatch]
    public static class GearCheckPatch
    {
        private static readonly System.Collections.Generic.HashSet<ulong> _warned = new();

        // Triggered when armor pieces are equipped/changed
        [HarmonyPatch(typeof(ArmorLevelSystem_Spawn), nameof(ArmorLevelSystem_Spawn.OnUpdate))]
        [HarmonyPostfix]
        public static void ArmorPostfix(ArmorLevelSystem_Spawn __instance)
            => CheckAllPlayers(__instance.EntityManager);

        // Triggered when weapon is equipped/changed
        [HarmonyPatch(typeof(WeaponLevelSystem_Spawn), nameof(WeaponLevelSystem_Spawn.OnUpdate))]
        [HarmonyPostfix]
        public static void WeaponPostfix(WeaponLevelSystem_Spawn __instance)
            => CheckAllPlayers(__instance.EntityManager);

        // Triggered when any item is equipped from inventory (drag & drop)
        [HarmonyPatch(typeof(EquipItemFromInventorySystem), nameof(EquipItemFromInventorySystem.OnUpdate))]
        [HarmonyPostfix]
        public static void EquipFromInventoryPostfix(EquipItemFromInventorySystem __instance)
            => CheckAllPlayers(__instance.EntityManager);

        // Triggered when any item is equipped via shortcut (right-click)
        [HarmonyPatch(typeof(EquipItemSystem), nameof(EquipItemSystem.OnUpdate))]
        [HarmonyPostfix]
        public static void EquipPostfix(EquipItemSystem __instance)
            => CheckAllPlayers(__instance.EntityManager);

        private static void CheckAllPlayers(EntityManager em)
        {
            if (!GearGuardSettings.Enabled.Value) return;

            var userEntities = em.CreateEntityQuery(ComponentType.ReadOnly<User>())
                                 .ToEntityArray(Allocator.Temp);
            try
            {
                foreach (var userEntity in userEntities)
                {
                    var user = em.GetComponentData<User>(userEntity);
                    if (!user.IsConnected) continue;

                    var characterEntity = user.LocalCharacter._Entity;
                    if (characterEntity == Entity.Null || !em.Exists(characterEntity)) continue;

                    if (!GearCheckerService.CheckViolation(em, characterEntity, out int armorMaxTier, out int weaponMaxTier))
                    {
                        _warned.Remove(user.PlatformId);
                        if (GearGuardSettings.DebuffEnabled.Value)
                            if (GearGuardSettings.DebuffEnabled.Value)
                                GearDebuffService.RemoveDebuff(em, characterEntity);
                        continue;
                    }

                    if (GearGuardSettings.DebuffEnabled.Value)
                        GearDebuffService.ApplyDebuff(em, characterEntity);

                    if (_warned.Contains(user.PlatformId)) continue;
                    _warned.Add(user.PlatformId);

                    SendWarning(em, user, armorMaxTier, weaponMaxTier);
                }
            }
            finally
            {
                userEntities.Dispose();
            }
        }

        private static void SendWarning(EntityManager em, User user, int armorMaxTier, int weaponMaxTier)
        {
            var line1 = (FixedString512Bytes)"<color=#ff5555>[GearGuard] WARNING: Your weapon/amulet tier is too high!</color>";
            var line2 = (FixedString512Bytes)$"<color=#ffaa00>Your weapon/amulet (Tier {weaponMaxTier}) exceeds your armor (Tier {armorMaxTier}) by more than {GearGuardSettings.MaxTierDifference.Value}.</color>";
            var line3 = (FixedString512Bytes)"<color=#ffaa00>Please adjust your equipment to comply with the server rules.</color>";

            ServerChatUtils.SendSystemMessageToClient(em, user, ref line1);
            ServerChatUtils.SendSystemMessageToClient(em, user, ref line2);
            ServerChatUtils.SendSystemMessageToClient(em, user, ref line3);
        }
    }
}