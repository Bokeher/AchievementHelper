using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private const string HideCompletedButtonName = "AchievementHelper_HideCompletedButton";

    private static Achievement? _currentAchievement;
    private static TMP_Text? _detailsHeaderText;
    private static Button? _revealButton;
    private static Button? _hideButton;
    private static Button? _hideCompletedButton;
    private static bool _hideCompletedActive;

    private static readonly Color CompletedColor = Color.green;
    private static readonly Color RevealedNameColor = new(0.88f, 0.88f, 0.88f, 1f);
    private static readonly Color RevealedProgressColor = new(1f, 0.85f, 0.4f, 1f);
    private static readonly Color HiddenColor = Color.gray;
    private static readonly Color FilterActiveTextColor = new(1f, 0.85f, 0.35f, 1f);
    private static readonly Color FilterActiveBgColor = new(0.85f, 0.65f, 0.2f, 1f);

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
        bool multiCondition = GetTotalConditionCount(achievement) > 1;
        UpdateHideCompletedButton(__instance, achievement, !concealed && multiCondition);

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

        int diffIndex = (int)achievement.m_difficultyRequirement;
        bool hideCompleted = _hideCompletedActive && multiCondition;

        if (hideCompleted && !HasAnyIncompleteCondition(achievement, diffIndex))
        {
            AchievementDetailUnlockCondition row = UnityEngine.Object.Instantiate(
                __instance.m_achievementDetailsElementPrefab,
                __instance.m_achievementDetailsListRoot);
            row.gameObject.SetActive(true);
            row.StatName.color = CompletedColor;
            row.Progress.color = CompletedColor;
            row.StatName.text = "All conditions completed";
            row.Progress.text = "100%";
            return false;
        }

        foreach (Achievement.PlayerStatRequirement req in achievement.m_statTrigger)
        {
            float currentAmount = AchievementService.GetEffectivePlayerStat(diffIndex, req.m_stat);
            if (hideCompleted && currentAmount >= req.m_amountAboveEquals)
            {
                continue;
            }
            __instance.CreateStatRow(req.m_stat.ToString(), currentAmount, req.m_amountAboveEquals);
        }

        foreach (Achievement other in achievement.m_otherAchievementTriggers)
        {
            if (other != null)
            {
                if (hideCompleted && other.m_unlocked)
                {
                    continue;
                }
                __instance.CreateStatRow(other.m_name, other.m_unlocked ? 1f : 0f, 1f);
            }
        }

        bool groupEnemiesByBiome = Plugin.GroupDetailsByBiome.Value &&
                                   (achievement.m_id == "KillAllCreatures" ||
                                    achievement.m_id == "KillAllCreaturesHard" ||
                                    achievement.m_id == "AllMiniBosses");
        bool groupFoodEatenByBiome = Plugin.GroupDetailsByBiome.Value &&
                                     achievement.m_id == "AllFoodEaten";
        bool groupCraftItemsByBiome = Plugin.GroupDetailsByBiome.Value &&
                                      (achievement.m_id == "AllItemCraft" ||
                                       achievement.m_id == "AllWeaponCraft" ||
                                       achievement.m_id == "AllFoodCooked");
        bool groupTrophiesByBiome = Plugin.GroupDetailsByBiome.Value &&
                                    achievement.m_id == "FindAllTrophies";
        bool groupBuildPiecesByBiome = Plugin.GroupDetailsByBiome.Value &&
                                       achievement.m_id == "AllBuildPieces";

        if (groupEnemiesByBiome)
        {
            BiomeClassifier.LogUncategorizedEnemies(achievement.m_id, achievement.m_enemyStatsTriggers.Select(t => t.m_stat));
            IEnumerable<GroupedStatEntry> enemyEntries = achievement.m_enemyStatsTriggers.Select(req =>
                new GroupedStatEntry(
                    req.m_stat,
                    AchievementService.GetEffectiveEnemyStat(diffIndex, req.m_modifier, req.m_stat),
                    req.m_amount,
                    AchievementService.FormatStatName(req.m_stat),
                    BiomeClassifier.GetEnemyBiome(req.m_stat)));
            __instance.StartCoroutine(PopulateGroupedEntriesEffective(__instance, achievement, enemyEntries, hideCompleted));
        }
        else
        {
            __instance.StartCoroutine(PopulateEnemyStatsEffective(__instance, achievement, diffIndex, hideCompleted));
        }

        if (groupFoodEatenByBiome)
        {
            BiomeClassifier.LogUncategorizedMaterialsForItems(achievement.m_id, achievement.m_foodEatenTriggers.Select(t => t.m_stat));
            IEnumerable<GroupedStatEntry> foodEntries = achievement.m_foodEatenTriggers.Select(req =>
                new GroupedStatEntry(
                    req.m_stat,
                    AchievementService.GetEffectiveDictStat(diffIndex, s => s.m_foodEatenStats, req.m_stat),
                    req.m_amount,
                    AchievementService.FormatStatName(req.m_stat),
                    BiomeClassifier.GetItemBiome(req.m_stat)));
            __instance.StartCoroutine(PopulateGroupedEntriesEffective(__instance, achievement, foodEntries, hideCompleted));
        }
        else
        {
            __instance.StartCoroutine(PopulateDetailPanelEffective(__instance, achievement.m_foodEatenTriggers, diffIndex, s => s.m_foodEatenStats, hideCompleted));
        }

        if (groupCraftItemsByBiome)
        {
            BiomeClassifier.LogUncategorizedMaterialsForItems(achievement.m_id, achievement.m_itemCraftTriggers.Select(t => t.m_stat));
            IEnumerable<GroupedStatEntry> craftEntries = achievement.m_itemCraftTriggers.Select(req =>
                new GroupedStatEntry(
                    req.m_stat,
                    AchievementService.GetEffectiveDictStat(diffIndex, s => s.m_itemCraftStats, req.m_stat),
                    req.m_amount,
                    AchievementService.FormatStatName(req.m_stat),
                    BiomeClassifier.GetItemBiome(req.m_stat)));
            __instance.StartCoroutine(PopulateGroupedEntriesEffective(__instance, achievement, craftEntries, hideCompleted));
        }
        else
        {
            __instance.StartCoroutine(PopulateDetailPanelEffective(__instance, achievement.m_itemCraftTriggers, diffIndex, s => s.m_itemCraftStats, hideCompleted));
        }

        if (groupTrophiesByBiome)
        {
            BiomeClassifier.LogUncategorizedMaterialsForItems(achievement.m_id, achievement.m_itemPickupTriggers.Select(t => t.m_stat));
            IEnumerable<GroupedStatEntry> trophyEntries = achievement.m_itemPickupTriggers.Select(req =>
                new GroupedStatEntry(
                    req.m_stat,
                    AchievementService.GetEffectiveDictStat(diffIndex, s => s.m_itemPickupStats, req.m_stat),
                    req.m_amount,
                    AchievementService.FormatStatName(req.m_stat),
                    BiomeClassifier.GetItemBiome(req.m_stat)));
            __instance.StartCoroutine(PopulateGroupedEntriesEffective(__instance, achievement, trophyEntries, hideCompleted));
        }
        else
        {
            __instance.StartCoroutine(PopulateDetailPanelEffective(__instance, achievement.m_itemPickupTriggers, diffIndex, s => s.m_itemPickupStats, hideCompleted));
        }
        __instance.StartCoroutine(PopulateDetailPanelEffective(__instance, achievement.m_knownCommandsTriggers, diffIndex, s => s.m_knownCommands, hideCompleted));
        __instance.StartCoroutine(PopulateDetailPanelEffective(__instance, achievement.m_knownWorldKeysTriggers, diffIndex, s => s.m_knownWorldKeys, hideCompleted));
        __instance.StartCoroutine(PopulateDetailPanelEffective(__instance, achievement.m_knownWorldTriggers, diffIndex, s => s.m_knownWorlds, hideCompleted));
        __instance.StartCoroutine(PopulateDetailPanelEffective(__instance, achievement.m_pickableTriggers, diffIndex, s => s.m_pickableStats, hideCompleted));

        if (groupBuildPiecesByBiome)
        {
            BiomeClassifier.LogUncategorizedPieces(achievement.m_id, achievement.m_piecePlacedTriggers.Select(t => t.m_stat));
            IEnumerable<GroupedStatEntry> pieceEntries = achievement.m_piecePlacedTriggers.Select(req =>
                new GroupedStatEntry(
                    req.m_stat,
                    AchievementService.GetEffectiveDictStat(diffIndex, s => s.m_piecesPlacedStats, req.m_stat),
                    req.m_amount,
                    AchievementService.FormatStatName(req.m_stat),
                    BiomeClassifier.GetPieceBiome(req.m_stat)));
            __instance.StartCoroutine(PopulateGroupedEntriesEffective(__instance, achievement, pieceEntries, hideCompleted));
        }
        else
        {
            __instance.StartCoroutine(PopulateDetailPanelEffective(__instance, achievement.m_piecePlacedTriggers, diffIndex, s => s.m_piecesPlacedStats, hideCompleted));
        }

        if (__instance.m_detailsScrollbar != null)
        {
            __instance.m_detailsScrollbar.value = 1f;
        }

        return false;
    }

    private static IEnumerator PopulateEnemyStatsEffective(AchievementsGui gui, Achievement achievement, int diffIndex, bool hideCompleted)
    {
        int count = 0;
        foreach (Achievement.EnemyStatRequirement req in achievement.m_enemyStatsTriggers)
        {
            float currentAmount = AchievementService.GetEffectiveEnemyStat(diffIndex, req.m_modifier, req.m_stat);
            if (hideCompleted && currentAmount >= req.m_amount)
            {
                continue;
            }

            gui.CreateStatRow(req.m_stat, currentAmount, req.m_amount);
            count++;
            if (count >= gui.m_detailStatsPerFrame)
            {
                yield return null;
                count = 0;
            }
        }
    }

    private static IEnumerator PopulateDetailPanelEffective(
        AchievementsGui gui,
        List<Achievement.DictStatRequirement> triggers,
        int diffIndex,
        Func<PlayerProfile.PlayerStats, Dictionary<string, float>> selector,
        bool hideCompleted)
    {
        int count = 0;
        foreach (Achievement.DictStatRequirement req in triggers)
        {
            float currentAmount = AchievementService.GetEffectiveDictStat(diffIndex, selector, req.m_stat);
            if (hideCompleted && currentAmount >= req.m_amount)
            {
                continue;
            }

            gui.CreateStatRow(req.m_stat, currentAmount, req.m_amount);
            count++;
            if (count >= gui.m_detailStatsPerFrame)
            {
                yield return null;
                count = 0;
            }
        }
    }

    private readonly struct GroupedStatEntry
    {
        public readonly string StatKey;
        public readonly float Current;
        public readonly float Target;
        public readonly string DisplayName;
        public readonly BiomeTier Biome;

        public GroupedStatEntry(string statKey, float current, float target, string displayName, BiomeTier biome)
        {
            StatKey = statKey;
            Current = current;
            Target = target;
            DisplayName = displayName;
            Biome = biome;
        }
    }

    private static IEnumerator PopulateGroupedEntriesEffective(
        AchievementsGui gui,
        Achievement achievement,
        IEnumerable<GroupedStatEntry> sourceEntries,
        bool hideCompleted)
    {
        AchievementDisplayMode mode = AchievementService.GetDisplayMode(achievement);
        bool allRevealed = mode == AchievementDisplayMode.All || AchievementService.IsRevealed(achievement);

        var groups = sourceEntries
            .GroupBy(entry => entry.Biome)
            .OrderBy(g => (int)g.Key)
            .ToList();

        int count = 0;
        foreach (var group in groups)
        {
            List<GroupedStatEntry> entries = group.ToList();
            int completedInGroup = 0;
            bool anyVisibleInGroup = allRevealed;

            foreach (GroupedStatEntry entry in entries)
            {
                if (entry.Current >= entry.Target)
                {
                    completedInGroup++;
                    anyVisibleInGroup = true;
                }
                else if (!anyVisibleInGroup && AchievementService.IsStatRevealed(achievement, entry.StatKey))
                {
                    anyVisibleInGroup = true;
                }
            }

            if (hideCompleted && completedInGroup >= entries.Count)
            {
                continue;
            }

            entries.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

            string headerLabel = anyVisibleInGroup || group.Key == BiomeTier.Uncategorized
                ? group.Key.GetDisplayName()
                : "???";

            CreateBiomeHeaderRow(gui, headerLabel, completedInGroup, entries.Count);
            count++;
            if (count >= gui.m_detailStatsPerFrame)
            {
                yield return null;
                count = 0;
            }

            foreach (GroupedStatEntry entry in entries)
            {
                if (hideCompleted && entry.Current >= entry.Target)
                {
                    continue;
                }

                gui.CreateStatRow(entry.StatKey, entry.Current, entry.Target);
                count++;
                if (count >= gui.m_detailStatsPerFrame)
                {
                    yield return null;
                    count = 0;
                }
            }
        }
    }

    private static void CreateBiomeHeaderRow(AchievementsGui gui, string biomeName, int completed, int total)
    {
        AchievementDetailUnlockCondition row = UnityEngine.Object.Instantiate(
            gui.m_achievementDetailsElementPrefab,
            gui.m_achievementDetailsListRoot);
        row.gameObject.SetActive(true);

        Image bg = row.gameObject.GetComponent<Image>() ?? row.gameObject.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.45f);
        bg.raycastTarget = false;

        Color headerColor = completed >= total && total > 0
            ? CompletedColor
            : new Color(1f, 0.85f, 0.35f, 1f);

        row.StatName.fontStyle = FontStyles.Bold;
        row.Progress.fontStyle = FontStyles.Bold;
        row.StatName.color = headerColor;
        row.Progress.color = headerColor;
        row.StatName.text = $"— {biomeName} —";
        row.Progress.text = $"{completed} / {total}";
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

    private static void UpdateHideCompletedButton(AchievementsGui gui, Achievement achievement, bool show)
    {
        Button? btn = EnsureCornerButton(gui, HideCompletedButtonName, "Hide Completed", ref _hideCompletedButton);
        if (btn == null)
        {
            return;
        }

        btn.gameObject.SetActive(show);
        if (!show)
        {
            return;
        }

        if (btn.transform is RectTransform rt)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(168f, 30f);
            rt.anchoredPosition = new Vector2(14f, -12f);
        }

        TMP_Text? label = btn.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.text = "Hide Completed";
            label.fontSize = 14f;
            label.color = _hideCompletedActive ? FilterActiveTextColor : Color.white;
        }

        Image? bg = btn.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = _hideCompletedActive ? FilterActiveBgColor : Color.white;
        }

        btn.transform.SetAsLastSibling();
        btn.onClick = new Button.ButtonClickedEvent();
        btn.onClick.AddListener(() =>
        {
            _hideCompletedActive = !_hideCompletedActive;
            gui.CloseDetails();
            gui.OnOpenAchievementDetails(achievement, true);
        });
    }

    private static int GetTotalConditionCount(Achievement achievement)
    {
        return achievement.m_statTrigger.Count +
               achievement.m_otherAchievementTriggers.Count +
               achievement.m_enemyStatsTriggers.Count +
               achievement.m_foodEatenTriggers.Count +
               achievement.m_itemCraftTriggers.Count +
               achievement.m_itemPickupTriggers.Count +
               achievement.m_knownCommandsTriggers.Count +
               achievement.m_knownWorldKeysTriggers.Count +
               achievement.m_knownWorldTriggers.Count +
               achievement.m_pickableTriggers.Count +
               achievement.m_piecePlacedTriggers.Count;
    }

    private static bool HasAnyIncompleteCondition(Achievement achievement, int diffIndex)
    {
        foreach (var req in achievement.m_statTrigger)
            if (AchievementService.GetEffectivePlayerStat(diffIndex, req.m_stat) < req.m_amountAboveEquals)
                return true;

        foreach (var other in achievement.m_otherAchievementTriggers)
            if (other != null && !other.m_unlocked)
                return true;

        foreach (var req in achievement.m_enemyStatsTriggers)
            if (AchievementService.GetEffectiveEnemyStat(diffIndex, req.m_modifier, req.m_stat) < req.m_amount)
                return true;

        return HasIncompleteDictTrigger(diffIndex, achievement.m_foodEatenTriggers, s => s.m_foodEatenStats) ||
               HasIncompleteDictTrigger(diffIndex, achievement.m_itemCraftTriggers, s => s.m_itemCraftStats) ||
               HasIncompleteDictTrigger(diffIndex, achievement.m_itemPickupTriggers, s => s.m_itemPickupStats) ||
               HasIncompleteDictTrigger(diffIndex, achievement.m_knownCommandsTriggers, s => s.m_knownCommands) ||
               HasIncompleteDictTrigger(diffIndex, achievement.m_knownWorldKeysTriggers, s => s.m_knownWorldKeys) ||
               HasIncompleteDictTrigger(diffIndex, achievement.m_knownWorldTriggers, s => s.m_knownWorlds) ||
               HasIncompleteDictTrigger(diffIndex, achievement.m_pickableTriggers, s => s.m_pickableStats) ||
               HasIncompleteDictTrigger(diffIndex, achievement.m_piecePlacedTriggers, s => s.m_piecesPlacedStats);
    }

    private static bool HasIncompleteDictTrigger(
        int diffIndex,
        List<Achievement.DictStatRequirement> triggers,
        Func<PlayerProfile.PlayerStats, Dictionary<string, float>> selector)
    {
        foreach (var req in triggers)
            if (AchievementService.GetEffectiveDictStat(diffIndex, selector, req.m_stat) < req.m_amount)
                return true;
        return false;
    }
}
