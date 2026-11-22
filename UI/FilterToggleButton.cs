using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementHelper.UI;

internal sealed class FilterToggleButton
{
    private static readonly Color ActiveTextColor = new(1f, 0.88f, 0.42f, 1f);
    private static readonly Color InactiveTextColor = new(0.9f, 0.9f, 0.9f, 1f);

    private static readonly Color ActiveButtonColor = new(1f, 0.78f, 0.32f, 1f);
    private static readonly Color ActiveButtonHoverColor = new(1f, 0.86f, 0.46f, 1f);

    private readonly Button _button;
    private readonly TMP_Text? _label;
    private readonly ColorBlock _defaultColors;
    private readonly Color _defaultGraphicColor;

    private string? _lastText;
    private bool? _lastActive;

    private FilterToggleButton(Button button)
    {
        _button = button;
        _label = button.GetComponentInChildren<TMP_Text>(true);
        _defaultColors = button.colors;
        _defaultGraphicColor = button.targetGraphic != null ? button.targetGraphic.color : Color.white;
    }

    public bool IsAlive => _button != null;

    public static FilterToggleButton? CreateOrAttach(
        Transform parent,
        string name,
        Button? template,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size,
        Action onClick)
    {
        Transform? existing = parent.Find(name);
        if (existing != null)
        {
            Button existingBtn = existing.GetComponent<Button>();
            if (existingBtn != null)
            {
                WireClick(existingBtn, onClick);
                return new FilterToggleButton(existingBtn);
            }
        }

        if (template == null)
        {
            return null;
        }

        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent);
        clone.name = name;
        clone.SetActive(true);
        clone.transform.SetAsLastSibling();

        for (int i = clone.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = clone.transform.GetChild(i);
            if (child.GetComponent<TMP_Text>() == null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        LayoutElement layoutElement = clone.GetComponent<LayoutElement>() ?? clone.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        if (clone.transform is RectTransform rt)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPosition;
        }

        Button button = clone.GetComponent<Button>();
        button.interactable = true;
        WireClick(button, onClick);

        TMP_Text? label = clone.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            if (label.transform is RectTransform labelRt)
            {
                labelRt.anchorMin = Vector2.zero;
                labelRt.anchorMax = Vector2.one;
                labelRt.offsetMin = new Vector2(6f, 2f);
                labelRt.offsetMax = new Vector2(-6f, -2f);
            }
            label.enableAutoSizing = false;
            label.fontSize = 15f;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
        }

        return new FilterToggleButton(button);
    }

    public void SetVisible(bool visible)
    {
        if (_button == null)
        {
            return;
        }

        if (_button.gameObject.activeSelf != visible)
        {
            _button.gameObject.SetActive(visible);
        }

        if (visible)
        {
            _button.transform.SetAsLastSibling();
        }
    }

    public void SetState(string text, bool active)
    {
        if (_button == null)
        {
            return;
        }

        if (_lastText != text)
        {
            _lastText = text;
            if (_label != null)
            {
                _label.text = text;
            }
        }

        if (_lastActive != active)
        {
            _lastActive = active;
            ApplyVisualState(active);
        }
    }

    private void ApplyVisualState(bool active)
    {
        if (_label != null)
        {
            _label.color = active ? ActiveTextColor : InactiveTextColor;
        }

        if (active)
        {
            ColorBlock colors = _defaultColors;
            colors.normalColor = ActiveButtonColor;
            colors.highlightedColor = ActiveButtonHoverColor;
            colors.selectedColor = ActiveButtonColor;
            _button.colors = colors;

            if (_button.targetGraphic != null && _button.transition != Selectable.Transition.ColorTint)
            {
                _button.targetGraphic.color = ActiveButtonColor;
            }
        }
        else
        {
            _button.colors = _defaultColors;
            if (_button.targetGraphic != null && _button.transition != Selectable.Transition.ColorTint)
            {
                _button.targetGraphic.color = _defaultGraphicColor;
            }
        }
    }

    private static void WireClick(Button button, Action onClick)
    {
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() => onClick());
    }
}
