using ProjectM;
using BestGearGuard.Utils;
using Stunlock.Core;
using Unity.Entities;

namespace BestGearGuard.Services
{
    public static class GearCheckerService
    {
        private static readonly EquipmentType[] ArmorSlots = new[]
        {
            EquipmentType.Chest,
            EquipmentType.Gloves,
            EquipmentType.Legs,
            EquipmentType.Footgear,
        };

        private static readonly EquipmentType[] WeaponSlots = new[]
        {
            EquipmentType.Weapon,
            EquipmentType.MagicSource,
        };

        public static bool CheckViolation(EntityManager em, Entity characterEntity, out int armorMaxTier, out int weaponMaxTier)
        {
            armorMaxTier = 0;
            weaponMaxTier = 0;

            if (!em.HasComponent<Equipment>(characterEntity)) return false;

            var equipment = em.GetComponentData<Equipment>(characterEntity);

            int armorMin = 99;
            armorMaxTier = 0;
            bool anyArmor = false;

            foreach (var slotType in ArmorSlots)
            {
                var guid = equipment.GetEquipmentItemId(slotType);
                if (guid.GuidHash == 0) continue;
                if (!TierDatabase.TryGetTier(guid, out int tier)) continue;
                anyArmor = true;
                if (tier > armorMaxTier) armorMaxTier = tier;
                if (tier < armorMin) armorMin = tier;
            }

            bool anyWeapon = false;
            foreach (var slotType in WeaponSlots)
            {
                var guid = equipment.GetEquipmentItemId(slotType);
                if (guid.GuidHash == 0) continue;
                if (!TierDatabase.TryGetTier(guid, out int tier)) continue;
                anyWeapon = true;
                if (tier > weaponMaxTier) weaponMaxTier = tier;
            }

            if (!anyArmor) return false;

            int maxAllowed = GearGuardSettings.MaxTierDifference.Value;

            // 1. Armor pieces must stay within MaxTierDifference of each other
            if (anyArmor && (armorMaxTier - armorMin) > maxAllowed)
                return true;

            // 2. Weapon/amulet must not exceed armor max tier by more than MaxTierDifference
            if (anyWeapon && weaponMaxTier > armorMaxTier + maxAllowed)
                return true;

            return false;
        }
    }
}