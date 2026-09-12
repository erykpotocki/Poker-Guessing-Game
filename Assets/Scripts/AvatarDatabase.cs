using UnityEngine;

[CreateAssetMenu(menuName = "Poker/Avatar Database")]
public class AvatarDatabase : ScriptableObject
{
    public Sprite[] avatars;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void PrepareLoadedDatabases()
    {
        foreach (AvatarDatabase database in Resources.FindObjectsOfTypeAll<AvatarDatabase>()) database.OnEnable();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying || avatars == null) return;
        var combined = new System.Collections.Generic.List<Sprite>(avatars);
        Sprite[] downloads = Resources.LoadAll<Sprite>("ShopAvatars");
        System.Array.Sort(downloads, (a, b) => System.StringComparer.Ordinal.Compare(a.name, b.name));
        foreach (Sprite sprite in downloads)
            if (!combined.Exists(item => item != null && item.name == sprite.name)) combined.Add(sprite);
        avatars = combined.ToArray();
    }
}
