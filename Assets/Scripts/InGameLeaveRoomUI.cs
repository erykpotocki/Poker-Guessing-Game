using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameLeaveRoomUI : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private GameObject confirmLeavePanel;

    [Header("Scene After Leave")]
    [SerializeField] private string sceneAfterLeave = "MainMenu";

    private bool isLeavingRoom = false;

    private void Start()
    {
        if (confirmLeavePanel != null)
            confirmLeavePanel.SetActive(false);
    }

    public void OnClickOpenLeaveConfirm()
    {
        if (isLeavingRoom)
            return;

        if (confirmLeavePanel != null)
            confirmLeavePanel.SetActive(true);
    }

    public void OnClickCancelLeave()
    {
        if (isLeavingRoom)
            return;

        if (confirmLeavePanel != null)
            confirmLeavePanel.SetActive(false);
    }

    public void OnClickConfirmLeave()
    {
        if (isLeavingRoom)
            return;

        isLeavingRoom = true;

        if (confirmLeavePanel != null)
            confirmLeavePanel.SetActive(false);

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        else
            SceneManager.LoadScene(sceneAfterLeave);
    }

    public override void OnLeftRoom()
    {
        isLeavingRoom = false;
        SceneManager.LoadScene(sceneAfterLeave);
    }
}