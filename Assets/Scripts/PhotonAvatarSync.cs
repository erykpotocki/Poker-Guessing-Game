using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PhotonAvatarSync : MonoBehaviourPunCallbacks
{
    private const string AvatarKey = "avatarIndex";
    private const string PrefKey = "avatarIndex";
    public const string ProfileIdKey = "profileIdV1";
    public const string GamesPlayedKey = "profileGamesV1";
    public const string GamesWonKey = "profileWinsV1";
    public const string FrameKey = "frameIdV1";
    public const string CardBackKey = "cardBackIdV1";

    private void Start()
    {
        PlayerProfileService.Changed += PushAvatarIndexToPhoton;
        // WAŻNE: jeśli już jesteśmy w pokoju (np. po zmianie sceny),
        // to OnJoinedRoom się nie wywoła ponownie — więc ustawiamy tu.
        if (PhotonNetwork.InRoom)
            PushAvatarIndexToPhoton();
    }

    private void OnDestroy()
    {
        PlayerProfileService.Changed -= PushAvatarIndexToPhoton;
    }

    public override void OnJoinedRoom()
    {
        PushAvatarIndexToPhoton();
    }
    public override void OnLeftRoom()=>AvatarPicker.ClearMatchAvatar();

    private void PushAvatarIndexToPhoton()
    {
        int idx = AvatarPicker.ForCurrentRoom();
        string profileId = PlayerPrefs.GetString("PhotonUserId", "");

        var props = new Hashtable
        {
            { AvatarKey, idx },
            {"roundWinsV1",PlayerProfileService.Data.Statistics.RoundsWon},
            {"eliminationsV1",PlayerProfileService.Data.Statistics.Eliminations},
            {"levelV1",PlayerProfileService.Data.Progression.Level},
            {"joinedV1",PlayerProfileService.Data.Profile.JoinedUtc??"—"},
            {"avatarsV1",PlayerProfileService.Data.Inventory.OwnedAvatars.Count},
            {"framesV1",PlayerProfileService.Data.Inventory.OwnedFrames.Count},
            {"backsV1",PlayerProfileService.Data.Inventory.OwnedCardBacks.Count},
            { ProfileIdKey, profileId },
            { GamesPlayedKey, PlayerProfileService.Data.Statistics.GamesPlayed },
            { GamesWonKey, PlayerProfileService.Data.Statistics.GamesWon },
            { FrameKey, PlayerProfileService.Data.Profile.SelectedFrameId ?? "none" },
            { CardBackKey, PlayerProfileService.Data.Profile.SelectedCardBackId ?? "2clasic" }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log($"[PhotonAvatarSync] Sent avatarIndex={idx} for {PhotonNetwork.NickName}");
    }
}
