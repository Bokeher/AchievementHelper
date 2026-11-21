using System.Reflection;
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

    internal static ConfigEntry<bool> ModEnabled = null!;
    internal static ConfigEntry<bool> OnlyHideSpoilerAchievements = null!;
    internal static ConfigEntry<bool> MasterRevealAll = null!;
    internal static ConfigEntry<string> RevealedAchievementIds = null!;

    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        ModEnabled = Config.Bind("General", "Enabled", true, "Enable or disable AchievementHelper.");
        OnlyHideSpoilerAchievements = Config.Bind("Achievements", "OnlyHideSpoilerAchievements", true, "Only keep spoiler and boss achievements concealed; show non-spoiler concealed achievements.");
        MasterRevealAll = Config.Bind("Achievements", "MasterRevealAll", false, "Reveal all hidden/spoiler achievements and conditions.");
        RevealedAchievementIds = Config.Bind("Achievements", "RevealedAchievementIds", "", "Comma-separated list of achievement IDs permanently revealed by the user.");

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGuid);
        Log.LogInfo($"{ModName} v{ModVersion} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}
