using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotSeatSetupUI : MonoBehaviour
{
    private enum HotSeatPhase
    {
        FirstCardPreview,
        TurnLoop
    }

    private class HotSeatPlayer
    {
        public string Name;
        public int CardCount = 1;
        public bool Eliminated = false;
        public bool PenaltyGoingUp = true;
        public readonly List<string> Cards = new List<string>();
    }

    [Header("Setup UI")]
    [SerializeField] private GameObject setupPanel;
    [SerializeField] private Button addPlayerButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Transform playerListRoot;
    [SerializeField] private GameObject playerNameRowPrefab;

    [Header("Card UI")]
    [SerializeField] private GameObject cardPanel;
    [SerializeField] private TextMeshProUGUI currentPlayerNameText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;
    [SerializeField] private TextMeshProUGUI cardText;

    [Header("Settings")]
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private int maxPlayers = 6;
    [SerializeField] private int maxNameLength = 10;

    [Header("Card Placeholder Colors")]
    [SerializeField] private Color cardBackColor = new Color(0.08f, 0.08f, 0.1f, 1f);
    [SerializeField] private Color cardFrontColor = new Color(0.92f, 0.88f, 0.78f, 1f);

    private readonly List<TMP_InputField> playerInputs = new List<TMP_InputField>();
    private readonly List<HotSeatPlayer> players = new List<HotSeatPlayer>();

    private HotSeatPhase currentPhase;
    private int starterIndex;
    private int currentPlayerIndex;
    private int firstPreviewCount;
    private bool cardVisible;

    private void Start()
    {
        addPlayerButton.onClick.AddListener(AddPlayer);
        startButton.onClick.AddListener(StartHotSeat);
        cardButton.onClick.AddListener(OnCardClicked);

        if (cardPanel != null)
            cardPanel.SetActive(false);

        RefreshButtons();
    }

    private void AddPlayer()
    {
        if (playerInputs.Count >= maxPlayers)
            return;

        int playerNumber = playerInputs.Count + 1;

        GameObject row = Instantiate(playerNameRowPrefab, playerListRoot);
        TMP_InputField input = row.GetComponentInChildren<TMP_InputField>();

        input.characterLimit = maxNameLength;
        input.text = "GRACZ" + playerNumber;

        input.onValueChanged.AddListener(value =>
        {
            string cleaned = CleanName(value);

            if (cleaned != value)
                input.SetTextWithoutNotify(cleaned);
        });

        playerInputs.Add(input);
        RefreshButtons();
    }

    private void StartHotSeat()
    {
        if (playerInputs.Count < minPlayers)
            return;

        players.Clear();

        for (int i = 0; i < playerInputs.Count; i++)
        {
            string playerName = CleanName(playerInputs[i].text);

            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "GRACZ" + (i + 1);

            players.Add(new HotSeatPlayer
            {
                Name = playerName,
                CardCount = 1,
                Eliminated = false,
                PenaltyGoingUp = true
            });
        }

        StartNewRound();
    }

    private void StartNewRound()
    {
        DealCards();

        starterIndex = Random.Range(0, players.Count);
        currentPlayerIndex = starterIndex;
        firstPreviewCount = 0;
        currentPhase = HotSeatPhase.FirstCardPreview;

        setupPanel.SetActive(false);
        cardPanel.SetActive(true);

        ShowCardBack();
    }

    private void OnCardClicked()
    {
        if (!cardVisible)
        {
            ShowCardFront();
            return;
        }

        HideCardAndContinue();
    }

    private void ShowCardBack()
    {
        cardVisible = false;

        HotSeatPlayer player = players[currentPlayerIndex];

        currentPlayerNameText.text = player.Name;
        cardImage.color = cardBackColor;

        EnsureCardText();
        cardText.text = "REWERS";
        cardText.color = Color.white;

        instructionText.text =
            "UPEWNIJ SIĘ, ŻE NIKT NIE PATRZY\n" +
            "NACIŚNIJ KARTĘ, ŻEBY ODKRYĆ";
    }

    private void ShowCardFront()
    {
        cardVisible = true;

        HotSeatPlayer player = players[currentPlayerIndex];

        currentPlayerNameText.text = player.Name;
        cardImage.color = cardFrontColor;

        EnsureCardText();
        cardText.text = string.Join("\n", player.Cards);
        cardText.color = Color.black;

        instructionText.text =
            "ZAPAMIĘTAJ SWOJĄ KARTĘ\n" +
            "NACIŚNIJ PONOWNIE, ŻEBY ZAKRYĆ";
    }

    private void HideCardAndContinue()
    {
        cardVisible = false;

        if (currentPhase == HotSeatPhase.FirstCardPreview)
        {
            firstPreviewCount++;

            if (firstPreviewCount >= GetActivePlayerCount())
            {
                currentPhase = HotSeatPhase.TurnLoop;
                currentPlayerIndex = starterIndex;
                ShowCardBack();
                return;
            }

            currentPlayerIndex = GetNextActivePlayerIndex(currentPlayerIndex);
            ShowCardBack();
            return;
        }

        currentPlayerIndex = GetNextActivePlayerIndex(currentPlayerIndex);
        ShowCardBack();
    }

    private void DealCards()
    {
        List<string> deck = CreateDeck();
        Shuffle(deck);

        int deckIndex = 0;

        foreach (HotSeatPlayer player in players)
        {
            player.Cards.Clear();

            if (player.Eliminated)
                continue;

            for (int i = 0; i < player.CardCount; i++)
            {
                if (deckIndex >= deck.Count)
                    break;

                player.Cards.Add(deck[deckIndex]);
                deckIndex++;
            }
        }
    }

    private List<string> CreateDeck()
    {
        List<string> deck = new List<string>();

        string[] ranks = { "9", "10", "J", "D", "K", "A" };
        string[] suits = { "DZWONEK ♦", "SERCE ♥", "ŻOŁĄDŹ ♣", "WINO ♠" };

        foreach (string rank in ranks)
        {
            foreach (string suit in suits)
            {
                deck.Add(GetRankName(rank) + " " + suit);
            }
        }

        return deck;
    }

    private string GetRankName(string rank)
    {
        switch (rank)
        {
            case "J": return "WALET";
            case "D": return "DAMA";
            case "K": return "KRÓL";
            case "A": return "AS";
            default: return rank;
        }
    }

    private void Shuffle(List<string> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }

    private int GetNextActivePlayerIndex(int fromIndex)
    {
        int index = fromIndex;

        for (int i = 0; i < players.Count; i++)
        {
            index++;

            if (index >= players.Count)
                index = 0;

            if (!players[index].Eliminated)
                return index;
        }

        return fromIndex;
    }

    private int GetActivePlayerCount()
    {
        int count = 0;

        foreach (HotSeatPlayer player in players)
        {
            if (!player.Eliminated)
                count++;
        }

        return count;
    }

    private string CleanName(string value)
    {
        string result = "";

        foreach (char c in value.ToUpper())
        {
            if (char.IsLetterOrDigit(c))
                result += c;

            if (result.Length >= maxNameLength)
                break;
        }

        return result;
    }

    private void RefreshButtons()
    {
        addPlayerButton.interactable = playerInputs.Count < maxPlayers;
        startButton.interactable = playerInputs.Count >= minPlayers;
    }

    private void EnsureCardText()
    {
        if (cardText != null)
            return;

        GameObject textObject = new GameObject("HS_CardText");
        textObject.transform.SetParent(cardImage.transform, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        cardText = textObject.AddComponent<TextMeshProUGUI>();
        cardText.alignment = TextAlignmentOptions.Center;
        cardText.fontSize = 70;
        cardText.fontStyle = FontStyles.Bold;
        cardText.raycastTarget = false;
    }
}