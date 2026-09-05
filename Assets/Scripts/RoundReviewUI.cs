using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RoundReviewUI : MonoBehaviour
{
    [Serializable] public class Entry
    {
        public int round;
        public string declaration;
        public string strongest;
        public bool exists;
        public int loser;
        public int[] participants;
        public int[] cards;
        public int[] matching;
        public int[] cardOwners;
        public string[] participantNames;
    }
    [Serializable] private class History { public List<Entry> rounds = new List<Entry>(); }
    public const string HistoryKey = "roundReviewHistoryV1";
    private History history = new History();
    private RectTransform canvasRect;
    private Button readyButton;
    private Button historyButton;
    private GameObject overlay;
    private TurnManager manager;
    private bool dealerAttached;

    public Entry Latest => history.rounds.Count == 0 ? null : history.rounds[history.rounds.Count - 1];

    public void Initialize(TurnManager turnManager)
    {
        manager = turnManager;
        HandRankPanelUI handPanel = FindFirstObjectByType<HandRankPanelUI>(FindObjectsInactive.Include);
        Canvas canvas = handPanel != null ? handPanel.GetComponentInParent<Canvas>() : FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        canvasRect = canvas.rootCanvas.transform as RectTransform;
        readyButton = MakeButton("RoundReady", canvasRect, "GOTOWY", manager.MarkRoundReady);
        RectTransform ready = readyButton.transform as RectTransform;
        ready.anchorMin = ready.anchorMax = new Vector2(1f, 0f);
        ready.pivot = new Vector2(1f, 0f);
        ready.anchoredPosition = new Vector2(-14f, 32f);
        ready.sizeDelta = new Vector2(336f, 78f);
        readyButton.gameObject.SetActive(false);
        historyButton = MakeButton("RoundHistory", canvasRect, "≡", ShowHistory);
        RectTransform historyRect = historyButton.transform as RectTransform;
        historyRect.anchorMin = historyRect.anchorMax = new Vector2(0.56f, 0.86f);
        historyRect.sizeDelta = new Vector2(72f, 64f);
        LoadHistory();
    }

    private void Update()
    {
        if (readyButton != null && canvasRect != null && Screen.height > 0)
            (readyButton.transform as RectTransform).anchoredPosition = new Vector2(-14f,
                Screen.safeArea.yMin * canvasRect.rect.height / Screen.height + 26f);
        if (dealerAttached || historyButton == null) return;
        foreach (SeatUIView seat in FindObjectsByType<SeatUIView>(FindObjectsSortMode.None))
        {
            if (seat.name != "Seat_Dealer") continue;
            RectTransform rect = historyButton.transform as RectTransform;
            rect.SetParent(seat.transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(116f, 12f);
            dealerAttached = true;
            break;
        }
    }

    public void LoadHistory()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(HistoryKey, out object raw) && raw is string json)
        {
            History loaded = JsonUtility.FromJson<History>(json);
            if (loaded != null && loaded.rounds != null) history = loaded;
        }
    }

    public void Record(int round, string declaration, string id, bool exists, int loser,
        List<int> participants, List<CardSpriteEntry> cards, Dictionary<int, List<CardSpriteEntry>> hands)
    {
        List<CardSpriteEntry> matches = MultiplayerHandRules.MatchingCards(id, cards, out _);
        Entry entry = new Entry
        {
            round = round, declaration = declaration, exists = exists, loser = loser,
            participants = participants.ToArray(),
            strongest = HandRankCatalog.GetDisplayName(MultiplayerHandRules.Strongest(cards)),
            cards = cards.ConvertAll(Code).ToArray(), matching = matches.ConvertAll(Code).ToArray()
        };
        List<int> cardCodes = new List<int>();
        List<int> owners = new List<int>();
        entry.participantNames = new string[participants.Count];
        for (int i = 0; i < participants.Count; i++)
        {
            int actor = participants[i];
            entry.participantNames[i] = LobbyBotRegistry.TryGetBot(actor, out LobbyBotInfo bot) ? bot.Name :
                PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Players.TryGetValue(actor, out var player) ? player.NickName : "Gracz " + actor;
            if (!hands.TryGetValue(actor, out List<CardSpriteEntry> hand)) continue;
            foreach (CardSpriteEntry card in hand) { cardCodes.Add(Code(card)); owners.Add(actor); }
        }
        if (cardCodes.Count == cards.Count) { entry.cards = cardCodes.ToArray(); entry.cardOwners = owners.ToArray(); }
        history.rounds.RemoveAll(item => item.round == round);
        history.rounds.Add(entry);
    }

    public string SerializeHistory() => JsonUtility.ToJson(history);

    public void SetReviewVisible(bool visible, bool ready = false)
    {
        if (readyButton == null) return;
        readyButton.gameObject.SetActive(visible);
        readyButton.interactable = !ready;
        readyButton.GetComponentInChildren<TMP_Text>().text = "GOTOWY";
        // Browsing history remains independent of readiness and new deals.
    }

    private static int Code(CardSpriteEntry card) => (int)card.rank * 4 + (int)card.suit;

    private void ShowHistory()
    {
        LoadHistory();
        if (overlay != null) Destroy(overlay);
        overlay = new GameObject("RoundHistoryOverlay", typeof(RectTransform), typeof(Image));
        RectTransform panel = overlay.GetComponent<RectTransform>();
        panel.SetParent(canvasRect, false);
        Stretch(panel);
        Canvas modalCanvas = overlay.AddComponent<Canvas>();
        modalCanvas.overrideSorting = true;
        modalCanvas.sortingOrder = 200;
        overlay.AddComponent<GraphicRaycaster>();
        overlay.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.025f, 1f);
        TMP_Text title = MakeText(panel, "HISTORIA RUND", 36f);
        title.rectTransform.anchorMin = new Vector2(0.05f, 0.84f);
        title.rectTransform.anchorMax = new Vector2(0.78f, 0.97f);
        title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
        Button close = MakeButton("HistoryClose", panel, "ZAMKNIJ", () => overlay.SetActive(false));
        RectTransform closeRect = close.transform as RectTransform;
        closeRect.anchorMin = new Vector2(0.8f, 0.85f);
        closeRect.anchorMax = new Vector2(0.95f, 0.96f);
        closeRect.offsetMin = closeRect.offsetMax = Vector2.zero;

        GameObject viewport = new GameObject("HistoryViewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
        RectTransform view = viewport.GetComponent<RectTransform>();
        view.SetParent(panel, false);
        view.anchorMin = new Vector2(0.05f, 0.08f);
        view.anchorMax = new Vector2(0.95f, 0.82f);
        view.offsetMin = view.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = Color.clear;
        GameObject content = new GameObject("HistoryRows", typeof(RectTransform));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.SetParent(view, false);
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, Mathf.Max(200f, history.rounds.Count * 320f));
        ScrollRect scroll = viewport.GetComponent<ScrollRect>();
        scroll.content = contentRect;
        scroll.viewport = view;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 55f;
        CardDatabase database = FindFirstObjectByType<CardDatabase>();
        CardSpriteEntry[] deck = database != null ? database.GetMultiplayerCards() : null;
        deck = deck ?? Array.Empty<CardSpriteEntry>();
        if (history.rounds.Count == 0)
        {
            TMP_Text empty = MakeText(contentRect, "Brak zakończonych rund.", 32f);
            Stretch(empty.rectTransform);
        }
        for (int i = 0; i < history.rounds.Count; i++)
        {
            Entry entry = history.rounds[history.rounds.Count - 1 - i];
            GameObject row = new GameObject("Round_" + entry.round, typeof(RectTransform));
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.SetParent(contentRect, false);
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = Vector2.one;
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, -i * 320f);
            rowRect.sizeDelta = new Vector2(0f, 300f);
            string result = entry.exists ? "UKŁAD BYŁ" : "UKŁADU NIE BYŁO";
            TMP_Text label = MakeText(rowRect, "RUNDA " + entry.round + " • " + entry.declaration +
                " • " + result + "\nNajwyższy układ w kartach: " + entry.strongest, 30f);
            label.rectTransform.anchorMin = new Vector2(0f, 0.68f);
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            bool hasOwners = entry.cardOwners != null && entry.cardOwners.Length == entry.cards.Length &&
                entry.participants != null && entry.participants.Length > 0;
            if (hasOwners)
                for (int p = 0; p < entry.participants.Length; p++)
                {
                    string name = entry.participantNames != null && p < entry.participantNames.Length ? entry.participantNames[p] : "Gracz " + entry.participants[p];
                    TMP_Text owner = MakeText(rowRect, name, 26f);
                    owner.richText = false;
                    owner.alignment = TextAlignmentOptions.Center;
                    owner.enableAutoSizing = true;
                    owner.fontSizeMin = 20f; owner.fontSizeMax = 26f;
                    owner.textWrappingMode = TextWrappingModes.NoWrap;
                    owner.rectTransform.anchorMin = new Vector2((float)p / entry.participants.Length, 0.51f);
                    owner.rectTransform.anchorMax = new Vector2((float)(p + 1) / entry.participants.Length, 0.65f);
                    owner.rectTransform.offsetMin = owner.rectTransform.offsetMax = Vector2.zero;
                }
            for (int c = 0; c < entry.cards.Length; c++)
            {
                CardSpriteEntry card = Array.Find(deck, item => item != null && Code(item) == entry.cards[c]);
                if (card == null) continue;
                GameObject cardObject = new GameObject("Card", typeof(RectTransform), typeof(Image));
                Image image = cardObject.GetComponent<Image>();
                RectTransform rect = image.rectTransform;
                rect.SetParent(rowRect, false);
                rect.anchorMin = new Vector2((float)c / entry.cards.Length, 0f);
                rect.anchorMax = new Vector2((float)(c + 1) / entry.cards.Length, 0.57f);
                if (hasOwners)
                {
                    int actor = entry.cardOwners[c];
                    int playerIndex = Array.IndexOf(entry.participants, actor);
                    int handCount = 0, handIndex = 0;
                    for (int j = 0; j < entry.cardOwners.Length; j++)
                        if (entry.cardOwners[j] == actor) { handCount++; if (j < c) handIndex++; }
                    float columns = entry.participants.Length;
                    rect.anchorMin = new Vector2((playerIndex + (float)handIndex / handCount) / columns, 0f);
                    rect.anchorMax = new Vector2((playerIndex + (float)(handIndex + 1) / handCount) / columns, 0.49f);
                }
                rect.offsetMin = new Vector2(3f, 2f);
                rect.offsetMax = new Vector2(-3f, -2f);
                image.sprite = card.sprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                if (Array.IndexOf(entry.matching, entry.cards[c]) >= 0)
                {
                    Outline outline = cardObject.AddComponent<Outline>();
                    outline.effectColor = entry.exists ? new Color(0.3f, 1f, 0.45f) : new Color(1f, 0.78f, 0.2f);
                    outline.effectDistance = new Vector2(3f, -3f);
                }
            }
        }
    }

    private static Button MakeButton(string name, Transform parent, string caption, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        Button button = obj.GetComponent<Button>();
        button.targetGraphic = obj.GetComponent<Image>();
        TMP_Text text = MakeText(obj.transform, caption, 36f);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.Center;
        button.onClick.AddListener(action);
        PokerButtonTheme.ApplyTo(button);
        return button;
    }

    private static TMP_Text MakeText(Transform parent, string value, float size)
    {
        GameObject obj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = size;
        text.color = new Color(1f, 0.91f, 0.7f);
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (overlay != null) Destroy(overlay);
        if (readyButton != null) Destroy(readyButton.gameObject);
        if (historyButton != null) Destroy(historyButton.gameObject);
    }
}
