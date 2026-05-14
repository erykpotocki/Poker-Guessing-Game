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
        roomReady = false;
        loadingFinished = false;
        minimumTimePassed = false;
        loadHandshakeStarted = false;

        if (loadingUI != null)
            loadingUI.ShowLoading("Ładowanie graczy...");

        StartCoroutine(MinimumLoadingTimer());
        StartCoroutine(WaitForRoomAndStartHandshake());
    }

    private IEnumerator WaitForRoomAndStartHandshake()
    {
        float timer = 0f;

        while (!PhotonNetwork.InRoom && timer < waitForRoomTimeout)
        {
            timer += Time.deltaTime;
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
        yield return new WaitForSeconds(minimumLoadingTime);
        minimumTimePassed = true;
        TryHideLoading();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        CheckAllPlayersLoaded();
    }

    public override void OnRoomPropertiesUpdate(Hashtable changedProps)
    {
        if (changedProps.ContainsKey(RoomCanStartKey))
        {
            roomReady = (bool)changedProps[RoomCanStartKey];
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
        if (!minimumTimePassed || !roomReady || loadingFinished)
            return;

        loadingFinished = true;

        if (loadingUI != null)
            loadingUI.HideLoading();

        StartCoroutine(StartDealAfterLoading());
    }

    private IEnumerator StartDealAfterLoading()
    {
        yield return null;

        if (cardDealTest != null)
            cardDealTest.StartDealTest();
    }
}