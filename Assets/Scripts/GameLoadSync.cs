using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameLoadSync : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameLoadingUI loadingUI;
    [SerializeField] private CardDealTest cardDealTest;
    [SerializeField] private float minimumLoadingTime = 5f;
    [SerializeField] private float waitForRoomTimeout = 10f;

    private const string PlayerLoadedKey = "gameLoaded";
    private const string RoomCanStartKey = "gameCanStart";

    private bool minimumTimePassed = false;
    private bool roomReady = false;
    private bool loadingFinished = false;
    private bool loadHandshakeStarted = false;

    private void Start()
    {
        // OnJoinedRoom may run between OnEnable and Start. Do not reset a
        // handshake already begun by that callback.
        loadingUI = MultiplayerLoadingTransition.UseForGame(loadingUI);
        if (loadingUI != null)
            loadingUI.ShowLoading("Ładowanie graczy...");

        StartCoroutine(MinimumLoadingTimer());
        StartCoroutine(WaitForRoomAndStartHandshake());
    }

    private void Update()
    {
        if (loadingFinished || loadingUI == null) return;
        Player[] players = PhotonNetwork.PlayerList;
        int loaded = 0;
        foreach (Player player in players)
            if (player.CustomProperties.TryGetValue(PlayerLoadedKey, out object value) && value is bool ready && ready) loaded++;
        float fraction = players.Length > 0 ? (float)loaded / players.Length : 0f;
        loadingUI.SetProgress(.95f * fraction, fraction < 1f ? "Oczekiwanie na graczy…" : "Przygotowywanie stołu…");
    }

    private IEnumerator WaitForRoomAndStartHandshake()
    {
        float timer = 0f;

        while (!PhotonNetwork.InRoom && timer < waitForRoomTimeout)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("GameLoadSync: nie udało się wejść do roomu na czas.");
            yield break;
        }

        StartLoadHandshake();
    }

    public override void OnJoinedRoom()
    {
        StartLoadHandshake();
    }

    private void StartLoadHandshake()
    {
        if (!PhotonNetwork.InRoom || loadHandshakeStarted)
            return;

        loadHandshakeStarted = true;
        roomReady = false;

        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable resetRoomProps = new Hashtable
            {
                { RoomCanStartKey, false }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(resetRoomProps);
        }

        Hashtable resetPlayerProps = new Hashtable
        {
            { PlayerLoadedKey, false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(resetPlayerProps);

        StartCoroutine(FinishLoadHandshakeNextFrame());
    }

    private IEnumerator FinishLoadHandshakeNextFrame()
    {
        yield return null;

        if (!PhotonNetwork.InRoom)
            yield break;

        Hashtable playerProps = new Hashtable
        {
            { PlayerLoadedKey, true }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        CheckAllPlayersLoaded();
        TryHideLoading();
    }

    private IEnumerator MinimumLoadingTimer()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(5f, minimumLoadingTime));
        minimumTimePassed = true;
        TryHideLoading();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        CheckAllPlayersLoaded();
    }

    public override void OnRoomPropertiesUpdate(Hashtable changedProps)
    {
        if (changedProps != null && changedProps.ContainsKey(RoomCanStartKey))
        {
            TryHideLoading();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckAllPlayersLoaded();
    }

    private void CheckAllPlayersLoaded()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;

        Player[] players = PhotonNetwork.PlayerList;
        if (players == null || players.Length == 0)
            return;

        foreach (Player player in players)
        {
            bool loaded =
                player.CustomProperties != null &&
                player.CustomProperties.ContainsKey(PlayerLoadedKey) &&
                player.CustomProperties[PlayerLoadedKey] is bool loadedValue &&
                loadedValue;

            if (!loaded)
                return;
        }

        Hashtable roomProps = new Hashtable
        {
            { RoomCanStartKey, true }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    private void TryHideLoading()
    {
        if (loadingFinished || !loadHandshakeStarted || !PhotonNetwork.InRoom)
            return;

        // A late joiner receives existing room properties, not necessarily a new
        // gameCanStart notification. Read current state rather than waiting for
        // an event that may already have happened before this scene was loaded.
        roomReady = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(
            RoomCanStartKey, out object readyValue) && readyValue is bool ready && ready;
        if (!minimumTimePassed || !roomReady || loadingFinished)
            return;

        loadingFinished = true;

        if (loadingUI != null)
            loadingUI.HideLoading();
        MultiplayerLoadingTransition.Finish();

        StartCoroutine(StartDealAfterLoading());
    }

    private IEnumerator StartDealAfterLoading()
    {
        yield return null;

        if (cardDealTest != null)
            cardDealTest.StartDealTest();
    }
}
