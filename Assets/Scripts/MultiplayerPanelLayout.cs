using UnityEngine;
using UnityEngine.UI;

public sealed class MultiplayerPanelLayout : MonoBehaviour
{
    private RectTransform panel, root, action, list, exit;
    private GameUtilityBar utilityBar;
    private void Start()
    {
        panel = transform as RectTransform;
        root = GetComponentInParent<Canvas>().rootCanvas.transform as RectTransform;
        action = transform.Find("CheckButton") as RectTransform;
        list = transform.Find("RankScrollView") as RectTransform;
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentMethodName(i) == "OnClickOpenLeaveConfirm")
                    exit = button.transform as RectTransform;
        utilityBar = gameObject.AddComponent<GameUtilityBar>();
        utilityBar.Initialize(GetComponentInParent<Canvas>(), exit != null ? exit.GetComponent<Button>() : null);
        ApplyLayout();
    }

    private void LateUpdate() => ApplyLayout();
    private void ApplyLayout()
    {
        if (root == null || root.rect.width <= 0f || Screen.width <= 0) return;
        Rect safe = Screen.safeArea;
        float sx = root.rect.width / Screen.width;
        float sy = root.rect.height / Screen.height;
        float right = (Screen.width - safe.xMax) * sx + 26f;
        float top = (Screen.height - safe.yMax) * sy + 26f;
        float bottom = safe.yMin * sy + 26f;
        float width = 364.52f;
        panel.anchorMin = new Vector2(1f, 0f);
        panel.anchorMax = Vector2.one;
        panel.pivot = new Vector2(1f, 0.5f);
        panel.offsetMin = new Vector2(-width, 0f);
        panel.offsetMax = Vector2.zero;
        if (action != null)
        {
            action.anchorMin = Vector2.zero;
            action.anchorMax = new Vector2(1f, 0f);
            action.pivot = new Vector2(0.5f, 0f);
            action.offsetMin = new Vector2(14f, bottom);
            action.offsetMax = new Vector2(-14f, bottom + 78f);
            TMPro.TMP_Text label = action.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (label != null) CenterLabel(label);
        }
        if (list != null)
        {
            list.anchorMin = Vector2.zero;
            list.anchorMax = Vector2.one;
            list.offsetMin = new Vector2(14f, bottom + 104f);
            list.offsetMax = new Vector2(-14f, -top - 90f);
            ScrollRect scroll = list.GetComponent<ScrollRect>();
            if (scroll != null && scroll.viewport != null)
            {
                scroll.viewport.anchorMin = Vector2.zero;
                scroll.viewport.anchorMax = Vector2.one;
                scroll.viewport.offsetMin = scroll.viewport.offsetMax = Vector2.zero;
            }
            foreach (VerticalLayoutGroup layout in list.GetComponentsInChildren<VerticalLayoutGroup>(true))
            {
                layout.padding.left = layout.padding.right = 0;
                layout.childControlWidth = true;
                layout.childForceExpandWidth = true;
                RectTransform rect = layout.transform as RectTransform;
                rect.anchorMin = new Vector2(0f, rect.anchorMin.y);
                rect.anchorMax = new Vector2(1f, rect.anchorMax.y);
                rect.sizeDelta = new Vector2(0f, rect.sizeDelta.y);
                rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
            }
            foreach (Button button in list.GetComponentsInChildren<Button>(true))
            {
                LayoutElement element = button.GetComponent<LayoutElement>();
                if (element != null) { element.minWidth = 0f; element.preferredWidth = -1f; element.flexibleWidth = 1f; }
                TMPro.TMP_Text label = button.GetComponentInChildren<TMPro.TMP_Text>(true);
                if (label != null) CenterLabel(label);
            }
        }
        if (utilityBar != null) utilityBar.SetBounds(top, right);
    }

    private static void CenterLabel(TMPro.TMP_Text label)
    {
        RectTransform rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.offsetMin = new Vector2(8f, 4f);
        rect.offsetMax = new Vector2(-8f, -4f);
        label.margin = Vector4.zero;
        label.alignment = TMPro.TextAlignmentOptions.Center;
        label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
    }
}
