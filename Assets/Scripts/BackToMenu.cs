using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class BackToMenu : MonoBehaviourPunCallbacks
{
    private const string ResumePendingPrefsKey = "ResumePending";
    private const string LastRoomCodePrefsKey = "lastRoomCode";

    public void GoMainMenu()
    {
        ClearResumeData();

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            return;
        }

        SceneManager.LoadScene("MainMenu");
    }

    public override void OnLeftRoom()
    {
        ClearResumeData();
        SceneManager.LoadScene("MainMenu");
    }

    private void ClearResumeData()
    {
        PlayerPrefs.SetInt(ResumePendingPrefsKey, 0);
        PlayerPrefs.DeleteKey(LastRoomCodePrefsKey);
        PlayerPrefs.Save();
    }
}