using AchievementHelper.Services;
using HarmonyLib;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(Achievements))]
internal static class AchievementsInitPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Achievements.Initialize))]
    private static void Initialize_Postfix()
    {
        AchievementService.OnAchievementsInitialized();
    }
}
