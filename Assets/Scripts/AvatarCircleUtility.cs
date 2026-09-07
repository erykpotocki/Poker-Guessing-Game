using UnityEngine;
using UnityEngine.UI;

/// <summary>Applies the same fixed circular viewport to avatars in every UI.</summary>
public static class AvatarCircleUtility
{
    private static Sprite maskSprite;

    public static Image Apply(Image source)
    {
        if (source == null) return null;
        Transform existing = source.transform.Find("CircularAvatarContent");
        if (existing != null)
        {
            Image existingContent = existing.GetComponent<Image>();
            if (existingContent != null && source.sprite != maskSprite)
                existingContent.sprite = source.sprite;
            source.sprite = MaskSprite();
            return existingContent ?? source;
        }

        Sprite avatar = source.sprite;
        Color tint = source.color;
        bool raycast = source.raycastTarget;
        source.sprite = MaskSprite();
        source.color = Color.white;
        source.type = Image.Type.Simple;
        Mask mask = source.GetComponent<Mask>() ?? source.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject child = new GameObject("CircularAvatarContent", typeof(RectTransform), typeof(Image));
        child.transform.SetParent(source.transform, false);
        RectTransform rect = child.transform as RectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image content = child.GetComponent<Image>();
        content.sprite = avatar; content.color = tint; content.preserveAspect = true;
        content.raycastTarget = raycast;
        source.raycastTarget = raycast;
        return content;
    }

    private static Sprite MaskSprite()
    {
        if (maskSprite != null) return maskSprite;
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * .5f, (size - 1) * .5f);
        float radius = size * .49f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float alpha = Mathf.Clamp01(radius - Vector2.Distance(new Vector2(x, y), center) + 1f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        texture.Apply();
        maskSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f);
        maskSprite.name = "CircularAvatarMask";
        return maskSprite;
    }
}
