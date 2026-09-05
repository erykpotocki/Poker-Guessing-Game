using Photon.Pun;
using TMPro;
using UnityEngine;

public class TurnDebugUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text turnTimerText;

    [Header("Timer thresholds")]
    [SerializeField] private float warningThresholdSeconds = 10f;

    [Header("Timer colors")]
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = new Color(1f, 0.65f, 0.25f, 1f);
    [SerializeField] private Color overtimeTimerColor = Color.red;

    [Header("Pulse settings")]
    [SerializeField] private float warningPulseSpeed = 1.2f;
    [SerializeField] private float warningPulseScaleAmount = 0.035f;
    [SerializeField] private float overtimePulseSpeed = 6f;
    [SerializeField] private float overtimePulseScaleAmount = 0.15f;

    private Vector3 timerBaseScale = Vector3.one;
    private RectTransform toolbarStatus;

    public void AttachToToolbar(RectTransform parent)
    {
        toolbarStatus = parent;
        if (turnTimerText != null) turnTimerText.transform.SetParent(parent, false);
        if (turnText != null) turnText.transform.SetParent(parent, false);
        normalTimerColor = new Color(0.85f, 0.81f, 0.7f);
        if (turnText != null) turnText.color = normalTimerColor;
        LayoutStatusLabels();
    }

    public void LayoutStatusLabels()
    {
        if (turnTimerText == null || turnText == null || toolbarStatus == null) return;
        float width = toolbarStatus.rect.width;
        float timerWidth = Mathf.Min(230f, width * 0.32f);
        float timerLeft = Mathf.Min(90f, width * 0.08f);
        ConfigureStatusLabel(turnTimerText, timerLeft, 6f, timerWidth);
        float turnLeft = Mathf.Max(timerLeft + timerWidth + 32f, width * 0.34f);
        ConfigureStatusLabel(turnText, turnLeft, 6f, Mathf.Max(1f, width - turnLeft));
        turnText.alignment = TextAlignmentOptions.Center;
    }

    private static void ConfigureStatusLabel(TMP_Text label, float x, float y, float width)
    {
        RectTransform rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, 56f);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.enableAutoSizing = true;
        label.fontSizeMin = 24f;
        label.fontSizeMax = 30f;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.margin = Vector4.zero;
        label.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void OnEnable()
    {
        if (turnManager != null)
            turnManager.OnActivePlayerChanged += HandleActivePlayerChanged;
    }

    private void OnDisable()
    {
        if (turnManager != null)
            turnManager.OnActivePlayerChanged -= HandleActivePlayerChanged;
    }

    private void Start()
    {
        LayoutStatusLabels();
        if (turnTimerText != null)
        {
            timerBaseScale = turnTimerText.rectTransform.localScale;
            turnTimerText.color = normalTimerColor;
            turnTimerText.rectTransform.localScale = timerBaseScale;
        }

        RefreshNow();
    }

    private void Update()
    {
        if (turnManager == null || !turnManager.IsInitialized)
            return;

        UpdateText(turnManager.CurrentPlayerActorNumber);
    }

    public void RefreshNow()
    {
        if (turnText == null)
            return;

        if (turnManager == null || !turnManager.IsInitialized)
        {
            turnText.text = string.Empty;

            if (turnTimerText != null)
            {
                turnTimerText.text = string.Empty;
                turnTimerText.color = normalTimerColor;
                turnTimerText.rectTransform.localScale = timerBaseScale;
            }

            return;
        }

        UpdateText(turnManager.CurrentPlayerActorNumber);
    }

    private void HandleActivePlayerChanged(int actorNumber)
    {
        if (turnTimerText != null)
        {
            turnTimerText.color = normalTimerColor;
            turnTimerText.rectTransform.localScale = timerBaseScale;
        }

        UpdateText(actorNumber);
    }

    private void UpdateText(int actorNumber)
    {
        if (turnText == null)
            return;

        if (turnManager != null && turnManager.IsResolutionLocked)
        {
            turnText.text = turnManager.IsAwaitingRoundReady ? "Czekamy na gotowość" : "";
            UpdateTimerVisuals();
            return;
        }

        if (actorNumber <= 0)
        {
            turnText.text = "Tura: brak aktywnego gracza";

            if (turnTimerText != null)
                turnTimerText.text = "Czas: ---";

            return;
        }

        bool isLocalTurn =
            PhotonNetwork.LocalPlayer != null &&
            actorNumber == PhotonNetwork.LocalPlayer.ActorNumber;

        if (isLocalTurn)
        {
            turnText.text = "Twoja tura";
        }
        else
        {
            string playerDisplayName = "Gracz " + actorNumber;

            if (LobbyBotRegistry.TryGetBot(actorNumber, out LobbyBotInfo bot))
            {
                playerDisplayName = bot.Name;
            }
            else if (PhotonNetwork.CurrentRoom != null &&
                PhotonNetwork.CurrentRoom.Players != null &&
                PhotonNetwork.CurrentRoom.Players.TryGetValue(actorNumber, out Photon.Realtime.Player player) &&
                player != null &&
                !string.IsNullOrWhiteSpace(player.NickName))
            {
                playerDisplayName = player.NickName;
            }

            turnText.text = "Ruch gracza " + playerDisplayName;
        }

        UpdateTimerVisuals();
    }

    private void UpdateTimerVisuals()
    {
        if (turnTimerText == null)
            return;

        if (turnManager == null || !turnManager.IsInitialized)
        {
            turnTimerText.text = string.Empty;
            turnTimerText.color = normalTimerColor;
            turnTimerText.rectTransform.localScale = timerBaseScale;
            return;
        }

        float currentTimeLeft = turnManager.CurrentTurnTimeLeft;

        int displaySeconds = currentTimeLeft > 0f
            ? Mathf.CeilToInt(currentTimeLeft)
            : -Mathf.FloorToInt(Mathf.Abs(currentTimeLeft));

        turnTimerText.text = "Czas: " + displaySeconds + " s";

        if (turnManager.IsResolutionLocked)
        {
            turnTimerText.text = turnManager.IsDealingCards ? string.Empty : "Czas: pauza";
            turnTimerText.color = normalTimerColor;
            turnTimerText.rectTransform.localScale = timerBaseScale;
            return;
        }

        if (currentTimeLeft > warningThresholdSeconds)
        {
            turnTimerText.color = normalTimerColor;
            turnTimerText.rectTransform.localScale = timerBaseScale;
            return;
        }

        if (currentTimeLeft > 0f)
        {
            turnTimerText.color = warningTimerColor;

            float pulse = 1f + Mathf.Abs(Mathf.Sin(Time.time * warningPulseSpeed)) * warningPulseScaleAmount;
            turnTimerText.rectTransform.localScale = timerBaseScale * pulse;
            return;
        }

        turnTimerText.color = overtimeTimerColor;

        float overtimePulse = 1f + Mathf.Abs(Mathf.Sin(Time.time * overtimePulseSpeed)) * overtimePulseScaleAmount;
        turnTimerText.rectTransform.localScale = timerBaseScale * overtimePulse;
    }
}
