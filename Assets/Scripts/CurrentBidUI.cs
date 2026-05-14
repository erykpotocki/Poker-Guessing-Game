using TMPro;
using UnityEngine;

public class CurrentBidUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TMP_Text currentBidText;

    [Header("Labels")]
    [SerializeField] private string emptyText = "";
    [SerializeField] private string prefix = "Na stole: ";

    [Header("Check banner")]
    [SerializeField] private string checkText = "SPRAWDZAM";
    [SerializeField] private bool animateCheckDots = true;
    [SerializeField] private int maxDots = 3;
    [SerializeField] private float dotsStepDuration = 0.35f;

    [Header("Check banner style")]
    [SerializeField] private int checkFontSize = 72;
    [SerializeField] private bool checkBold = true;
    [SerializeField] private string checkColorHex = "#FFFFFF";
    [SerializeField] private string checkedRankColorHex = "#FFD133";

    private bool isShowingCheck = false;
    private float dotsTimer = 0f;
    private int currentDots = 0;

    private void OnEnable()
    {
        TryResolveReferences();

        if (turnManager != null)
        {
            turnManager.OnCurrentBidDisplayChanged += HandleCurrentBidChanged;
        }

        RefreshNow();
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.OnCurrentBidDisplayChanged -= HandleCurrentBidChanged;
        }
    }

    private void Start()
    {
        RefreshNow();
    }

    private void Update()
    {
        if (!isShowingCheck || !animateCheckDots || currentBidText == null)
            return;

        dotsTimer += Time.deltaTime;

        if (dotsTimer >= dotsStepDuration)
        {
            dotsTimer = 0f;
            currentDots++;

            if (currentDots > maxDots)
                currentDots = 1;

            currentBidText.text = BuildCheckDisplayText(currentDots);
        }
    }

    private void TryResolveReferences()
    {
        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>(FindObjectsInactive.Include);
        }

        if (currentBidText == null)
        {
            currentBidText = GetComponent<TMP_Text>();
        }
    }

    private void RefreshNow()
    {
        if (currentBidText == null)
            return;

        if (turnManager == null)
        {
            isShowingCheck = false;
            currentBidText.text = emptyText;
            return;
        }

        ApplyDisplayValue(turnManager.CurrentBidDisplayText);
    }

    private void HandleCurrentBidChanged(string value)
    {
        if (currentBidText == null)
            return;

        ApplyDisplayValue(value);
    }

    private void ApplyDisplayValue(string value)
    {
        string trimmed = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        if (trimmed == "Sprawdzam")
        {
            isShowingCheck = true;
            dotsTimer = 0f;
            currentDots = animateCheckDots ? 1 : 0;
            currentBidText.text = BuildCheckDisplayText(currentDots);
            return;
        }

        isShowingCheck = false;
        currentBidText.text = FormatDisplayText(trimmed);
    }

    private string FormatDisplayText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return emptyText;

        if (value.StartsWith("<"))
            return value;

        if (value.StartsWith("Na stole:"))
            return value;

        return prefix + value;
    }

    private string BuildCheckDisplayText(int dotsCount)
    {
        string dots = "";
    
        for (int i = 0; i < dotsCount; i++)
        {
            dots += ".";
        }

        string declaredRankText = "";

        if (turnManager != null && !string.IsNullOrWhiteSpace(turnManager.CurrentDeclaredRankText))
        {
            declaredRankText = turnManager.CurrentDeclaredRankText.Trim();
        }

        string content = checkText;

        if (!string.IsNullOrWhiteSpace(declaredRankText))
        {
            content += " <color=" + checkedRankColorHex + ">" + declaredRankText + "</color>";
        }

        content += dots;

        string openTags = "";
        openTags += "<size=" + checkFontSize + ">";
        openTags += "<color=" + checkColorHex + ">";

        if (checkBold)
            openTags += "<b>";

        string closeTags = "";

        if (checkBold)
            closeTags += "</b>";

        closeTags += "</color>";
        closeTags += "</size>";

        return openTags + content + closeTags;
    }
}