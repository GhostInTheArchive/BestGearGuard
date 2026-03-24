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

        public enum ViolationType
        {
            None,
            ArmorSpread,       // armor pieces too far apart from each other
            WeaponTooHigh,     // weapon exceeds armor max tier
            AmuletTooHigh,     // amulet exceeds armor max tier
            AmuletTooLow,      // amulet is below armor min tier (new)
        }

        public static bool CheckViolation(
            EntityManager em,
            Entity characterEntity,
            out int armorMaxTier,
            out int weaponTier,
            out int amuletTier,
            out ViolationType violation)
        {
            armorMaxTier = 0;
            weaponTier = 0;
            amuletTier = 0;
            violation = ViolationType.None;

            if (!em.HasComponent<Equipment>(characterEntity)) return false;

            var equipment = em.GetComponentData<Equipment>(characterEntity);

            // ── Armor ────────────────────────────────────────────────────────
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

            if (!anyArmor) return false;

            // ── Weapon ───────────────────────────────────────────────────────
            bool anyWeapon = false;
            {
                var guid = equipment.GetEquipmentItemId(EquipmentType.Weapon);
                if (guid.GuidHash != 0 && TierDatabase.TryGetTier(guid, out int tier))
                {
                    anyWeapon = true;
                    weaponTier = tier;
                }
            }

            // ── Amulet ───────────────────────────────────────────────────────
            bool anyAmulet = false;
            {
                var guid = equipment.GetEquipmentItemId(EquipmentType.MagicSource);
                if (guid.GuidHash != 0 && TierDatabase.TryGetTier(guid, out int tier))
                {
                    anyAmulet = true;
                    amuletTier = tier;
                }
            }

            int maxAllowed = GearGuardSettings.MaxTierDifference.Value;
            int minAmuletGap = GearGuardSettings.MinAmuletTierBelowArmor.Value;

            // 1. Armor pieces must stay within MaxTierDifference of each other
            if ((armorMaxTier - armorMin) > maxAllowed)
            {
                violation = ViolationType.ArmorSpread;
                return true;
            }

            // 2. Weapon must not exceed armor max tier by more than MaxTierDifference
            if (anyWeapon && weaponTier > armorMaxTier + maxAllowed)
            {
                violation = ViolationType.WeaponTooHigh;
                return true;
            }

            // 3. Amulet must not exceed armor max tier by more than MaxTierDifference
            if (anyAmulet && amuletTier > armorMaxTier + maxAllowed)
            {
                violation = ViolationType.AmuletTooHigh;
                return true;
            }

            // 4. Amulet must not be below armor max tier by more than MinAmuletTierBelowArmor
            if (anyAmulet && amuletTier < armorMaxTier - minAmuletGap)
            {
                violation = ViolationType.AmuletTooLow;
                return true;
            }

            return false;
        }
    }
}