using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayersListUI : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private Transform container;            // PlayersListContainer
    [SerializeField] private GameObject rowPrefab;           // prefab PlayerRow
    [SerializeField] private TMP_Text playersCountText;      // PlayerCountText (Gracze: x/6)

    [Header("Avatars")]
    [SerializeField] private AvatarDatabase avatarDatabase;  // wspólny asset

    private const string AvatarKey = "avatarIndex";
    private readonly List<GameObject> spawned = new();
    private readonly Dictionary<int, GameObject> rowsByActorNumber = new();
    private float currentRowHeight = 108f;
    private float currentAvatarSize = 88f;
    private bool previousCloseConnectionSetting;

    public override void OnEnable()
    {
        base.OnEnable();
        previousCloseConnectionSetting = PhotonNetwork.EnableCloseConnection;
        PhotonNetwork.EnableCloseConnection = true;
    }

    public override void OnDisable()
    {
        PhotonNetwork.EnableCloseConnection = previousCloseConnectionSetting;
        base.OnDisable();
    }

    public override void OnMasterClientSwitched(Player newMasterClient) => Refresh();

    public int AvatarCount => avatarDatabase != null && avatarDatabase.avatars != null
        ? avatarDatabase.avatars.Length
        : 0;

    private void Start()
    {
        ConfigureModernListLayout(1);
        Refresh();
    }

    public override void OnJoinedRoom() => Refresh();
    public override void OnPlayerEnteredRoom(Player newPlayer) => Refresh();
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer != null && otherPlayer.IsInactive)
        {
            StartCoroutine(RefreshAfterPlayerListUpdate());
            return;
        }

        if (otherPlayer != null &&
            rowsByActorNumber.TryGetValue(otherPlayer.ActorNumber, out GameObject row))
        {
            rowsByActorNumber.Remove(otherPlayer.ActorNumber);
            spawned.Remove(row);
            if (row != null)
            {
                row.SetActive(false);
                Destroy(row);
            }
        }

        StartCoroutine(RefreshAfterPlayerListUpdate());
    }

    public override void OnLeftRoom()
    {
        ClearRows();
        if (playersCountText != null)
            playersCountText.text = "Gracze: -/-";
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) => Refresh();

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged != null &&
            propertiesThatChanged.ContainsKey(LobbyBotRegistry.RoomPropertyKey))
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (container == null || rowPrefab == null) return;

        if (!PhotonNetwork.InRoom)
        {
            if (playersCountText != null) playersCountText.text = "Gracze: -/-";
            ClearRows();
            return;
        }

        int count = PhotonNetwork.CurrentRoom != null
            ? PhotonNetwork.CurrentRoom.PlayerCount + LobbyBotRegistry.GetBots().Count
            : 0;
        ConfigureModernListLayout(count);

        if (playersCountText != null && PhotonNetwork.CurrentRoom != null)
        {
            int max = PhotonNetwork.CurrentRoom.MaxPlayers;
            playersCountText.text = $"GRACZE: {Mathf.Min(count, max)}/{max}";
        }

        ClearRows();

        int iRow = 1;
        List<Player> roomPlayers = new List<Player>(
            PhotonNetwork.CurrentRoom.Players.Values);
        roomPlayers.Sort((a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

        foreach (Player p in roomPlayers)
        {
            int idx = 0;
            if (p.CustomProperties != null && p.CustomProperties.ContainsKey(AvatarKey))
                idx = (int)p.CustomProperties[AvatarKey];

            string playerName = p.IsInactive
                ? $"{p.NickName} (WRÓCI ZA CHWILĘ…)"
                : p.NickName;
            SpawnRow(p.ActorNumber, iRow, playerName, idx);

            iRow++;
        }

        List<LobbyBotInfo> bots = LobbyBotRegistry.GetBots();
        for (int i = 0; i < bots.Count; i++)
        {
            LobbyBotInfo bot = bots[i];
            SpawnRow(bot.ActorNumber, iRow,
                bot.Name + "\n<size=65%><color=#B8AA8A>Początkujący</color></size>",
                bot.AvatarIndex);

            iRow++;
        }
    }

    private void SpawnRow(int actorNumber, int rowNumber, string displayName, int avatarIndex)
    {
        GameObject go = Instantiate(rowPrefab, container);
        spawned.Add(go);
        rowsByActorNumber[actorNumber] = go;

        TMP_Text nameText = go.transform.Find("NameText")?.GetComponent<TMP_Text>();
        Image avatarImg = go.transform.Find("AvatarImage")?.GetComponent<Image>();

        if (nameText != null)
        {
            nameText.text = $"{rowNumber}. {displayName}";
            nameText.fontStyle = FontStyles.Bold;
            nameText.fontWeight = FontWeight.Bold;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 28f;
            nameText.fontSizeMax = 36f;
            nameText.textWrappingMode = TextWrappingModes.NoWrap;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            nameText.rectTransform.sizeDelta = new Vector2(400f, currentRowHeight - 20f);
        }

        if (avatarImg != null && avatarDatabase != null &&
            avatarDatabase.avatars != null && avatarDatabase.avatars.Length > 0)
        {
            avatarIndex = Mathf.Clamp(avatarIndex, 0, avatarDatabase.avatars.Length - 1);
            avatarImg.sprite = avatarDatabase.avatars[avatarIndex];
            avatarImg.preserveAspect = true;

            if (avatarImg.transform is RectTransform avatarRect)
                avatarRect.sizeDelta = new Vector2(currentAvatarSize, currentAvatarSize);

            LayoutElement avatarLayout = avatarImg.GetComponent<LayoutElement>();
            if (avatarLayout != null)
            {
                avatarLayout.minWidth = currentAvatarSize;
                avatarLayout.preferredWidth = currentAvatarSize;
                avatarLayout.minHeight = currentAvatarSize;
                avatarLayout.preferredHeight = currentAvatarSize;
            }
        }

        if (go.transform is RectTransform rowRect)
            rowRect.sizeDelta = new Vector2(560f, currentRowHeight);

        LayoutElement rowLayout = go.GetComponent<LayoutElement>();
        if (rowLayout != null)
        {
            rowLayout.minHeight = currentRowHeight;
            rowLayout.preferredHeight = currentRowHeight;
        }

        HorizontalLayoutGroup horizontal = go.GetComponent<HorizontalLayoutGroup>();
        if (horizontal != null)
        {
            horizontal.padding = new RectOffset(20, 20, 10, 10);
            horizontal.spacing = 24f;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;
        }

        // The action column sits outside the original name/avatar layout.
        // Reserve it even for the host, whose own row has no remove action.
        GameObject actionSlot = new GameObject("RemoveActionColumn", typeof(RectTransform), typeof(LayoutElement));
        RectTransform actionRect = actionSlot.GetComponent<RectTransform>();
        actionRect.SetParent(go.transform, false);
        actionSlot.GetComponent<LayoutElement>().ignoreLayout = true;
        actionRect.anchorMin = actionRect.anchorMax = new Vector2(1f, 0.5f);
        actionRect.pivot = new Vector2(0f, 0.5f);
        // The row has 20 units of right padding: 4 outside gives a 24-unit gap.
        actionRect.anchoredPosition = new Vector2(4f, 0f);
        actionRect.sizeDelta = new Vector2(58f, 58f);

        if (PhotonNetwork.IsMasterClient && actorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            CreateRemoveButton(actionRect, actorNumber);
    }

    private void CreateRemoveButton(Transform row, int actorNumber)
    {
        GameObject control = new GameObject("RemovePlayer", typeof(RectTransform),
            typeof(Image), typeof(Button), typeof(LayoutElement));
        control.transform.SetParent(row, false);
        control.transform.SetAsLastSibling();
        RectTransform controlRect = control.GetComponent<RectTransform>();
        controlRect.anchorMin = Vector2.zero;
        controlRect.anchorMax = Vector2.one;
        controlRect.offsetMin = controlRect.offsetMax = Vector2.zero;
        LayoutElement layout = control.GetComponent<LayoutElement>();
        layout.minWidth = layout.preferredWidth = 58f;
        layout.minHeight = layout.preferredHeight = 58f;
        layout.flexibleWidth = layout.flexibleHeight = 0f;
        Image border = control.GetComponent<Image>();
        border.color = new Color(0.78f, 0.56f, 0.20f);
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.SetParent(control.transform, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.one * 2f;
        fillRect.offsetMax = Vector2.one * -2f;
        fill.GetComponent<Image>().color = new Color(0.28f, 0.045f, 0.025f);
        fill.GetComponent<Image>().raycastTarget = false;
        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.transform.SetParent(control.transform, false);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
        text.text = "×";
        text.fontSize = 42f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.88f, 0.60f);
        text.raycastTarget = false;
        Button button = control.GetComponent<Button>();
        button.targetGraphic = border;
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        button.onClick.AddListener(() => RemoveParticipant(actorNumber));
        // An absent client cannot receive a kick until it reconnects.
        if (PhotonNetwork.CurrentRoom.Players.TryGetValue(actorNumber, out Player player))
            button.interactable = !player.IsInactive;
    }

    private void RemoveParticipant(int actorNumber)
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient ||
            actorNumber == PhotonNetwork.LocalPlayer.ActorNumber ||
            (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameStarted", out object started)
             && started is bool value && value))
            return;

        if (LobbyBotRegistry.IsBot(actorNumber))
        {
            FindFirstObjectByType<FakePlayers>()?.RemoveBot(actorNumber);
            return;
        }

        if (PhotonNetwork.CurrentRoom.Players.TryGetValue(actorNumber, out Player target)
            && !target.IsInactive)
            PhotonNetwork.CloseConnection(target);
    }

    private void ConfigureModernListLayout(int participantCount)
    {
        if (container is not RectTransform rect)
            return;

        if (participantCount <= 3)
        {
            currentRowHeight = 108f;
            currentAvatarSize = 88f;
        }
        else if (participantCount == 4)
        {
            currentRowHeight = 96f;
            currentAvatarSize = 78f;
        }
        else
        {
            currentRowHeight = 82f;
            currentAvatarSize = 66f;
        }

        // Reserve the middle of the lobby for participants; actions live below.
        rect.anchorMin = new Vector2(0.5f, 0.36f);
        rect.anchorMax = new Vector2(0.5f, 0.76f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -64f);
        rect.sizeDelta = new Vector2(600f, 0f);
        ContentSizeFitter fitter = container.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            fitter.enabled = false;

        float spacing = participantCount <= 3 ? 20f : participantCount == 4 ? 14f : 10f;
        float availableRowHeight = (rect.rect.height - 24f -
            Mathf.Max(0, participantCount - 1) * spacing) / Mathf.Max(1, participantCount);
        currentRowHeight = Mathf.Min(currentRowHeight, Mathf.Max(48f, availableRowHeight));
        currentAvatarSize = Mathf.Min(currentAvatarSize, currentRowHeight - 20f);

        VerticalLayoutGroup vertical = container.GetComponent<VerticalLayoutGroup>();
        if (vertical != null)
        {
            vertical.padding = new RectOffset(20, 20, 12, 12);
            vertical.spacing = spacing;
            vertical.childAlignment = TextAnchor.MiddleCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandWidth = false;
            vertical.childForceExpandHeight = false;
        }
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null) Destroy(spawned[i]);
        spawned.Clear();
        rowsByActorNumber.Clear();
    }

    private IEnumerator RefreshAfterPlayerListUpdate()
    {
        yield return null;
        Refresh();
    }
}
