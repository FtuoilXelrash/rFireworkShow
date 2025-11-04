# rFireworksShow

A Rust server plugin that spawns randomized firework effects at configurable intervals and locations.

## Features

- **Two Show Modes**:
  - **Time-Based Shows** - Only spawn between specified hours with dice roll chance (e.g., 7:50 PM to 7:50 AM)
  - **Automatic Shows** - Traditional scheduled shows with configurable intervals
- **Random Colors**: Each firework randomly selects from 5 different colors (Champagne, Green, Blue, Violet, Red)
- **Loot Drops**: Optional drops of 10x Gunpowder items from each firework explosion (falls naturally to ground)
- **Smart Positioning**: Shows can be centered near players or at random map locations
- **Multiple Command Modes**: Manual trigger with `/fs`, test behavior with `/fsrand`, toggle modes with `/fstoggle`
- **Flexible Spawning**: Spawn at your location, specific coordinates, or fully random
- **Customizable Effects**: Configure fireworks count, spread radius, height, and timing
- **Map Markers**: Green circular markers appear on maps during shows for player visibility
- **Server Console Support**: Reload configuration without restarting
- **Error Handling**: Robust exception handling and logging

## Installation

1. Place `rFireworkShow.cs` in your Rust server's `oxide/plugins/` directory
2. Reload the plugin or restart the server
3. Configure settings in the generated config file

## Configuration

The plugin creates a configuration file at `oxide/config/rFireworksShow.json` with the following options:

```json
{
  "OnlyWhenPlayersOnline": true,
  "EnableMapMarkers": true,
  "EnableStaggeredFireMode": true,
  "EnableLootDrops": true,
  "LootDropChance": 50.0,
  "LootDropItems": {
    "gunpowder": { "min": 5, "max": 15 },
    "cloth": { "min": 10, "max": 20 },
    "charcoal": { "min": 8, "max": 18 },
    "metal.fragments": { "min": 15, "max": 30 }
  },
  "SpawnAtRandomPlayersMapLocation": false,
  "OnlySpawnOnLand": true,
  "OnlySpawnAtMonuments": false,
  "SpreadRadius": 30.0,
  "HeightOffset": 30.0,
  "PlayerSelectionRadius": 500.0,
  "AutomaticShowsEnabled": false,
  "AutomaticShowsIntervalMinSeconds": 3600.0,
  "AutomaticShowsIntervalMaxSeconds": 10800.0,
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
  "TimeBasedShowsDiceRollChancePercent": 50
}
```

### Configuration Options

**Display & Effect Settings:**

| Option | Default | Description |
|--------|---------|-------------|
| `OnlyWhenPlayersOnline` | true | Only run automatic shows when at least one player is online |
| `EnableMapMarkers` | true | Display green map markers at firework show locations |
| `EnableStaggeredFireMode` | true | If true, fireworks fire with cumulative staggered delays; if false, use independent random delays |
| `EnableLootDrops` | true | If true, drop loot items when each firework explodes (falls from sky to ground) |
| `LootDropChance` | 50.0 | Percentage chance (0-100) for loot to drop from each firework |
| `LootDropItems` | See JSON | Dictionary of item names with min/max drop quantities (e.g., gunpowder: 5-15) |
| `SpawnAtRandomPlayersMapLocation` | false | If true, spawn around random players; if false, spawn at random map location |
| `OnlySpawnOnLand` | true | If true, never spawn in water (uses WaterLevel check); if false, can spawn in water |
| `OnlySpawnAtMonuments` | false | If true, only spawn at map monuments; if false, spawn at random locations |
| `SpreadRadius` | 30.0 | Radius around center point for firework spread |
| `HeightOffset` | 30.0 | Height above ground to spawn fireworks |
| `PlayerSelectionRadius` | 500.0 | Radius around player to center show |

**Automatic Show Settings (independent scheduler):**

