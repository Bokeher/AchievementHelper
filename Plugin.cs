using System.Reflection;
using AchievementHelper.Services;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace AchievementHelper;

[BepInPlugin(ModGuid, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string ModGuid = "com.szymon.achievementhelper";
    public const string ModName = "AchievementHelper";
    public const string ModVersion = "0.1.0";

    internal static ManualLogSource Log = null!;
    internal static Plugin Instance = null!;

    // General
    internal static ConfigEntry<bool> ModEnabled = null!;
    internal static ConfigEntry<ProgressScope> ProgressScopeConfig = null!;
    internal static ConfigEntry<bool> CountCheatedCharacters = null!;
    internal static ConfigEntry<bool> SyncSteamUnlocks = null!;

    // Achievements Tab
    internal static ConfigEntry<bool> OnlyHideSpoilerAchievements = null!;
    internal static ConfigEntry<bool> MasterRevealAll = null!;
    internal static ConfigEntry<string> RevealedAchievementIds = null!;

    // Feature Toggles
    internal static ConfigEntry<bool> ShowItemTooltips = null!;
    internal static ConfigEntry<bool> ShowCraftingFilter = null!;
    internal static ConfigEntry<bool> ShowCookingStationPrompt = null!;
    internal static ConfigEntry<bool> ShowBuildIcons = null!;
    internal static ConfigEntry<bool> ShowEnemyIndicators = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        ModEnabled = Config.Bind("General", "Enabled", true, "Enable or disable AchievementHelper.");
        ProgressScopeConfig = Config.Bind(
            "General",
            "ProgressScope",
            ProgressScope.Character,
            "Character: track progress for the current character only. Global: aggregate progress across all characters in memory (does not modify save files).");
        CountCheatedCharacters = Config.Bind(
            "General",
            "CountCheatedCharacters",
            false,
            "When ProgressScope is Global, include characters marked as having used cheats.");
        SyncSteamUnlocks = Config.Bind(
            "General",
            "SyncSteamUnlocks",
            false,
            "Mark achievements already completed on Steam as unlocked in-game.");

        OnlyHideSpoilerAchievements = Config.Bind(
            "Achievements",
            "OnlyHideSpoilerAchievements",
            true,
            "Only keep spoiler and boss achievements concealed; show non-spoiler concealed achievements.");
        MasterRevealAll = Config.Bind(
            "Achievements",
            "MasterRevealAll",
            false,
            "Reveal all hidden/spoiler achievements and conditions.");
        RevealedAchievementIds = Config.Bind(
            "Achievements",
            "RevealedAchievementIds",
            "",
            "Comma-separated list of achievement IDs permanently revealed by the user.");

        ShowItemTooltips = Config.Bind("Features", "ShowItemTooltips", true, "Show achievement status on item and raw food tooltips.");
        ShowCraftingFilter = Config.Bind("Features", "ShowCraftingFilter", true, "Show not-crafted / not-cooked filter in crafting and cooking menus.");
        ShowCookingStationPrompt = Config.Bind("Features", "ShowCookingStationPrompt", true, "Show missing roasted food indicator on cooking station hover prompts.");
        ShowBuildIcons = Config.Bind("Features", "ShowBuildIcons", true, "Show built status icons on building pieces.");
        ShowEnemyIndicators = Config.Bind("Features", "ShowEnemyIndicators", true, "Show undefeated and missing trophy icons on enemy health bars.");

        ProgressScopeConfig.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);
        CountCheatedCharacters.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);
        SyncSteamUnlocks.SettingChanged += (_, _) => AchievementService.RefreshProgressScope(recheckUnlocks: true);

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGuid);
        Log.LogInfo($"{ModName} v{ModVersion} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}
