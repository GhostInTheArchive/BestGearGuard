using VampireCommandFramework;
using BestGearGuard.Services;
using BestGearGuard.Utils;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace BestGearGuard.Commands
{
    internal class GearGuardCommand
    {
        [Command("gg-reload", "ggr", description: "Reloads gearguard.cfg from disk", adminOnly: true)]
        public static void Reload(ChatCommandContext ctx)
        {
            GearGuardSettings.Reload();
            ctx.Reply("<color=#00ff88>[GearGuard]</color> Configuration reloaded.");
        }

        [Command("gg-toggle", "ggt", description: "Enable or disable GearGuard. Usage: .gg-toggle on|off", adminOnly: true)]
        public static void Toggle(ChatCommandContext ctx, string state)
        {
            switch (state.ToLower())
            {
                case "on":
                    GearGuardSettings.Enabled.Value = true;
                    ctx.Reply("<color=#00ff88>[GearGuard]</color> Gear enforcement <color=#00ff00>enabled</color>.");
                    break;
                case "off":
                    GearGuardSettings.Enabled.Value = false;
                    ctx.Reply("<color=#00ff88>[GearGuard]</color> Gear enforcement <color=#ff5555>disabled</color>.");
                    break;
                default:
                    ctx.Reply("<color=#ff5555>[GearGuard]</color> Usage: .gg-toggle on|off");
                    break;
            }
        }

        [Command("gg-check", "ggc", description: "Check a player's gear tier compliance. Usage: .gg-check <playerName>", adminOnly: true)]
        public static void CheckPlayer(ChatCommandContext ctx, string playerName)
        {
            var em = VWorld.ServerEntityManager;
            var (targetUserEntity, targetUser) = FindPlayer(em, playerName);

            if (targetUserEntity == Entity.Null)
            {
                ctx.Reply($"<color=#ff5555>[GearGuard]</color> Player '{playerName}' not found or offline.");
                return;
            }

            var characterEntity = targetUser.LocalCharacter._Entity;
            if (characterEntity == Entity.Null || !em.Exists(characterEntity))
            {
                ctx.Reply($"<color=#ff5555>[GearGuard]</color> Could not find character entity for '{playerName}'.");
                return;
            }

            if (GearCheckerService.CheckViolation(
                    em, characterEntity,
                    out int armorMaxTier,
                    out int weaponTier,
                    out int amuletTier,
                    out var violation))
            {
                ctx.Reply($"<color=#ff5555>[GearGuard] WARNING: {playerName} is in violation! ({violation})</color>");
                ctx.Reply($"  Armor max : <color=#ffffff>Tier {armorMaxTier}</color>  |  Weapon: <color=#ffffff>Tier {weaponTier}</color>  |  Amulet: <color=#ffffff>Tier {amuletTier}</color>");
            }
            else
            {
                ctx.Reply($"<color=#00ff88>[GearGuard]</color> {playerName}'s gear is compliant. (Armor T{armorMaxTier} / Weapon T{weaponTier} / Amulet T{amuletTier})");
            }
        }

        [Command("gg-inspect", "ggi", description: "Inspect all equipped items of a player. Usage: .gg-inspect <playerName>", adminOnly: true)]
        public static void InspectPlayer(ChatCommandContext ctx, string playerName)
        {
            var em = VWorld.ServerEntityManager;
            var (targetUserEntity, targetUser) = FindPlayer(em, playerName);

            if (targetUserEntity == Entity.Null)
            {
                ctx.Reply($"<color=#ff5555>[GearGuard]</color> Player '{playerName}' not found or offline.");
                return;
            }

            var characterEntity = targetUser.LocalCharacter._Entity;
            if (characterEntity == Entity.Null || !em.Exists(characterEntity))
            {
                ctx.Reply($"<color=#ff5555>[GearGuard]</color> Could not find character entity for '{playerName}'.");
                return;
            }

            if (!em.HasComponent<Equipment>(characterEntity))
            {
                ctx.Reply($"<color=#ff5555>[GearGuard]</color> No Equipment component found on '{playerName}'.");
                return;
            }

            var equipment = em.GetComponentData<Equipment>(characterEntity);

            var slotLabels = new (EquipmentType type, string label)[]
            {
                (EquipmentType.Chest,       "Chest  "),
                (EquipmentType.Gloves,      "Gloves "),
                (EquipmentType.Legs,        "Legs   "),
                (EquipmentType.Footgear,    "Boots  "),
                (EquipmentType.Weapon,      "Weapon "),
                (EquipmentType.MagicSource, "Amulet "),
            };

            ctx.Reply($"<color=#ffaa00>--- Equipment of {playerName} ---</color>");

            foreach (var (slotType, label) in slotLabels)
            {
                var guid = equipment.GetEquipmentItemId(slotType);
                if (guid.GuidHash == 0)
                {
                    ctx.Reply($"  {label}: <color=#888888>empty</color>");
                    continue;
                }

                bool known = TierDatabase.TryGetTier(guid, out int tier);
                string tierStr = known
                    ? $"<color=#00ff88>Tier {tier}</color>"
                    : $"<color=#ffaa00>Tier ?</color>";
                string rawName = NameDatabase.GetName(guid.GuidHash);
                string itemName = rawName != null
                    ? System.Text.RegularExpressions.Regex.Replace(rawName, @"^Item_[^_]+_", "")
                    : $"[{guid.GuidHash}]";

                ctx.Reply($"  {label}: {tierStr}  <color=#aaaaaa>{itemName}</color>");
            }

            ctx.Reply($"<color=#ffaa00>Levels</color> — Weapon: <color=#ffffff>{equipment.WeaponLevel.Value:F0}</color>  Armor: <color=#ffffff>{equipment.ArmorLevel.Value:F0}</color>  Spell: <color=#ffffff>{equipment.SpellLevel.Value:F0}</color>");
        }

        [Command("gg-debuff", "ggd", description: "Enable or disable the violation debuff. Usage: .gg-debuff on|off", adminOnly: true)]
        public static void Debuff(ChatCommandContext ctx, string state)
        {
            switch (state.ToLower())
            {
                case "on":
                    GearGuardSettings.DebuffEnabled.Value = true;
                    ctx.Reply("<color=#00ff88>[GearGuard]</color> Violation debuff <color=#00ff00>enabled</color>.");
                    break;
                case "off":
                    GearGuardSettings.DebuffEnabled.Value = false;
                    ctx.Reply("<color=#00ff88>[GearGuard]</color> Violation debuff <color=#ff5555>disabled</color>.");
                    break;
                default:
                    ctx.Reply("<color=#ff5555>[GearGuard]</color> Usage: .gg-debuff on|off");
                    break;
            }
        }

        [Command("gg-status", "ggs", description: "Shows current GearGuard configuration", adminOnly: true)]
        public static void Status(ChatCommandContext ctx)
        {
            string state = GearGuardSettings.Enabled.Value ? "<color=#00ff00>ON</color>" : "<color=#ff5555>OFF</color>";
            string debuffState = GearGuardSettings.DebuffEnabled.Value ? "<color=#00ff00>ON</color>" : "<color=#ff5555>OFF</color>";
            ctx.Reply("<color=#ffaa00>--- GearGuard Status ---</color>");
            ctx.Reply($"  Enforcement          : {state}");
            ctx.Reply($"  Max tier diff        : <color=#ffffff>{GearGuardSettings.MaxTierDifference.Value}</color>  <color=#888888>(weapon/amulet ceiling above armor)</color>");
            ctx.Reply($"  Min amulet gap       : <color=#ffffff>{GearGuardSettings.MinAmuletTierBelowArmor.Value}</color>  <color=#888888>(amulet floor below armor max)</color>");
            ctx.Reply($"  Debuff               : {debuffState}");
        }

        // ── Helper ──────────────────────────────────────────────────────────
        private static (Entity userEntity, User user) FindPlayer(EntityManager em, string playerName)
        {
            var userEntities = em.CreateEntityQuery(ComponentType.ReadOnly<User>())
                                 .ToEntityArray(Allocator.Temp);
            try
            {
                foreach (var ue in userEntities)
                {
                    var u = em.GetComponentData<User>(ue);
                    if (u.CharacterName.ToString().ToLower() == playerName.ToLower())
                        return (ue, u);
                }
            }
            finally
            {
                userEntities.Dispose();
            }
            return (Entity.Null, default);
        }
    }
}