| Option | Default | Description |
|--------|---------|-------------|
| `AutomaticShowsEnabled` | false | Enable traditional automatic scheduled shows |
| `AutomaticShowsIntervalMinSeconds` | 3600.0 | Minimum seconds between automatic shows (1 hour) |
| `AutomaticShowsIntervalMaxSeconds` | 10800.0 | Maximum seconds between automatic shows (3 hours) |
| `AutomaticShowsFireworksMin` | 1 | Minimum fireworks spawned per automatic show |
| `AutomaticShowsFireworksMax` | 6 | Maximum fireworks spawned per automatic show |
| `AutomaticShowsDiceRollChancePercent` | 50 | Percentage chance (0-100) for automatic show to spawn each attempt |

**Time-Based Show Settings (independent scheduler):**

| Option | Default | Description |
|--------|---------|-------------|
| `TimeBasedShowsEnabled` | true | Enable time-based shows (only spawn between specified hours) |
| `TimeBasedShowsFireworksMin` | 3 | Minimum fireworks spawned per time-based show |
| `TimeBasedShowsFireworksMax` | 60 | Maximum fireworks spawned per time-based show |
| `TimeBasedStartHour` | 19.50 | Start time for shows (19:50 = 7:50 PM) - format: HH.MM (decimal) |
| `TimeBasedShowEndHour` | 7.50 | End time for shows (07:50 = 7:50 AM) - format: HH.MM (decimal) |
| `TimedShowIntervalMinSeconds` | 15.0 | Minimum seconds between timed show attempts |
| `TimedShowIntervalMaxSeconds` | 30.0 | Maximum seconds between timed show attempts |
| `TimeBasedShowsDiceRollChancePercent` | 50 | Percentage chance (0-100) for time-based show to spawn each roll |

### Available Firework Colors

The plugin randomly selects from these prefabs for each firework:
- `assets/prefabs/deployable/fireworks/mortarchampagne.prefab` (Champagne)
- `assets/prefabs/deployable/fireworks/mortargreen.prefab` (Green)
- `assets/prefabs/deployable/fireworks/mortarblue.prefab` (Blue)
- `assets/prefabs/deployable/fireworks/mortarviolet.prefab` (Violet)
- `assets/prefabs/deployable/fireworks/mortarred.prefab` (Red)

## Commands

### Chat Commands

**`/fs [count]`** - Manually trigger a fireworks show
- Requires admin
- Optional: specify number of fireworks (default: config value)
- Shows spawn in front of the player
- Example: `/fs 10` - Spawns 10 fireworks in front of you

**`/fs x y z [count]`** - Trigger show at specific coordinates
- Example: `/fs 1000 100 2000` - Spawns at coordinates (1000, 100, 2000)

**`/fsrand`** - Test automatic show behavior (random center point)
- Requires admin
- Uses the same logic as scheduled automatic shows
- Picks random player proximity or random map location

**`/fsrand local`** - Trigger show at your current location
- Requires admin
- Spawns at your feet (useful for testing)

**`/fsrand x y z`** - Trigger show at specific coordinates
- Requires admin
- Example: `/fsrand 1000 100 2000` - Spawns at those coordinates

**`/fstoggle`** - Toggle automatic shows on/off
- Requires admin
- Toggles `AutomaticShowsEnabled` setting
- Saves configuration and restarts scheduler
- Messages: "automatic shows are now ENABLED" or "DISABLED"

### Console Commands

**`rf.reload`** - Reload configuration
- Can be run from server console or by admins in-game
- Restarts the show scheduler with new settings

## Permissions

**Admin-only** - All commands and features require server admin status.

## How It Works

Both schedulers run independently and can be enabled/disabled separately.

### Time-Based Shows
When `TimeBasedShowsEnabled` is true:
1. Scheduler continuously checks if current time is between `TimeBasedStartHour` and `TimeBasedShowEndHour`
2. When outside time window: scheduler waits and checks again
3. When inside time window: rolls dice with `DiceRollChancePercent` chance
4. If dice wins: spawns a show at random location or around random players (based on `SpawnAtRandomPlayersMapLocation`)
5. Repeats every `TimedShowMinSeconds` to `TimedShowMaxSeconds` (randomized)

**Example**: With default settings (19.50-7.50, 50% chance, 2 min intervals):
- Between 7:50 PM and 7:50 AM: shows attempt every 2 mins with 50% success rate
- Between 7:50 AM and 7:50 PM: no time-based shows

