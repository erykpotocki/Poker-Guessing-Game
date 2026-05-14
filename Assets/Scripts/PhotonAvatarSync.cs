using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PhotonAvatarSync : MonoBehaviourPunCallbacks
{
    private const string AvatarKey = "avatarIndex";
    private const string PrefKey = "avatarIndex";

    private void Start()
    {
        // WAŻNE: jeśli już jesteśmy w pokoju (np. po zmianie sceny),
        // to OnJoinedRoom się nie wywoła ponownie — więc ustawiamy tu.
        if (PhotonNetwork.InRoom)
            PushAvatarIndexToPhoton();
    }

    public override void OnJoinedRoom()
    {
        PushAvatarIndexToPhoton();
    }

    private void PushAvatarIndexToPhoton()
    {
        int idx = PlayerPrefs.GetInt(PrefKey, 0);

        var props = new Hashtable
        {
            { AvatarKey, idx }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log($"[PhotonAvatarSync] Sent avatarIndex={idx} for {PhotonNetwork.NickName}");
    }
}