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
        public string checkerName, declarerName;
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
        List<int> participants, List<CardSpriteEntry> cards, Dictionary<int, List<CardSpriteEntry>> hands, int checker, int declarer)
    {
        List<CardSpriteEntry> matches = MultiplayerHandRules.MatchingCards(id, cards, out _);
        Entry entry = new Entry
        {
            round = round, declaration = declaration, exists = exists, loser = loser,
            checkerName = PlayerName(checker), declarerName = PlayerName(declarer),
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
    private static string PlayerName(int actor) => LobbyBotRegistry.TryGetBot(actor, out LobbyBotInfo bot) ? bot.Name :
        PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Players.TryGetValue(actor, out var player) ? player.NickName : "Gracz " + actor;

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
        contentRect.sizeDelta = new Vector2(0f, Mathf.Max(200f, history.rounds.Count * 380f));
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
            BuildHistoryRow(contentRect, history.rounds[history.rounds.Count - 1 - i], i, deck);
    }

    private static void BuildHistoryRow(RectTransform parent, Entry entry, int index, CardSpriteEntry[] deck)
    {
        RectTransform row = new GameObject("Round_" + entry.round, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        row.SetParent(parent, false);
        row.anchorMin = new Vector2(0f, 1f); row.anchorMax = Vector2.one;
        row.pivot = new Vector2(.5f, 1f);
        row.anchoredPosition = new Vector2(0f, -index * 380f);
        row.sizeDelta = new Vector2(0f, 360f);
        row.GetComponent<Image>().color = new Color(.05f,.045f,.035f);
        Image accent = new GameObject("StatusAccent", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        accent.transform.SetParent(row, false);
        accent.rectTransform.anchorMin = Vector2.zero; accent.rectTransform.anchorMax = new Vector2(0f,1f);
        accent.rectTransform.offsetMin = Vector2.zero; accent.rectTransform.offsetMax = new Vector2(6f,0f);
        accent.color = entry.exists ? new Color(.25f,.85f,.4f) : new Color(.9f,.26f,.23f);
        string who = string.IsNullOrEmpty(entry.checkerName) ? "" : entry.checkerName + " → sprawdził → " + entry.declarerName + "\n";
        TMP_Text heading = MakeText(row, "RUNDA " + entry.round + " • " + (entry.exists ? "BYŁ" : "NIE BYŁ") + "\n" +
            who + "Sprawdzany układ: " + entry.declaration, 28f);
        heading.richText = false;
        heading.rectTransform.anchorMin = new Vector2(0f,1f); heading.rectTransform.anchorMax = Vector2.one;
        heading.rectTransform.pivot = new Vector2(.5f,1f);
        heading.rectTransform.offsetMin = new Vector2(20f,-112f); heading.rectTransform.offsetMax = new Vector2(-16f,-8f);

        RectTransform view = new GameObject("CardsViewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(HistoryRowScroll)).GetComponent<RectTransform>();
        view.SetParent(row,false);
        view.anchorMin = Vector2.zero; view.anchorMax = Vector2.one;
        view.offsetMin = new Vector2(16f,8f); view.offsetMax = new Vector2(-16f,-116f);
        view.GetComponent<Image>().color = Color.clear;
        RectTransform content = new GameObject("Hands",typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(view,false);
        content.anchorMin = content.anchorMax = new Vector2(0f,.5f); content.pivot = new Vector2(0f,.5f);
        HistoryRowScroll scroll = view.GetComponent<HistoryRowScroll>();
        scroll.viewport = view; scroll.content = content; scroll.horizontal = true; scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 45f;
        bool owners = entry.cardOwners != null && entry.cardOwners.Length == entry.cards.Length &&
            entry.participants != null && entry.participants.Length > 0;
        int count = owners ? entry.participants.Length : 1;
        float x = 0f;
        for (int p = 0; p < count; p++)
        {
            var indices = new System.Collections.Generic.List<int>();
            for (int c = 0; c < entry.cards.Length; c++)
                if (!owners || entry.cardOwners[c] == entry.participants[p]) indices.Add(c);
            float width = Mathf.Max(170f, indices.Count * 128f + 20f);
            string playerName = owners && entry.participantNames != null && p < entry.participantNames.Length ? entry.participantNames[p] : "Karty na stole";
            TMP_Text name = MakeText(content, playerName, 27f);
            name.richText = false;
            SetCardRect(name.rectTransform,x,182f,width,40f);
            name.alignment = TextAlignmentOptions.Center;
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.overflowMode = TextOverflowModes.Ellipsis;
            for (int h = 0; h < indices.Count; h++)
            {
                int code = entry.cards[indices[h]];
                CardSpriteEntry card = Array.Find(deck,item => item != null && Code(item) == code);
                if (card == null) continue;
                Image image = new GameObject("Card",typeof(RectTransform),typeof(Image)).GetComponent<Image>();
                image.transform.SetParent(content,false);
                SetCardRect(image.rectTransform,x+10f+h*128f,12f,112f,156f);
                image.sprite = card.sprite; image.preserveAspect = true; image.raycastTarget = false;
                if (entry.matching != null && Array.IndexOf(entry.matching,code) >= 0)
                {
                    Outline outline = image.gameObject.AddComponent<Outline>();
                    outline.effectColor = accent.color; outline.effectDistance = new Vector2(3f,-3f);
                }
            }
            x += width + 20f;
        }
        content.sizeDelta = new Vector2(x,228f);
    }
    private static void SetCardRect(RectTransform rect,float x,float y,float width,float height)
    {
        rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(x,y); rect.sizeDelta = new Vector2(width,height);
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
