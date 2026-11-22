using System.Collections.Generic;
using AchievementHelper.Services;
using AchievementHelper.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementHelper.Patches;

[HarmonyPatch(typeof(EnemyHud))]
internal static class EnemyHudPatches
{
    private const string IndicatorsRootName = "AchievementHelper_EnemyIndicators";
    private const string UndefeatedSlotName = "Undefeated";
    private const string TrophySlotName = "Trophy";
    private const float RefreshIntervalSeconds = 0.25f;

    private static readonly Color BackdropColor = new(0.05f, 0.05f, 0.05f, 0.72f);
    private static readonly Color UndefeatedIconColor = new(1f, 0.42f, 0.35f, 1f);
    private static readonly Color TrophyIconColor = new(1f, 0.85f, 0.32f, 1f);

    private static float _nextRefreshTime;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyHud.ShowHud))]
    private static void ShowHud_Postfix(EnemyHud __instance, Character c, bool isMount)
    {
        if (isMount || c == null || c.IsPlayer() || c.IsBoss())
        {
            return;
        }

        if (!__instance.m_huds.TryGetValue(c, out EnemyHud.HudData hudData) || hudData?.m_gui == null)
        {
            return;
        }

        UpdateHudIndicators(hudData);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyHud.UpdateHuds))]
    private static void UpdateHuds_Postfix(EnemyHud __instance)
    {
        if (Time.unscaledTime < _nextRefreshTime)
        {
            return;
        }

        _nextRefreshTime = Time.unscaledTime + RefreshIntervalSeconds;

        foreach (KeyValuePair<Character, EnemyHud.HudData> kvp in __instance.m_huds)
        {
            EnemyHud.HudData hudData = kvp.Value;
            if (hudData == null || hudData.m_isMount || hudData.m_character == null || hudData.m_gui == null)
            {
                continue;
            }

            if (hudData.m_character.IsPlayer() || hudData.m_character.IsBoss() || !hudData.m_gui.activeSelf)
            {
                continue;
            }

            UpdateHudIndicators(hudData);
        }
    }

    private static void UpdateHudIndicators(EnemyHud.HudData hudData)
    {
        Character character = hudData.m_character;
        bool featureEnabled = Plugin.ModEnabled.Value && Plugin.ShowEnemyIndicators.Value;

        bool showUndefeated = featureEnabled && AchievementService.IsEnemyMissingForAchievement(character);
        bool showTrophy = featureEnabled && AchievementService.IsEnemyMissingTrophyForAchievement(character);

        Transform? healthTransform = hudData.m_gui.transform.Find("Health");
        Transform parent = healthTransform != null ? healthTransform : hudData.m_gui.transform;
        Transform? root = parent.Find(IndicatorsRootName);

        if (!showUndefeated && !showTrophy)
        {
            if (root != null && root.gameObject.activeSelf)
            {
                root.gameObject.SetActive(false);
            }
            return;
        }

        if (root == null)
        {
            root = CreateIndicatorsRoot(parent);
        }

        if (!root.gameObject.activeSelf)
        {
            root.gameObject.SetActive(true);
        }

        Transform? undefeatedSlot = root.Find(UndefeatedSlotName);
        if (undefeatedSlot != null && undefeatedSlot.gameObject.activeSelf != showUndefeated)
        {
            undefeatedSlot.gameObject.SetActive(showUndefeated);
        }

        Transform? trophySlot = root.Find(TrophySlotName);
        if (trophySlot != null && trophySlot.gameObject.activeSelf != showTrophy)
        {
            trophySlot.gameObject.SetActive(showTrophy);
        }
    }

    private static Transform CreateIndicatorsRoot(Transform parent)
    {
        GameObject rootObj = new GameObject(IndicatorsRootName, typeof(RectTransform));
        rootObj.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)rootObj.transform;
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(4f, 0f);
        rt.sizeDelta = new Vector2(40f, 18f);

        HorizontalLayoutGroup layout = rootObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 3f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = rootObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateIconSlot(rootObj.transform, UndefeatedSlotName, IconLoader.UndefeatedSprite, UndefeatedIconColor);
        CreateIconSlot(rootObj.transform, TrophySlotName, IconLoader.TrophySprite, TrophyIconColor);

        return rootObj.transform;
    }

    private static void CreateIconSlot(Transform parent, string name, Sprite? sprite, Color iconColor)
    {
        GameObject slotObj = new GameObject(name, typeof(RectTransform));
        slotObj.transform.SetParent(parent, false);

        RectTransform slotRt = (RectTransform)slotObj.transform;
        slotRt.sizeDelta = new Vector2(18f, 18f);

        Image backdrop = slotObj.AddComponent<Image>();
        backdrop.color = BackdropColor;
        backdrop.raycastTarget = false;

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
        iconObj.transform.SetParent(slotObj.transform, false);

        RectTransform iconRt = (RectTransform)iconObj.transform;
        iconRt.anchorMin = new Vector2(0.12f, 0.12f);
        iconRt.anchorMax = new Vector2(0.88f, 0.88f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;

        Image icon = iconObj.AddComponent<Image>();
        icon.sprite = sprite;
        icon.color = iconColor;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
    }
}
