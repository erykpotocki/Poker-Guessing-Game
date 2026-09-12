using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoResumeRoom : MonoBehaviourPunCallbacks
{
    private const string ResumePendingPrefsKey = "ResumePending";
    private const string LastRoomCodePrefsKey = "lastRoomCode";
    private const string GameStartedKey = "gameStarted";
    private const string GameEndedKey = "gameEnded";

    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private float waitForPhotonSeconds = 8f;

    private bool triedAutoResume = false;
    private bool leavingRejectedRoom = false;
    private bool connecting;
    public System.Action<string> Status;
    private IEnumerator Start()
    {
        yield return null;
        if(SceneManager.GetActiveScene().name!="MainMenu"||PlayerPrefs.GetInt(ResumePendingPrefsKey,0)!=1||string.IsNullOrWhiteSpace(PlayerPrefs.GetString(LastRoomCodePrefsKey,"")))yield break;
        Canvas canvas=GetComponentInParent<Canvas>();if(canvas==null)canvas=FindFirstObjectByType<Canvas>();
        if(canvas!=null)ProfileTestTools.ShowResumePrompt(canvas,this);
    }

    // Launch always stays in the portrait menu. A saved match may only be
    // resumed by an explicit user action, never automatically during startup.
    public void ResumeSavedRoom()
    {
        if(triedAutoResume||connecting)return;
        connecting=true;
        StartCoroutine(TryAutoResume());
    }

    private IEnumerator TryAutoResume()
    {
        if (PlayerPrefs.GetInt(ResumePendingPrefsKey, 0) != 1)
        {connecting=false;yield break;}

        string roomCode = PlayerPrefs.GetString(LastRoomCodePrefsKey, "");
        if (string.IsNullOrWhiteSpace(roomCode))
        {
            ClearResumePrefs();
            connecting=false;
            yield break;
        }

        float timer = 0f;
        Status?.Invoke("Łączę z Twoją grą…");
        FindFirstObjectByType<NetworkBootstrap>()?.ConnectIfNeeded();
        while (!PhotonNetwork.IsConnectedAndReady && timer < waitForPhotonSeconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Status?.Invoke("Brak połączenia. Spróbuj ponownie.");
            connecting=false;
            yield break;
        }

        if (triedAutoResume)
            yield break;

        connecting=false;triedAutoResume = true;
        if(!PhotonNetwork.RejoinRoom(roomCode)){triedAutoResume=false;Status?.Invoke("Połączenie jeszcze nie jest gotowe. Spróbuj ponownie.");}
    }

    public override void OnJoinedRoom()
    {
        if (!triedAutoResume) return;
        bool gameStarted = false;
        bool gameEnded = false;

        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties != null)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(GameStartedKey, out object startedValue) &&
                startedValue is bool startedBool)
            {
                gameStarted = startedBool;
            }

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(GameEndedKey, out object endedValue) &&
                endedValue is bool endedBool)
            {
                gameEnded = endedBool;
            }
        }

        if (gameEnded)
        {
            Status?.Invoke("Ta gra już się zakończyła. Możesz zamknąć to okno i rozpocząć nową.");
            ClearResumePrefs();

            if (PhotonNetwork.InRoom && !leavingRejectedRoom)
            {
                leavingRejectedRoom = true;
                PhotonNetwork.LeaveRoom();
            }

            return;
        }

        if (gameStarted)
        {
            MultiplayerLoadingTransition.Begin();
            StartCoroutine(EnterStartedGame());
        }
        else SceneManager.LoadScene(lobbySceneName);
    }

    private IEnumerator EnterStartedGame()
    {
        yield return null;
        if (!PhotonNetwork.InRoom)
        {
            MultiplayerLoadingTransition.Finish();
            HotSeatOrientationLock.LockPortrait();
            yield break;
        }
        if (PhotonNetwork.IsMessageQueueRunning)
            PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnLeftRoom()
    {
        if (!triedAutoResume) return;
        leavingRejectedRoom = false;

        if (SceneManager.GetActiveScene().name != "MainMenu")
            SceneManager.LoadScene("MainMenu");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (!triedAutoResume) return;
        triedAutoResume=false;
        connecting=false;
        Status?.Invoke("Nie można wrócić. Pokój mógł wygasnąć lub mecz się zakończył.");
        Debug.LogWarning($"AutoResumeRoom: Rejoin failed: {message} ({returnCode})");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if(!connecting&&!triedAutoResume)return;
        StopAllCoroutines();connecting=false;triedAutoResume=false;
        MultiplayerLoadingTransition.Finish();
        HotSeatOrientationLock.LockPortrait();
        Status?.Invoke("Połączenie zostało przerwane. Spróbuj ponownie.");
    }

    private void ClearResumePrefs()
    {
        PlayerPrefs.SetInt(ResumePendingPrefsKey, 0);
        PlayerPrefs.DeleteKey(LastRoomCodePrefsKey);
        PlayerPrefs.Save();
    }
}
