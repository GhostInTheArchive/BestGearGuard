# BestGearGuard

A V Rising server-side mod that enforces gear tier consistency between players' equipped items. Players who violate the gear rules receive a sun damage debuff until they comply.

---

## How It Works

BestGearGuard monitors every player's equipped gear in real time. Each item in the game is assigned a **tier** (1–9). When a player equips or changes an item, the mod checks three rules:

1. **Armor coherence** — All equipped armor pieces (chest, gloves, legs, boots) must stay within `MaxTierDifference` tiers of each other.
2. **Weapon/amulet ceiling** — The equipped weapon and amulet must not exceed the armor max tier by more than `MaxTierDifference`.
3. **Amulet floor** — The equipped amulet must not be more than `MinAmuletTierBelowArmor` tiers *below* the armor max tier. This prevents players from using low-tier amulets to avoid investing in a proper magic source.

If any rule is violated, the player receives a **permanent sun damage debuff** until they fix their gear. The debuff is removed automatically as soon as the player is back in compliance. A specific warning message is sent explaining exactly what is wrong and what tier is required.

> Wearing a lower-tier **weapon** than your armor is always allowed — some builds intentionally use lower-tier weapons for their skills. The amulet floor rule exists specifically to close the magic source exploit.

---

## Tier Reference

| Tier | Armor | Weapons | Magic Sources |
|------|-------|---------|---------------|
| 1 | Bone | Bone | Bone Ring |
| 2 | Bone Reinforced | Bone Reinforced | Blood Bone Ring |
| 3 | Cloth | Copper | Gravedigger Ring |
| 4 | Copper (Brute/Rogue/Scholar/Warrior) | Copper Reinforced | Ruby Ring, Sorcerer Ring… |
| 5 | Cotton | Iron | Relic |
| 6 | Iron (Brute/Rogue/Scholar/Warrior) | Iron Reinforced | Amethyst Pendant, Emerald Necklace… |
| 7 | Silk | Dark Silver + Legendary T06 | Bloodwine Amulet |
| 8 | Dark Silver (Brute/Rogue/Scholar/Warrior) | Sanguine | T08 Magic Sources + Soul Shards |
| 9 | Dracula (all variants) | Shadow Matter + Legendary T08 + Unique T08 | — |

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
| `MaxTierDifference` | `1` | Maximum allowed tier gap between armor pieces, and between weapon/amulet and the armor max tier (upward). |
| `MinAmuletTierBelowArmor` | `0` | Maximum number of tiers the amulet can be *below* the armor max tier. `0` = amulet must match armor tier exactly. Set to a high number to disable this check. |
| `DebuffEnabled` | `true` | Whether to apply a sun damage debuff to violating players. |

Changes to the config file can be applied live with `.gg-reload` — no restart needed.

---

## Admin Commands

All commands require admin privileges.

| Command | Alias | Description |
|---------|-------|-------------|
| `.gg-status` | `.ggs` | Show current configuration (enabled state, tier settings, debuff state). |
| `.gg-toggle on\|off` | `.ggt` | Enable or disable gear enforcement on the fly. |
| `.gg-debuff on\|off` | `.ggd` | Enable or disable the violation debuff without disabling the check. |
| `.gg-reload` | `.ggr` | Reload `gearguard.cfg` from disk without restarting. |
| `.gg-check <player>` | `.ggc` | Check whether a specific player is currently in violation, showing armor, weapon and amulet tiers and the violation type. |
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

### Example output — `.gg-check`

```
[GearGuard] PlayerName's gear is compliant. (Armor T6 / Weapon T7 / Amulet T6)
```

```
[GearGuard] WARNING: PlayerName is in violation! (AmuletTooLow)
  Armor max : Tier 8  |  Weapon: Tier 8  |  Amulet: Tier 4
```

---

## Violation Behaviour

When a player is detected in violation:

- They receive **specific warning messages** explaining exactly what is wrong — which slot is out of range and what tier is required or allowed.
- The warning is only sent **once per violation session** — it does not spam on every equipment check.
- A **sun damage debuff** is applied (if `DebuffEnabled = true`) with a reduced damage rate, making it a persistent inconvenience rather than an instant kill.
- The debuff is **permanent** until the player fixes their gear, at which point it is removed automatically.
- Checks are triggered on **every gear interaction** — equipping, unequipping, drag & drop, right-click from inventory, and right-click from external containers (chests, bags).

---

## Dependencies

- [BepInEx](https://github.com/BepInEx/BepInEx) for V Rising
- [VampireCommandFramework](https://github.com/decaprime/VampireCommandFramework)
