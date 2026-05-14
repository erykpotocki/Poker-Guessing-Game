using UnityEngine;
using UnityEngine.UI;

public class AvatarPicker : MonoBehaviour
{
    [SerializeField] private Image avatarPreview;
    [SerializeField] private AvatarDatabase avatarDatabase;

    private const string PrefKey = "avatarIndex";

    private void Start()
    {
        if (avatarDatabase == null || avatarDatabase.avatars.Length == 0)
            return;

        int idx = PlayerPrefs.GetInt(PrefKey, Random.Range(0, avatarDatabase.avatars.Length));
        SetAvatar(idx);
    }

    public void RollAvatar()
    {
        if (avatarDatabase == null || avatarDatabase.avatars.Length == 0)
            return;

        int idx = Random.Range(0, avatarDatabase.avatars.Length);
        SetAvatar(idx);
    }

    private void SetAvatar(int idx)
    {
        idx = Mathf.Clamp(idx, 0, avatarDatabase.avatars.Length - 1);

        PlayerPrefs.SetInt(PrefKey, idx);
        PlayerPrefs.Save();

        if (avatarPreview != null)
            avatarPreview.sprite = avatarDatabase.avatars[idx];
    }
}