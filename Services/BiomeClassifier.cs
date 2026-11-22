using System;
using System.Collections.Generic;
using System.Linq;

namespace AchievementHelper.Services;

public static class BiomeClassifier
{
    // Base materials mapped by prefab name (case-insensitive)
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

    private static readonly Dictionary<string, Recipe> RecipeBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> PrefabNameBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BiomeTier> ItemBiomeCache = new(StringComparer.Ordinal);
    private static readonly HashSet<string> LoggedSummaryKeys = new(StringComparer.OrdinalIgnoreCase);
    private static bool _itemIndexBuilt;

    public static void ResetCache()
    {
        RecipeBySharedName.Clear();
        PrefabNameBySharedName.Clear();
        ItemBiomeCache.Clear();
        LoggedSummaryKeys.Clear();
        _itemIndexBuilt = false;
    }

    public static BiomeTier GetItemBiome(string sharedName)
    {
        if (string.IsNullOrEmpty(sharedName))
        {
            return BiomeTier.Uncategorized;
        }

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

    public static void LogUncategorizedMaterialsForItems(string logKey, IEnumerable<string> sharedNames)
    {
        if (!LoggedSummaryKeys.Add("items:" + logKey))
        {
            return;
        }

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
                if (!BaseMaterialBiomes.ContainsKey(leaf))
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
            if (!BaseMaterialBiomes.TryGetValue(prefab, out BiomeTier tier))
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
            BaseMaterialBiomes.ContainsKey(prefabName))
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

                if (!string.IsNullOrEmpty(ingPrefab) && BaseMaterialBiomes.ContainsKey(ingPrefab))
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

    private static void EnsureItemIndexBuilt()
    {
        if (_itemIndexBuilt || ObjectDB.instance == null)
        {
            return;
        }

        _itemIndexBuilt = true;

        foreach (UnityEngine.GameObject itemObj in ObjectDB.instance.m_items)
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
