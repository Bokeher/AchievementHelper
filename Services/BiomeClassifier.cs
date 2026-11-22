using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AchievementHelper.Services;

public static class BiomeClassifier
{
    // Manual enemy overrides by Character.m_name (e.g. dungeon-only or special enemies not in SpawnSystem)
    private static readonly Dictionary<string, BiomeTier> EnemyBiomeOverrides =
        new(StringComparer.Ordinal);

    // Manual base material overrides by prefab name (case-insensitive; take precedence over mob-derived drops)
    private static readonly Dictionary<string, BiomeTier> BaseMaterialBiomes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // 1. Meadows
            ["Stone"] = BiomeTier.Meadows,
            ["Wood"] = BiomeTier.Meadows,
            ["Resin"] = BiomeTier.Meadows,
            ["Flint"] = BiomeTier.Meadows,

            // 2. Black Forest
            ["Bronze"] = BiomeTier.BlackForest,
            ["Copper"] = BiomeTier.BlackForest,
            ["Tin"] = BiomeTier.BlackForest,
            ["BoneFragments"] = BiomeTier.BlackForest,
            ["SurtlingCore"] = BiomeTier.BlackForest,
            ["TrollHide"] = BiomeTier.BlackForest,
            ["BjornHide"] = BiomeTier.BlackForest,
            ["BearHide"] = BiomeTier.BlackForest,

            // 3. Swamp
            ["Iron"] = BiomeTier.Swamp,
            ["WitheredBone"] = BiomeTier.Swamp,
            ["ElderBark"] = BiomeTier.Swamp,

            // 4. Mountain
            ["Silver"] = BiomeTier.Mountain,
            ["Obsidian"] = BiomeTier.Mountain,
            ["FreezeGland"] = BiomeTier.Mountain,

            // 5. Plains
            ["BlackMetal"] = BiomeTier.Plains,
            ["Flax"] = BiomeTier.Plains,
            ["Barley"] = BiomeTier.Plains,

            // 6. Mistlands
            ["Eitr"] = BiomeTier.Mistlands,
            ["Sap"] = BiomeTier.Mistlands,

            // 7. Ashlands

