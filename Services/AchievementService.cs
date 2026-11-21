using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AchievementHelper.Services;

public static class AchievementService
{
    private static readonly Dictionary<string, AchievementDisplayMode> ModeById =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // 1. Bosses & Combat
            ["Boss1Eikthyr"] = AchievementDisplayMode.Hidden,
            ["Boss2Elder"] = AchievementDisplayMode.Hidden,
            ["Boss3Bonemass"] = AchievementDisplayMode.Hidden,
            ["Boss4Moder"] = AchievementDisplayMode.Hidden,
            ["Boss5Yagluth"] = AchievementDisplayMode.Hidden,
            ["Boss6Queen"] = AchievementDisplayMode.Hidden,
            ["Boss7Fader"] = AchievementDisplayMode.Hidden,
            ["Boss8FrozenKing"] = AchievementDisplayMode.Hidden,
            ["AllMiniBosses"] = AchievementDisplayMode.Revealable,
            ["AllBosses"] = AchievementDisplayMode.Revealable,
            ["AllBossesNormal"] = AchievementDisplayMode.Revealable,
            ["AllBossesHard"] = AchievementDisplayMode.Revealable,
            ["SoloBoss"] = AchievementDisplayMode.Simple,
            ["MultiplayerBoss"] = AchievementDisplayMode.Simple,
            ["KillAllCreatures"] = AchievementDisplayMode.Revealable,
            ["KillAllCreaturesHard"] = AchievementDisplayMode.Revealable,

            // 2. Crafting, Cooking & Building
            ["AllWeaponCraft"] = AchievementDisplayMode.Revealable,
            ["AllItemCraft"] = AchievementDisplayMode.Revealable,
            ["AllFoodCooked"] = AchievementDisplayMode.Revealable,
            ["AllFoodEaten"] = AchievementDisplayMode.Revealable,
            ["AllBuildPieces"] = AchievementDisplayMode.Revealable,
            ["FindAllTrophies"] = AchievementDisplayMode.Revealable,
            ["BuildDyrnwyn"] = AchievementDisplayMode.Hidden,
            ["GrindCook"] = AchievementDisplayMode.All,
            ["GrindEat"] = AchievementDisplayMode.All,
            ["GrindBuild"] = AchievementDisplayMode.All,
            ["BuildHouse"] = AchievementDisplayMode.All,
            ["BuildVillage"] = AchievementDisplayMode.All,
            ["BuildHigh"] = AchievementDisplayMode.All,
            ["BuildHighWorld"] = AchievementDisplayMode.All,
            ["GrindComfort"] = AchievementDisplayMode.All,

            // 3. Gathering & Farming
            ["GrindTrees"] = AchievementDisplayMode.All,
            ["GrindHarvest"] = AchievementDisplayMode.All,
            ["GrindForager"] = AchievementDisplayMode.All,
            ["GrindHoney"] = AchievementDisplayMode.All,
            ["GrindFish"] = AchievementDisplayMode.Revealable,
            ["BigFish"] = AchievementDisplayMode.Simple,

            // 4. Exploration, Sailing & Survival
            ["Arrived"] = AchievementDisplayMode.Simple,
            ["GrindDays"] = AchievementDisplayMode.All,
            ["ExploreNSEW"] = AchievementDisplayMode.All,
            ["ExploreNSEWNoMap"] = AchievementDisplayMode.All,
            ["GrindSail"] = AchievementDisplayMode.All,
            ["GrindSailHelm"] = AchievementDisplayMode.All,
            ["GrindTreasure"] = AchievementDisplayMode.All,
            ["GrindLeviathan"] = AchievementDisplayMode.Hidden,
            ["SailEdge"] = AchievementDisplayMode.Hidden,

