using UnityEngine;

public class CardBackDatabase : MonoBehaviour
{
    [Header("Card back sprites")]
    [SerializeField] private Sprite[] backSprites;

    private Sprite[] resourceBackSprites;
    public bool Online;
    private static Sprite[] onlineSprites;
    public static Sprite[] OnlineSprites
    {
        get
        {
            if(onlineSprites!=null)return onlineSprites;
            var textures=Resources.LoadAll<Texture2D>("OnlineBacks");
            System.Array.Sort(textures,(a,b)=>a.name==b.name?0:a.name=="6"?-1:b.name=="6"?1:string.CompareOrdinal(a.name,b.name));
            onlineSprites=new Sprite[textures.Length];
            for(int i=0;i<textures.Length;i++)
            {var t=textures[i];onlineSprites[i]=Sprite.Create(t,new Rect(t.width*.04f,t.height*.045f,t.width*.92f,t.height*.91f),new Vector2(.5f,.5f),100);onlineSprites[i].name=t.name;}
            return onlineSprites;
        }
    }
    public static Sprite FindOnline(string id)=>System.Array.Find(OnlineSprites,s=>s.name==id)??(OnlineSprites.Length>0?OnlineSprites[0]:null);

    public int BackCount => GetAvailableBackSprites().Length;

    private Sprite[] GetAvailableBackSprites()
    {
        if(Online||gameObject.scene.name=="Game")return OnlineSprites;
        if (resourceBackSprites == null)
        {
            Texture2D[] textures = Resources.LoadAll<Texture2D>("CardBacks");
            resourceBackSprites = new Sprite[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D texture = textures[i];
                resourceBackSprites[i] = Sprite.Create(
                    texture,
                    GetCardArtworkRect(texture),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
            }
        }

        return resourceBackSprites.Length > 0
            ? resourceBackSprites
            : backSprites ?? System.Array.Empty<Sprite>();
    }

    private static Rect GetCardArtworkRect(Texture2D texture)
    {
        float left = 0f;
        float bottom = 0f;
        float width = 1f;
        float height = 1f;

        if (texture.name.Contains("Ornate"))
        {
            left = 0.105f;
            bottom = 0.115f;
            width = 0.79f;
            height = 0.77f;
        }
        else if (texture.name.Contains("RedDiamond"))
        {
            left = 0.135f;
            bottom = 0.125f;
            width = 0.73f;
            height = 0.75f;
        }
        else if (texture.name.Contains("Suits"))
        {
            left = 0.07f;
            bottom = 0.045f;
            width = 0.86f;
            height = 0.91f;
        }

        return new Rect(
            texture.width * left,
            texture.height * bottom,
            texture.width * width,
            texture.height * height
        );
    }

    public Sprite GetBackSprite(int index = 0)
    {
        Sprite[] availableBackSprites = GetAvailableBackSprites();

        if (availableBackSprites.Length == 0)
        {
            Debug.LogError("CardBackDatabase: brak przypisanych rewersów kart.");
            return null;
        }

        if (index < 0 || index >= availableBackSprites.Length)
        {
            Debug.LogWarning($"CardBackDatabase: index {index} poza zakresem. Zwracam pierwszy rewers.");
            return availableBackSprites[0];
        }

        return availableBackSprites[index];
    }

    public int FindBackIndex(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return 0;
        Sprite[] available = GetAvailableBackSprites();
        for (int i = 0; i < available.Length; i++)
            if (available[i] != null && available[i].texture != null && available[i].texture.name == id)
                return i;
        return 0;
    }

}
