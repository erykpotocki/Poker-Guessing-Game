using Photon.Pun;
using TMPro;
using UnityEngine;

public class InGameRoomCodeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private string prefix = "Kod gry: ";

    private void Start()
    {
        if (roomCodeText == null)
            roomCodeText = GetComponent<TMP_Text>();

        if (roomCodeText == null)
            return;

        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
            roomCodeText.text = prefix + PhotonNetwork.CurrentRoom.Name;
        else
            roomCodeText.text = prefix + "---";
    }
}