            // 5. Deaths & Quirky
            ["FirstDeath"] = AchievementDisplayMode.Hidden,
            ["DeathByAllTypes"] = AchievementDisplayMode.Revealable,
            ["DeathByTree"] = AchievementDisplayMode.Simple,
            ["DeathByTreeAll"] = AchievementDisplayMode.Revealable,
            ["DeathByObliterator"] = AchievementDisplayMode.Hidden,

            // 6. Meta-Achievements
            ["ChildOfOdin"] = AchievementDisplayMode.All,
            ["KingsQueens"] = AchievementDisplayMode.All,
        };

    private static readonly Dictionary<string, AchievementDisplayMode> ModeByLocalizedName =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // 1. Bosses & Combat
            ["Eikthyr"] = AchievementDisplayMode.Hidden,
            ["The Elder"] = AchievementDisplayMode.Hidden,
            ["Bonemass"] = AchievementDisplayMode.Hidden,
            ["Moder"] = AchievementDisplayMode.Hidden,
            ["Yagluth"] = AchievementDisplayMode.Hidden,
            ["The Seeker Queen"] = AchievementDisplayMode.Hidden,
            ["Fader"] = AchievementDisplayMode.Hidden,
            ["Kall Fimbulbringer"] = AchievementDisplayMode.Hidden,
            ["Odin is Pleased"] = AchievementDisplayMode.Revealable,
            ["The Viking"] = AchievementDisplayMode.Revealable,
            ["The Tough Viking"] = AchievementDisplayMode.Revealable,
            ["The Toughest Viking"] = AchievementDisplayMode.Revealable,
            ["Lone Wolf"] = AchievementDisplayMode.Simple,
            ["Stronger Together"] = AchievementDisplayMode.Simple,
            ["The Hunter"] = AchievementDisplayMode.Revealable,
            ["The Master Hunter"] = AchievementDisplayMode.Revealable,

            // 2. Crafting, Cooking & Building
            ["The Blacksmith"] = AchievementDisplayMode.Revealable,
            ["The Artisan"] = AchievementDisplayMode.Revealable,
            ["The Master Chef"] = AchievementDisplayMode.Revealable,
            ["The Gourmet"] = AchievementDisplayMode.Revealable,
            ["The Carpenter"] = AchievementDisplayMode.Revealable,
            ["The Trophy Collector"] = AchievementDisplayMode.Revealable,
            ["The Chosen One"] = AchievementDisplayMode.Hidden,
            ["The Cook"] = AchievementDisplayMode.All,
            ["No Adventuring On an Empty Stomach"] = AchievementDisplayMode.All,
            ["The Builder"] = AchievementDisplayMode.All,
            ["The Homesteader"] = AchievementDisplayMode.All,
            ["The Architect"] = AchievementDisplayMode.All,
            ["Mighty Halls"] = AchievementDisplayMode.All,
            ["Top of the World"] = AchievementDisplayMode.All,
            ["Comfort is King"] = AchievementDisplayMode.All,

            // 3. Gathering & Farming
            ["The Lumberjack"] = AchievementDisplayMode.All,
            ["The Farmer"] = AchievementDisplayMode.All,
            ["The Forager"] = AchievementDisplayMode.All,
            ["The Beekeeper"] = AchievementDisplayMode.All,
            ["The Fisherman"] = AchievementDisplayMode.Revealable,
            ["A Catch Worthy of Njord"] = AchievementDisplayMode.Simple,

            // 4. Exploration, Sailing & Survival
            ["I Have Arrived!"] = AchievementDisplayMode.Simple,
            ["The Survivor"] = AchievementDisplayMode.All,
            ["The Explorer"] = AchievementDisplayMode.All,
            ["The Pathfinder"] = AchievementDisplayMode.All,
            ["The Sailor"] = AchievementDisplayMode.All,
            ["The Captain"] = AchievementDisplayMode.All,
            ["The Treasure Hunter"] = AchievementDisplayMode.All,
            ["No Time For Snoozing"] = AchievementDisplayMode.Hidden,
            ["The Legends Are True"] = AchievementDisplayMode.Hidden,

            // 5. Deaths & Quirky
            ["The First of Many"] = AchievementDisplayMode.Hidden,
            ["Are You Alright?"] = AchievementDisplayMode.Revealable,
            ["Accidents Happen"] = AchievementDisplayMode.Simple,
            ["That Can't Be an Accident"] = AchievementDisplayMode.Revealable,
            ["Obliterated"] = AchievementDisplayMode.Hidden,

            // 6. Meta-Achievements
            ["Child of Odin"] = AchievementDisplayMode.All,
            ["Kings and Queens of Valheim"] = AchievementDisplayMode.All,
        };

    private static readonly HashSet<string> RevealedIds = new(StringComparer.OrdinalIgnoreCase);
    private static bool _revealedLoaded;

    private static readonly HashSet<string> CraftTargets = new();
    private static readonly HashSet<string> CookTargets = new();
    private static readonly HashSet<string> FoodEatenTargets = new();
    private static readonly HashSet<string> BuildPieceTargets = new();
    private static readonly HashSet<string> EnemyTargets = new();
    private static readonly HashSet<string> TrophyTargets = new();
    private static readonly Dictionary<string, string> RawToCookedMap = new();

    public static void OnAchievementsInitialized()
    {
        EnsureRevealedLoaded();
        RebuildTriggerCaches();
        DumpAllAchievements();
    }

    public static AchievementDisplayMode GetDisplayMode(Achievement achievement)
    {
        if (achievement == null)
        {
            return AchievementDisplayMode.Hidden;
        }

        if (ModeById.TryGetValue(achievement.m_id, out AchievementDisplayMode byId))
        {
            return byId;
        }

        string localizedName = Localization.instance != null
            ? Localization.instance.Localize(achievement.m_name)
            : achievement.m_name;

        if (ModeByLocalizedName.TryGetValue(localizedName, out AchievementDisplayMode byName))
        {
            return byName;
        }

        return achievement.m_isSecret
            ? AchievementDisplayMode.Hidden
            : AchievementDisplayMode.Revealable;
    }

    public static bool IsRevealed(Achievement achievement)
    {
        if (achievement == null)
        {
            return false;
        }

        if (achievement.m_unlocked || Plugin.MasterRevealAll.Value)
        {
            return true;
        }

        EnsureRevealedLoaded();
        return RevealedIds.Contains(achievement.m_id);
    }

    public static bool IsStatRevealed(Achievement achievement, string statKey)
    {
        if (IsRevealed(achievement))
        {
            return true;
        }

        if (achievement == null || string.IsNullOrEmpty(statKey))
        {
            return false;
        }

        EnsureRevealedLoaded();
        return RevealedIds.Contains($"{achievement.m_id}:{statKey}");
    }

    public static void RevealAchievement(Achievement achievement)
    {
        if (achievement == null || string.IsNullOrEmpty(achievement.m_id))
        {
            return;
        }

        EnsureRevealedLoaded();
        if (RevealedIds.Add(achievement.m_id))
        {
            SaveRevealedIds();
        }
    }

    public static void RevealStat(Achievement achievement, string statKey)
    {
        if (achievement == null || string.IsNullOrEmpty(achievement.m_id) || string.IsNullOrEmpty(statKey))
        {
            return;
        }

        EnsureRevealedLoaded();
        if (RevealedIds.Add($"{achievement.m_id}:{statKey}"))
        {
            SaveRevealedIds();
        }
    }

    public static bool HasAnyUnrevealedCondition(Achievement achievement)
    {
        if (achievement == null || achievement.m_unlocked || IsRevealed(achievement))
        {
            return false;
        }

        PlayerProfile? profile = Game.instance != null ? Game.instance.GetPlayerProfile() : null;
        if (profile == null)
        {
            return false;
        }

        int diffIndex = (int)achievement.m_difficultyRequirement;
        if (diffIndex < 0 || diffIndex >= profile.m_playerStats.Length)
        {
            return false;
        }

        PlayerProfile.PlayerStats stats = profile.m_playerStats[diffIndex];

        foreach (var req in achievement.m_statTrigger)
        {
            stats.m_stats.TryGetValue(req.m_stat, out float cur);
            if (cur < req.m_amountAboveEquals && !IsStatRevealed(achievement, req.m_stat.ToString()))
                return true;
        }

        foreach (var other in achievement.m_otherAchievementTriggers)
        {
            if (other != null && !other.m_unlocked && !IsStatRevealed(achievement, other.m_name))
                return true;
        }

        foreach (var req in achievement.m_enemyStatsTriggers)
        {
            stats.m_enemyStats[(int)req.m_modifier].TryGetValue(req.m_stat, out float cur);
            if (cur < req.m_amount && !IsStatRevealed(achievement, req.m_stat))
                return true;
        }

        return HasUnrevealedDictTrigger(achievement, achievement.m_foodEatenTriggers, stats.m_foodEatenStats) ||
               HasUnrevealedDictTrigger(achievement, achievement.m_itemCraftTriggers, stats.m_itemCraftStats) ||
               HasUnrevealedDictTrigger(achievement, achievement.m_itemPickupTriggers, stats.m_itemPickupStats) ||
               HasUnrevealedDictTrigger(achievement, achievement.m_knownCommandsTriggers, stats.m_knownCommands) ||
               HasUnrevealedDictTrigger(achievement, achievement.m_knownWorldKeysTriggers, stats.m_knownWorldKeys) ||
               HasUnrevealedDictTrigger(achievement, achievement.m_knownWorldTriggers, stats.m_knownWorlds) ||
               HasUnrevealedDictTrigger(achievement, achievement.m_pickableTriggers, stats.m_pickableStats) ||
               HasUnrevealedDictTrigger(achievement, achievement.m_piecePlacedTriggers, stats.m_piecesPlacedStats);
    }

    private static bool HasUnrevealedDictTrigger(
        Achievement achievement,
        List<Achievement.DictStatRequirement> triggers,
        Dictionary<string, float> dict)
    {
        foreach (var req in triggers)
        {
            dict.TryGetValue(req.m_stat, out float cur);
            if (cur < req.m_amount && !IsStatRevealed(achievement, req.m_stat))
                return true;
        }

        return false;
    }

    private static void SaveRevealedIds()
    {
        Plugin.RevealedAchievementIds.Value = string.Join(",", RevealedIds.OrderBy(id => id));
        Plugin.Instance.Config.Save();
    }

    public static void RevealAllSpoilers()
    {
        Plugin.MasterRevealAll.Value = true;
        Plugin.Instance.Config.Save();
    }

    public static bool HasAnyManuallyRevealedEntry(Achievement achievement)
    {
        if (achievement == null || string.IsNullOrEmpty(achievement.m_id))
        {
            return false;
        }

        EnsureRevealedLoaded();
        string prefix = achievement.m_id + ":";
        foreach (string entry in RevealedIds)
        {
            if (entry.Equals(achievement.m_id, StringComparison.OrdinalIgnoreCase) ||
                entry.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static void HideAchievement(Achievement achievement)
    {
        if (achievement == null || string.IsNullOrEmpty(achievement.m_id))
        {
            return;
        }

        EnsureRevealedLoaded();
        string prefix = achievement.m_id + ":";
        int removed = RevealedIds.RemoveWhere(entry =>
            entry.Equals(achievement.m_id, StringComparison.OrdinalIgnoreCase) ||
            entry.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (removed > 0)
        {
            SaveRevealedIds();
        }
    }

    public static void HideAllSpoilers()
    {
        EnsureRevealedLoaded();
        Plugin.MasterRevealAll.Value = false;
        RevealedIds.Clear();
        Plugin.RevealedAchievementIds.Value = string.Empty;
        Plugin.Instance.Config.Save();
    }

    public static bool IsConcealed(Achievement achievement)
    {
        if (achievement == null || achievement.m_unlocked || Plugin.MasterRevealAll.Value)
        {
            return false;
        }

        if (!achievement.m_isSecret)
        {
            return false;
        }

        AchievementDisplayMode mode = GetDisplayMode(achievement);
        if (Plugin.OnlyHideSpoilerAchievements.Value && mode != AchievementDisplayMode.Hidden)
        {
            return false;
        }

        return true;
    }

    public static bool HasAnyDetails(Achievement achievement)
    {
        if (achievement == null)
        {
            return false;
        }

        return achievement.m_statTrigger.Count > 0 ||
               achievement.m_enemyStatsTriggers.Count > 0 ||
               achievement.m_foodEatenTriggers.Count > 0 ||
               achievement.m_itemCraftTriggers.Count > 0 ||
               achievement.m_itemPickupTriggers.Count > 0 ||
               achievement.m_knownCommandsTriggers.Count > 0 ||
               achievement.m_knownWorldKeysTriggers.Count > 0 ||
               achievement.m_knownWorldTriggers.Count > 0 ||
               achievement.m_pickableTriggers.Count > 0 ||
               achievement.m_piecePlacedTriggers.Count > 0 ||
               achievement.m_otherAchievementTriggers.Count > 0;
    }

    public static string FormatStatName(string statKey)
    {
        if (string.IsNullOrEmpty(statKey))
        {
            return string.Empty;
        }

        string lookupKey = Enum.TryParse<PlayerStatType>(statKey, out _)
            ? "$stat_" + statKey
            : statKey;

        string localized = Localization.instance != null
            ? Localization.instance.Localize(lookupKey)
            : lookupKey;

        if (!string.IsNullOrEmpty(localized) && !(localized.StartsWith("[") && localized.EndsWith("]")))
        {
            return localized;
        }

        return SplitPascalCase(statKey.TrimStart('$'));
    }

    public static string FormatAmount(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value))
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.#");
    }

    private static string SplitPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder(input.Length + 8);
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(input[i - 1]) && input[i - 1] != ' ' && input[i - 1] != '_')
            {
                sb.Append(' ');
            }
            sb.Append(c == '_' ? ' ' : c);
        }

        return sb.ToString();
    }

    public static float GetCompletionPercentage(Achievement achievement)
    {
        if (achievement == null)
        {
            return 0f;
        }

        if (achievement.m_unlocked)
        {
            return 100f;
        }

        PlayerProfile? profile = Game.instance != null ? Game.instance.GetPlayerProfile() : null;
        if (profile == null)
        {
            return 0f;
        }

        int diffIndex = (int)achievement.m_difficultyRequirement;
        if (diffIndex < 0 || diffIndex >= profile.m_playerStats.Length)
        {
            return 0f;
        }

        PlayerProfile.PlayerStats stats = profile.m_playerStats[diffIndex];
        float totalRatio = 0f;
        int totalCount = 0;

        foreach (Achievement.PlayerStatRequirement req in achievement.m_statTrigger)
        {
            stats.m_stats.TryGetValue(req.m_stat, out float current);
            float target = Mathf.Max(1f, req.m_amountAboveEquals);
            totalRatio += Mathf.Clamp01(current / target);
            totalCount++;
        }

        foreach (Achievement.EnemyStatRequirement req in achievement.m_enemyStatsTriggers)
        {
            stats.m_enemyStats[(int)req.m_modifier].TryGetValue(req.m_stat, out float current);
            totalRatio += req.IsMet(stats.m_enemyStats[(int)req.m_modifier], achievement.m_difficultyRequirement)
                ? 1f
                : Mathf.Clamp01(current / Mathf.Max(1f, req.m_amount));
            totalCount++;
        }

        AccumulateDictTriggers(achievement.m_itemCraftTriggers, stats.m_itemCraftStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(achievement.m_itemPickupTriggers, stats.m_itemPickupStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(achievement.m_foodEatenTriggers, stats.m_foodEatenStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(achievement.m_piecePlacedTriggers, stats.m_piecesPlacedStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(achievement.m_pickableTriggers, stats.m_pickableStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(achievement.m_knownCommandsTriggers, stats.m_knownCommands, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(achievement.m_knownWorldKeysTriggers, stats.m_knownWorldKeys, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(achievement.m_knownWorldTriggers, stats.m_knownWorlds, ref totalRatio, ref totalCount);

        foreach (Achievement other in achievement.m_otherAchievementTriggers)
        {
            totalRatio += other != null && other.m_unlocked ? 1f : 0f;
            totalCount++;
        }

        return totalCount > 0 ? (totalRatio / totalCount) * 100f : 0f;
    }

    public static bool IsItemCraftTarget(string sharedName) => CraftTargets.Contains(sharedName);
    public static bool IsFoodCookTarget(string sharedName) => CookTargets.Contains(sharedName);
    public static bool IsFoodEatenTarget(string sharedName) => FoodEatenTargets.Contains(sharedName);
    public static bool IsBuildPieceTarget(string pieceName) => BuildPieceTargets.Contains(pieceName);
    public static bool IsEnemyTarget(string enemyName) => EnemyTargets.Contains(enemyName);
    public static bool IsTrophyTarget(string trophySharedName) => TrophyTargets.Contains(trophySharedName);
    public static bool TryGetCookedVariant(string rawSharedName, out string cookedSharedName) =>
        RawToCookedMap.TryGetValue(rawSharedName, out cookedSharedName);

    public static bool HasCraftedItem(string sharedName)
    {
        return TryGetCurrentWorldStats(out var stats) &&
               stats.m_itemCraftStats.TryGetValue(sharedName, out float count) &&
               count >= 1f;
    }

    public static bool HasEatenFood(string sharedName)
    {
        return TryGetCurrentWorldStats(out var stats) &&
               stats.m_foodEatenStats.TryGetValue(sharedName, out float count) &&
               count >= 1f;
    }

    public static bool HasBuiltPiece(string pieceName)
    {
        return TryGetCurrentWorldStats(out var stats) &&
               stats.m_piecesPlacedStats.TryGetValue(pieceName, out float count) &&
               count >= 1f;
    }

    public static bool HasKilledEnemy(string enemyName)
    {
        if (!TryGetCurrentWorldStats(out var stats))
        {
            return false;
        }

        return stats.m_enemyStats[0].TryGetValue(enemyName, out float count) && count >= 1f;
    }

    public static bool HasPickedUpTrophy(string trophySharedName)
    {
        return TryGetCurrentWorldStats(out var stats) &&
               stats.m_itemPickupStats.TryGetValue(trophySharedName, out float count) &&
               count >= 1f;
    }

    private static bool TryGetCurrentWorldStats(out PlayerProfile.PlayerStats stats)
    {
        PlayerProfile? profile = Game.instance != null ? Game.instance.GetPlayerProfile() : null;
        if (profile == null)
        {
            stats = null!;
            return false;
        }

        int diffIndex = Achievements.GetCurrentAchievementDifficultyIndex();
        if (diffIndex < 0 || diffIndex >= profile.m_playerStats.Length)
        {
            diffIndex = (int)DifficultyRequirement.Any;
        }

        stats = profile.m_playerStats[diffIndex];
        return stats != null;
    }

    private static void AccumulateDictTriggers(
        List<Achievement.DictStatRequirement> triggers,
        Dictionary<string, float> dict,
        ref float totalRatio,
        ref int totalCount)
    {
        foreach (Achievement.DictStatRequirement req in triggers)
        {
            dict.TryGetValue(req.m_stat, out float current);
            float target = Mathf.Max(1f, req.m_amount);
            totalRatio += Mathf.Clamp01(current / target);
            totalCount++;
        }
    }

    private static void EnsureRevealedLoaded()
    {
        if (_revealedLoaded)
        {
            return;
        }

        _revealedLoaded = true;
        string raw = Plugin.RevealedAchievementIds.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        foreach (string id in raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            RevealedIds.Add(id.Trim());
        }
    }

    private static void RebuildTriggerCaches()
    {
        CraftTargets.Clear();
        CookTargets.Clear();
        FoodEatenTargets.Clear();
        BuildPieceTargets.Clear();
        EnemyTargets.Clear();
        TrophyTargets.Clear();
        RawToCookedMap.Clear();

        if (ZNetScene.instance != null)
        {
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
            {
                if (prefab == null) continue;
                CookingStation station = prefab.GetComponent<CookingStation>();
                if (station == null) continue;

                foreach (CookingStation.ItemConversion conv in station.m_conversion)
                {
                    string? fromName = conv.m_from?.m_itemData?.m_shared?.m_name;
                    ItemDrop.ItemData.SharedData? toShared = conv.m_to?.m_itemData?.m_shared;
                    if (string.IsNullOrEmpty(fromName) || toShared == null || string.IsNullOrEmpty(toShared.m_name))
                    {
                        continue;
                    }

                    if (toShared.m_food > 0f &&
                        toShared.m_itemType == ItemDrop.ItemData.ItemType.Consumable &&
                        !RawToCookedMap.ContainsKey(fromName!))
                    {
                        RawToCookedMap[fromName!] = toShared.m_name;
                    }
                }
            }
        }

        if (Achievements.m_instance == null)
        {
            return;
        }

        foreach (AchievementList list in Achievements.m_instance.m_achievementLists)
        {
            foreach (Achievement ach in list.m_achievements)
            {
                switch (ach.m_id)
                {
                    case "AllItemCraft":
                    case "AllWeaponCraft":
                        foreach (var req in ach.m_itemCraftTriggers)
                            if (!string.IsNullOrEmpty(req.m_stat)) CraftTargets.Add(req.m_stat);
                        break;
                    case "AllFoodCooked":
                        foreach (var req in ach.m_itemCraftTriggers)
                            if (!string.IsNullOrEmpty(req.m_stat)) CookTargets.Add(req.m_stat);
                        break;
                    case "AllFoodEaten":
                        foreach (var req in ach.m_foodEatenTriggers)
                            if (!string.IsNullOrEmpty(req.m_stat)) FoodEatenTargets.Add(req.m_stat);
                        break;
                    case "AllBuildPieces":
                        foreach (var req in ach.m_piecePlacedTriggers)
                            if (!string.IsNullOrEmpty(req.m_stat)) BuildPieceTargets.Add(req.m_stat);
                        break;
                    case "KillAllCreatures":
                    case "KillAllCreaturesHard":
                        foreach (var req in ach.m_enemyStatsTriggers)
                            if (!string.IsNullOrEmpty(req.m_stat)) EnemyTargets.Add(req.m_stat);
                        break;
                    case "FindAllTrophies":
                        foreach (var req in ach.m_itemPickupTriggers)
                            if (!string.IsNullOrEmpty(req.m_stat)) TrophyTargets.Add(req.m_stat);
                        break;
                }
            }
        }
    }

    private static void DumpAllAchievements()
    {
        if (Achievements.m_instance == null)
        {
            return;
        }

        int count = 0;
        foreach (AchievementList list in Achievements.m_instance.m_achievementLists)
        {
            foreach (Achievement ach in list.m_achievements)
            {
                count++;
                string locName = Localization.instance != null
                    ? Localization.instance.Localize(ach.m_name)
                    : ach.m_name;
                AchievementDisplayMode mode = GetDisplayMode(ach);
                Plugin.Log.LogInfo(
                    $"[Ach #{count:00}] id={ach.m_id} | name=\"{locName}\" | secret={ach.m_isSecret} | diff={ach.m_difficultyRequirement} | mode={mode}");
            }
        }
    }
}
