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

    // Launch always stays in the portrait menu. A saved match may only be
    // resumed by an explicit user action, never automatically during startup.
    public void ResumeSavedRoom()
    {
        StartCoroutine(TryAutoResume());
    }

    private IEnumerator TryAutoResume()
    {
        if (PlayerPrefs.GetInt(ResumePendingPrefsKey, 0) != 1)
            yield break;

        string roomCode = PlayerPrefs.GetString(LastRoomCodePrefsKey, "");
        if (string.IsNullOrWhiteSpace(roomCode))
        {
            ClearResumePrefs();
            yield break;
        }

        float timer = 0f;
        while (!PhotonNetwork.IsConnectedAndReady && timer < waitForPhotonSeconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
            yield break;

        if (triedAutoResume)
            yield break;

        triedAutoResume = true;
        PhotonNetwork.RejoinRoom(roomCode);
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
            ClearResumePrefs();

            if (PhotonNetwork.InRoom && !leavingRejectedRoom)
            {
                leavingRejectedRoom = true;
                PhotonNetwork.LeaveRoom();
            }

            return;
        }

        ClearResumePrefs();
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
        ClearResumePrefs();
        Debug.LogWarning($"AutoResumeRoom: Rejoin failed: {message} ({returnCode})");
    }

    private void ClearResumePrefs()
    {
        PlayerPrefs.SetInt(ResumePendingPrefsKey, 0);
        PlayerPrefs.DeleteKey(LastRoomCodePrefsKey);
        PlayerPrefs.Save();
    }
}
