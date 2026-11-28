using System;
using System.Collections.Generic;
using System.Linq;
using Splatform;
using Steamworks;
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

    private static readonly HashSet<PlayerStatType> RecordStatTypes = new()
    {
        PlayerStatType.ConsecutiveDaysSurvivedMax,
        PlayerStatType.MaxComfort,
        PlayerStatType.MaxBuildingHeight,
        PlayerStatType.MaxBuildingHeightWorld
    };

    private static readonly HashSet<string> RevealedIds = new(StringComparer.OrdinalIgnoreCase);
    private static bool _revealedLoaded;

    private static PlayerProfile.PlayerStats[]? _otherCharactersStats;

    private static readonly HashSet<string> CraftTargets = new();
    private static readonly HashSet<string> CookTargets = new();
    private static readonly HashSet<string> FoodEatenTargets = new();
    private static readonly HashSet<string> BuildPieceTargets = new();
    private static readonly HashSet<string> EnemyTargets = new();
    private static readonly HashSet<string> TrophyTargets = new();
    private static readonly HashSet<string> FishTargets = new();
    private static readonly Dictionary<string, string> RawToCookedMap = new();
    private static readonly Dictionary<string, string> CharacterTrophyMap = new();

    public static void OnAchievementsInitialized()
    {
        EnsureRevealedLoaded();
        BiomeClassifier.ResetCache();
        RebuildTriggerCaches();
        RefreshProgressScope(recheckUnlocks: true);
        DumpAllAchievements();
    }

    public static void RefreshProgressScope(bool recheckUnlocks)
    {
        if (Plugin.ProgressScopeConfig.Value == ProgressScope.Global)
        {
            RebuildOtherCharactersCache();
        }
        else
        {
            _otherCharactersStats = null;
        }

        if (!recheckUnlocks || Achievements.m_instance == null || Game.instance == null)
        {
            return;
        }

        ReevaluateAllAchievements();

        if (InventoryGui.instance != null &&
            InventoryGui.instance.m_achievementsPanel != null &&
            InventoryGui.instance.m_achievementsPanel.gameObject.activeInHierarchy)
        {
            InventoryGui.instance.UpdateAchievementsList();
        }
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

        foreach (var req in achievement.m_statTrigger)
        {
            float cur = GetEffectivePlayerStat(diffIndex, req.m_stat);
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
            float cur = GetEffectiveEnemyStat(diffIndex, req.m_modifier, req.m_stat);
            if (cur < req.m_amount && !IsStatRevealed(achievement, req.m_stat))
                return true;
        }

        return HasUnrevealedDictTrigger(achievement, diffIndex, achievement.m_foodEatenTriggers, s => s.m_foodEatenStats) ||
               HasUnrevealedDictTrigger(achievement, diffIndex, achievement.m_itemCraftTriggers, s => s.m_itemCraftStats) ||
               HasUnrevealedDictTrigger(achievement, diffIndex, achievement.m_itemPickupTriggers, s => s.m_itemPickupStats) ||
               HasUnrevealedDictTrigger(achievement, diffIndex, achievement.m_knownCommandsTriggers, s => s.m_knownCommands) ||
               HasUnrevealedDictTrigger(achievement, diffIndex, achievement.m_knownWorldKeysTriggers, s => s.m_knownWorldKeys) ||
               HasUnrevealedDictTrigger(achievement, diffIndex, achievement.m_knownWorldTriggers, s => s.m_knownWorlds) ||
               HasUnrevealedDictTrigger(achievement, diffIndex, achievement.m_pickableTriggers, s => s.m_pickableStats) ||
               HasUnrevealedDictTrigger(achievement, diffIndex, achievement.m_piecePlacedTriggers, s => s.m_piecesPlacedStats);
    }

    private static bool HasUnrevealedDictTrigger(
        Achievement achievement,
        int diffIndex,
        List<Achievement.DictStatRequirement> triggers,
        Func<PlayerProfile.PlayerStats, Dictionary<string, float>> selector)
    {
        foreach (var req in triggers)
        {
            float cur = GetEffectiveDictStat(diffIndex, selector, req.m_stat);
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

        float totalRatio = 0f;
        int totalCount = 0;

        foreach (Achievement.PlayerStatRequirement req in achievement.m_statTrigger)
        {
            float current = GetEffectivePlayerStat(diffIndex, req.m_stat);
            float target = Mathf.Max(1f, req.m_amountAboveEquals);
            totalRatio += Mathf.Clamp01(current / target);
            totalCount++;
        }

        foreach (Achievement.EnemyStatRequirement req in achievement.m_enemyStatsTriggers)
        {
            bool hasVal = TryGetEffectiveEnemyStat(diffIndex, req.m_modifier, req.m_stat, out float current);
            totalRatio += EvaluateOperator(hasVal, current, req.m_amount, req.m_operator)
                ? 1f
                : Mathf.Clamp01(current / Mathf.Max(1f, req.m_amount));
            totalCount++;
        }

        AccumulateDictTriggers(diffIndex, achievement.m_itemCraftTriggers, s => s.m_itemCraftStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(diffIndex, achievement.m_itemPickupTriggers, s => s.m_itemPickupStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(diffIndex, achievement.m_foodEatenTriggers, s => s.m_foodEatenStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(diffIndex, achievement.m_piecePlacedTriggers, s => s.m_piecesPlacedStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(diffIndex, achievement.m_pickableTriggers, s => s.m_pickableStats, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(diffIndex, achievement.m_knownCommandsTriggers, s => s.m_knownCommands, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(diffIndex, achievement.m_knownWorldKeysTriggers, s => s.m_knownWorldKeys, ref totalRatio, ref totalCount);
        AccumulateDictTriggers(diffIndex, achievement.m_knownWorldTriggers, s => s.m_knownWorlds, ref totalRatio, ref totalCount);

        foreach (Achievement other in achievement.m_otherAchievementTriggers)
        {
            totalRatio += other != null && other.m_unlocked ? 1f : 0f;
            totalCount++;
        }

        return totalCount > 0 ? (totalRatio / totalCount) * 100f : 0f;
    }

    public static bool EvaluateAchievementUnlocked(Achievement achievement)
    {
        if (achievement == null)
        {
            return false;
        }

        if (achievement.m_unlocked)
        {
            return true;
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

        foreach (Achievement.PlayerStatRequirement req in achievement.m_statTrigger)
        {
            if (GetEffectivePlayerStat(diffIndex, req.m_stat) < req.m_amountAboveEquals)
                return false;
        }

        if (!AreDictTriggersMet(diffIndex, achievement.m_knownWorldTriggers, s => s.m_knownWorlds)) return false;
        if (!AreDictTriggersMet(diffIndex, achievement.m_knownWorldKeysTriggers, s => s.m_knownWorldKeys)) return false;
        if (!AreDictTriggersMet(diffIndex, achievement.m_knownCommandsTriggers, s => s.m_knownCommands)) return false;

        foreach (Achievement.EnemyStatRequirement req in achievement.m_enemyStatsTriggers)
        {
            bool hasVal = TryGetEffectiveEnemyStat(diffIndex, req.m_modifier, req.m_stat, out float current);
            if (!EvaluateOperator(hasVal, current, req.m_amount, req.m_operator))
                return false;
        }

        if (!AreDictTriggersMet(diffIndex, achievement.m_itemPickupTriggers, s => s.m_itemPickupStats)) return false;
        if (!AreDictTriggersMet(diffIndex, achievement.m_itemCraftTriggers, s => s.m_itemCraftStats)) return false;
        if (!AreDictTriggersMet(diffIndex, achievement.m_pickableTriggers, s => s.m_pickableStats)) return false;
        if (!AreDictTriggersMet(diffIndex, achievement.m_foodEatenTriggers, s => s.m_foodEatenStats)) return false;
        if (!AreDictTriggersMet(diffIndex, achievement.m_piecePlacedTriggers, s => s.m_piecesPlacedStats)) return false;

        foreach (Achievement other in achievement.m_otherAchievementTriggers)
        {
            if (other != null && !other.m_unlocked)
                return false;
        }

        return true;
    }

    public static float GetEffectivePlayerStat(int diffIndex, PlayerStatType stat)
    {
        PlayerProfile? current = Game.instance != null ? Game.instance.GetPlayerProfile() : null;
        float currentVal = 0f;

        if (current != null &&
            diffIndex >= 0 && diffIndex < current.m_playerStats.Length &&
            IncludeCurrentProfileInScope(current))
        {
            current.m_playerStats[diffIndex].m_stats.TryGetValue(stat, out currentVal);
        }

        if (Plugin.ProgressScopeConfig.Value != ProgressScope.Global ||
            _otherCharactersStats == null ||
            diffIndex < 0 || diffIndex >= _otherCharactersStats.Length)
        {
            return currentVal;
        }

        _otherCharactersStats[diffIndex].m_stats.TryGetValue(stat, out float otherVal);
        return RecordStatTypes.Contains(stat)
            ? Mathf.Max(currentVal, otherVal)
            : currentVal + otherVal;
    }

    public static float GetEffectiveEnemyStat(int diffIndex, KillModifiers modifier, string statKey)
    {
        TryGetEffectiveEnemyStat(diffIndex, modifier, statKey, out float value);
        return value;
    }

    public static bool TryGetEffectiveEnemyStat(int diffIndex, KillModifiers modifier, string statKey, out float value)
    {
        int modIndex = (int)modifier;
        return TryGetEffectiveDictStat(diffIndex, s => s.m_enemyStats[modIndex], statKey, out value);
    }

    public static float GetEffectiveDictStat(
        int diffIndex,
        Func<PlayerProfile.PlayerStats, Dictionary<string, float>> selector,
        string statKey)
    {
        TryGetEffectiveDictStat(diffIndex, selector, statKey, out float value);
        return value;
    }

    public static bool TryGetEffectiveDictStat(
        int diffIndex,
        Func<PlayerProfile.PlayerStats, Dictionary<string, float>> selector,
        string statKey,
        out float value)
    {
        PlayerProfile? current = Game.instance != null ? Game.instance.GetPlayerProfile() : null;
        bool hasCurrent = false;
        float currentVal = 0f;

        if (current != null &&
            diffIndex >= 0 && diffIndex < current.m_playerStats.Length &&
            IncludeCurrentProfileInScope(current))
        {
            hasCurrent = selector(current.m_playerStats[diffIndex]).TryGetValue(statKey, out currentVal);
        }

        if (Plugin.ProgressScopeConfig.Value != ProgressScope.Global ||
            _otherCharactersStats == null ||
            diffIndex < 0 || diffIndex >= _otherCharactersStats.Length)
        {
            value = currentVal;
            return hasCurrent;
        }

        bool hasOther = selector(_otherCharactersStats[diffIndex]).TryGetValue(statKey, out float otherVal);
        value = currentVal + otherVal;
        return hasCurrent || hasOther;
    }

    public static bool IsItemCraftTarget(string sharedName) => CraftTargets.Contains(sharedName);
    public static bool IsFoodCookTarget(string sharedName) => CookTargets.Contains(sharedName);
    public static bool IsFoodEatenTarget(string sharedName) => FoodEatenTargets.Contains(sharedName);
    public static bool IsBuildPieceTarget(string pieceName) => BuildPieceTargets.Contains(pieceName);
    public static bool IsEnemyTarget(string enemyName) => EnemyTargets.Contains(enemyName);
    public static bool IsTrophyTarget(string trophySharedName) => TrophyTargets.Contains(trophySharedName);
    public static bool IsFishTarget(string fishSharedName) => FishTargets.Contains(fishSharedName);
    public static bool TryGetCookedVariant(string rawSharedName, out string cookedSharedName) =>
        RawToCookedMap.TryGetValue(rawSharedName, out cookedSharedName);

    public static bool IsRecipeMissingForAchievement(Recipe recipe, out bool isFoodRecipe)
    {
        isFoodRecipe = false;
        string? sharedName = recipe?.m_item?.m_itemData?.m_shared?.m_name;
        if (string.IsNullOrEmpty(sharedName))
        {
            return false;
        }

        bool isDirectCook = IsFoodCookTarget(sharedName!);
        bool hasCookedVariant = TryGetCookedVariant(sharedName!, out string cookedName) &&
                                IsFoodCookTarget(cookedName);
        isFoodRecipe = isDirectCook || hasCookedVariant;

        if (isDirectCook && !HasCraftedItem(sharedName!))
        {
            return true;
        }

        if (hasCookedVariant && !HasCraftedItem(cookedName))
        {
            return true;
        }

        if (IsItemCraftTarget(sharedName!) && !HasCraftedItem(sharedName!))
        {
            return true;
        }

        return false;
    }

    public static bool HasCraftedItem(string sharedName)
    {
        return GetEffectiveDictStat((int)DifficultyRequirement.Any, s => s.m_itemCraftStats, sharedName) >= 1f;
    }

    public static bool HasEatenFood(string sharedName)
    {
        return GetEffectiveDictStat((int)DifficultyRequirement.Any, s => s.m_foodEatenStats, sharedName) >= 1f;
    }

    public static bool HasBuiltPiece(string pieceName)
    {
        return GetEffectiveDictStat((int)DifficultyRequirement.Any, s => s.m_piecesPlacedStats, pieceName) >= 1f;
    }

    public static bool IsPieceMissingForAchievement(Piece? piece)
    {
        return piece != null &&
               !string.IsNullOrEmpty(piece.m_name) &&
               IsBuildPieceTarget(piece.m_name) &&
               !HasBuiltPiece(piece.m_name);
    }

    public static bool HasKilledEnemy(string enemyName)
    {
        DifficultyRequirement currentDiff = Achievements.GetCurrentAchievementDifficulty();
        int diffIndex = currentDiff >= DifficultyRequirement.Hard
            ? (int)DifficultyRequirement.Hard
            : (int)DifficultyRequirement.Any;
        return GetEffectiveEnemyStat(diffIndex, 0, enemyName) >= 1f;
    }

    public static bool HasPickedUpTrophy(string trophySharedName)
    {
        return GetEffectiveDictStat((int)DifficultyRequirement.Any, s => s.m_itemPickupStats, trophySharedName) >= 1f;
    }

    public static bool HasCaughtFish(string fishSharedName)
    {
        return GetEffectiveDictStat((int)DifficultyRequirement.Any, s => s.m_pickableStats, fishSharedName) >= 1f;
    }

    public static bool IsEnemyMissingForAchievement(Character? character)
    {
        return character != null &&
               !character.IsPlayer() &&
               !character.IsBoss() &&
               !character.IsTamed() &&
               !string.IsNullOrEmpty(character.m_name) &&
               IsEnemyTarget(character.m_name) &&
               !HasKilledEnemy(character.m_name);
    }

    public static bool IsEnemyMissingTrophyForAchievement(Character? character)
    {
        if (character == null || character.IsPlayer() || character.IsBoss() || character.IsTamed())
        {
            return false;
        }

        return TryGetCharacterTrophy(character, out string trophySharedName) &&
               IsTrophyTarget(trophySharedName) &&
               !HasPickedUpTrophy(trophySharedName);
    }

    public static bool TryGetCharacterTrophy(Character character, out string trophySharedName)
    {
        trophySharedName = string.Empty;
        if (character == null || string.IsNullOrEmpty(character.m_name))
        {
            return false;
        }

        if (CharacterTrophyMap.TryGetValue(character.m_name, out string cached))
        {
            trophySharedName = cached;
            return !string.IsNullOrEmpty(cached);
        }

        string found = FindTrophyDropName(character.GetComponent<CharacterDrop>());
        CharacterTrophyMap[character.m_name] = found;
        trophySharedName = found;
        return !string.IsNullOrEmpty(found);
    }

    private static string FindTrophyDropName(CharacterDrop? dropComponent)
    {
        if (dropComponent == null || dropComponent.m_drops == null)
        {
            return string.Empty;
        }

        foreach (CharacterDrop.Drop drop in dropComponent.m_drops)
        {
            ItemDrop? itemDrop = drop.m_prefab != null ? drop.m_prefab.GetComponent<ItemDrop>() : null;
            ItemDrop.ItemData.SharedData? shared = itemDrop?.m_itemData?.m_shared;
            if (shared != null &&
                shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy &&
                !string.IsNullOrEmpty(shared.m_name))
            {
                return shared.m_name;
            }
        }

        return string.Empty;
    }

    private static bool IncludeCurrentProfileInScope(PlayerProfile current)
    {
        if (Plugin.ProgressScopeConfig.Value != ProgressScope.Global)
        {
            return true;
        }

        return !current.m_usedCheats || Plugin.CountCheatedCharacters.Value;
    }

    private static bool AreDictTriggersMet(
        int diffIndex,
        List<Achievement.DictStatRequirement> triggers,
        Func<PlayerProfile.PlayerStats, Dictionary<string, float>> selector)
    {
        foreach (Achievement.DictStatRequirement req in triggers)
        {
            bool hasVal = TryGetEffectiveDictStat(diffIndex, selector, req.m_stat, out float current);
            if (!EvaluateOperator(hasVal, current, req.m_amount, req.m_operator))
                return false;
        }

        return true;
    }

    private static bool EvaluateOperator(bool hasValue, float current, float target, RequirementOperator op)
    {
        if (!hasValue)
        {
            return false;
        }

        return op switch
        {
            RequirementOperator.AboveEquals => current >= target,
            RequirementOperator.BelowEquals => current <= target,
            RequirementOperator.Equals => current == target,
            RequirementOperator.NotEquals => current != target,
            _ => false
        };
    }

    private static void AccumulateDictTriggers(
        int diffIndex,
        List<Achievement.DictStatRequirement> triggers,
        Func<PlayerProfile.PlayerStats, Dictionary<string, float>> selector,
        ref float totalRatio,
        ref int totalCount)
    {
        foreach (Achievement.DictStatRequirement req in triggers)
        {
            float current = GetEffectiveDictStat(diffIndex, selector, req.m_stat);
            float target = Mathf.Max(1f, req.m_amount);
            totalRatio += Mathf.Clamp01(current / target);
            totalCount++;
        }
    }

    private static void RebuildOtherCharactersCache()
    {
        PlayerProfile? current = Game.instance != null ? Game.instance.GetPlayerProfile() : null;
        if (current == null)
        {
            _otherCharactersStats = null;
            return;
        }

        int slotCount = current.m_playerStats.Length;
        PlayerProfile.PlayerStats[] combined = new PlayerProfile.PlayerStats[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            combined[i] = new PlayerProfile.PlayerStats();
        }

        string currentFilename = current.GetFilename() ?? string.Empty;
        long currentPlayerId = current.GetPlayerID();
        int mergedCount = 0;

        try
        {
            List<PlayerProfile> profiles = SaveSystem.GetAllPlayerProfiles();
            foreach (PlayerProfile other in profiles)
            {
                if (other == null) continue;

                string otherFilename = other.GetFilename() ?? string.Empty;
                if ((!string.IsNullOrEmpty(currentFilename) &&
                     otherFilename.Equals(currentFilename, StringComparison.OrdinalIgnoreCase)) ||
                    (currentPlayerId != 0L && other.GetPlayerID() == currentPlayerId))
                {
                    continue;
                }

                if (other.m_usedCheats && !Plugin.CountCheatedCharacters.Value)
                {
                    continue;
                }

                int maxSlots = Mathf.Min(slotCount, other.m_playerStats.Length);
                for (int i = 0; i < maxSlots; i++)
                {
                    MergeStatsSlot(combined[i], other.m_playerStats[i]);
                }

                mergedCount++;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Failed to aggregate other character profiles: {ex.Message}");
        }

        _otherCharactersStats = combined;
        Plugin.Log.LogInfo($"Global progress cache built from {mergedCount} other character(s).");
    }

    private static void MergeStatsSlot(PlayerProfile.PlayerStats target, PlayerProfile.PlayerStats source)
    {
        if (source == null)
        {
            return;
        }

        foreach (KeyValuePair<PlayerStatType, float> kvp in source.m_stats)
        {
            if (kvp.Value == 0f) continue;

            target.m_stats.TryGetValue(kvp.Key, out float existing);
            target.m_stats[kvp.Key] = RecordStatTypes.Contains(kvp.Key)
                ? Mathf.Max(existing, kvp.Value)
                : existing + kvp.Value;
        }

        MergeStringDict(target.m_knownWorlds, source.m_knownWorlds);
        MergeStringDict(target.m_knownWorldKeys, source.m_knownWorldKeys);
        MergeStringDict(target.m_knownCommands, source.m_knownCommands);
        MergeStringDict(target.m_itemPickupStats, source.m_itemPickupStats);
        MergeStringDict(target.m_itemCraftStats, source.m_itemCraftStats);
        MergeStringDict(target.m_pickableStats, source.m_pickableStats);
        MergeStringDict(target.m_foodEatenStats, source.m_foodEatenStats);
        MergeStringDict(target.m_piecesPlacedStats, source.m_piecesPlacedStats);

        int enemySlots = Mathf.Min(target.m_enemyStats.Length, source.m_enemyStats.Length);
        for (int i = 0; i < enemySlots; i++)
        {
            MergeStringDict(target.m_enemyStats[i], source.m_enemyStats[i]);
        }
    }

    private static void MergeStringDict(Dictionary<string, float> target, Dictionary<string, float> source)
    {
        if (source == null)
        {
            return;
        }

        foreach (KeyValuePair<string, float> kvp in source)
        {
            if (kvp.Value == 0f) continue;

            target.TryGetValue(kvp.Key, out float existing);
            target[kvp.Key] = existing + kvp.Value;
        }
    }

    private static void ReevaluateAllAchievements()
    {
        if (Achievements.m_instance == null)
        {
            return;
        }

        List<Achievement> allAchievements = new();
        foreach (AchievementList list in Achievements.m_instance.m_achievementLists)
        {
            foreach (Achievement ach in list.m_achievements)
            {
                if (ach != null)
                {
                    allAchievements.Add(ach);
                }
            }
        }

        foreach (Achievement ach in allAchievements)
        {
            ach.m_unlocked = false;
        }

        if (Plugin.SyncSteamUnlocks.Value)
        {
            foreach (Achievement ach in allAchievements)
            {
                if (IsUnlockedOnSteam(ach))
                {
                    ach.m_unlocked = true;
                }
            }
        }

        bool anyNewlyUnlocked = false;
        // Pass 1: standard achievements
        foreach (Achievement ach in allAchievements)
        {
            if (ach.m_otherAchievementTriggers.Count > 0 || ach.m_unlocked)
            {
                continue;
            }

            if (ach.m_lenientBuildAchievement)
            {
                Piece.CheckLenientBuildAchUnlocked(ach, false, false);
            }

            if (ach.CheckUnlocked())
            {
                Achievements.AchievementEvent(ach, false, false);
                anyNewlyUnlocked = true;
            }
        }

        // Pass 2: meta-achievements (Child of Odin, Kings and Queens of Valheim)
        foreach (Achievement ach in allAchievements)
        {
            if (ach.m_otherAchievementTriggers.Count == 0 || ach.m_unlocked)
            {
                continue;
            }

            if (ach.CheckUnlocked())
            {
                Achievements.AchievementEvent(ach, false, false);
                anyNewlyUnlocked = true;
            }
        }

        if (anyNewlyUnlocked)
        {
            PlatformManager.DistributionPlatform?.AchievementManager?.Synchronize();
        }
    }

    private static bool IsUnlockedOnSteam(Achievement achievement)
    {
        try
        {
            string platformId = Achievements.GetAchievementInternalPlatformId(achievement);
            if (!string.IsNullOrEmpty(platformId) &&
                SteamUserStats.GetAchievement(platformId, out bool achieved))
            {
                return achieved;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogDebug($"Steam unlock check skipped for {achievement.m_id}: {ex.Message}");
        }

        return false;
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
        FishTargets.Clear();
        RawToCookedMap.Clear();
        CharacterTrophyMap.Clear();

        if (ZNetScene.instance != null)
        {
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
            {
                if (prefab == null) continue;

                Character character = prefab.GetComponent<Character>();
                if (character != null && !string.IsNullOrEmpty(character.m_name) && !CharacterTrophyMap.ContainsKey(character.m_name))
                {
                    string trophyName = FindTrophyDropName(prefab.GetComponent<CharacterDrop>());
                    if (!string.IsNullOrEmpty(trophyName))
                    {
                        CharacterTrophyMap[character.m_name] = trophyName;
                    }
                }

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
                    case "GrindFish":
                        foreach (var req in ach.m_pickableTriggers)
                            if (!string.IsNullOrEmpty(req.m_stat)) FishTargets.Add(req.m_stat);
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
                Plugin.Log.LogDebug(
                    $"[Ach #{count:00}] id={ach.m_id} | name=\"{locName}\" | secret={ach.m_isSecret} | diff={ach.m_difficultyRequirement} | mode={mode}");
            }
        }
    }
}
