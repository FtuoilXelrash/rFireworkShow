// File: rFireworkShow.cs
// Place in: oxide/plugins/rFireworkShow.cs
// Note: Change the plugin version string below if you manage versions manually.

using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("rFireworkShow", "Ftuoil Xelrash", "0.1.30")] // <-- change version string as you like
    [Description("Spawns randomized firework effects at randomized locations (near players or anywhere). Configurable and admin-triggerable.")]
    public class rFireworkShow : RustPlugin
    {
        #region Config

        private PluginConfig config;

        private class LootItemConfig
        {
            public int min { get; set; } = 5;
            public int max { get; set; } = 15;
        }

        private class PluginConfig
        {
            // Display & Effect Settings
            public bool OnlyWhenPlayersOnline { get; set; } = true; // if true, only run automatic shows when at least one player is online
            public bool EnableMapMarkers { get; set; } = true; // if true, display green map markers at firework show locations
            public bool EnableStaggeredFireMode { get; set; } = true; // if true, fireworks fire with cumulative staggered delays for natural rhythm; if false, independent random delays
            public bool EnableLootDrops { get; set; } = true; // if true, drop loot items when fireworks explode

            // Loot Drop Settings
            public double LootDropChance { get; set; } = 50.0; // percentage chance (0-100) for loot to drop from each firework
            public Dictionary<string, LootItemConfig> LootDropItems { get; set; } = new Dictionary<string, LootItemConfig>
            {
                { "gunpowder", new LootItemConfig { min = 3, max = 5 } },
                { "cloth", new LootItemConfig { min = 3, max = 5 } },
                { "charcoal", new LootItemConfig { min = 5, max = 10 } },
                { "metal.fragments", new LootItemConfig { min = 3, max = 5 } }
            };

            public bool SpawnAtRandomPlayersMapLocation { get; set; } = false; // if true, spawn around random players; if false, spawn at random map location
            public bool OnlySpawnOnLand { get; set; } = true; // if true, never spawn in water; if false, can spawn anywhere
            public bool OnlySpawnAtMonuments { get; set; } = false; // if true, only spawn at map monuments; if false, spawn at random locations
            public float SpreadRadius { get; set; } = 30f;         // radius around the center point to randomize individual fireworks
            public float HeightOffset { get; set; } = 30f;         // height above ground to spawn fireworks
            public float PlayerSelectionRadius { get; set; } = 500f; // pick a player to center show near; if no players, random map location

            // Automatic Show Settings
            public bool AutomaticShowsEnabled { get; set; } = false; // if true, automatic scheduled shows are enabled
            public double AutomaticShowsIntervalMinSeconds { get; set; } = 3600.0; // minimum seconds between automatic shows (1 hour)
            public double AutomaticShowsIntervalMaxSeconds { get; set; } = 7200.0; // maximum seconds between automatic shows (2 hours)
            public int AutomaticShowsFireworksMin { get; set; } = 1;   // minimum number of fireworks spawned per automatic show
            public int AutomaticShowsFireworksMax { get; set; } = 6;  // maximum number of fireworks spawned per automatic show
            public int AutomaticShowsDiceRollChancePercent { get; set; } = 50; // percentage chance (0-100) for automatic show to spawn

            // Time-based Show Settings
            public bool TimeBasedShowsEnabled { get; set; } = true; // if true, shows only happen during specified hours
            public int TimeBasedShowsFireworksMin { get; set; } = 3;   // minimum number of fireworks spawned per time-based show
            public int TimeBasedShowsFireworksMax { get; set; } = 60;  // maximum number of fireworks spawned per time-based show
            public double TimeBasedStartHour { get; set; } = 19.50; // 19:50 (7:50 PM) - shows start at this time (decimal format)
            public double TimeBasedShowEndHour { get; set; } = 7.50;    // 07:50 (7:50 AM) - shows stop at this time (decimal format)
            public double TimedShowIntervalMinSeconds { get; set; } = 15.0; // minimum seconds between timed show attempts
            public double TimedShowIntervalMaxSeconds { get; set; } = 30.0; // maximum seconds between timed show attempts
            public int TimeBasedShowsDiceRollChancePercent { get; set; } = 50; // percentage chance (0-100) for time-based show to spawn each roll
        }

        protected override void LoadDefaultConfig()
        {
            config = new PluginConfig();
            SaveConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<PluginConfig>();
                if (config == null) throw new Exception("config null");
            }
            catch
            {
                PrintWarning("Configuration was corrupt or missing - creating default configuration.");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig() => Config.WriteObject(config, true);

        #endregion

        #region Localization

        private void Init()
        {
            LoadConfig(); // ensure config is loaded
        }

        #endregion

        #region Plugin lifecycle

        private Timer scheduleTimer;

        private List<string> fireworkPrefabs = new List<string>()
        {
            "assets/prefabs/deployable/fireworks/mortarchampagne.prefab",
            "assets/prefabs/deployable/fireworks/mortargreen.prefab",
            "assets/prefabs/deployable/fireworks/mortarblue.prefab",
            "assets/prefabs/deployable/fireworks/mortarviolet.prefab",
            "assets/prefabs/deployable/fireworks/mortarred.prefab",
        };

        private List<MonumentInfo> cachedMonuments = new List<MonumentInfo>();

        private void OnServerInitialized()
        {
            // Cache monuments from the map
            CacheMonuments();
            // Start the repeating scheduler
            StartSchedule();
            Puts("rFireworkShow: initialized.");
        }

        private void Unload()
        {
            scheduleTimer?.Destroy();
        }

        #endregion

        #region Scheduling & Spawning

        private void StartSchedule()
        {
            scheduleTimer?.Destroy();

            // Both schedulers can run independently
            bool startedAny = false;

            // Start automatic scheduler if enabled
            if (config.AutomaticShowsEnabled)
            {
                if (config.AutomaticShowsIntervalMinSeconds <= 0 || config.AutomaticShowsIntervalMaxSeconds <= 0)
                {
                    PrintWarning("rFireworkShow: invalid automatic intervals in config. Automatic scheduler not started.");
                }
                else
                {
                    Puts("rFireworkShow: starting automatic scheduler.");
                    ScheduleNextShow();
                    startedAny = true;
                }
            }

            // Start time-based scheduler if enabled
            if (config.TimeBasedShowsEnabled)
            {
                if (config.TimedShowIntervalMinSeconds <= 0 || config.TimedShowIntervalMaxSeconds <= 0)
                {
                    PrintWarning("rFireworkShow: invalid timed intervals in config. Time-based scheduler not started.");
                }
                else
                {
                    Puts("rFireworkShow: starting time-based scheduler.");
                    ScheduleTimedShow();
                    startedAny = true;
                }
            }

            if (!startedAny)
            {
                Puts("rFireworkShow: no schedulers enabled.");
            }
        }

        private void ScheduleNextShow()
        {
            float nextInterval = UnityEngine.Random.Range((float)config.AutomaticShowsIntervalMinSeconds, (float)config.AutomaticShowsIntervalMaxSeconds);
            // Use timer.Once to schedule the show
            timer.Once(nextInterval, () =>
            {
                try
                {
                    // Check if we should skip due to no players
                    if (config.OnlyWhenPlayersOnline)
                    {
                        if (BasePlayer.activePlayerList == null || BasePlayer.activePlayerList.Count == 0)
                        {
                            // no players and we're configured to skip
                            Puts("rFireworkShow: no players online, skipping automatic show.");
                        }
                        else
                        {
                            // Roll dice for automatic show spawn
                            if (RollDiceWithChance(config.AutomaticShowsDiceRollChancePercent))
                            {
                                SpawnRandomShowWithRange(config.AutomaticShowsFireworksMin, config.AutomaticShowsFireworksMax, "AutomaticShow");
                            }
                        }
                    }
                    else
                    {
                        // Roll dice for automatic show spawn
                        if (RollDiceWithChance(config.AutomaticShowsDiceRollChancePercent))
                        {
                            SpawnRandomShowWithRange(config.AutomaticShowsFireworksMin, config.AutomaticShowsFireworksMax, "AutomaticShow");
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintError($"rFireworkShow: exception during automatic show: {ex}");
                }
                finally
                {
                    // Always schedule next
                    ScheduleNextShow();
                }
            });
        }

        private void ScheduleTimedShow()
        {
            // Check if we're currently in the time window
            if (!IsCurrentTimeInWindow())
            {
                // Not in time window, schedule a check for later
                float checkLater = UnityEngine.Random.Range((float)config.TimedShowIntervalMinSeconds, (float)config.TimedShowIntervalMaxSeconds);
                timer.Once(checkLater, () =>
                {
                    ScheduleTimedShow();
                });
                return;
            }

            // We're in the time window, do dice roll
            if (RollDice())
            {
                // Dice won! Spawn a show with time-based fireworks range
                SpawnRandomShowWithRange(config.TimeBasedShowsFireworksMin, config.TimeBasedShowsFireworksMax, "TimeBasedShow");
            }

            // Schedule next attempt
            float nextWait = UnityEngine.Random.Range((float)config.TimedShowIntervalMinSeconds, (float)config.TimedShowIntervalMaxSeconds);
            timer.Once(nextWait, () =>
            {
                ScheduleTimedShow();
            });
        }

        private bool IsCurrentTimeInWindow()
        {
            // Get current server game time as decimal (e.g., 19.50 for 19:50, 7.50 for 07:50)
            float timeOfDay = TOD_Sky.Instance.Cycle.Hour;
            int currentHour = Mathf.FloorToInt(timeOfDay);
            int currentMinute = Mathf.FloorToInt((timeOfDay - currentHour) * 60);
            double currentTime = currentHour + (currentMinute / 100.0);

            double startTime = config.TimeBasedStartHour;  // e.g., 19.50
            double endTime = config.TimeBasedShowEndHour;   // e.g., 7.50

            // Handle overnight range (e.g., 19:50 to 07:50)
            if (startTime > endTime)
            {
                // Range wraps around midnight
                return currentTime >= startTime || currentTime <= endTime;
            }
            else
            {
                // Normal range (e.g., 07:00 to 19:00)
                return currentTime >= startTime && currentTime <= endTime;
            }
        }

        private bool RollDiceWithChance(int chancePercent)
        {
            // Roll dice with given percentage chance
            int roll = UnityEngine.Random.Range(0, 100);
            return roll < chancePercent;
        }

        private bool RollDice()
        {
            // Roll dice for time-based shows
            return RollDiceWithChance(config.TimeBasedShowsDiceRollChancePercent);
        }

        private void SpawnRandomShow(int count = 0)
        {
            // If count not specified, use default (8 fireworks)
            if (count <= 0) count = 8;

            Vector3 center = GetRandomCenter(out string monumentName);
            RunFireworkShow(center, count, "Show", monumentName);
        }

        private void SpawnRandomShowWithRange(int minCount, int maxCount, string showType = "Show")
        {
            // Randomly select count between min and max
            int count = UnityEngine.Random.Range(minCount, maxCount + 1);
            Vector3 center = GetRandomCenter(out string monumentName);
            RunFireworkShow(center, count, showType, monumentName);
        }

        private Vector3 GetRandomCenter(out string monumentName)
        {
            monumentName = null;

            // Check if we should spawn around players or at random map location
            if (!config.SpawnAtRandomPlayersMapLocation)
            {
                // Spawn at random map location
                return GetRandomMapPosition(out monumentName);
            }

            // Spawn centered around a random player if available
            try
            {
                var players = BasePlayer.activePlayerList;
                if (players != null && players.Count > 0)
                {
                    // choose a random player that is not sleeping/short-distance? We'll pick any active player
                    var player = players[UnityEngine.Random.Range(0, players.Count)];
                    if (player != null && Vector3Ex.IsNaNOrInfinity(player.transform.position) == false)
                    {
                        Vector3 chosen = player.transform.position;
                        // optionally random offset within PlayerSelectionRadius
                        float angle = UnityEngine.Random.Range(0f, 360f) * (Mathf.PI / 180f);
                        float r = UnityEngine.Random.Range(0f, config.PlayerSelectionRadius);
                        Vector3 offset = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
                        chosen += offset;

                        // make sure fireworks are above ground by HeightOffset
                        chosen.y = TerrainMeta.HeightMap.GetHeight(chosen) + config.HeightOffset;
                        return chosen;
                    }
                }

                // Fallback: random map location (no valid players found)
                return GetRandomMapPosition(out monumentName);
            }
            catch (Exception ex)
            {
                PrintError($"rFireworkShow: GetRandomCenter exception: {ex}");
                return GetRandomMapPosition(out monumentName);
            }
        }

        private Vector3 GetRandomMapPosition(out string monumentName)
        {
            monumentName = null;

            // If OnlySpawnAtMonuments is enabled, try to spawn at a monument
            if (config.OnlySpawnAtMonuments)
            {
                MonumentInfo monument = GetRandomMonument();
                if (monument != null)
                {
                    Vector3 monumentPos = monument.transform.position;
                    monumentPos.y += config.HeightOffset;

                    // Still respect OnlySpawnOnLand check
                    if (config.OnlySpawnOnLand)
                    {
                        Vector3 probePoint = new Vector3(monumentPos.x, monumentPos.y + 0.25f, monumentPos.z);
                        bool isWater = WaterLevel.Test(probePoint, true, true, null);
                        if (isWater)
                        {
                            // Monument is in water, skip and fall through to random
                            monument = null;
                        }
                    }

                    if (monument != null)
                    {
                        monumentName = GetMonumentName(monument);
                        return monumentPos;
                    }
                }

                // If no valid monument found and OnlySpawnAtMonuments is true, fallback to land
                if (config.OnlySpawnOnLand)
                {
                    if (TryPickRandomLandPoint(out Vector3 landPoint))
                    {
                        landPoint.y += config.HeightOffset;
                        return landPoint;
                    }
                }
            }

            // If OnlySpawnAtMonuments is disabled, or as fallback
            // If OnlySpawnOnLand is enabled, try to find a valid land location
            if (config.OnlySpawnOnLand)
            {
                if (TryPickRandomLandPoint(out Vector3 landPoint))
                {
                    landPoint.y += config.HeightOffset;
                    return landPoint;
                }
            }

            // Fallback or if OnlySpawnOnLand is disabled: choose random point anywhere
            var size = TerrainMeta.Size.x; // map is square; Size.x = Size.z
            float x = UnityEngine.Random.Range(0f, size) - (size / 2f);
            float z = UnityEngine.Random.Range(0f, size) - (size / 2f);
            Vector3 pos = new Vector3(x, 0f, z);
            pos.y = TerrainMeta.HeightMap.GetHeight(pos) + config.HeightOffset;
            return pos;
        }

        private bool TryPickRandomLandPoint(out Vector3 position, int maxAttempts = 40)
        {
            float mapHalfExtent = TerrainMeta.Size.x * 0.48f;

            for (int attemptIndex = 0; attemptIndex < maxAttempts; attemptIndex++)
            {
                float xCoordinate = UnityEngine.Random.Range(-mapHalfExtent, mapHalfExtent);
                float zCoordinate = UnityEngine.Random.Range(-mapHalfExtent, mapHalfExtent);

                float terrainHeight = TerrainMeta.HeightMap.GetHeight(new Vector3(xCoordinate, 0f, zCoordinate));

                Vector3 probePoint = new Vector3(xCoordinate, terrainHeight + 0.25f, zCoordinate);
                bool isWater = WaterLevel.Test(probePoint, true, true, null);
                if (!isWater)
                {
                    position = new Vector3(xCoordinate, terrainHeight, zCoordinate);
                    return true;
                }
            }

            float centerHeight = TerrainMeta.HeightMap.GetHeight(Vector3.zero);
            position = new Vector3(0f, centerHeight, 0f);
            return true;
        }

        private float GetSurfaceHeight(Vector3 position)
        {
            return WaterLevel.GetWaterOrTerrainSurface(position, true, true, null) + 0.25f;
        }

        private void CacheMonuments()
        {
            cachedMonuments.Clear();

            if (TerrainMeta.Path == null || TerrainMeta.Path.Monuments == null)
            {
                PrintWarning("rFireworkShow: no monuments found on this map.");
                return;
            }

            // Whitelist of safe, above-ground monument names
            HashSet<string> allowedMonuments = new HashSet<string>
            {
                "airfield_1",
                "arctic_research_base_a",
                "bandit_town",
                "compound",
                "desert_military_base_a",
                "desert_military_base_b",
                "desert_military_base_c",
                "desert_military_base_d",
                "excavator_1",
                "ferry_terminal_1",
                "fishing_village_a",
                "fishing_village_b",
                "fishing_village_c",
                "gas_station_1",
                "harbor_1",
                "harbor_2",
                "junkyard_1",
                "launch_site_1",
                "lighthouse",
                "oilrig_1",
                "oilrig_2",
                "powerplant_1",
                "radtown_small_3",
                "satellite_dish",
                "stables_a",
                "stables_b",
                "supermarket_1",
                "warehouse",
                "water_treatment_plant_1"
            };

            foreach (var monument in TerrainMeta.Path.Monuments)
            {
                if (monument != null && monument.transform.position.y > 0)
                {
                    // Extract short name from prefab path (e.g., "airfield_1" from "assets/bundled/prefabs/autospawn/monument/large/airfield_1.prefab")
                    string shortName = monument.name;
                    if (!string.IsNullOrEmpty(shortName))
                    {
                        // Get filename without extension
                        if (shortName.Contains("/"))
                        {
                            shortName = shortName.Substring(shortName.LastIndexOf("/") + 1);
                        }
                        if (shortName.EndsWith(".prefab"))
                        {
                            shortName = shortName.Substring(0, shortName.Length - 7);
                        }
                    }

                    // Check if monument name is in whitelist
                    if (!string.IsNullOrEmpty(shortName) && allowedMonuments.Contains(shortName))
                    {
                        cachedMonuments.Add(monument);
                    }
                }
            }

            Puts($"rFireworkShow: cached {cachedMonuments.Count} monuments.");
        }

        private MonumentInfo GetRandomMonument()
        {
            if (cachedMonuments == null || cachedMonuments.Count == 0)
            {
                return null;
            }

            return cachedMonuments[UnityEngine.Random.Range(0, cachedMonuments.Count)];
        }

        private string GetMonumentName(MonumentInfo monument)
        {
            if (monument == null)
                return "Unknown";

            // Try displayPhrase first
            if (monument.displayPhrase != null && !string.IsNullOrEmpty(monument.displayPhrase.english))
            {
                return monument.displayPhrase.english;
            }

            // Fallback to name
            if (!string.IsNullOrEmpty(monument.name))
            {
                return monument.name;
            }

            // No more fallbacks available
            return "Unknown";
        }

        private string GetGridReference(Vector3 position)
        {
            // Use Rust's built-in MapHelper for correct grid calculation
            return MapHelper.GridToString(MapHelper.PositionToGrid(position));
        }

        private void RunFireworkShow(Vector3 center, int count, string showType = "Show", string monumentName = null)
        {
            if (count <= 0) return;

            string gridRef = GetGridReference(center);
            string timeInfo = "";
            if (showType == "TimeBasedShow")
            {
                // Use server game time, not local machine time
                float timeOfDay = TOD_Sky.Instance.Cycle.Hour;
                int hours = Mathf.FloorToInt(timeOfDay);
                int minutes = Mathf.FloorToInt((timeOfDay - hours) * 60);
                double time = hours + (minutes / 100.0);
                timeInfo = $"[{time:F2}]";
            }

            // Build console message with optional monument name
            string locationInfo = !string.IsNullOrEmpty(monumentName)
                ? $"Monument({monumentName})"
                : $"Location{center}";

            Puts($"{showType}{timeInfo}: Grid({gridRef}) - {locationInfo} with {count} fireworks.");

            // Create map marker if enabled
            MapMarkerGenericRadius marker = null;
            if (config.EnableMapMarkers)
            {
                try
                {
                    marker = GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", center) as MapMarkerGenericRadius;
                    if (marker != null)
                    {
                        marker.enableSaving = false;
                        marker.Spawn();

                        // Green color: 25D976 with 70% transparency (0.3 alpha = 30% opacity)
                        marker.color1 = new Color(0x25 / 255f, 0xD9 / 255f, 0x76 / 255f, 1f);
                        marker.alpha = 0.3f;
                        marker.radius = 0.5f;  // Similar to F1 tornado marker size
                        marker.SendUpdate();
                        marker.SendNetworkUpdate();
                    }
                }
                catch (Exception ex)
                {
                    PrintError($"rFireworkShow: exception creating map marker: {ex}");
                }
            }

            // Spawn fireworks with optional staggered delays
            if (config.EnableStaggeredFireMode)
            {
                // Cumulative staggered delays for natural variation
                float cumulativeDelay = 0f;
                for (int i = 0; i < count; i++)
                {
                    // Random delay between this and last firework (creates natural groupings and gaps)
                    float delayBetween = UnityEngine.Random.Range(0.1f, 1.5f);
                    cumulativeDelay += delayBetween;

                    int index = i;
                    bool isLastFirework = (i == count - 1);
                    float firingTime = cumulativeDelay;

                    timer.Once(firingTime, () =>
                    {
                        try
                        {
                            Vector3 pos = GetRandomPointInCircle(center, config.SpreadRadius);
                            pos.y = TerrainMeta.HeightMap.GetHeight(pos) + config.HeightOffset;

                            // Pick a random firework color
                            string prefab = fireworkPrefabs[UnityEngine.Random.Range(0, fireworkPrefabs.Count)];

                            // Create the firework entity
                            RepeatingFirework firework = GameManager.server.CreateEntity(prefab, pos) as RepeatingFirework;

                            if (firework != null)
                            {
                                firework.enableSaving = false;
                                firework.Spawn();
                                // Fire the firework
                                firework.ClientRPC(null, "RPCFire");
                                // Clean up after firing
                                firework.Kill();

                                // Schedule loot drop when firework explodes (~2.5 seconds after firing)
                                timer.Once(2.5f, () =>
                                {
                                    if (config.EnableLootDrops && config.LootDropItems.Count > 0)
                                    {
                                        // Roll dice for loot drop chance
                                        if (RollDiceWithChance((int)config.LootDropChance))
                                        {
                                            try
                                            {
                                                // Pick random item from loot items list
                                                var lootItemsList = config.LootDropItems.Keys.ToList();
                                                string itemName = lootItemsList[UnityEngine.Random.Range(0, lootItemsList.Count)];
                                                LootItemConfig lootConfig = config.LootDropItems[itemName];

                                                // Roll for quantity between min and max
                                                int quantity = UnityEngine.Random.Range(lootConfig.min, lootConfig.max + 1);

                                                // Drop loot from 200 feet above the spawn position
                                                Vector3 lootDropPos = pos + Vector3.up * 200f;
                                                Item lootItem = ItemManager.Create(ItemManager.FindItemDefinition(itemName), quantity);
                                                if (lootItem != null)
                                                {
                                                    lootItem.Drop(lootDropPos, Vector3.zero);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                PrintError($"rFireworkShow: exception dropping loot: {ex}");
                                            }
                                        }
                                    }
                                });

                                // If this is the last firework, schedule marker removal after it fires
                                if (isLastFirework && marker != null && config.EnableMapMarkers)
                                {
                                    timer.Once(15f, () =>
                                    {
                                        if (marker != null && !marker.IsDestroyed)
                                        {
                                            marker.Kill();
                                        }
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            PrintError($"rFireworkShow: exception spawning individual firework #{index}: {ex}");
                        }
                    });
                }
            }
            else
            {
                // Independent random delays (old behavior)
                for (int i = 0; i < count; i++)
                {
                    float delay = UnityEngine.Random.Range(0f, 2f);
                    int index = i;
                    bool isLastFirework = (i == count - 1);

                    timer.Once(delay, () =>
                    {
                        try
                        {
                            Vector3 pos = GetRandomPointInCircle(center, config.SpreadRadius);
                            pos.y = TerrainMeta.HeightMap.GetHeight(pos) + config.HeightOffset;

                            // Pick a random firework color
                            string prefab = fireworkPrefabs[UnityEngine.Random.Range(0, fireworkPrefabs.Count)];

                            // Create the firework entity
                            RepeatingFirework firework = GameManager.server.CreateEntity(prefab, pos) as RepeatingFirework;

                            if (firework != null)
                            {
                                firework.enableSaving = false;
                                firework.Spawn();
                                // Fire the firework
                                firework.ClientRPC(null, "RPCFire");
                                // Clean up after firing
                                firework.Kill();

                                // Schedule loot drop when firework explodes (~2.5 seconds after firing)
                                timer.Once(2.5f, () =>
                                {
                                    if (config.EnableLootDrops && config.LootDropItems.Count > 0)
                                    {
                                        // Roll dice for loot drop chance
                                        if (RollDiceWithChance((int)config.LootDropChance))
                                        {
                                            try
                                            {
                                                // Pick random item from loot items list
                                                var lootItemsList = config.LootDropItems.Keys.ToList();
                                                string itemName = lootItemsList[UnityEngine.Random.Range(0, lootItemsList.Count)];
                                                LootItemConfig lootConfig = config.LootDropItems[itemName];

                                                // Roll for quantity between min and max
                                                int quantity = UnityEngine.Random.Range(lootConfig.min, lootConfig.max + 1);

                                                // Drop loot from 200 feet above the spawn position
                                                Vector3 lootDropPos = pos + Vector3.up * 200f;
                                                Item lootItem = ItemManager.Create(ItemManager.FindItemDefinition(itemName), quantity);
                                                if (lootItem != null)
                                                {
                                                    lootItem.Drop(lootDropPos, Vector3.zero);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                PrintError($"rFireworkShow: exception dropping loot: {ex}");
                                            }
                                        }
                                    }
                                });

                                // If this is the last firework, schedule marker removal after it fires
                                if (isLastFirework && marker != null && config.EnableMapMarkers)
                                {
                                    timer.Once(15f, () =>
                                    {
                                        if (marker != null && !marker.IsDestroyed)
                                        {
                                            marker.Kill();
                                        }
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            PrintError($"rFireworkShow: exception spawning individual firework #{index}: {ex}");
                        }
                    });
                }
            }
        }

        private Vector3 GetRandomPointInCircle(Vector3 center, float radius)
        {
            float t = (float)(2 * Math.PI * UnityEngine.Random.value);
            float u = UnityEngine.Random.value + UnityEngine.Random.value;
            float r = (u > 1) ? 2 - u : u;
            float x = r * (float)Math.Cos(t) * radius;
            float z = r * (float)Math.Sin(t) * radius;
            return new Vector3(center.x + x, center.y, center.z + z);
        }

        #endregion

        #region Commands

        [ChatCommand("fs")]
        private void CmdFireworkShow(BasePlayer player, string command, string[] args)
        {
            if (!HasAdminPermission(player))
            {
                SendReply(player, "You do not have permission to use that command.");
                return;
            }

            int count = config.AutomaticShowsFireworksMax;
            if (args.Length >= 1)
            {
                if (!int.TryParse(args[0], out count) || count < 1) count = config.AutomaticShowsFireworksMax;
            }

            Vector3 center;
            if (args.Length >= 3)
            {
                // allow /fs x y z count
                float x, y, z;
                if (float.TryParse(args[0], out x) && float.TryParse(args[1], out y) && float.TryParse(args[2], out z))
                {
                    center = new Vector3(x, y, z);
                }
                else
                {
                    center = player.transform.position + (player.transform.forward * 5f);
                }
            }
            else
            {
                center = player.transform.position + (player.transform.forward * 5f);
            }

            RunFireworkShow(center, count, "Local Show");
            SendReply(player, $"rFireworkShow: launched {count} fireworks at {center}.");
        }

        [ChatCommand("fsrand")]
        private void CmdFireworkShowRandom(BasePlayer player, string command, string[] args)
        {
            if (!HasAdminPermission(player))
            {
                SendReply(player, "You do not have permission to use that command.");
                return;
            }

            Vector3 center;
            string showType = "Random Show";
            string monumentName = null;

            // Check for variations: /fsrand, /fsrand local, /fsrand x y z
            if (args.Length == 0)
            {
                // Auto mode: use the same logic as scheduled shows
                center = GetRandomCenter(out monumentName);
                showType = "Random Show";
                SendReply(player, $"rFireworkShow: launched random show at {center}.");
            }
            else if (args.Length >= 1 && args[0].ToLower() == "local")
            {
                // Local mode: at player's feet
                center = player.transform.position;
                showType = "Local Show";
                SendReply(player, $"rFireworkShow: launched local show at {center}.");
            }
            else if (args.Length >= 3)
            {
                // Coordinate mode: /fsrand x y z
                float x, y, z;
                if (float.TryParse(args[0], out x) && float.TryParse(args[1], out y) && float.TryParse(args[2], out z))
                {
                    center = new Vector3(x, y, z);
                    showType = "Random Show";
                    SendReply(player, $"rFireworkShow: launched show at {center}.");
                }
                else
                {
                    SendReply(player, "Invalid coordinates. Usage: /fsrand x y z");
                    return;
                }
            }
            else
            {
                SendReply(player, "Usage: /fsrand | /fsrand local | /fsrand x y z");
                return;
            }

            int count = UnityEngine.Random.Range(config.TimeBasedShowsFireworksMin, config.TimeBasedShowsFireworksMax + 1);
            RunFireworkShow(center, count, showType, monumentName);
        }

        [ChatCommand("fstoggle")]
        private void CmdToggleAutomaticShows(BasePlayer player, string command, string[] args)
        {
            if (!HasAdminPermission(player))
            {
                SendReply(player, "You do not have permission to use that command.");
                return;
            }

            // Toggle the setting
            config.AutomaticShowsEnabled = !config.AutomaticShowsEnabled;
            SaveConfig();

            // Restart the scheduler to apply the change
            StartSchedule();

            if (config.AutomaticShowsEnabled)
            {
                SendReply(player, "rFireworkShow: automatic shows are now ENABLED.");
            }
            else
            {
                SendReply(player, "rFireworkShow: automatic shows are now DISABLED.");
            }
        }

        private bool HasAdminPermission(BasePlayer player)
        {
            if (player == null) return false;
            return player.IsAdmin;
        }

        #endregion

        #region Config Commands (Optional convenience commands, admin-only)

        [ConsoleCommand("rf.reload")]
        private void cmdReload(ConsoleSystem.Arg arg)
        {
            if (arg == null || arg.Connection == null) // server console allowed
            {
                LoadConfig();
                StartSchedule();
                Puts("rFireworkShow: config reloaded.");
                return;
            }

            BasePlayer player = arg.Player();
            if (!HasAdminPermission(player))
            {
                SendReply(player, "You do not have permission to use that command.");
                return;
            }

            LoadConfig();
            StartSchedule();
            SendReply(player, "rFireworkShow: config reloaded.");
        }

        #endregion
    }
}
