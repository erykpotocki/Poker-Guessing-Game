using UnityEngine;
using UnityEngine.UI;

public class AvatarPicker : MonoBehaviour
{
    [SerializeField] private Image avatarPreview;
    [SerializeField] private AvatarDatabase avatarDatabase;

    private const string PrefKey = "avatarIndex";
    private static int? temporaryAvatar;
    private static string avatarRoom;
    public static void ClearMatchAvatar(){temporaryAvatar=null;avatarRoom=null;}
    public static int ForCurrentRoom()
    {
        string room=Photon.Pun.PhotonNetwork.CurrentRoom?.Name;
        if(avatarRoom!=null && avatarRoom!=room)ClearMatchAvatar();
        if(room!=null)avatarRoom=room;
        return temporaryAvatar??PlayerProfileService.AvatarIndex;
    }

    private void Start()
    {
        if(!Photon.Pun.PhotonNetwork.InRoom)ClearMatchAvatar();
        if (avatarDatabase == null || avatarDatabase.avatars.Length == 0)
            return;

        int idx = PlayerProfileService.AvatarIndex;
        SetAvatar(idx);
    }

    public void RollAvatar()
    {
        if (avatarDatabase == null || avatarDatabase.avatars.Length == 0)
            return;

        var available=new System.Collections.Generic.List<int>();
        for(int i=0;i<avatarDatabase.avatars.Length;i++)
            if(avatarDatabase.avatars[i]!=null && PlayerProfileService.Data.Inventory.OwnedAvatars.Contains(PlayerProfileService.AvatarId(i,avatarDatabase.avatars[i])))available.Add(i);
        if(available.Count==0)return;
        int idx = available[Random.Range(0,available.Count)];
        temporaryAvatar=idx;
        SetAvatar(idx);
    }

    private void SetAvatar(int idx)
    {
        idx = Mathf.Clamp(idx, 0, avatarDatabase.avatars.Length - 1);

        if (avatarPreview != null)
        {
            avatarPreview.sprite = avatarDatabase.avatars[idx];
            AvatarCircleUtility.Apply(avatarPreview);
        }
    }
}
