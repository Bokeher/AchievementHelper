using System.Collections.Generic;
using AchievementHelper.Services;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(InventoryGui))]
internal static class InventoryGuiAchievementsPatches
{
    private const string ProgressBadgeName = "AchievementHelper_ProgressBadge";
    private const string MasterRevealButtonName = "AchievementHelper_MasterRevealButton";

    private static Button? _masterRevealButton;
    private static bool _awaitingMasterConfirm;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryGui.UpdateAchievementsList))]
    private static bool UpdateAchievementsList_Prefix(InventoryGui __instance)
    {
        if (!Plugin.ModEnabled.Value)
        {
            return true;
        }

        if (Player.m_localPlayer == null)
        {
            return false;
        }

        foreach (GameObject obj in __instance.m_achievementsPanel.m_achievementsList)
        {
            UnityEngine.Object.Destroy(obj);
        }
        __instance.m_achievementsPanel.m_achievementsList.Clear();

        List<List<Achievement>> tiers = new List<List<Achievement>>(10);
        for (int i = 0; i <= 9; i++)
        {
            tiers.Add(new List<Achievement>());
        }

        int unlockedCount = 0;
        foreach (AchievementList achievementList in Achievements.m_instance.m_achievementLists)
        {
            foreach (Achievement achievement in achievementList.m_achievements)
            {
                if (achievement.m_iconLocked != null &&
                    achievement.m_iconLocked.name.Length >= 2 &&
                    int.TryParse(achievement.m_iconLocked.name.Substring(0, 2), out int tierIndex) &&
                    tierIndex >= 0 && tierIndex < tiers.Count)
                {
                    tiers[tierIndex].Add(achievement);
                }
                else
                {
                    tiers[tiers.Count - 1].Add(achievement);
                }

                if (achievement.m_unlocked)
                {
                    unlockedCount++;
                }
            }
        }

        List<Achievement> ordered = new List<Achievement>();
        for (int j = 1; j <= 9; j++)
        {
            ordered.AddRange(tiers[j]);
        }

        __instance.m_achievementsCompletionRateText.text = $"{unlockedCount}/{ordered.Count}";
        AchievementsGui achievementsGui = __instance.m_achievementsPanel.GetComponent<AchievementsGui>();

        foreach (Achievement item in ordered)
        {
            RectTransform tile = (RectTransform)UnityEngine.Object.Instantiate(
                __instance.m_achievementsElementPrefab,
                __instance.m_achievementsListRoot).transform;

            __instance.m_achievementsPanel.m_achievementsList.Add(tile.gameObject);
            tile.gameObject.SetActive(true);

            AchievementDisplayMode mode = AchievementService.GetDisplayMode(item);
            bool concealed = AchievementService.IsConcealed(item);
            bool hasDetails = AchievementService.HasAnyDetails(item);
            bool clickable = !concealed &&
                             mode != AchievementDisplayMode.Simple &&
                             (item.m_clickable || hasDetails);

            Button button = tile.GetComponent<Button>();
            if (clickable)
            {
                Achievement captured = item;
                button.onClick.AddListener(() => achievementsGui.OnOpenAchievementDetails(captured, true));
            }
            else if (button != null)
            {
                UnityEngine.Object.Destroy(button);
            }

            Transform iconTransform = tile.Find("icon");
            TMP_Text nameText = tile.Find("name").GetComponent<TMP_Text>();
            TMP_Text descText = tile.Find("description").GetComponent<TMP_Text>();

            if (!concealed)
            {
                iconTransform.GetComponent<Image>().sprite = item.m_unlocked ? item.m_icon : item.m_iconLocked;
                nameText.text = Localization.instance.Localize(item.m_name);
                descText.text = Localization.instance.Localize(item.m_description);

                if (!item.m_unlocked && mode != AchievementDisplayMode.Simple && hasDetails)
                {
                    int pct = Mathf.FloorToInt(AchievementService.GetCompletionPercentage(item));
                    if (pct < 100)
                    {
                        AddProgressBadge(iconTransform, nameText, pct);
                    }
                }
            }
            else
            {
                iconTransform.GetComponent<Image>().sprite = item.m_iconLocked;
                nameText.text = Localization.instance.Localize("$inventory_achievement_secret");
                descText.text = Localization.instance.Localize("$inventory_achievement_secret_description");
            }
        }

        __instance.m_achievementsListScroll.value = 1f;
        UpdateCheatedBanner(__instance);
        UpdateMasterRevealButton(__instance);
        return false;
    }

    private static void AddProgressBadge(Transform iconTransform, TMP_Text referenceText, int percent)
    {
        GameObject badgeObj = new GameObject(ProgressBadgeName, typeof(RectTransform));
        badgeObj.transform.SetParent(iconTransform, false);

        RectTransform rt = (RectTransform)badgeObj.transform;
        rt.anchorMin = new Vector2(0.12f, 0.32f);
        rt.anchorMax = new Vector2(0.88f, 0.68f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = badgeObj.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.62f);
        bg.raycastTarget = false;

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(badgeObj.transform, false);

        RectTransform textRt = (RectTransform)textObj.transform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI badgeText = textObj.AddComponent<TextMeshProUGUI>();
        badgeText.font = referenceText.font;
        badgeText.fontSharedMaterial = referenceText.fontSharedMaterial;
        badgeText.fontSize = 22f;
        badgeText.fontStyle = FontStyles.Bold;
        badgeText.alignment = TextAlignmentOptions.Center;
        badgeText.color = percent > 0 ? new Color(1f, 0.88f, 0.35f, 1f) : new Color(0.9f, 0.9f, 0.9f, 0.95f);
        badgeText.raycastTarget = false;
        badgeText.text = $"{percent}%";
    }

    private static void UpdateMasterRevealButton(InventoryGui gui)
    {
        _awaitingMasterConfirm = false;
        Button? button = EnsureMasterRevealButton(gui);
        if (button == null)
        {
            return;
        }

        if (Plugin.MasterRevealAll.Value)
        {
            button.gameObject.SetActive(false);
            return;
        }

        button.gameObject.SetActive(true);
        TMP_Text? label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = "Reveal All";
        }

        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() =>
        {
            if (!_awaitingMasterConfirm)
            {
                _awaitingMasterConfirm = true;
                if (label != null)
                {
                    label.text = "Confirm?";
                }
                return;
            }

            _awaitingMasterConfirm = false;
            AchievementService.RevealAllSpoilers();
            gui.UpdateAchievementsList();
        });
    }

    private static Button? EnsureMasterRevealButton(InventoryGui gui)
    {
        if (_masterRevealButton != null)
        {
            return _masterRevealButton;
        }

        Transform panelRoot = gui.m_achievementsPanel.transform;
        Transform? existing = panelRoot.Find(MasterRevealButtonName);
        if (existing != null)
        {
            _masterRevealButton = existing.GetComponent<Button>();
            return _masterRevealButton;
        }

        Button? template = null;
        foreach (Button candidate in panelRoot.GetComponentsInChildren<Button>(true))
        {
            if (candidate.transform.IsChildOf(gui.m_achievementsListRoot))
            {
                continue;
            }

            if (gui.m_achievementsPanel.m_achievementDetails != null &&
                candidate.transform.IsChildOf(gui.m_achievementsPanel.m_achievementDetails.transform))
            {
                continue;
            }

            template = candidate;
            break;
        }

        if (template == null)
        {
            return null;
        }

        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, template.transform.parent);
        clone.name = MasterRevealButtonName;
        clone.transform.SetAsLastSibling();

        LayoutElement layoutElement = clone.GetComponent<LayoutElement>() ?? clone.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        Button button = clone.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();

        if (clone.transform is RectTransform rt)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(130f, 30f);
            rt.anchoredPosition = new Vector2(18f, -12f);
        }

        TMP_Text? label = clone.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = "Reveal All";
            label.fontSize = Mathf.Min(label.fontSize, 16f);
        }

        _masterRevealButton = button;
        return _masterRevealButton;
    }

    private static void UpdateCheatedBanner(InventoryGui gui)
    {
        if (PlayerProfile.s_bypassCheatChecks &&
            (Achievements.IsWorldCheated() ||
             Game.instance.GetPlayerProfile().m_usedCheats ||
             Player.m_localPlayer.GetInventory().AnyCheatedItem()))
        {
            gui.m_achievementsCheatedText.text = Localization.instance.Localize("$achievements_permanently_cheated_bypass");
            return;
        }

        if (Achievements.IsWorldCheated())
        {
            gui.m_achievementsCheatedText.text = Localization.instance.Localize("$achievements_permanently_cheated_world");
            return;
        }

        if (Game.instance.GetPlayerProfile().m_usedCheats)
        {
            gui.m_achievementsCheatedText.text = Localization.instance.Localize("$achievements_permanently_cheated_character");
            return;
        }

        if (Player.m_localPlayer.GetInventory().AnyCheatedItem())
        {
            gui.m_achievementsCheatedText.text = Localization.instance.Localize("$achievements_temporarily_cheated");
            return;
        }

        gui.m_achievementsCheatedText.text = string.Empty;
    }
}
