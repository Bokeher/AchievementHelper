using HarmonyLib;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(AchievementsGui))]
internal static class AchievementsGuiPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(AchievementsGui.OnOpenAchievementDetails))]
    private static void OnOpenAchievementDetails_Postfix(AchievementsGui __instance, Achievement achievement, bool clickable)
    {
        if (!Plugin.ModEnabled.Value || !clickable)
        {
            return;
        }

        // TODO: Replace "ACHIEVEMENT DETAILS" header with localized achievement.m_name
        // TODO: Apply show mode (all / revealable / hidden) and persistent Reveal button
    }
}
