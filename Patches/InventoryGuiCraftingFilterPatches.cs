using System.Collections.Generic;
using AchievementHelper.Services;
using AchievementHelper.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(InventoryGui))]
internal static class InventoryGuiCraftingFilterPatches
{
    private const string FilterButtonName = "AchievementHelper_CraftFilterButton";

    private static FilterToggleButton? _filterButton;
    private static bool _filterActive;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.Hide))]
    private static void Hide_Postfix()
    {
        if (Plugin.ResetCraftingFilterOnClose.Value)
        {
            _filterActive = false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryGui.OnTabUpgradePressed))]
    private static void OnTabUpgradePressed_Prefix()
    {
        _filterActive = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.DoCrafting))]
    private static void DoCrafting_Postfix(InventoryGui __instance)
    {
        if (Plugin.ModEnabled.Value &&
            (Plugin.EnableNotCraftedFilter.Value ||
             Plugin.EnableNotCookedFilter.Value ||
             Plugin.ShowNotCraftedBadges.Value ||
             Plugin.ShowNotCookedBadges.Value) &&
            __instance.InCraftTab())
        {
            __instance.UpdateCraftingPanel(false);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.AddRecipeToList))]
    private static void AddRecipeToList_Postfix(InventoryGui __instance, Recipe recipe, ItemDrop.ItemData? item)
    {
        if (!Plugin.ModEnabled.Value ||
            !__instance.InCraftTab() ||
            item != null ||
            recipe == null ||
            __instance.m_availableRecipes.Count == 0)
        {
            return;
        }

        if (!AchievementService.IsRecipeMissingForAchievement(recipe, out bool isFood))
        {
            return;
        }

        bool showBadge = isFood
            ? Plugin.ShowNotCookedBadges.Value
            : Plugin.ShowNotCraftedBadges.Value;
        if (!showBadge)
        {
            return;
        }

        GameObject element = __instance.m_availableRecipes[__instance.m_availableRecipes.Count - 1].InterfaceElement;
        if (element == null)
        {
            return;
        }

        Transform? nameTransform = element.transform.Find("name");
        TMP_Text? refText = nameTransform?.GetComponent<TMP_Text>();
        if (refText != null)
        {
            Vector4 margin = refText.margin;
            if (margin.z < 24f)
            {
                refText.margin = new Vector4(margin.x, margin.y, 24f, margin.w);
            }
        }

        Transform? badge = element.transform.Find(BuildUiPieceButtonPatches.UnbuiltBadgeName);
        if (badge == null)
        {
            badge = BuildUiPieceButtonPatches.CreateUnbuiltBadge(element.transform, refText);
            if (badge is RectTransform rt)
            {
                rt.anchorMin = new Vector2(1f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.sizeDelta = new Vector2(18f, 18f);
                rt.anchoredPosition = new Vector2(-6f, 0f);
            }
        }

        badge.gameObject.SetActive(true);
        badge.SetAsLastSibling();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryGui.UpdateRecipeList))]
    private static void UpdateRecipeList_Prefix(InventoryGui __instance, ref List<Recipe> recipes)
    {
        if (!Plugin.ModEnabled.Value || !__instance.InCraftTab())
        {
            _filterButton?.SetVisible(false);
            return;
        }

        int totalCraftable = 0;
        int foodRecipeCount = 0;
        int missingCount = 0;
        List<Recipe>? filtered = _filterActive ? new List<Recipe>() : null;

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null || recipe.m_noCraftOnlyUpgrade)
            {
                continue;
            }

            totalCraftable++;
            bool missing = AchievementService.IsRecipeMissingForAchievement(recipe, out bool isFood);
            if (isFood)
            {
                foodRecipeCount++;
            }

            if (missing)
            {
                missingCount++;
                filtered?.Add(recipe);
            }
        }

        bool isCookingMode = foodRecipeCount > 0 && foodRecipeCount * 2 >= totalCraftable;
        bool filterEnabled = isCookingMode
            ? Plugin.EnableNotCookedFilter.Value
            : Plugin.EnableNotCraftedFilter.Value;

        if (!filterEnabled)
        {
            _filterButton?.SetVisible(false);
            return;
        }

        UpdateFilterButton(__instance, isCookingMode, missingCount);

        if (_filterActive && filtered != null)
        {
            recipes = filtered;
        }
    }

    private static void UpdateFilterButton(InventoryGui gui, bool isCookingMode, int missingCount)
    {
        FilterToggleButton? button = EnsureFilterButton(gui);
        if (button == null)
        {
            return;
        }

        string baseText = isCookingMode ? "Not Cooked" : "Not Crafted";
        button.SetVisible(true);
        button.SetState($"{baseText} ({missingCount})", _filterActive);
    }

    private static FilterToggleButton? EnsureFilterButton(InventoryGui gui)
    {
        if (_filterButton != null && _filterButton.IsAlive)
        {
            return _filterButton;
        }

        if (gui.m_crafting == null)
        {
            return null;
        }

        Button? template = gui.m_craftButton != null ? gui.m_craftButton : gui.m_tabUpgrade;
        _filterButton = FilterToggleButton.CreateOrAttach(
            gui.m_crafting,
            FilterButtonName,
            template,
            anchor: new Vector2(0f, 0f),
            pivot: new Vector2(0f, 1f),
            anchoredPosition: new Vector2(6f, -6f),
            size: new Vector2(185f, 32f),
            onClick: () =>
            {
                _filterActive = !_filterActive;
                gui.UpdateCraftingPanel(false);
            });

        return _filterButton;
    }
}
