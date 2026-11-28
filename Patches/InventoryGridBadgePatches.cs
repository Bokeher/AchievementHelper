using AchievementHelper.Services;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(InventoryGrid))]
internal static class InventoryGridBadgePatches
{
    private const string InventoryBadgeName = "AchievementHelper_InventoryBadge";

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGrid.UpdateGui))]
    private static void UpdateGui_Postfix(InventoryGrid __instance)
    {
        if (__instance == null || __instance.m_elements == null)
        {
            return;
        }

        Inventory? inventory = __instance.m_inventory;
        bool enabled = Plugin.ModEnabled.Value &&
                       Plugin.ShowInventoryItemBadges.Value &&
                       Player.m_localPlayer != null &&
                       inventory != null;

        int width = enabled && inventory != null ? inventory.GetWidth() : 0;

        for (int index = 0; index < __instance.m_elements.Count; index++)
        {
            InventoryElement element = __instance.m_elements[index];
            if (element == null)
            {
                continue;
            }

            bool showBadge = false;
            if (enabled && inventory != null && element.m_used && width > 0)
            {
                int x = index % width;
                int y = index / width;
                ItemDrop.ItemData? item = inventory.GetItemAt(x, y);
                showBadge = IsItemMissingForAchievement(item);
            }

            Transform? badge = element.transform.Find(InventoryBadgeName);
            if (!showBadge)
            {
                if (badge != null)
                {
                    badge.gameObject.SetActive(false);
                }
                continue;
            }

            if (badge == null)
            {
                TMP_Text? refText = element.m_amount != null ? element.m_amount : element.m_quality;
                badge = BuildUiPieceButtonPatches.CreateUnbuiltBadge(element.transform, refText);
                badge.name = InventoryBadgeName;
            }

            if (badge is RectTransform rt)
            {
                ApplyCorner(rt, Plugin.InventoryBadgeCorner.Value);
            }

            badge.gameObject.SetActive(true);
            badge.SetAsLastSibling();
        }
    }

    private static void ApplyCorner(RectTransform rt, BadgeCorner corner)
    {
        rt.sizeDelta = new Vector2(15f, 15f);
        switch (corner)
        {
            case BadgeCorner.TopRight:
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-2f, -2f);
                break;
            case BadgeCorner.BottomLeft:
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2(2f, 5f);
                break;
            case BadgeCorner.BottomRight:
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-2f, 5f);
                break;
            default:
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(2f, -2f);
                break;
        }
    }

    private static bool IsItemMissingForAchievement(ItemDrop.ItemData? item)
    {
        string? sharedName = item?.m_shared?.m_name;
        if (string.IsNullOrEmpty(sharedName))
        {
            return false;
        }

        if (AchievementService.IsFoodEatenTarget(sharedName!) && !AchievementService.HasEatenFood(sharedName!))
        {
            return true;
        }

        if (AchievementService.IsFoodCookTarget(sharedName!) && !AchievementService.HasCraftedItem(sharedName!))
        {
            return true;
        }

        if (AchievementService.IsItemCraftTarget(sharedName!) && !AchievementService.HasCraftedItem(sharedName!))
        {
            return true;
        }

        if (AchievementService.TryGetCookedVariant(sharedName!, out string cookedName))
        {
            if ((AchievementService.IsFoodCookTarget(cookedName) && !AchievementService.HasCraftedItem(cookedName)) ||
                (AchievementService.IsFoodEatenTarget(cookedName) && !AchievementService.HasEatenFood(cookedName)))
            {
                return true;
            }
        }

        return false;
    }
}
