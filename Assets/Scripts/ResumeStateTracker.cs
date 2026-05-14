using Photon.Pun;
using UnityEngine;

public class ResumeStateTracker : MonoBehaviour
{
    private const string ResumePendingPrefsKey = "ResumePending";

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            return;

        MarkResumePendingIfInRoom();
    }

    private void OnApplicationQuit()
    {
        MarkResumePendingIfInRoom();
    }

    private void MarkResumePendingIfInRoom()
    {
        if (!PhotonNetwork.InRoom)
            return;

        PlayerPrefs.SetInt(ResumePendingPrefsKey, 1);
        PlayerPrefs.Save();
    }
}