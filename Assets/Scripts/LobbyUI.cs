using Photon.Pun;
using TMPro;
using UnityEngine;

public class LobbyUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text gameModeText;

    private void Start()
    {
        RefreshUI();
    }

    public override void OnJoinedRoom()
    {
        RefreshUI();
    }

    public override void OnLeftRoom()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        RefreshCode();
        RefreshGameMode();
    }

    private void RefreshCode()
    {
        if (codeText == null)
            return;

        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
            codeText.text = $"ID: {PhotonNetwork.CurrentRoom.Name}";
        else
            codeText.text = "ID: -";
    }

    private void RefreshGameMode()
    {
        if (gameModeText == null)
            return;

        gameModeText.text = $"Tryb: {GameModeSelectUI.GetSelectedGameMode()}";
    }
}