            // 8. Deep North
        };

    private static readonly Dictionary<string, BiomeTier> EnemyBiomeByName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BiomeTier> DerivedMaterialBiomes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Recipe> RecipeBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> PrefabNameBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BiomeTier> ItemBiomeCache = new(StringComparer.Ordinal);
    private static readonly HashSet<string> LoggedSummaryKeys = new(StringComparer.OrdinalIgnoreCase);

    private static bool _enemyIndexBuilt;
    private static bool _itemIndexBuilt;

    public static void ResetCache()
    {
        EnemyBiomeByName.Clear();
        DerivedMaterialBiomes.Clear();
        RecipeBySharedName.Clear();
        PrefabNameBySharedName.Clear();
        ItemBiomeCache.Clear();
        LoggedSummaryKeys.Clear();
        _enemyIndexBuilt = false;
        _itemIndexBuilt = false;
    }

    public static BiomeTier GetEnemyBiome(string enemySharedName)
    {
        if (string.IsNullOrEmpty(enemySharedName))
        {
            return BiomeTier.Uncategorized;
        }

        if (EnemyBiomeOverrides.TryGetValue(enemySharedName, out BiomeTier overridden))
        {
            return overridden;
        }

        EnsureEnemyIndexBuilt();
        return EnemyBiomeByName.TryGetValue(enemySharedName, out BiomeTier tier)
            ? tier
            : BiomeTier.Uncategorized;
    }

    public static BiomeTier GetItemBiome(string sharedName)
    {
        if (string.IsNullOrEmpty(sharedName))
        {
            return BiomeTier.Uncategorized;
        }

        EnsureEnemyIndexBuilt();
        EnsureItemIndexBuilt();

        if (ItemBiomeCache.TryGetValue(sharedName, out BiomeTier cached))
        {
            return cached;
        }

        HashSet<string> visited = new(StringComparer.Ordinal);
        HashSet<string> leafPrefabs = new(StringComparer.OrdinalIgnoreCase);
        CollectBaseMaterials(sharedName, visited, leafPrefabs);

        BiomeTier result = EvaluateLeafMaterials(leafPrefabs);
        ItemBiomeCache[sharedName] = result;
        return result;
    }

    public static void LogUncategorizedEnemies(string logKey, IEnumerable<string> enemyNames)
    {
        if (!LoggedSummaryKeys.Add("enemies:" + logKey))
        {
            return;
        }

        EnsureEnemyIndexBuilt();

        List<string> uncategorized = new();
        foreach (string enemyName in enemyNames)
        {
            if (GetEnemyBiome(enemyName) == BiomeTier.Uncategorized)
            {
                string display = AchievementService.FormatStatName(enemyName);
                uncategorized.Add($"{display} ({enemyName})");
            }
        }

        if (uncategorized.Count == 0)
        {
            Plugin.Log.LogInfo($"[BiomeClassifier] {logKey}: all enemies categorized.");
            return;
        }

        uncategorized.Sort(StringComparer.OrdinalIgnoreCase);
        Plugin.Log.LogInfo(
            $"[BiomeClassifier] {logKey}: {uncategorized.Count} uncategorized enemies: {string.Join(", ", uncategorized)}");
    }

    public static void LogUncategorizedMaterialsForItems(string logKey, IEnumerable<string> sharedNames)
    {
        if (!LoggedSummaryKeys.Add("items:" + logKey))
        {
            return;
        }

        EnsureEnemyIndexBuilt();
        EnsureItemIndexBuilt();

        Dictionary<string, int> unknownCounts = new(StringComparer.OrdinalIgnoreCase);
        int uncategorizedItems = 0;

        foreach (string sharedName in sharedNames)
        {
            if (GetItemBiome(sharedName) != BiomeTier.Uncategorized)
            {
                continue;
            }

            uncategorizedItems++;
            HashSet<string> visited = new(StringComparer.Ordinal);
            HashSet<string> leafPrefabs = new(StringComparer.OrdinalIgnoreCase);
            CollectBaseMaterials(sharedName, visited, leafPrefabs);

            foreach (string leaf in leafPrefabs)
            {
                if (!TryGetKnownMaterialBiome(leaf, out _))
                {
                    unknownCounts.TryGetValue(leaf, out int c);
                    unknownCounts[leaf] = c + 1;
                }
            }
        }

        if (uncategorizedItems == 0)
        {
            Plugin.Log.LogInfo($"[BiomeClassifier] {logKey}: all craft items categorized.");
            return;
        }

        string topUnknown = string.Join(
            ", ",
            unknownCounts
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key)
                .Take(30)
                .Select(kvp => $"{kvp.Key}({kvp.Value})"));

        Plugin.Log.LogInfo(
            $"[BiomeClassifier] {logKey}: {uncategorizedItems} uncategorized craft items. Top unknown base materials: {topUnknown}");
    }

    private static bool TryGetKnownMaterialBiome(string prefabName, out BiomeTier tier)
    {
        if (BaseMaterialBiomes.TryGetValue(prefabName, out tier))
        {
            return true;
        }

        return DerivedMaterialBiomes.TryGetValue(prefabName, out tier);
    }

    private static BiomeTier EvaluateLeafMaterials(HashSet<string> leafPrefabs)
    {
        if (leafPrefabs.Count == 0)
        {
            return BiomeTier.Uncategorized;
        }

        BiomeTier highest = BiomeTier.Meadows;
        bool anyKnown = false;

        foreach (string prefab in leafPrefabs)
        {
            if (!TryGetKnownMaterialBiome(prefab, out BiomeTier tier))
            {
                return BiomeTier.Uncategorized;
            }

            anyKnown = true;
            if (tier > highest)
            {
                highest = tier;
            }
        }

        return anyKnown ? highest : BiomeTier.Uncategorized;
    }

    private static void CollectBaseMaterials(
        string sharedName,
        HashSet<string> visited,
        HashSet<string> leafPrefabs)
    {
        if (string.IsNullOrEmpty(sharedName) || !visited.Add(sharedName))
        {
            return;
        }

        if (PrefabNameBySharedName.TryGetValue(sharedName, out string prefabName) &&
            TryGetKnownMaterialBiome(prefabName, out _))
        {
            leafPrefabs.Add(prefabName);
            return;
        }

        if (RecipeBySharedName.TryGetValue(sharedName, out Recipe recipe) &&
            recipe.m_resources != null &&
            recipe.m_resources.Length > 0)
        {
            bool addedAnyIngredient = false;
            foreach (Piece.Requirement req in recipe.m_resources)
            {
                if (req == null || req.m_resItem == null || req.m_amount <= 0)
                {
                    continue;
                }

                string ingPrefab = Utils.GetPrefabName(req.m_resItem.gameObject);
                string? ingShared = req.m_resItem.m_itemData?.m_shared?.m_name;
                addedAnyIngredient = true;

                if (!string.IsNullOrEmpty(ingPrefab) && TryGetKnownMaterialBiome(ingPrefab, out _))
                {
                    leafPrefabs.Add(ingPrefab);
                }
                else if (!string.IsNullOrEmpty(ingShared) && RecipeBySharedName.ContainsKey(ingShared!))
                {
                    CollectBaseMaterials(ingShared!, visited, leafPrefabs);
                }
                else if (!string.IsNullOrEmpty(ingPrefab))
                {
                    leafPrefabs.Add(ingPrefab);
                }
            }

            if (addedAnyIngredient)
            {
                return;
            }
        }

        if (!string.IsNullOrEmpty(prefabName))
        {
            leafPrefabs.Add(prefabName);
        }
    }

    private static void EnsureEnemyIndexBuilt()
    {
        if (_enemyIndexBuilt)
        {
            return;
        }

        SpawnSystem? spawnSystem = ZoneSystem.instance != null && ZoneSystem.instance.m_zoneCtrlPrefab != null
            ? ZoneSystem.instance.m_zoneCtrlPrefab.GetComponent<SpawnSystem>()
            : null;

        if (spawnSystem == null && SpawnSystem.m_instances != null && SpawnSystem.m_instances.Count > 0)
        {
            spawnSystem = SpawnSystem.m_instances[0];
        }

        if (spawnSystem == null)
        {
            return;
        }

        _enemyIndexBuilt = true;
        ItemBiomeCache.Clear();

        List<GameObject> spawnedPrefabs = new();

        if (spawnSystem.m_spawnLists != null)
        {
            foreach (SpawnSystemList list in spawnSystem.m_spawnLists)
            {
                if (list == null || list.m_spawners == null) continue;
                IndexSpawnerList(list.m_spawners, Heightmap.Biome.None, spawnedPrefabs);
            }
        }

        if (AltBiomeList.m_altBiomes != null)
        {
            foreach (AltBiome altBiome in AltBiomeList.m_altBiomes)
            {
                if (altBiome == null || !altBiome.m_enabled || altBiome.m_spawn == null) continue;
                IndexSpawnerList(altBiome.m_spawn, altBiome.m_biome, spawnedPrefabs);
            }
        }

        // Apply manual overrides on top of SpawnSystem
        foreach (KeyValuePair<string, BiomeTier> kvp in EnemyBiomeOverrides)
        {
            EnemyBiomeByName[kvp.Key] = kvp.Value;
        }

        // Propagate classified enemy biomes to their CharacterDrop items (lowest biome wins)
        HashSet<GameObject> processedPrefabs = new();
        foreach (GameObject prefab in spawnedPrefabs)
        {
            if (prefab != null && processedPrefabs.Add(prefab))
            {
                DeriveDropsFromCharacterPrefab(prefab);
            }
        }

        if (ZNetScene.instance != null && ZNetScene.instance.m_prefabs != null)
        {
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
            {
                if (prefab != null && processedPrefabs.Add(prefab))
                {
                    DeriveDropsFromCharacterPrefab(prefab);
                }
            }
        }

        Plugin.Log.LogInfo(
            $"[BiomeClassifier] Indexed {EnemyBiomeByName.Count} enemies from SpawnSystem and derived {DerivedMaterialBiomes.Count} material biomes from mob drops.");
    }

    private static void IndexSpawnerList(
        List<SpawnSystem.SpawnData> spawners,
        Heightmap.Biome fallbackBiomeMask,
        List<GameObject> spawnedPrefabs)
    {
        foreach (SpawnSystem.SpawnData spawnData in spawners)
        {
            if (spawnData == null || !spawnData.m_enabled || spawnData.m_devDisabled || spawnData.m_prefab == null)
            {
                continue;
            }

            // Skip global-key and persistent-event spawners (e.g. post-boss night spawns of Fulings/Seekers/Charred in Meadows)
            if (!string.IsNullOrEmpty(spawnData.m_requiredGlobalKey) ||
                !string.IsNullOrEmpty(spawnData.m_requiredPersistentEvent))
            {
                continue;
            }

            Character character = spawnData.m_prefab.GetComponent<Character>();
            if (character == null || string.IsNullOrEmpty(character.m_name))
            {
                continue;
            }

            Heightmap.Biome effectiveMask = spawnData.m_biome;
            if (fallbackBiomeMask != Heightmap.Biome.None)
            {
                Heightmap.Biome intersection = effectiveMask & fallbackBiomeMask;
                effectiveMask = intersection != Heightmap.Biome.None ? intersection : fallbackBiomeMask;
            }

            BiomeTier tier = MapLowestBiome(effectiveMask);
            if (tier == BiomeTier.Uncategorized)
            {
                continue;
            }

            spawnedPrefabs.Add(spawnData.m_prefab);

            if (!EnemyBiomeByName.TryGetValue(character.m_name, out BiomeTier existing) || tier < existing)
            {
                EnemyBiomeByName[character.m_name] = tier;
            }
        }
    }

    private static void DeriveDropsFromCharacterPrefab(GameObject prefab)
    {
        Character character = prefab.GetComponent<Character>();
        if (character == null || string.IsNullOrEmpty(character.m_name))
        {
            return;
        }

        if (!EnemyBiomeByName.TryGetValue(character.m_name, out BiomeTier enemyTier) ||
            enemyTier == BiomeTier.Uncategorized)
        {
            return;
        }

        CharacterDrop charDrop = prefab.GetComponent<CharacterDrop>();
        if (charDrop == null || charDrop.m_drops == null)
        {
            return;
        }

        foreach (CharacterDrop.Drop drop in charDrop.m_drops)
        {
            if (drop == null || drop.m_prefab == null || drop.m_chance <= 0f || drop.m_amountMax <= 0)
            {
                continue;
            }

            ItemDrop itemDrop = drop.m_prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                continue;
            }

            string dropPrefabName = Utils.GetPrefabName(drop.m_prefab);
            if (string.IsNullOrEmpty(dropPrefabName))
            {
                continue;
            }

            if (!DerivedMaterialBiomes.TryGetValue(dropPrefabName, out BiomeTier existing) || enemyTier < existing)
            {
                DerivedMaterialBiomes[dropPrefabName] = enemyTier;
            }
        }
    }

    private static BiomeTier MapLowestBiome(Heightmap.Biome mask)
    {
        if ((mask & Heightmap.Biome.Meadows) != 0) return BiomeTier.Meadows;
        if ((mask & Heightmap.Biome.BlackForest) != 0) return BiomeTier.BlackForest;
        if ((mask & Heightmap.Biome.Ocean) != 0) return BiomeTier.Ocean;
        if ((mask & Heightmap.Biome.Swamp) != 0) return BiomeTier.Swamp;
        if ((mask & Heightmap.Biome.Mountain) != 0) return BiomeTier.Mountain;
        if ((mask & Heightmap.Biome.Plains) != 0) return BiomeTier.Plains;
        if ((mask & Heightmap.Biome.Mistlands) != 0) return BiomeTier.Mistlands;
        if ((mask & Heightmap.Biome.AshLands) != 0) return BiomeTier.Ashlands;
        if ((mask & Heightmap.Biome.DeepNorth) != 0) return BiomeTier.DeepNorth;
        return BiomeTier.Uncategorized;
    }

    private static void EnsureItemIndexBuilt()
    {
        if (_itemIndexBuilt || ObjectDB.instance == null)
        {
            return;
        }

        _itemIndexBuilt = true;

        foreach (GameObject itemObj in ObjectDB.instance.m_items)
        {
            if (itemObj == null) continue;
            ItemDrop drop = itemObj.GetComponent<ItemDrop>();
            string? sharedName = drop?.m_itemData?.m_shared?.m_name;
            if (!string.IsNullOrEmpty(sharedName) && !PrefabNameBySharedName.ContainsKey(sharedName!))
            {
                PrefabNameBySharedName[sharedName!] = Utils.GetPrefabName(itemObj);
            }
        }

        foreach (Recipe recipe in ObjectDB.instance.m_recipes)
        {
            if (recipe == null || recipe.m_item == null) continue;
            string? sharedName = recipe.m_item.m_itemData?.m_shared?.m_name;
            if (string.IsNullOrEmpty(sharedName)) continue;

            if (!PrefabNameBySharedName.ContainsKey(sharedName!))
            {
                PrefabNameBySharedName[sharedName!] = Utils.GetPrefabName(recipe.m_item.gameObject);
            }

            if (!RecipeBySharedName.ContainsKey(sharedName!))
            {
                RecipeBySharedName[sharedName!] = recipe;
            }
        }
    }
}
