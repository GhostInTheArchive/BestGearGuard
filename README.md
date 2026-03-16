# BestGearGuard

A V Rising server-side mod that enforces gear tier consistency between players' equipped items. Players who equip weapons or amulets far above their armor tier receive a sun damage debuff until they comply.

---

## How It Works

BestGearGuard monitors every player's equipped gear in real time. Each item in the game is assigned a **tier** (1–9). When a player equips or changes an item, the mod checks two rules:

1. **Armor coherence** — All equipped armor pieces (chest, gloves, legs, boots) must stay within `MaxTierDifference` tiers of each other.
2. **Weapon cap** — The equipped weapon and amulet must not exceed the highest armor tier by more than `MaxTierDifference`.

If either rule is violated, the player receives a **permanent sun damage debuff** until they fix their gear. The debuff is removed automatically as soon as the player is back in compliance.

> The armor tier sets the ceiling. A player in full T6 armor can equip weapons up to T7 (with `MaxTierDifference = 1`). Wearing a lower-tier weapon than your armor is always fine.

---

## Tier Reference

| Tier | Armor | Weapons |
|------|-------|---------|
| 1 | Bone | Bone |
| 2 | Bone Reinforced | Bone Reinforced |
| 3 | Cloth | Copper |
| 4 | Copper (Brute/Rogue/Scholar/Warrior) | Copper Reinforced |
| 5 | Cotton | Iron |
| 6 | Iron (Brute/Rogue/Scholar/Warrior) | Iron Reinforced |
| 7 | Silk | Dark Silver + Legendary T06 |
| 8 | Dark Silver (Brute/Rogue/Scholar/Warrior) | Sanguine |
| 9 | Dracula (all variants) | Shadow Matter + Legendary T08 + Unique T08 |

---

## Installation

1. Install **BepInEx** on your V Rising dedicated server.
2. Install **VampireCommandFramework**.
3. Drop `BestGearGuard.dll` into your `BepInEx/plugins/` folder.
4. Start the server — the config file is auto-generated on first launch at:
   ```
   BepInEx/config/BestGearGuard/gearguard.cfg
   ```

---

## Configuration

Config file: `BepInEx/config/BestGearGuard/gearguard.cfg`

| Key | Default | Description |
|-----|---------|-------------|
| `Enabled` | `true` | Enable or disable gear tier enforcement entirely. |
| `MaxTierDifference` | `1` | Maximum allowed tier gap between armor pieces, and between weapon/amulet and the armor max tier. |
| `DebuffEnabled` | `true` | Whether to apply a sun damage debuff to violating players. |

Changes to the config file can be applied live with `.gg-reload` — no restart needed.

---

## Admin Commands

All commands require admin privileges.

| Command | Alias | Description |
|---------|-------|-------------|
| `.gg-status` | `.ggs` | Show current configuration (enabled state, max tier diff, debuff state). |
| `.gg-toggle on\|off` | `.ggt` | Enable or disable gear enforcement on the fly. |
| `.gg-debuff on\|off` | `.ggd` | Enable or disable the violation debuff without disabling the check. |
| `.gg-reload` | `.ggr` | Reload `gearguard.cfg` from disk without restarting. |
| `.gg-check <player>` | `.ggc` | Check whether a specific player is currently in violation and display their armor/weapon max tiers. |
| `.gg-inspect <player>` | `.ggi` | Show a full breakdown of all equipped items for a player, with tier and item name for each slot. |

### Example output — `.gg-inspect`

```
--- Equipment of PlayerName ---
  Chest  : Tier 6  Iron_Warrior
  Gloves : Tier 6  Iron_Warrior
  Legs   : Tier 6  Iron_Warrior
  Boots  : Tier 6  Iron_Warrior
  Weapon : Tier 7  Sword_T07_DarkSilver
  Amulet : Tier 6  MistStoneNecklace
Levels — Weapon: 64  Armor: 62  Spell: 48
```

---

## Violation Behaviour

When a player is detected in violation:

- They receive **3 in-game warning messages** explaining which tiers are mismatched.
- The warning is only sent **once per violation session** — it does not spam on every equipment check.
- A **sun damage debuff** is applied (if `DebuffEnabled = true`) with a reduced damage rate, making it an inconvenience rather than an instant kill.
- The debuff is **permanent** until the player fixes their gear, at which point it is removed automatically.

---

## Dependencies

- [BepInEx](https://github.com/BepInEx/BepInEx) for V Rising
- [VampireCommandFramework](https://github.com/decaprime/VampireCommandFramework)