### Automatic Shows
When `AutomaticShowsEnabled` is true:
- Traditional scheduled shows run on fixed intervals (`AutomaticShowsIntervalMinSeconds` to `AutomaticShowsIntervalMaxSeconds`)
- Respects `OnlyWhenPlayersOnline` setting
- Spawns at random map location or around random players (based on `SpawnAtRandomPlayersMapLocation`)

## Console Output

The plugin logs shows to server console with the format:

```
Local Show[21.82]: Grid(R13) - Location(782.99, 51.10, 168.09) with 6 fireworks.
Random Show: Grid(H5) - Monument(Airfield) - Grid(M8) with 15 fireworks.
AutomaticShow: Grid(A1) - Location(50.00, 80.00, 120.00) with 3 fireworks.
TimeBasedShow[19.50]: Monument(Harbor) - Grid(M8) with 12 fireworks.
```

When `OnlySpawnAtMonuments` is enabled, shows display monument names instead of coordinates. When disabled, location coordinates are shown.

Grid references use the standard Rust map grid system (A-Z columns, 0-25+ rows). Grid coordinates are calculated using Rust's built-in `MapHelper` for accuracy.

Console times displayed in brackets (e.g., `[19.50]`) use server game time, matching the in-game time shown to players. This ensures TimeBasedShow events trigger at the correct configured hours.

## Notes

- Each firework randomly selects from 5 available colors for visual variety
- Loot drops controlled by `EnableLootDrops`:
  - When enabled (default): Each firework may drop random loot items based on configuration
  - When disabled: No loot items spawn
  - `LootDropChance` controls probability (100% = always drop, 50% = half the time, 0% = never)
  - `LootDropItems` dictionary defines available items and their min/max quantities
  - Each drop randomly picks an item from the list, then rolls quantity between min and max
  - Items spawn 200 feet above the firework location and fall naturally to ground (matches explosion height)
  - Applies to all show types
  - Default items: Gunpowder (5-15), Cloth (10-20), Charcoal (8-18), Metal Fragments (15-30)
- Firework timing controlled by `EnableStaggeredFireMode`:
  - When enabled (default): Cumulative staggered delays (0.1-1.5 seconds between each) for natural, chaotic appearance
  - When disabled: Independent random delays (0-2 seconds per firework) for unpredictable pattern
- Effects respect terrain height to avoid spawning underground
- The `/fs` command is for manual positioning; `/fsrand` is for testing show behavior
- `/fsrand` and `/fsrand local` commands spawn variable counts (3-60 by default) from TimeBasedShowsFireworksMin/Max
- `/fstoggle` controls the `AutomaticShowsEnabled` setting
- `SpawnAtRandomPlayersMapLocation` (false = random map, true = random player proximity) applies to both schedulers
- All commands require admin status (IsAdmin check)
- Time format uses decimal HH.MM (e.g., 19.50 = 19:50, 7.50 = 07:50)
- Both automatic and time-based schedulers can run simultaneously
- Automatic shows default to 1-6 fireworks per show (configurable min/max)
- Console messages include grid reference for easy player location identification
- Grid calculations and time displays use server APIs for accuracy (MapHelper and TOD_Sky)
- Green map markers appear on player maps for all show types (configurable via `EnableMapMarkers`)
- Markers automatically persist for the duration of each show and then disappear
- Time-based show intervals use consistent naming: `TimedShowIntervalMinSeconds` and `TimedShowIntervalMaxSeconds`
- Spawn location checking prevents fireworks from spawning in water when `OnlySpawnOnLand` is enabled (uses `WaterLevel.Test()` from Tornado.cs)
- Monument spawning uses `TerrainMeta.Path.Monuments` for dynamic caching - works on vanilla AND custom maps (no dependency on Monument Finder plugin)
- When `OnlySpawnAtMonuments` is enabled, the plugin filters to only 29 known safe, above-ground monuments (airfield, harbor, radtown, etc.) to prevent spawning in caves or underground locations

## Version History

See [CHANGES.md](CHANGES.md) for detailed version history.

## License

This plugin is provided as-is for Rust servers using Oxide/uMod.

## Support

For issues or suggestions, please report them in the plugin repository.
