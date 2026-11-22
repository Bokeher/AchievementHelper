using System.Collections.Generic;
using AchievementHelper.Services;
using AchievementHelper.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementHelper.Patches;

internal static class BuildingFilterState
{
    internal static bool FilterActive;
    internal static bool TableHasBuildTargets;
    internal static int TotalMissingCount;

    internal static void ToggleFilter(BuildUi? buildUi = null)
    {
        FilterActive = !FilterActive;
        if (Player.m_localPlayer != null && Player.m_localPlayer.m_buildPieces != null)
        {
            Player.m_localPlayer.UpdateAvailablePiecesList();
        }

        if (buildUi != null && buildUi.gameObject.activeInHierarchy)
        {
            buildUi.UpdateTagButtons(true);
            buildUi.UpdatePieceButtons();
        }
    }
}

[HarmonyPatch(typeof(PieceTable))]
internal static class PieceTableFilterPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.UpdateAvailable))]
    private static void UpdateAvailable_Postfix(PieceTable __instance)
    {
        bool hasTargets = false;
        int missingCount = 0;

        foreach (Piece piece in __instance.m_availablePieces)
        {
            if (piece == null || string.IsNullOrEmpty(piece.m_name))
            {
                continue;
            }

            if (AchievementService.IsBuildPieceTarget(piece.m_name))
            {
                hasTargets = true;
                if (!AchievementService.HasBuiltPiece(piece.m_name))
                {
                    missingCount++;
                }
            }
        }

        BuildingFilterState.TableHasBuildTargets = hasTargets;
        BuildingFilterState.TotalMissingCount = missingCount;

        if (!Plugin.ModEnabled.Value ||
            !Plugin.ShowBuildIcons.Value ||
            !BuildingFilterState.FilterActive ||
            !hasTargets)
        {
            return;
        }

        __instance.m_availablePieces.RemoveWhere(p => !AchievementService.IsPieceMissingForAchievement(p));

        for (int cat = 0; cat < __instance.m_availablePiecesByCategory.Count; cat++)
        {
            List<Piece> catList = __instance.m_availablePiecesByCategory[cat];
            catList.RemoveAll(p => !AchievementService.IsPieceMissingForAchievement(p));

            if (cat < __instance.m_selectedPiece.Length)
            {
                Vector2Int sel = __instance.m_selectedPiece[cat];
                int flatIndex = sel.y * 15 + sel.x;
                if (flatIndex < 0 || flatIndex >= catList.Count)
                {
                    __instance.m_selectedPiece[cat] = Vector2Int.zero;
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Player))]
internal static class PlayerBuildingFilterPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player.SetPlaceMode))]
    private static void SetPlaceMode_Prefix(Player __instance, PieceTable buildPieces)
    {
        if (__instance != Player.m_localPlayer)
        {
            return;
        }

        if (buildPieces == null || buildPieces != __instance.m_buildPieces)
        {
            BuildingFilterState.FilterActive = false;
            BuildingFilterState.TableHasBuildTargets = false;
            BuildingFilterState.TotalMissingCount = 0;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(
        nameof(Player.PlacePiece),
        typeof(Piece),
        typeof(Vector3),
        typeof(Quaternion),
        typeof(bool),
        typeof(bool))]
    private static void PlacePiece_Postfix(Player __instance)
    {
        if (__instance != Player.m_localPlayer || !Plugin.ModEnabled.Value || !Plugin.ShowBuildIcons.Value)
        {
            return;
        }

        if (__instance.m_buildPieces != null)
        {
            __instance.UpdateAvailablePiecesList();
        }
    }
}

[HarmonyPatch(typeof(BuildUiPieceButton))]
internal static class BuildUiPieceButtonPatches
{
    internal const string UnbuiltBadgeName = "AchievementHelper_UnbuiltBadge";

    private static readonly Color BadgeBgColor = new(0.08f, 0.08f, 0.08f, 0.85f);
    private static readonly Color BadgeGoldColor = new(1f, 0.85f, 0.3f, 1f);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BuildUiPieceButton.Setup))]
    private static void Setup_Postfix(BuildUiPieceButton __instance, Piece pieceInfo)
    {
        bool showBadge = Plugin.ModEnabled.Value &&
                         Plugin.ShowBuildIcons.Value &&
                         AchievementService.IsPieceMissingForAchievement(pieceInfo);

        Transform? badge = __instance.transform.Find(UnbuiltBadgeName);
        if (!showBadge)
        {
            if (badge != null)
            {
                badge.gameObject.SetActive(false);
            }
            return;
        }

        if (badge == null)
        {
            TMP_Text? refText = Hud.instance != null ? Hud.instance.m_buildSelection : null;
            badge = CreateUnbuiltBadge(__instance.transform, refText);
        }

        badge.gameObject.SetActive(true);
        badge.SetAsLastSibling();
    }

    internal static Transform CreateUnbuiltBadge(Transform tileTransform, TMP_Text? referenceText)
    {
        GameObject badgeObj = new GameObject(UnbuiltBadgeName, typeof(RectTransform));
        badgeObj.transform.SetParent(tileTransform, false);

        RectTransform rt = (RectTransform)badgeObj.transform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(16f, 16f);
        rt.anchoredPosition = new Vector2(-2f, -2f);

        Image bg = badgeObj.AddComponent<Image>();
        bg.color = BadgeBgColor;
        bg.raycastTarget = false;

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(badgeObj.transform, false);

        RectTransform textRt = (RectTransform)textObj.transform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        if (referenceText != null)
        {
            text.font = referenceText.font;
            text.fontSharedMaterial = referenceText.fontSharedMaterial;
        }
        text.fontSize = 13f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = BadgeGoldColor;
        text.raycastTarget = false;
        text.text = "!";

        return badgeObj.transform;
    }
}

