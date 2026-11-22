using System.Collections.Generic;
using AchievementHelper.Services;
using AchievementHelper.UI;
using HarmonyLib;
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
            (Plugin.EnableNotCraftedFilter.Value || Plugin.EnableNotCookedFilter.Value) &&
            __instance.InCraftTab())
        {
            __instance.UpdateCraftingPanel(false);
        }
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
