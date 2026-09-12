using ExitGames.Client.Photon;
using IEnumerator = System.Collections.IEnumerator;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyStartPhoton : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private Button startButton;
    [SerializeField] private int minPlayers = 2;

    private const string GameStartedKey = "gameStarted";
    private bool startingGame;
    public Button StartButton => startButton;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        FakePlayers bots = FindFirstObjectByType<FakePlayers>();
        if (bots == null)
            bots = gameObject.AddComponent<FakePlayers>();
        bots.Initialize(startButton);
        RefreshStartButton();
    }

    public override void OnJoinedRoom() => RefreshStartButton();
    public override void OnDisconnected(DisconnectCause cause)
    {
        startingGame = false;
        RefreshStartButton();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer) => RefreshStartButton();
    public override void OnPlayerLeftRoom(Player otherPlayer) => RefreshStartButton();
    public override void OnMasterClientSwitched(Player newMasterClient) => RefreshStartButton();

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged != null &&
            propertiesThatChanged.ContainsKey(LobbyBotRegistry.RoomPropertyKey))
        {
            RefreshStartButton();
        }

        if (propertiesThatChanged != null &&
            propertiesThatChanged.TryGetValue(GameStartedKey, out object value) &&
            value is bool started && started)
        {
            startingGame = true;
            MultiplayerLoadingTransition.Begin();
        }
    }

    public void RefreshStartButton()
    {
        if (startButton == null) return;

        bool isMaster = PhotonNetwork.IsMasterClient;
        int count = PhotonNetwork.CurrentRoom != null
            ? PhotonNetwork.CurrentRoom.PlayerCount + LobbyBotRegistry.GetBots().Count
            : 0;

        startButton.interactable = !startingGame && isMaster && (count >= minPlayers);
    }

    public void OnClickStart()
    {
        if (startingGame) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom == null) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount + LobbyBotRegistry.GetBots().Count < minPlayers) return;

        startingGame = true;
        RefreshStartButton();
        MultiplayerLoadingTransition.Begin();

        Hashtable roomProps = new Hashtable();
        roomProps[GameStartedKey] = true;
        roomProps["rewardStartedMs"] = PhotonNetwork.ServerTimestamp;
        roomProps["rewardBots"] = LobbyBotRegistry.GetBots().Count;
        var rewardActors = new System.Collections.Generic.List<int>();
        foreach (var player in PhotonNetwork.PlayerList)
            if (!player.IsInactive) rewardActors.Add(player.ActorNumber);
        roomProps["rewardActors"] = rewardActors.ToArray();
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

        StartCoroutine(LoadGameAfterShowingLoading());
    }

    private IEnumerator LoadGameAfterShowingLoading()
    {
        // Let the landscape loading screen render before Photon starts loading.
        yield return null;
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
        {
            startingGame = false;
            MultiplayerLoadingTransition.Finish();
            HotSeatOrientationLock.LockPortrait();
            RefreshStartButton();
            yield break;
        }
        PhotonNetwork.LoadLevel(gameSceneName);
    }
}
