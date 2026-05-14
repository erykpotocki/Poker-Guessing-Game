using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundLogUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewportRoot;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TMP_Text logText;

    [Header("Text")]
    [SerializeField, Min(1f)] private float logFontSize = 24f;

    [Header("Scroll")]
    [SerializeField, Range(0f, 0.25f)] private float autoFollowThreshold = 0.06f;
    [SerializeField] private bool lockHorizontalScrolling = true;
    [SerializeField] private int forceBottomFrames = 3;

    [Header("Layout")]
    [SerializeField] private float topPadding = 8f;
    [SerializeField] private float bottomPadding = 8f;
    [SerializeField] private float horizontalPadding = 8f;

    [Header("Colors")]
    [SerializeField] private Color checkActionColor = new Color(1f, 0.75f, 0.2f, 1f);

    private readonly List<string> lines = new List<string>();

    private bool pendingRefresh = false;
    private bool forceScrollOnRefresh = false;
    private Coroutine forceBottomCoroutine;

    private void Awake()
    {
        ResolveRefs();
        ConfigureStaticSettings();
        QueueRefresh(true);
    }

    private void OnEnable()
    {
        ResolveRefs();
        ConfigureStaticSettings();
        QueueRefresh(true);
    }

    private void LateUpdate()
    {
        if (!pendingRefresh)
            return;

        pendingRefresh = false;
        RefreshVisuals();
    }

    public void ClearLog()
    {
        lines.Clear();
        QueueRefresh(true);
    }

    public void AddSystemMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        string trimmed = text.Trim();
        trimmed = NormalizePlayerNamesForLog(trimmed);

        if (trimmed.Equals("Start", StringComparison.OrdinalIgnoreCase))
        {
            AppendLine(WrapWithColor("<b>Start</b>", checkActionColor), true);
            return;
        }

        if (IsEliminationMessage(trimmed))
        {
            AppendLine(WrapWithColor(trimmed, checkActionColor), false);
            return;
        }

        AppendLine(trimmed, false);
    }

    public void AddRoundHeader(int roundNumber)
    {
        if (lines.Count > 0)
        {
            lines.Add(string.Empty);
        }

        string header = "<b>Runda " + roundNumber + "</b>";
        AppendLine(WrapWithColor(header, checkActionColor), true);
    }

    public void AddPlayerRaise(string playerName, string selectedRankText)
    {
        string safePlayerName = EscapeRichText(playerName).ToUpperInvariant();
        string safeRankText = EscapeRichText(selectedRankText);

        AppendLine("<b>" + safePlayerName + "</b>: wybiera " + safeRankText, false);
    }

    public void AddPlayerCheck(string playerName)
    {
        string safePlayerName = EscapeRichText(playerName).ToUpperInvariant();
        string coloredCheckText = WrapWithColor("SPRAWDZAM", checkActionColor);

        AppendLine("<b>" + safePlayerName + "</b>: wybiera " + coloredCheckText, false);
    }

    public void AddPlayerLeftGame(string playerName)
    {
        string safePlayerName = EscapeRichText(playerName).ToUpperInvariant();
        AppendLine("<b>" + safePlayerName + "</b> wyszedł z gry", false);
    }

    public void AddPlayerRejoinedGame(string playerName)
    {
        string safePlayerName = EscapeRichText(playerName).ToUpperInvariant();
        AppendLine("<b>" + safePlayerName + "</b> wrócił do gry", false);
    }

    private void AppendLine(string line, bool forceScroll)
    {
        bool shouldFollow = forceScroll || IsNearBottom() || lines.Count == 0;
        lines.Add(line);
        QueueRefresh(shouldFollow);
    }

    private void QueueRefresh(bool forceScroll)
    {
        pendingRefresh = true;

        if (forceScroll)
            forceScrollOnRefresh = true;
    }

    private void RefreshVisuals()
    {
        ResolveRefs();
        ConfigureStaticSettings();

        if (scrollRect == null || contentRoot == null || logText == null)
            return;

        logText.text = string.Join("\n", lines);
        logText.ForceMeshUpdate();

        Canvas.ForceUpdateCanvases();
        RebuildBottomLayout();

        if (lockHorizontalScrolling)
            ForceHorizontalLocked();

        if (forceScrollOnRefresh)
        {
            if (forceBottomCoroutine != null)
                StopCoroutine(forceBottomCoroutine);

            forceBottomCoroutine = StartCoroutine(ForceBottomRoutine());
        }

        forceScrollOnRefresh = false;
    }

    private void RebuildBottomLayout()
    {
        if (scrollRect == null || contentRoot == null || logText == null)
            return;

        RectTransform logRect = logText.rectTransform;

        ConfigureBottomStretch(contentRoot);
        ConfigureBottomStretch(logRect);

        float viewportHeight = GetViewportHeight();
        float availableWidth = GetAvailableTextWidth();

        Vector2 preferred = logText.GetPreferredValues(logText.text, availableWidth, 0f);
        float textHeight = Mathf.Ceil(preferred.y);

        float contentHeight = Mathf.Max(viewportHeight, textHeight + topPadding + bottomPadding);

        contentRoot.offsetMin = new Vector2(0f, 0f);
        contentRoot.offsetMax = new Vector2(0f, contentHeight);

        logRect.offsetMin = new Vector2(horizontalPadding, bottomPadding);
        logRect.offsetMax = new Vector2(-horizontalPadding, bottomPadding + textHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(logRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator ForceBottomRoutine()
    {
        int frames = Mathf.Max(1, forceBottomFrames);

        for (int i = 0; i < frames; i++)
        {
            yield return null;

            ResolveRefs();
            ConfigureStaticSettings();

            Canvas.ForceUpdateCanvases();
            RebuildBottomLayout();

            if (scrollRect != null)
            {
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = 0f;
            }

            if (lockHorizontalScrolling)
                ForceHorizontalLocked();
        }

        forceBottomCoroutine = null;
    }

    private void ForceHorizontalLocked()
    {
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.horizontalNormalizedPosition = 0f;
        }

        if (contentRoot != null)
        {
            Vector2 anchored = contentRoot.anchoredPosition;
            anchored.x = 0f;
            contentRoot.anchoredPosition = anchored;
        }
    }

    private bool IsNearBottom()
    {
        if (scrollRect == null)
            return true;

        return scrollRect.verticalNormalizedPosition <= autoFollowThreshold;
    }

    private float GetViewportHeight()
    {
        if (viewportRoot != null)
            return viewportRoot.rect.height;

        if (scrollRect != null && scrollRect.viewport != null)
            return scrollRect.viewport.rect.height;

        if (scrollRect != null)
            return ((RectTransform)scrollRect.transform).rect.height;

        return 0f;
    }

    private float GetAvailableTextWidth()
    {
        float width = 300f;

        if (viewportRoot != null)
            width = viewportRoot.rect.width;
        else if (scrollRect != null && scrollRect.viewport != null)
            width = scrollRect.viewport.rect.width;
        else if (scrollRect != null)
            width = ((RectTransform)scrollRect.transform).rect.width;

        width -= horizontalPadding * 2f;
        return Mathf.Max(50f, width);
    }

    private void ResolveRefs()
    {
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>(true);

        if (viewportRoot == null && scrollRect != null)
            viewportRoot = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

        if (contentRoot == null && scrollRect != null)
            contentRoot = scrollRect.content;

        if (logText == null && contentRoot != null)
            logText = contentRoot.GetComponentInChildren<TMP_Text>(true);
    }

    private void ConfigureStaticSettings()
    {
        if (scrollRect != null)
        {
            scrollRect.vertical = true;
            scrollRect.horizontal = !lockHorizontalScrolling ? scrollRect.horizontal : false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
        }

        if (logText != null)
        {
            logText.fontSize = logFontSize;
            logText.richText = true;
            logText.textWrappingMode = TextWrappingModes.Normal;
            logText.overflowMode = TextOverflowModes.Overflow;
            logText.alignment = TextAlignmentOptions.BottomLeft;
        }
    }

    private void ConfigureBottomStretch(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
    }

    private bool IsEliminationMessage(string text)
    {
        return text.IndexOf("odpada z gry", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private string NormalizePlayerNamesForLog(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        string result = text;

        result = UppercaseBoldTagContents(result);
        result = BoldAndUppercaseLeadingNameBeforeKeyword(result, " przegrywa");
        result = BoldAndUppercaseLeadingNameBeforeKeyword(result, " ma teraz ");
        result = BoldAndUppercaseNameAfterPrefix(result, "Koniec gry. Wygrywa: ");

        return result;
    }

    private string UppercaseBoldTagContents(string text)
    {
        return Regex.Replace(
            text,
            "<b>(.*?)</b>",
            match => "<b>" + match.Groups[1].Value.ToUpperInvariant() + "</b>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
    }

    private string BoldAndUppercaseLeadingNameBeforeKeyword(string text, string keyword)
    {
        int keywordIndex = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (keywordIndex <= 0)
            return text;

        string candidate = text.Substring(0, keywordIndex);

        if (candidate.Contains("<") || candidate.Contains(">"))
            return text;

        return "<b>" + EscapeRichText(candidate).ToUpperInvariant() + "</b>" + text.Substring(keywordIndex);
    }

    private string BoldAndUppercaseNameAfterPrefix(string text, string prefix)
    {
        int prefixIndex = text.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (prefixIndex < 0)
            return text;

        int nameStartIndex = prefixIndex + prefix.Length;
        if (nameStartIndex >= text.Length)
            return text;

        string namePart = text.Substring(nameStartIndex);

        if (namePart.Contains("<") || namePart.Contains(">"))
            return text;

        return text.Substring(0, nameStartIndex) + "<b>" + EscapeRichText(namePart).ToUpperInvariant() + "</b>";
    }

    private string EscapeRichText(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private string WrapWithColor(string text, Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGBA(color);
        return "<color=#" + hex + ">" + text + "</color>";
    }
}