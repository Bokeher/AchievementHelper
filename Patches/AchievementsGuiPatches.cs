using System.Collections.Generic;
using AchievementHelper.Services;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(AchievementsGui))]
internal static class AchievementsGuiPatches
{
    private const string RevealButtonName = "AchievementHelper_RevealButton";
    private const string HideButtonName = "AchievementHelper_HideButton";

    private static Achievement? _currentAchievement;
    private static TMP_Text? _detailsHeaderText;
    private static Button? _revealButton;
    private static Button? _hideButton;

    private static readonly Color CompletedColor = Color.green;
    private static readonly Color RevealedNameColor = new(0.88f, 0.88f, 0.88f, 1f);
    private static readonly Color RevealedProgressColor = new(1f, 0.85f, 0.4f, 1f);
    private static readonly Color HiddenColor = Color.gray;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AchievementsGui.OnOpenAchievementDetails))]
    private static bool OnOpenAchievementDetails_Prefix(AchievementsGui __instance, Achievement achievement, bool clickable)
    {
        if (!Plugin.ModEnabled.Value)
        {
            return true;
        }

        if (__instance.m_achievementDetails.gameObject.activeSelf || !clickable || achievement == null)
        {
            return false;
        }

        __instance.StopAllCoroutines();
        ClearDetailsRows(__instance);

        _currentAchievement = achievement;
        __instance.m_achievementDetails.SetActive(true);

        UpdateDetailsHeader(__instance, achievement);
        UpdateRevealButton(__instance, achievement);

        bool concealed = AchievementService.IsConcealed(achievement);
        if (concealed)
        {
            AchievementDetailUnlockCondition row = UnityEngine.Object.Instantiate(
                __instance.m_achievementDetailsElementPrefab,
                __instance.m_achievementDetailsListRoot);
            row.gameObject.SetActive(true);
            row.StatName.color = HiddenColor;
            row.Progress.color = HiddenColor;
            row.StatName.text = "???";
            row.Progress.text = "??? / ???";
            return false;
        }

        PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
        Dictionary<PlayerStatType, float> stats =
            playerProfile.m_playerStats[(int)achievement.m_difficultyRequirement].m_stats;

        foreach (Achievement.PlayerStatRequirement req in achievement.m_statTrigger)
        {
            stats.TryGetValue(req.m_stat, out float currentAmount);
            __instance.CreateStatRow(req.m_stat.ToString(), currentAmount, req.m_amountAboveEquals);
        }

        foreach (Achievement other in achievement.m_otherAchievementTriggers)
        {
            if (other != null)
            {
                __instance.CreateStatRow(other.m_name, other.m_unlocked ? 1f : 0f, 1f);
            }
        }

        __instance.StartCoroutine(__instance.PopulateEnemyStats(achievement, playerProfile));
        __instance.StartCoroutine(__instance.PopulateDetailPanel(
            achievement.m_foodEatenTriggers, achievement, playerProfile, s => s.m_foodEatenStats));
        __instance.StartCoroutine(__instance.PopulateDetailPanel(
            achievement.m_itemCraftTriggers, achievement, playerProfile, s => s.m_itemCraftStats));
        __instance.StartCoroutine(__instance.PopulateDetailPanel(
            achievement.m_itemPickupTriggers, achievement, playerProfile, s => s.m_itemPickupStats));
        __instance.StartCoroutine(__instance.PopulateDetailPanel(
            achievement.m_knownCommandsTriggers, achievement, playerProfile, s => s.m_knownCommands));
        __instance.StartCoroutine(__instance.PopulateDetailPanel(
            achievement.m_knownWorldKeysTriggers, achievement, playerProfile, s => s.m_knownWorldKeys));
        __instance.StartCoroutine(__instance.PopulateDetailPanel(
            achievement.m_knownWorldTriggers, achievement, playerProfile, s => s.m_knownWorlds));
        __instance.StartCoroutine(__instance.PopulateDetailPanel(
            achievement.m_pickableTriggers, achievement, playerProfile, s => s.m_pickableStats));
        __instance.StartCoroutine(__instance.PopulateDetailPanel(
            achievement.m_piecePlacedTriggers, achievement, playerProfile, s => s.m_piecesPlacedStats));

        if (__instance.m_detailsScrollbar != null)
        {
            __instance.m_detailsScrollbar.value = 1f;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AchievementsGui.CreateStatRow))]
    private static bool CreateStatRow_Prefix(
        AchievementsGui __instance,
        string statKey,
        float currentAmount,
        float totalAmount)
    {
        if (!Plugin.ModEnabled.Value)
        {
            return true;
        }

        AchievementDetailUnlockCondition row = UnityEngine.Object.Instantiate(
            __instance.m_achievementDetailsElementPrefab,
            __instance.m_achievementDetailsListRoot);
        row.gameObject.SetActive(true);

        string displayName = AchievementService.FormatStatName(statKey);
        string progressText = $"{AchievementService.FormatAmount(currentAmount)} / {AchievementService.FormatAmount(totalAmount)}";

        if (currentAmount >= totalAmount)
        {
            row.StatName.color = CompletedColor;
            row.Progress.color = CompletedColor;
            row.StatName.text = displayName;
            row.Progress.text = progressText;
            return false;
        }

        Achievement? ach = _currentAchievement;
        AchievementDisplayMode mode = ach != null
            ? AchievementService.GetDisplayMode(ach)
            : AchievementDisplayMode.Revealable;
        bool statRevealed = ach != null && AchievementService.IsStatRevealed(ach, statKey);

        if (mode == AchievementDisplayMode.All || statRevealed)
        {
            row.StatName.color = RevealedNameColor;
            row.Progress.color = RevealedProgressColor;
            row.StatName.text = displayName;
            row.Progress.text = progressText;
            return false;
        }

        if (mode == AchievementDisplayMode.Revealable && ach != null)
        {
            row.StatName.color = HiddenColor;
            row.Progress.color = RevealedProgressColor;
            row.StatName.text = "???  (click to reveal)";
            row.Progress.text = progressText;

            MakeRowClickableToReveal(__instance, row, ach, statKey, displayName);
            return false;
        }

        row.StatName.color = HiddenColor;
        row.Progress.color = HiddenColor;
        row.StatName.text = "???";
        row.Progress.text = "??? / ???";
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AchievementsGui.CloseDetails))]
    private static void CloseDetails_Prefix(AchievementsGui __instance)
    {
        if (!Plugin.ModEnabled.Value)
        {
            return;
        }

        __instance.StopAllCoroutines();
        _currentAchievement = null;
    }

    private static void MakeRowClickableToReveal(
        AchievementsGui gui,
        AchievementDetailUnlockCondition row,
        Achievement achievement,
        string statKey,
        string displayName)
    {
        Image bg = row.gameObject.GetComponent<Image>();
        if (bg == null)
        {
            bg = row.gameObject.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.01f);
        }
        bg.raycastTarget = true;

        Button rowButton = row.gameObject.GetComponent<Button>();
        if (rowButton == null)
        {
            rowButton = row.gameObject.AddComponent<Button>();
        }

        ColorBlock colors = rowButton.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.01f);
        colors.highlightedColor = new Color(1f, 0.85f, 0.4f, 0.14f);
        colors.pressedColor = new Color(1f, 0.85f, 0.4f, 0.25f);
        colors.selectedColor = colors.highlightedColor;
        rowButton.colors = colors;
        rowButton.targetGraphic = bg;

        rowButton.onClick = new Button.ButtonClickedEvent();
        rowButton.onClick.AddListener(() =>
        {
            AchievementService.RevealStat(achievement, statKey);
            row.StatName.color = RevealedNameColor;
            row.StatName.text = displayName;
            bg.color = Color.clear;
            UnityEngine.Object.Destroy(rowButton);

            if (!AchievementService.HasAnyUnrevealedCondition(achievement))
            {
                AchievementService.RevealAchievement(achievement);
            }

            UpdateRevealButton(gui, achievement);
        });
    }

    private static void ClearDetailsRows(AchievementsGui gui)
    {
        foreach (Transform child in gui.m_achievementDetailsListRoot)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    private static void UpdateDetailsHeader(AchievementsGui gui, Achievement achievement)
    {
        if (_detailsHeaderText == null)
        {
            _detailsHeaderText = FindDetailsHeaderText(gui);
        }

        if (_detailsHeaderText != null)
        {
            _detailsHeaderText.text = Localization.instance != null
                ? Localization.instance.Localize(achievement.m_name)
                : achievement.m_name;
        }
    }

    private static TMP_Text? FindDetailsHeaderText(AchievementsGui gui)
    {
        foreach (TMP_Text text in gui.m_achievementDetails.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.transform.IsChildOf(gui.m_achievementDetailsListRoot))
            {
                continue;
            }

            if (text.GetComponentInParent<Button>() != null)
            {
                continue;
            }

            return text;
        }

        return null;
    }

    private static void UpdateRevealButton(AchievementsGui gui, Achievement achievement)
    {
        Button? revealBtn = EnsureCornerButton(gui, RevealButtonName, "Reveal", ref _revealButton);
        Button? hideBtn = EnsureCornerButton(gui, HideButtonName, "Hide", ref _hideButton);

        AchievementDisplayMode mode = AchievementService.GetDisplayMode(achievement);
        bool eligible = !achievement.m_unlocked &&
                        !Plugin.MasterRevealAll.Value &&
                        mode == AchievementDisplayMode.Revealable;

        bool showReveal = eligible && AchievementService.HasAnyUnrevealedCondition(achievement);
        bool showHide = eligible && AchievementService.HasAnyManuallyRevealedEntry(achievement);

        if (revealBtn != null)
        {
            revealBtn.gameObject.SetActive(showReveal);
            if (showReveal)
            {
                if (revealBtn.transform is RectTransform rt)
                {
                    rt.anchoredPosition = new Vector2(-14f, -12f);
                }

                revealBtn.transform.SetAsLastSibling();
                revealBtn.onClick = new Button.ButtonClickedEvent();
                revealBtn.onClick.AddListener(() =>
                {
                    AchievementService.RevealAchievement(achievement);
                    gui.CloseDetails();
                    gui.OnOpenAchievementDetails(achievement, true);
                });
            }
        }

        if (hideBtn != null)
        {
            hideBtn.gameObject.SetActive(showHide);
            if (showHide)
            {
                if (hideBtn.transform is RectTransform rt)
                {
                    rt.anchoredPosition = new Vector2(showReveal ? -126f : -14f, -12f);
                }

                hideBtn.transform.SetAsLastSibling();
                hideBtn.onClick = new Button.ButtonClickedEvent();
                hideBtn.onClick.AddListener(() =>
                {
                    AchievementService.HideAchievement(achievement);
                    gui.CloseDetails();
                    gui.OnOpenAchievementDetails(achievement, true);
                });
            }
        }
    }

    private static Button? EnsureCornerButton(AchievementsGui gui, string name, string defaultLabel, ref Button? cached)
    {
        if (cached != null)
        {
            return cached;
        }

        Button? template = gui.m_achievementDetails.GetComponentInChildren<Button>(true);
        if (template == null)
        {
            Plugin.Log.LogWarning("Could not find template button in m_achievementDetails.");
            return null;
        }

        Transform container = template.transform.parent != null
            ? template.transform.parent
            : gui.m_achievementDetails.transform;

        Transform? existing = container.Find(name);
        if (existing != null)
        {
            cached = existing.GetComponent<Button>();
            return cached;
        }

        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, container);
        clone.name = name;
        clone.transform.SetAsLastSibling();

        LayoutElement layoutElement = clone.GetComponent<LayoutElement>() ?? clone.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        Button button = clone.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();

        if (clone.transform is RectTransform rt)
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(105f, 30f);
            rt.anchoredPosition = new Vector2(-14f, -12f);
        }

        TMP_Text? label = clone.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = defaultLabel;
            label.fontSize = Mathf.Min(label.fontSize, 16f);
        }

        cached = button;
        return cached;
    }
}
