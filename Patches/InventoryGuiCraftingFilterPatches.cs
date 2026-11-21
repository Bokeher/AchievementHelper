using System.Collections.Generic;
using AchievementHelper.Services;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(InventoryGui))]
internal static class InventoryGuiCraftingFilterPatches
{
    private const string FilterButtonName = "AchievementHelper_CraftFilterButton";

    private static readonly Color ActiveTextColor = new(1f, 0.85f, 0.35f, 1f);
    private static readonly Color InactiveTextColor = new(0.9f, 0.9f, 0.9f, 1f);

    private static Button? _filterButton;
    private static bool _filterActive;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.Hide))]
    private static void Hide_Postfix()
    {
        _filterActive = false;
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
        if (Plugin.ModEnabled.Value && Plugin.ShowCraftingFilter.Value && __instance.InCraftTab())
        {
            __instance.UpdateCraftingPanel(false);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryGui.UpdateRecipeList))]
    private static void UpdateRecipeList_Prefix(InventoryGui __instance, ref List<Recipe> recipes)
    {
        if (!Plugin.ModEnabled.Value || !Plugin.ShowCraftingFilter.Value || !__instance.InCraftTab())
        {
            if (_filterButton != null)
            {
                _filterButton.gameObject.SetActive(false);
            }
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
        UpdateFilterButton(__instance, isCookingMode, missingCount);

        if (_filterActive && filtered != null)
        {
            recipes = filtered;
        }
    }

    private static void UpdateFilterButton(InventoryGui gui, bool isCookingMode, int missingCount)
    {
        Button? button = EnsureFilterButton(gui);
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(true);
        button.transform.SetAsLastSibling();

        TMP_Text? label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            string baseText = isCookingMode ? "Not Cooked" : "Not Crafted";
            label.text = _filterActive
                ? $"[✓] {baseText} ({missingCount})"
                : $"{baseText} ({missingCount})";
            label.color = _filterActive ? ActiveTextColor : InactiveTextColor;
        }

        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() =>
        {
            _filterActive = !_filterActive;
            gui.UpdateCraftingPanel(false);
        });
    }

    private static Button? EnsureFilterButton(InventoryGui gui)
    {
        if (_filterButton != null)
        {
            return _filterButton;
        }

        if (gui.m_crafting == null)
        {
            return null;
        }

        Transform? existing = gui.m_crafting.Find(FilterButtonName);
        if (existing != null)
        {
            _filterButton = existing.GetComponent<Button>();
            return _filterButton;
        }

        Button? template = gui.m_craftButton != null ? gui.m_craftButton : gui.m_tabUpgrade;
        if (template == null)
        {
            Plugin.Log.LogWarning("Could not find template button for crafting filter.");
            return null;
        }

        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, gui.m_crafting);
        clone.name = FilterButtonName;
        clone.SetActive(true);
        clone.transform.SetAsLastSibling();

        // Remove any gamepad hint children cloned from m_craftButton
        for (int i = clone.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = clone.transform.GetChild(i);
            if (child.GetComponent<TMP_Text>() == null)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        LayoutElement layoutElement = clone.GetComponent<LayoutElement>() ?? clone.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        Button button = clone.GetComponent<Button>();
        button.interactable = true;
        button.onClick = new Button.ButtonClickedEvent();

        if (clone.transform is RectTransform rt)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(185f, 32f);
            rt.anchoredPosition = new Vector2(6f, -6f);
        }

        TMP_Text? label = clone.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.fontSize = 15f;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.text = "Not Crafted (0)";
        }

        _filterButton = button;
        return _filterButton;
    }
}
