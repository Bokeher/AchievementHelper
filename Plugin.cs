using System.Reflection;
using AchievementHelper.Patches;
using AchievementHelper.Services;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace AchievementHelper;

[BepInPlugin(ModGuid, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string ModGuid = "com.bokeher.achievementhelper";
    public const string ModName = "AchievementHelper";
    public const string ModVersion = "0.1.0";

    internal static ManualLogSource Log = null!;
    internal static Plugin Instance = null!;

    // 1 - General
    internal static ConfigEntry<bool> ModEnabled = null!;

    // 2 - Progression
    internal static ConfigEntry<ProgressScope> ProgressScopeConfig = null!;
    internal static ConfigEntry<bool> CountCheatedCharacters = null!;
    internal static ConfigEntry<bool> SyncSteamUnlocks = null!;

    // 3 - Achievements Tab
    internal static ConfigEntry<bool> OnlyHideSpoilerAchievements = null!;
    internal static ConfigEntry<bool> ShowCompletionPercentages = null!;
    internal static ConfigEntry<bool> GroupDetailsByBiome = null!;
    internal static ConfigEntry<bool> ShowHideCompletedFilter = null!;

    // 4 - Item Tooltips
    internal static ConfigEntry<bool> ShowNotCraftedTooltip = null!;
    internal static ConfigEntry<bool> ShowNotCookedTooltip = null!;
    internal static ConfigEntry<bool> ShowNotEatenTooltip = null!;
    internal static ConfigEntry<bool> ShowRawRoastedCookedTooltip = null!;
    internal static ConfigEntry<bool> ShowRawRoastedEatenTooltip = null!;
    internal static ConfigEntry<bool> ShowInventoryItemBadges = null!;

    // 5 - Crafting & Cooking
    internal static ConfigEntry<bool> ShowNotCraftedBadges = null!;
    internal static ConfigEntry<bool> ShowNotCookedBadges = null!;
    internal static ConfigEntry<bool> EnableNotCraftedFilter = null!;
    internal static ConfigEntry<bool> EnableNotCookedFilter = null!;
    internal static ConfigEntry<bool> ResetCraftingFilterOnClose = null!;

    // 6 - Building
    internal static ConfigEntry<bool> ShowUnbuiltPieceBadges = null!;
    internal static ConfigEntry<bool> ShowUnbuiltPieceDescription = null!;
    internal static ConfigEntry<bool> EnableNotBuiltFilter = null!;
    internal static ConfigEntry<bool> ResetBuildFilterOnClose = null!;

    // 7 - Enemies
    internal static ConfigEntry<bool> ShowUndefeatedEnemyIcon = null!;
    internal static ConfigEntry<bool> ShowMissingTrophyIcon = null!;

    // 8 - Saved State
    internal static ConfigEntry<bool> MasterRevealAll = null!;
    internal static ConfigEntry<string> RevealedAchievementIds = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        // [1 - General]
        ModEnabled = Config.Bind(
            "1 - General",
            "Enabled",
            true,
            "Enable or disable the entire mod.");

        // [2 - Progression]
        ProgressScopeConfig = Config.Bind(
            "2 - Progression",
            "Progress Mode",
            ProgressScope.Character,
            "Character: track progress for the current character only. Global: aggregate progress across all characters in memory (read-only; never modifies save files).");
        CountCheatedCharacters = Config.Bind(
            "2 - Progression",
            "Include Cheated Characters In Global Mode",
            false,
            "Include characters that have used cheats when calculating Global progress.");
        SyncSteamUnlocks = Config.Bind(
            "2 - Progression",
            "Import Steam Unlocks",
            false,
            "Also show achievements already unlocked on your Steam account as completed, even if the current character has not earned them yet. When off, only in-game progress is used.");

        // [3 - Achievements Tab]
        OnlyHideSpoilerAchievements = Config.Bind(
            "3 - Achievements Tab",
            "Reveal Non-Spoiler Achievements",
            true,
            "Show non-spoiler concealed achievements while keeping bosses and story secrets hidden. When off, uses default game hiding for all concealed achievements.");
        ShowCompletionPercentages = Config.Bind(
            "3 - Achievements Tab",
            "Show Progress Percentage",
            true,
            "Show completion percentage (%) centered on locked achievement tiles.");
        GroupDetailsByBiome = Config.Bind(
            "3 - Achievements Tab",
            "Group Details By Biome",
            true,
            "Group supported achievement detail lists (such as enemies and craftable items) by biome progression.");
        ShowHideCompletedFilter = Config.Bind(
            "3 - Achievements Tab",
            "Show Hide Completed Button",
            true,
            "Show the 'Hide Completed' toggle button in multi-entry achievement detail windows.");

        // [4 - Item Tooltips]
        ShowNotCraftedTooltip = Config.Bind(
            "4 - Item Tooltips",
            "Show Not Crafted",
            true,
            "Show 'Not crafted for achievement' on craftable items.");
        ShowNotCookedTooltip = Config.Bind(
            "4 - Item Tooltips",
            "Show Not Cooked",
            true,
            "Show 'Not cooked for achievement' on cookable dishes.");
        ShowNotEatenTooltip = Config.Bind(
            "4 - Item Tooltips",
            "Show Not Eaten",
            true,
            "Show 'Not eaten for achievement' on edible food.");
        ShowRawRoastedCookedTooltip = Config.Bind(
            "4 - Item Tooltips",
            "Show Raw Item Not Cooked",
            true,
            "Show 'Not cooked for achievement' on raw cookable items.");
        ShowRawRoastedEatenTooltip = Config.Bind(
            "4 - Item Tooltips",
            "Show Raw Item Not Eaten",
            true,
            "Show 'Cooked variant not eaten for achievement' on raw cookable items.");
        ShowInventoryItemBadges = Config.Bind(
            "4 - Item Tooltips",
            "Show Inventory Item Badge",
            true,
            "Show the corner '!' badge on items in your inventory and chests that are not yet crafted, cooked, or eaten for achievements.");

        // [5 - Crafting & Cooking]
        ShowNotCraftedBadges = Config.Bind(
            "5 - Crafting & Cooking",
            "Show Not Crafted Badge",
            true,
            "Show the corner '!' badge on uncrafted achievement recipes at crafting stations.");
        ShowNotCookedBadges = Config.Bind(
            "5 - Crafting & Cooking",
            "Show Not Cooked Badge",
            true,
            "Show the corner '!' badge on uncooked achievement recipes at cooking stations.");
        EnableNotCraftedFilter = Config.Bind(
            "5 - Crafting & Cooking",
            "Show Not Crafted Filter",
            true,
            "Show the 'Not Crafted (N)' toggle button at crafting stations.");
        EnableNotCookedFilter = Config.Bind(
            "5 - Crafting & Cooking",
            "Show Not Cooked Filter",
            true,
            "Show the 'Not Cooked (N)' toggle button at cooking stations.");
        ResetCraftingFilterOnClose = Config.Bind(
            "5 - Crafting & Cooking",
            "Reset Filter On Inventory Close",
            true,
            "Turn off the crafting/cooking filter automatically when closing the inventory.");

        // [6 - Building]
        ShowUnbuiltPieceBadges = Config.Bind(
            "6 - Building",
            "Show Unbuilt Piece Badge",
            true,
            "Show the corner '!' badge on unbuilt achievement pieces in the build menu.");
        ShowUnbuiltPieceDescription = Config.Bind(
            "6 - Building",
            "Show Unbuilt Description",
            true,
            "Show 'Not built for achievement' under the selected piece description.");
        EnableNotBuiltFilter = Config.Bind(
            "6 - Building",
            "Show Not Built Filter",
            true,
            "Show the 'Not Built (N)' toggle button in the build menu.");
        ResetBuildFilterOnClose = Config.Bind(
            "6 - Building",
            "Reset Filter On Menu Close",
            false,
            "Turn off the Not Built filter every time the build menu closes. When off, it stays active until you unequip the tool.");

        // [7 - Enemies]
        ShowUndefeatedEnemyIcon = Config.Bind(
            "7 - Enemies",
            "Show Undefeated Icon",
            true,
            "Show crossed-swords icon next to the health bar of undefeated achievement enemies.");
        ShowMissingTrophyIcon = Config.Bind(
            "7 - Enemies",
            "Show Missing Trophy Icon",
            true,
            "Show trophy cup icon next to the health bar of enemies whose trophy has not been collected.");

        // [8 - Saved State] (hidden in ConfigurationManager)
        ConfigurationManagerAttributes hiddenInConfigManager = new() { Browsable = false };
        MasterRevealAll = Config.Bind(
            "8 - Saved State",
            "Reveal All Active",
            false,
            new ConfigDescription(
                "Managed automatically by the in-game 'Reveal All / Hide All' button.",
                null,
                hiddenInConfigManager));
        RevealedAchievementIds = Config.Bind(
            "8 - Saved State",
            "Revealed Achievement IDs",
            "",
            new ConfigDescription(
                "Managed automatically by in-game 'Reveal / Hide' buttons and clicking '???' rows.",
                null,
                hiddenInConfigManager));

        ProgressScopeConfig.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);
        CountCheatedCharacters.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);
        SyncSteamUnlocks.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);
        OnlyHideSpoilerAchievements.SettingChanged += (_, _) => RefreshOpenAchievementsPanel();
        ShowCompletionPercentages.SettingChanged += (_, _) => RefreshOpenAchievementsPanel();
        EnableNotBuiltFilter.SettingChanged += (_, _) =>
        {
            if (!EnableNotBuiltFilter.Value)
            {
                BuildingFilterState.ResetFilterPreservingSelection();
            }
        };

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGuid);
        Log.LogInfo($"{ModName} v{ModVersion} loaded.");
    }

    private static void RefreshOpenAchievementsPanel()
    {
        if (InventoryGui.instance != null &&
            InventoryGui.instance.m_achievementsPanel != null &&
            InventoryGui.instance.m_achievementsPanel.gameObject.activeInHierarchy)
        {
            InventoryGui.instance.UpdateAchievementsList();
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    private sealed class ConfigurationManagerAttributes
    {
        public bool? Browsable;
    }
}
