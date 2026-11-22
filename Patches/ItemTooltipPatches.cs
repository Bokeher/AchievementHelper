using System.Text;
using AchievementHelper.Services;
using HarmonyLib;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(ItemDrop.ItemData))]
internal static class ItemTooltipPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(
        nameof(ItemDrop.ItemData.GetTooltip),
        typeof(ItemDrop.ItemData),
        typeof(int),
        typeof(bool),
        typeof(float),
        typeof(int),
        typeof(bool))]
    private static void GetTooltip_Postfix(
        ItemDrop.ItemData item,
        bool appending,
        ref string __result)
    {
        if (appending || !Plugin.ModEnabled.Value || Player.m_localPlayer == null || item?.m_shared == null)
        {
            return;
        }

        string sharedName = item.m_shared.m_name;
        if (string.IsNullOrEmpty(sharedName))
        {
            return;
        }

        bool isCookTarget = AchievementService.IsFoodCookTarget(sharedName);
        bool missingCook = Plugin.ShowNotCookedTooltip.Value &&
                           isCookTarget &&
                           !AchievementService.HasCraftedItem(sharedName);

        bool missingCraft = Plugin.ShowNotCraftedTooltip.Value &&
                            !isCookTarget &&
                            AchievementService.IsItemCraftTarget(sharedName) &&
                            !AchievementService.HasCraftedItem(sharedName);

        bool missingEat = Plugin.ShowNotEatenTooltip.Value &&
                          AchievementService.IsFoodEatenTarget(sharedName) &&
                          !AchievementService.HasEatenFood(sharedName);

        bool missingRoastedCook = false;
        bool missingRoastedEat = false;
        if ((Plugin.ShowRawRoastedCookedTooltip.Value || Plugin.ShowRawRoastedEatenTooltip.Value) &&
            AchievementService.TryGetCookedVariant(sharedName, out string cookedName))
        {
            missingRoastedCook = Plugin.ShowRawRoastedCookedTooltip.Value &&
                                 AchievementService.IsFoodCookTarget(cookedName) &&
                                 !AchievementService.HasCraftedItem(cookedName);
            missingRoastedEat = Plugin.ShowRawRoastedEatenTooltip.Value &&
                                AchievementService.IsFoodEatenTarget(cookedName) &&
                                !AchievementService.HasEatenFood(cookedName);
        }

        if (!missingCook && !missingCraft && !missingEat && !missingRoastedCook && !missingRoastedEat)
        {
            return;
        }

        StringBuilder sb = new StringBuilder(__result);
        sb.Append('\n');

        if (missingCook)
        {
            sb.Append("\n<color=#FFD166>Not cooked for achievement</color>");
        }
        else if (missingCraft)
        {
            sb.Append("\n<color=#FFD166>Not crafted for achievement</color>");
        }

        if (missingEat)
        {
            sb.Append("\n<color=#FF9F43>Not eaten for achievement</color>");
        }

        if (missingRoastedCook)
        {
            sb.Append("\n<color=#FFD166>Not cooked for achievement</color>");
        }

        if (missingRoastedEat)
        {
            sb.Append("\n<color=#FF9F43>Cooked variant not eaten for achievement</color>");
        }

        __result = sb.ToString();
    }
}
