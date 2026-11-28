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
    public const string ModVersion = "1.0.0";

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
    internal static ConfigEntry<bool> ShowMissingTrophyTooltip = null!;
    internal static ConfigEntry<bool> ShowUncaughtFishTooltip = null!;

    // 5 - Badges
    internal static ConfigEntry<bool> ShowInventoryItemBadges = null!;
    internal static ConfigEntry<BadgeCorner> InventoryBadgeCorner = null!;
    internal static ConfigEntry<bool> ShowNotCraftedBadges = null!;
    internal static ConfigEntry<bool> ShowNotCookedBadges = null!;
    internal static ConfigEntry<bool> ShowUnbuiltPieceBadges = null!;

    // 6 - Crafting & Cooking
    internal static ConfigEntry<bool> EnableNotCraftedFilter = null!;
    internal static ConfigEntry<bool> EnableNotCookedFilter = null!;
    internal static ConfigEntry<bool> ResetCraftingFilterOnClose = null!;

    // 7 - Building
    internal static ConfigEntry<bool> ShowUnbuiltPieceDescription = null!;
    internal static ConfigEntry<bool> EnableNotBuiltFilter = null!;
    internal static ConfigEntry<bool> ResetBuildFilterOnClose = null!;

    // 8 - Enemies
    internal static ConfigEntry<bool> ShowUndefeatedEnemyIcon = null!;
    internal static ConfigEntry<bool> ShowMissingTrophyIcon = null!;

    // 9 - Saved State
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
        ShowMissingTrophyTooltip = Config.Bind(
            "4 - Item Tooltips",
            "Show Missing Trophy",
            true,
            "Show 'Not collected for achievement' on uncollected trophies. Achievement is counted when a trophy is added to your inventory.");
        ShowUncaughtFishTooltip = Config.Bind(
            "4 - Item Tooltips",
            "Show Uncaught Fish",
            true,
            "Show 'Not caught for achievement' on uncaught fish. Achievement is counted only when caught with a fishing rod (not from chests or ground pickups).");

        // [5 - Badges]
        ShowInventoryItemBadges = Config.Bind(
            "5 - Badges",
            "Show Inventory Badge",
            true,
            new ConfigDescription(
                "Show the corner '!' badge on items in your inventory and chests that are missing for achievements (crafting, cooking, eating, trophies, or fish).",
                null,
                new ConfigurationManagerAttributes { Order = 50 }));
        InventoryBadgeCorner = Config.Bind(
            "5 - Badges",
            "Inventory Badge Position",
            BadgeCorner.TopLeft,
            new ConfigDescription(
                "Corner of the inventory slot where the '!' badge is displayed.",
                null,
                new ConfigurationManagerAttributes { Order = 40 }));
        ShowNotCraftedBadges = Config.Bind(
            "5 - Badges",
            "Show Crafting Recipe Badge",
            true,
            new ConfigDescription(
                "Show the '!' badge on uncrafted achievement recipes at crafting stations.",
                null,
                new ConfigurationManagerAttributes { Order = 30 }));
        ShowNotCookedBadges = Config.Bind(
            "5 - Badges",
            "Show Cooking Recipe Badge",
            true,
            new ConfigDescription(
                "Show the '!' badge on uncooked achievement recipes at cooking stations.",
                null,
                new ConfigurationManagerAttributes { Order = 20 }));
        ShowUnbuiltPieceBadges = Config.Bind(
            "5 - Badges",
            "Show Building Piece Badge",
            true,
            new ConfigDescription(
                "Show the corner '!' badge on unbuilt achievement pieces in the build menu.",
                null,
                new ConfigurationManagerAttributes { Order = 10 }));

        // [6 - Crafting & Cooking]
        EnableNotCraftedFilter = Config.Bind(
            "6 - Crafting & Cooking",
            "Show Not Crafted Filter",
            true,
            "Show the 'Not Crafted (N)' toggle button at crafting stations.");
        EnableNotCookedFilter = Config.Bind(
            "6 - Crafting & Cooking",
            "Show Not Cooked Filter",
            true,
            "Show the 'Not Cooked (N)' toggle button at cooking stations.");
        ResetCraftingFilterOnClose = Config.Bind(
            "6 - Crafting & Cooking",
            "Reset Filter On Inventory Close",
            true,
            "Turn off the crafting/cooking filter automatically when closing the inventory.");

        // [7 - Building]
        ShowUnbuiltPieceDescription = Config.Bind(
            "7 - Building",
            "Show Unbuilt Description",
            true,
            "Show 'Not built for achievement' under the selected piece description.");
        EnableNotBuiltFilter = Config.Bind(
            "7 - Building",
            "Show Not Built Filter",
            true,
            "Show the 'Not Built (N)' toggle button in the build menu.");
        ResetBuildFilterOnClose = Config.Bind(
            "7 - Building",
            "Reset Filter On Menu Close",
            false,
            "Turn off the Not Built filter every time the build menu closes. When off, it stays active until you unequip the tool.");

        // [8 - Enemies]
        ShowUndefeatedEnemyIcon = Config.Bind(
            "8 - Enemies",
            "Show Undefeated Icon",
            true,
            "Show crossed-swords icon next to the health bar of undefeated achievement enemies.");
        ShowMissingTrophyIcon = Config.Bind(
            "8 - Enemies",
            "Show Missing Trophy Icon",
            true,
            "Show trophy cup icon next to the health bar of enemies whose trophy has not been collected.");

        // [9 - Saved State] (hidden in ConfigurationManager)
        ConfigurationManagerAttributes hiddenInConfigManager = new() { Browsable = false };
        MasterRevealAll = Config.Bind(
            "9 - Saved State",
            "Reveal All Active",
            false,
            new ConfigDescription(
                "Managed automatically by the in-game 'Reveal All / Hide All' button.",
                null,
                hiddenInConfigManager));
        RevealedAchievementIds = Config.Bind(
            "9 - Saved State",
            "Revealed Achievement IDs",
            "",
            new ConfigDescription(
                "Managed automatically by in-game 'Reveal / Hide' buttons and clicking '???' rows.",
                null,
                hiddenInConfigManager));

        ModEnabled.SettingChanged += (_, _) =>
        {
            if (!ModEnabled.Value)
            {
                InventoryGuiCraftingFilterPatches.ResetFilter();
                BuildingFilterState.ResetFilterPreservingSelection();
            }

            RefreshOpenAchievementsPanel();
            AchievementsGuiPatches.RefreshOpenDetailsIfVisible();
            InventoryGuiCraftingFilterPatches.RefreshCraftingPanelIfOpen();
            InventoryGridBadgePatches.RefreshOpenInventoryGrids();
            BuildUiFilterPatches.RefreshOpenBuildUi();
        };

        ProgressScopeConfig.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);
        CountCheatedCharacters.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);
        SyncSteamUnlocks.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);
        OnlyHideSpoilerAchievements.SettingChanged += (_, _) => RefreshOpenAchievementsPanel();
        ShowCompletionPercentages.SettingChanged += (_, _) => RefreshOpenAchievementsPanel();
        GroupDetailsByBiome.SettingChanged += (_, _) => AchievementsGuiPatches.RefreshOpenDetailsIfVisible();
        ShowHideCompletedFilter.SettingChanged += (_, _) => AchievementsGuiPatches.RefreshOpenDetailsIfVisible();

        ShowInventoryItemBadges.SettingChanged += (_, _) => InventoryGridBadgePatches.RefreshOpenInventoryGrids();
        InventoryBadgeCorner.SettingChanged += (_, _) => InventoryGridBadgePatches.RefreshOpenInventoryGrids();
        ShowNotCraftedBadges.SettingChanged += (_, _) => InventoryGuiCraftingFilterPatches.RefreshCraftingPanelIfOpen();
        ShowNotCookedBadges.SettingChanged += (_, _) => InventoryGuiCraftingFilterPatches.RefreshCraftingPanelIfOpen();
        ShowUnbuiltPieceBadges.SettingChanged += (_, _) => BuildUiFilterPatches.RefreshOpenBuildUi();

        EnableNotCraftedFilter.SettingChanged += (_, _) =>
        {
            if (!EnableNotCraftedFilter.Value)
            {
                InventoryGuiCraftingFilterPatches.ResetFilter();
            }
            InventoryGuiCraftingFilterPatches.RefreshCraftingPanelIfOpen();
        };
        EnableNotCookedFilter.SettingChanged += (_, _) =>
        {
            if (!EnableNotCookedFilter.Value)
            {
                InventoryGuiCraftingFilterPatches.ResetFilter();
            }
            InventoryGuiCraftingFilterPatches.RefreshCraftingPanelIfOpen();
        };
        ShowUnbuiltPieceDescription.SettingChanged += (_, _) => BuildUiFilterPatches.RefreshOpenBuildUi();
        EnableNotBuiltFilter.SettingChanged += (_, _) =>
        {
            if (!EnableNotBuiltFilter.Value)
            {
                BuildingFilterState.ResetFilterPreservingSelection();
            }
            BuildUiFilterPatches.RefreshOpenBuildUi();
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
        public int? Order;
    }
}
