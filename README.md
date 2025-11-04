# 🎆 rFireworksShow

[![Game](https://img.shields.io/badge/Game-Rust-orange.svg)](https://rust.facepunch.com/)
[![Framework](https://img.shields.io/badge/Framework-Umod-blue.svg)](https://umod.org/)
[![Version](https://img.shields.io/badge/Version-0.1.40-green.svg)](https://github.com/FtuoilXelrash/rFireworksShow)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Advanced firework show system with customizable scheduling, loot drops, and intelligent spawn placement.**

---

## ✨ Features

### 🎯 Core Functionality
- **🌙 Two Independent Schedulers** - Time-based shows (specific hours) + Automatic shows (fixed intervals)
- **🎨 Random Colors** - Each firework randomly selects from 5 vibrant colors (Champagne, Green, Blue, Violet, Red)
- **📍 Smart Spawn Placement** - Spawn near players, at monuments, or fully random map locations
- **🗺️ Monument Support** - 29 whitelisted safe, above-ground monuments (airfield, harbor, radtown, etc.)
- **⚙️ Flexible Positioning** - Manual player location, specific coordinates, or automatic placement
- **💰 Loot Drops** - Configurable probability and item quantities that fall from explosions
- **🎪 Visual Feedback** - Green map markers show show locations in real-time
- **🔄 Dual Firing Modes** - Cumulative staggered delays (natural) or independent random (chaotic)

### ⚡ Advanced Options
- **🚫 Water Detection** - Prevent spawning in water bodies (optional)
- **⏰ Time Window Support** - Overnight time windows (e.g., 7:50 PM to 7:50 AM)
- **🎲 Probability Control** - Independent dice roll chances for each scheduler type
- **👥 Player Proximity** - Shows can spawn around active players or completely random
- **📊 Grid References** - Console logging with Rust map grid coordinates
- **🎛️ Server Time** - Uses server game time (not local machine time)

---

## 📦 Installation

1. **Download** `rFireworkShow.cs` from the [GitHub repository](https://github.com/FtuoilXelrash/rFireworksShow)
2. **Place** in your Rust server's `oxide/plugins/` directory
3. **Reload** the plugin or restart your server
4. **Configure** settings in the generated `oxide/config/rFireworksShow.json`

---

## ⚙️ Configuration

The plugin auto-generates a configuration file at `oxide/config/rFireworksShow.json`. Below is the complete default config:

```json
{
  "OnlyWhenPlayersOnline": true,
  "EnableMapMarkers": true,
  "EnableStaggeredFireMode": true,
  "EnableLootDrops": true,
  "LootDropChance": 50.0,
  "LootDropItems": {
    "gunpowder": { "min": 3, "max": 5 },
    "cloth": { "min": 3, "max": 5 },
    "charcoal": { "min": 3, "max": 5 },
    "metal.fragments": { "min": 3, "max": 5 }
  },
  "SpawnAtRandomPlayersMapLocation": false,
  "OnlySpawnOnLand": true,
  "OnlySpawnAtMonuments": false,
  "SpreadRadius": 30.0,
  "HeightOffset": 30.0,
  "PlayerSelectionRadius": 500.0,
  "AutomaticShowsEnabled": false,
  "AutomaticShowsIntervalMinSeconds": 3600.0,
  "AutomaticShowsIntervalMaxSeconds": 7200.0,
  "AutomaticShowsFireworksMin": 1,
  "AutomaticShowsFireworksMax": 6,
  "AutomaticShowsDiceRollChancePercent": 50,
  "TimeBasedShowsEnabled": true,
  "TimeBasedShowsFireworksMin": 3,
  "TimeBasedShowsFireworksMax": 60,
  "TimeBasedStartHour": 19.50,
  "TimeBasedShowEndHour": 7.50,
  "TimedShowIntervalMinSeconds": 15.0,
  "TimedShowIntervalMaxSeconds": 30.0,
  "TimeBasedShowsDiceRollChancePercent": 75
}
```

### 🎨 Display & Effect Settings

| Option | Default | Description |
|--------|---------|-------------|
| `OnlyWhenPlayersOnline` | true | Only run automatic shows when at least one player is online |
| `EnableMapMarkers` | true | Display green map markers at firework show locations |
| `EnableStaggeredFireMode` | true | Cumulative staggered delays (natural) vs independent random delays (chaotic) |
| `EnableLootDrops` | true | Drop loot items when fireworks explode |
| `LootDropChance` | 50.0 | Percentage chance (0-100) for loot per firework |
| `LootDropItems` | See JSON | Dictionary of item names with min/max quantities |
| `SpawnAtRandomPlayersMapLocation` | false | Spawn near random players (true) or fully random map (false) |
| `OnlySpawnOnLand` | true | Prevent spawning in water bodies |
| `OnlySpawnAtMonuments` | false | Only spawn at whitelisted monuments |
| `SpreadRadius` | 30.0 | Radius around center point for firework spread |
| `HeightOffset` | 30.0 | Height above ground to spawn fireworks |
| `PlayerSelectionRadius` | 500.0 | Radius around player to center show |

### 🤖 Automatic Show Settings

| Option | Default | Description |
|--------|---------|-------------|
| `AutomaticShowsEnabled` | false | Enable traditional scheduled shows |
| `AutomaticShowsIntervalMinSeconds` | 3600.0 | Minimum seconds between automatic shows (1 hour) |
| `AutomaticShowsIntervalMaxSeconds` | 7200.0 | Maximum seconds between automatic shows (2 hours) |
| `AutomaticShowsFireworksMin` | 1 | Minimum fireworks per show |
| `AutomaticShowsFireworksMax` | 6 | Maximum fireworks per show |
| `AutomaticShowsDiceRollChancePercent` | 50 | Spawn probability (0-100%) |

### 🌙 Time-Based Show Settings

| Option | Default | Description |
|--------|---------|-------------|
| `TimeBasedShowsEnabled` | true | Enable time-based shows |
| `TimeBasedShowsFireworksMin` | 3 | Minimum fireworks per show |
| `TimeBasedShowsFireworksMax` | 60 | Maximum fireworks per show |
| `TimeBasedStartHour` | 19.50 | Start time (19:50 = 7:50 PM) - format: HH.MM |
| `TimeBasedShowEndHour` | 7.50 | End time (07:50 = 7:50 AM) - format: HH.MM |
| `TimedShowIntervalMinSeconds` | 15.0 | Minimum seconds between attempts |
| `TimedShowIntervalMaxSeconds` | 30.0 | Maximum seconds between attempts |
| `TimeBasedShowsDiceRollChancePercent` | 75 | Spawn probability during time window (0-100%) |

### 🎪 Available Firework Colors

The plugin randomly selects one color per firework from:

| Color | Prefab |
|-------|--------|
| 🟨 Champagne | `mortarchampagne.prefab` |
| 🟩 Green | `mortargreen.prefab` |
| 🟦 Blue | `mortarblue.prefab` |
| 🟪 Violet | `mortarviolet.prefab` |
| 🟥 Red | `mortarred.prefab` |

---

## 💬 Commands

### 🎮 Chat Commands

#### `/fs [count]` - Manual Show Trigger
```
Spawn fireworks at your player location
Requires: Admin
Optional: Firework count (default: config value)
Example: /fs 10 → Spawns 10 fireworks
```

#### `/fs x y z [count]` - Show at Coordinates
```
Spawn fireworks at specific map coordinates
Requires: Admin
Example: /fs 1000 100 2000 15 → Spawns 15 fireworks at those coords
```

#### `/fsrand` - Random Show (Test Mode)
```
Test automatic show behavior with random center point
Requires: Admin
Uses same logic as scheduled automatic shows
Picks random player proximity or random map location
```

#### `/fsrand local` - Show at Your Feet
```
Spawn fireworks at your current location (for testing)
Requires: Admin
Useful for testing effects and positioning
```

#### `/fsrand x y z` - Random Show at Coordinates
```
Spawn fireworks with show logic at specific coordinates
Requires: Admin
Example: /fsrand 1000 100 2000 → Random show at those coords
```

#### `/fstoggle` - Toggle Automatic Shows
```
Enable/disable automatic scheduled shows on-the-fly
Requires: Admin
Toggles: AutomaticShowsEnabled setting
Messages: "automatic shows are now ENABLED" or "DISABLED"
Saves config and restarts scheduler
```

### 🖥️ Console Commands

#### `rf.reload` - Reload Configuration
```
Reload plugin settings without restarting server
Usage: Can be run from server console or by admins in-game
Effect: Restarts show scheduler with new settings
```

---

## 🔐 Permissions

**Admin-Only** - All commands and features require **server admin status** (IsAdmin check).

---

## 📖 How It Works

### 🌙 Time-Based Shows

When `TimeBasedShowsEnabled` is enabled:

1. **Check Time Window** - Continuously monitors if current time is between `TimeBasedStartHour` and `TimeBasedShowEndHour`
2. **Wait Outside Window** - If outside time range, scheduler pauses and checks again
3. **Roll Dice Inside Window** - When inside time window, rolls dice with `TimeBasedShowsDiceRollChancePercent` chance
4. **Spawn Show** - If dice succeeds, spawns show at configured location type
5. **Repeat** - Tries again every `TimedShowIntervalMinSeconds` to `TimedShowIntervalMaxSeconds` (randomized)

**Example**: With defaults (19:50-7:50, 75% chance, 15-30 second intervals):
- Between 7:50 PM and 7:50 AM: attempts every 15-30 seconds with 75% success rate
- Between 7:50 AM and 7:50 PM: scheduler sleeps (no attempts)

### 🤖 Automatic Shows

When `AutomaticShowsEnabled` is enabled:

1. **Check Players** - Respects `OnlyWhenPlayersOnline` setting (waits if no players)
2. **Wait Interval** - Pauses for `AutomaticShowsIntervalMinSeconds` to `AutomaticShowsIntervalMaxSeconds` (randomized)
3. **Roll Dice** - Rolls dice with `AutomaticShowsDiceRollChancePercent` chance
4. **Spawn Show** - If dice succeeds, spawns show at configured location type
5. **Repeat** - Cycle continues indefinitely

**Note**: Both schedulers run independently and simultaneously. You can enable/disable either one without affecting the other.

### 📍 Spawn Location Logic

The plugin determines spawn locations based on these settings (in order):

1. **Monument Spawning** - If `OnlySpawnAtMonuments` is true, randomly picks one of 29 whitelisted monuments
2. **Player Proximity** - If `SpawnAtRandomPlayersMapLocation` is true, spawns near a random active player
3. **Random Map** - Falls back to random map location
4. **Water Check** - If `OnlySpawnOnLand` is true, validates location isn't in water (retries up to 40 times)
5. **Final Fallback** - Uses center of map if all else fails

---

## 📊 Console Output

The plugin logs all shows to server console with helpful information:

```
Local Show[21.82]: Grid(R13) - Location(782.99, 51.10, 168.09) with 6 fireworks.
Random Show: Grid(H5) - Monument(Airfield) with 15 fireworks.
AutomaticShow: Grid(A1) - Location(50.00, 80.00, 120.00) with 3 fireworks.
TimeBasedShow[19.50]: Monument(Harbor) - Grid(M8) with 12 fireworks.
```

### Format Breakdown

| Component | Description |
|-----------|-------------|
| **Show Type** | Local Show, Random Show, AutomaticShow, or TimeBasedShow |
| **[Time]** | Server game time in brackets (TimeBasedShow only) |
| **Grid(XY)** | Rust map grid reference (A-Z columns, 0-25+ rows) |
| **Monument(Name)** | Monument name (only when `OnlySpawnAtMonuments` enabled) |
| **Location(x,y,z)** | Precise coordinates (only when monuments disabled) |
| **Fireworks** | Total fireworks in this show |

**Technical Details:**
- Grid calculations use Rust's `MapHelper` API for accuracy
- Times use server game time (`TOD_Sky.Instance.Cycle.Hour`) not local machine time
- Times match the in-game time shown to players for precise scheduling

---

## 💡 Tips & Configuration Guide

### 🎨 Loot Drop System

**How It Works:**
- Each firework has an independent `LootDropChance` roll
- On success, randomly picks one item type from `LootDropItems` dictionary
- Rolls quantity between min/max for that item
- Loot spawns 200 feet above firework location and falls naturally to ground

**Configuration Examples:**

*Conservative (rare drops):*
```json
"LootDropChance": 25.0,
"LootDropItems": {
  "gunpowder": { "min": 1, "max": 2 },
  "cloth": { "min": 1, "max": 2 }
}
```

*Generous (common drops):*
```json
"LootDropChance": 100.0,
"LootDropItems": {
  "gunpowder": { "min": 5, "max": 10 },
  "cloth": { "min": 5, "max": 10 },
  "charcoal": { "min": 5, "max": 10 },
  "metal.fragments": { "min": 10, "max": 20 }
}
```

### ⏰ Firing Modes

**Staggered Mode (Enabled by Default):**
- Fireworks fire with cumulative delays of 0.1-1.5 seconds between each
- Creates natural, chaotic rhythm (more realistic)
- Show duration varies based on cumulative delays
- Better for immersion and visual appeal

**Independent Mode:**
- Each firework fires with independent random delay (0-2 seconds)
- Creates unpredictable pattern
- All fireworks complete in ~2 seconds
- Better for rapid-fire effects

### 🗺️ Monument Spawning

**Whitelisted Safe Monuments (29 total):**
- Large: airfield_1, excavator_1, launch_site_1, military_tunnel_1, powerplant_1, trainyard_1, water_treatment_plant_1
- Medium: bandit_town, compound, junkyard_1, nuclear_missile_silo, radtown_small_3
- Small: fishing_village_a/b/c, gas_station_1, harbor_1/2, lighthouse, mining_quarry_a/b/c, oilrig_1/2, satellite_dish, sphere_tank, stables_a/b, supermarket_1, warehouse
- Special: arctic_research_base_a, desert_military_base_a/b/c/d, ferry_terminal_1

**Note:** These monuments were selected to prevent underground spawns (excludes caves, bunkers, power substations, etc.)

### 👥 Player Proximity Spawning

When `SpawnAtRandomPlayersMapLocation` is enabled:
- Randomly picks one active player
- Generates random angle around player (0-360°)
- Random distance from player (0 to `PlayerSelectionRadius` units)
- Creates shows near players for better engagement

### ⏳ Time Format

Time values use **decimal HH.MM format**:

| Time | Value |
|------|-------|
| 7:50 AM | 7.50 |
| 12:00 PM | 12.00 |
| 7:50 PM | 19.50 |
| 11:59 PM | 23.59 |

**Overnight Windows:**
The plugin correctly handles overnight time windows (e.g., 19:50-7:50 spans across midnight):
- 7:50 PM to midnight ✓
- Midnight to 7:50 AM ✓

---

## 📝 Notes

- **Colors**: Each firework independently selects a random color from the 5 available options
- **Map Markers**: Green circular markers (0.5 unit radius) appear on player maps during shows and automatically disappear after completion
- **Water Safety**: The land detection uses `WaterLevel.Test()` method from Rust's built-in APIs - not invasive, just height checks
- **Monument Data**: Uses `TerrainMeta.Path.Monuments` for dynamic caching - works on vanilla AND custom maps with no external dependencies
- **Code Quality**: All dead code has been removed, zero unused methods or configuration options
- **Console Reload**: Configuration changes take effect immediately on reload without requiring server restart
- **Performance**: Both schedulers run independently with minimal overhead

---

## 📋 Version History

See [CHANGES.md](CHANGES.md) for detailed changelog and version information.

---

## ⚖️ License

This plugin is provided as-is for Rust servers using Oxide/uMod under the MIT License.

---

## 💬 Support & Contributing

- **Issues?** Report them on [GitHub Issues](https://github.com/FtuoilXelrash/rFireworksShow/issues)
- **Suggestions?** Open a GitHub Discussion or Pull Request
- **Found a bug?** Please provide console output and reproduction steps

---

**Made with ❤️ for the Rust community**
