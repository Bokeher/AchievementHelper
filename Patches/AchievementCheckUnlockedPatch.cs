using AchievementHelper.Services;
using HarmonyLib;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(Achievement))]
internal static class AchievementCheckUnlockedPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Achievement.CheckUnlocked))]
    private static bool CheckUnlocked_Prefix(Achievement __instance, ref bool __result)
    {
        if (!Plugin.ModEnabled.Value || Plugin.ProgressScopeConfig.Value != ProgressScope.Global)
        {
            return true;
        }

        if (__instance.m_unlocked)
        {
            __result = true;
            return false;
        }

        bool met = AchievementService.EvaluateAchievementUnlocked(__instance);
        if (met)
        {
            __instance.m_unlocked = true;
        }

        __result = met;
        return false;
    }
}
