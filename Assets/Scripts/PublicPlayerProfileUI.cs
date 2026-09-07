using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PublicPlayerProfileUI : MonoBehaviour
{
    public static void Show(Canvas canvas, Sprite avatar, string nickname, int games, int wins, string profileId)
    {
        if (canvas == null) return;
        Transform old = canvas.rootCanvas.transform.Find("PublicPlayerProfile");
        if (old != null) Destroy(old.gameObject);

        GameObject overlay = new GameObject("PublicPlayerProfile", typeof(RectTransform), typeof(Image),
            typeof(Canvas), typeof(GraphicRaycaster));
        overlay.transform.SetParent(canvas.rootCanvas.transform, false);
        RectTransform root = overlay.transform as RectTransform;
        root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        PortraitMenuTopBar.ApplyOverlayInset(root);
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, .78f);
        Canvas layer = overlay.GetComponent<Canvas>(); layer.overrideSorting = true; layer.sortingOrder = 620;

        RectTransform card = MakeRect("ProfileCard", root, new Vector2(.5f,.5f), Vector2.zero, new Vector2(680f,480f));
        card.gameObject.AddComponent<Image>().color = new Color(.12f,.025f,.018f,.98f);

        Image portrait = MakeRect("Avatar", card, new Vector2(.5f,1f), new Vector2(0f,-52f), new Vector2(150f,150f))
            .gameObject.AddComponent<Image>();
        portrait.sprite = avatar; portrait.preserveAspect = true; portrait.raycastTarget = false;
        AvatarCircleUtility.Apply(portrait);
        Label(card, nickname, new Vector2(0f,-220f), new Vector2(620f,62f), 40f);
        Label(card, "Rozegrane gry: " + Mathf.Max(0,games), new Vector2(0f,-292f), new Vector2(620f,48f), 30f);
        Label(card, "Wygrane: " + Mathf.Max(0,wins), new Vector2(0f,-344f), new Vector2(620f,48f), 30f);
        PokerProfile.OpponentRecord record = PlayerProfileService.GetOpponent(profileId);
        string versus = record == null ? "Brak wspólnych zakończonych gier" :
            "Wspólne gry: " + record.GamesTogether + "   Twoje wygrane: " + record.WinsAgainst;
        Label(card, versus, new Vector2(0f,-398f), new Vector2(620f,44f), 25f);

        Button close = MakeRect("Close", card, new Vector2(1f,1f), new Vector2(-28f,-28f), new Vector2(100f,64f))
            .gameObject.AddComponent<Button>();
        close.gameObject.AddComponent<Image>(); close.targetGraphic = close.GetComponent<Image>();
        TMP_Text closeText = Label(close.transform, "ZAMKNIJ", Vector2.zero, new Vector2(100f,64f), 20f);
        closeText.rectTransform.anchorMin = Vector2.zero; closeText.rectTransform.anchorMax = Vector2.one;
        closeText.rectTransform.offsetMin = closeText.rectTransform.offsetMax = Vector2.zero;
        close.onClick.AddListener(() => Destroy(overlay)); PokerButtonTheme.ApplyTo(close);
    }

    private static RectTransform MakeRect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        RectTransform rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent,false); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor;
        rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
    }

    private static TMP_Text Label(Transform parent,string value,Vector2 position,Vector2 size,float fontSize)
    {
        RectTransform rect = MakeRect("Label",parent,new Vector2(.5f,1f),position,size);
        TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.text = value;
        text.fontSize = fontSize; text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f,.91f,.72f); text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap; return text;
    }
}