[HarmonyPatch(typeof(BuildUi))]
internal static class BuildUiFilterPatches
{
    private const string BuildFilterButtonName = "AchievementHelper_BuildUiFilterButton";

    private static FilterToggleButton? _filterButton;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BuildUi.OpenBuildMenu))]
    private static void OpenBuildMenu_Postfix(BuildUi __instance)
    {
        RefreshFilterButton(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BuildUi.UpdatePieceButtons))]
    private static void UpdatePieceButtons_Postfix(BuildUi __instance)
    {
        RefreshFilterButton(__instance);
    }

    private static void RefreshFilterButton(BuildUi buildUi)
    {
        bool show = Plugin.ModEnabled.Value &&
                    Plugin.ShowBuildIcons.Value &&
                    BuildingFilterState.TableHasBuildTargets &&
                    buildUi.m_currentBuildTool != null &&
                    !buildUi.m_currentBuildTool.m_hideAdvancedMenu;

        if (!show)
        {
            _filterButton?.SetVisible(false);
            return;
        }

        FilterToggleButton? button = EnsureFilterButton(buildUi);
        if (button == null)
        {
            return;
        }

        button.SetVisible(true);
        button.SetState($"Not Built ({BuildingFilterState.TotalMissingCount})", BuildingFilterState.FilterActive);
    }

    private static FilterToggleButton? EnsureFilterButton(BuildUi buildUi)
    {
        if (_filterButton != null && _filterButton.IsAlive)
        {
            return _filterButton;
        }

        Transform container = buildUi.m_tabContainer != null && buildUi.m_tabContainer.parent != null
            ? buildUi.m_tabContainer.parent
            : buildUi.transform;

        Button? template = InventoryGui.instance != null ? InventoryGui.instance.m_craftButton : null;
        if (template == null && buildUi.m_tabButtons.Count > 0)
        {
            template = buildUi.m_tabButtons[0];
        }

        _filterButton = FilterToggleButton.CreateOrAttach(
            container,
            BuildFilterButtonName,
            template,
            anchor: new Vector2(1f, 1f),
            pivot: new Vector2(1f, 0f),
            anchoredPosition: new Vector2(-8f, 4f),
            size: new Vector2(175f, 32f),
            onClick: () => BuildingFilterState.ToggleFilter(buildUi));

        return _filterButton;
    }
}

[HarmonyPatch(typeof(Hud))]
internal static class HudBuildingPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdatePieceList))]
    private static void UpdatePieceList_Postfix(Hud __instance, Player player)
    {
        if (player == null)
        {
            return;
        }

        List<Piece> buildPieces = player.GetBuildPieces();
        bool enabled = Plugin.ModEnabled.Value && Plugin.ShowBuildIcons.Value;

        for (int i = 0; i < __instance.m_pieceIcons.Count; i++)
        {
            Hud.PieceIconData iconData = __instance.m_pieceIcons[i];
            if (iconData?.m_go == null)
            {
                continue;
            }

            bool showBadge = enabled &&
                             i < buildPieces.Count &&
                             AchievementService.IsPieceMissingForAchievement(buildPieces[i]);

            Transform? badge = iconData.m_go.transform.Find(BuildUiPieceButtonPatches.UnbuiltBadgeName);
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
                badge = BuildUiPieceButtonPatches.CreateUnbuiltBadge(iconData.m_go.transform, __instance.m_buildSelection);
                badge.SetAsLastSibling();
            }

            if (!badge.gameObject.activeSelf)
            {
                badge.gameObject.SetActive(true);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.SetupPieceInfo))]
    private static void SetupPieceInfo_Postfix(Hud __instance, Piece piece)
    {
        if (!Plugin.ModEnabled.Value ||
            !Plugin.ShowBuildIcons.Value ||
            piece == null ||
            __instance.m_pieceDescription == null)
        {
            return;
        }

        if (!AchievementService.IsPieceMissingForAchievement(piece))
        {
            return;
        }

        string note = "<color=#FFD166>Not built for achievement</color>";
        string current = __instance.m_pieceDescription.text;
        __instance.m_pieceDescription.text = string.IsNullOrEmpty(current)
            ? note
            : current + "\n" + note;
    }
}
