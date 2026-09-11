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
    private int previousCueActor = -1;
    private int previousCueRound = -1;

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
        float timerWidth = Mathf.Min(230f, width * 0.28f);
        float timerLeft = Mathf.Max(0,width-timerWidth-12);
        ConfigureStatusLabel(turnTimerText, timerLeft, 6f, timerWidth);
        if (turnText.transform.parent != toolbarStatus) turnText.transform.SetParent(toolbarStatus, false);
        float statusWidth=Mathf.Max(1,timerLeft-24);
        Transform table=toolbarStatus.GetComponentInParent<Canvas>().rootCanvas.transform.Find("TablePresentation/Table");
        float center=table!=null?toolbarStatus.InverseTransformPoint(table.TransformPoint(((RectTransform)table).rect.center)).x:statusWidth*.5f;
        float half=Mathf.Max(1,Mathf.Min(260,Mathf.Min(center,statusWidth-center)));
        ConfigureStatusLabel(turnText, center-half, 6f, half*2);
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
        CasinoAudio.SetUrgency(false);
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
            turnText.text = "TWOJA TURA";
            if (previousCueActor != actorNumber || previousCueRound != turnManager.CurrentRoundNumber)
                CasinoAudio.PlayLocalTurn();
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

            turnText.text = "TURA: " + playerDisplayName;
        }

        previousCueActor = actorNumber;
        previousCueRound = turnManager.CurrentRoundNumber;

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
        CasinoAudio.SetUrgency(currentTimeLeft<=0 && !turnManager.IsResolutionLocked && PhotonNetwork.LocalPlayer!=null && turnManager.CurrentPlayerActorNumber==PhotonNetwork.LocalPlayer.ActorNumber);

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
