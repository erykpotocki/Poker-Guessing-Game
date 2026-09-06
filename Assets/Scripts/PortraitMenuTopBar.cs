using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PortraitMenuTopBar : MonoBehaviour
{
    private static readonly string[] MenuScenes = { "MainMenu", "GameModeSelect", "CreateRoom", "JoinRoom", "Lobby" };
    private static Sprite spinSprite;
    private Canvas owner;
    private RectTransform bar;
    private TMP_Text goldText, diamondText;
    private Image profileImage;
    private Vector2Int lastScreen;
    private Rect lastSafeArea;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallBootstrap()
    {
        if (FindFirstObjectByType<PortraitMenuTopBarBootstrap>() != null) return;
        GameObject obj = new GameObject("PortraitMenuTopBarBootstrap", typeof(PortraitMenuTopBarBootstrap));
        DontDestroyOnLoad(obj);
    }

    public static bool Supports(string sceneName)
    {
        foreach (string item in MenuScenes) if (item == sceneName) return true;
        return false;
    }

    public static void Ensure(Canvas canvas)
    {
        if (canvas == null || canvas.rootCanvas.transform.Find("PortraitMenuTopBar") != null) return;
        GameObject obj = new GameObject("PortraitMenuTopBar", typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(GraphicRaycaster), typeof(PortraitMenuTopBar));
        obj.transform.SetParent(canvas.rootCanvas.transform, false);
        RectTransform root = obj.transform as RectTransform;
        root.anchorMin = new Vector2(0f, 1f); root.anchorMax = Vector2.one; root.pivot = new Vector2(.5f, 1f);
        root.offsetMin = root.offsetMax = Vector2.zero;
        Image background = obj.GetComponent<Image>(); background.color = new Color(.006f, .008f, .008f, .96f); background.raycastTarget = false;
        Canvas layer = obj.GetComponent<Canvas>(); layer.overrideSorting = true; layer.sortingOrder = 460;
        PortraitMenuTopBar component = obj.GetComponent<PortraitMenuTopBar>(); component.owner = canvas.rootCanvas; component.Build();
    }

    private void Build()
    {
        bar = transform as RectTransform;
        goldText = CreateText("Gold", new Color(1f, .72f, .16f));
        diamondText = CreateText("Diamonds", new Color(.3f, .78f, 1f));
        CreateIconButton("Spin", CreateSpinSprite(), ShowSpin, out _);
        CreateIconButton("Profile", null, ShowProfile, out profileImage);
        PlayerProfileService.Changed += Refresh;
        Layout(); Refresh();
    }

    private TMP_Text CreateText(string name, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); obj.transform.SetParent(transform, false);
        TMP_Text text = obj.GetComponent<TextMeshProUGUI>(); text.fontSize = 31; text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.MidlineLeft; text.color = color; text.raycastTarget = false; return text;
    }

    private void CreateIconButton(string name, Sprite sprite, UnityEngine.Events.UnityAction action, out Image icon)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Button)); buttonObject.transform.SetParent(transform, false);
        Button button = buttonObject.GetComponent<Button>(); button.onClick.AddListener(action);
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image)); iconObject.transform.SetParent(buttonObject.transform, false);
        icon = iconObject.GetComponent<Image>(); icon.sprite = sprite; icon.preserveAspect = true;
        RectTransform iconRect = icon.rectTransform; iconRect.anchorMin = Vector2.zero; iconRect.anchorMax = Vector2.one; iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
        button.targetGraphic = icon; ColorBlock colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(1f, .9f, .58f); colors.pressedColor = new Color(.72f, .72f, .72f); button.colors = colors;
    }

    private void Refresh()
    {
        if (goldText == null) return;
        var data = PlayerProfileService.Data;
        goldText.text = "●  " + data.Wallet.Coins;
        diamondText.text = "◆  " + data.Wallet.RewardCurrency;
        AvatarDatabase avatars = Resources.Load<AvatarDatabase>("ProfileAvatars");
        if (profileImage != null && avatars != null && avatars.avatars != null && avatars.avatars.Length > 0)
            profileImage.sprite = avatars.avatars[Mathf.Clamp(PlayerProfileService.AvatarIndex, 0, avatars.avatars.Length - 1)];
    }

    private void Update()
    {
        if (lastScreen != new Vector2Int(Screen.width, Screen.height) || lastSafeArea != Screen.safeArea) Layout();
    }

    private void Layout()
    {
        if (owner == null || bar == null || Screen.width <= 0 || Screen.height <= 0) return;
        lastScreen = new Vector2Int(Screen.width, Screen.height); lastSafeArea = Screen.safeArea;
        RectTransform canvasRect = owner.transform as RectTransform;
        float sx = canvasRect.rect.width / Screen.width, sy = canvasRect.rect.height / Screen.height;
        float safeTop = (Screen.height - Screen.safeArea.yMax) * sy;
        float safeLeft = Screen.safeArea.xMin * sx, safeRight = (Screen.width - Screen.safeArea.xMax) * sx;
        float height = safeTop + 100f; bar.sizeDelta = new Vector2(0f, height);
        Place(goldText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(safeLeft + 28f, -(safeTop + 50f)), new Vector2(170f, 70f));
        Place(diamondText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(safeLeft + 205f, -(safeTop + 50f)), new Vector2(190f, 70f));
        RectTransform spin = transform.Find("Spin") as RectTransform, profile = transform.Find("Profile") as RectTransform;
        Place(profile, Vector2.one, Vector2.one, new Vector2(-(safeRight + 54f), -(safeTop + 50f)), new Vector2(72f, 72f));
        Place(spin, Vector2.one, Vector2.one, new Vector2(-(safeRight + 146f), -(safeTop + 50f)), new Vector2(68f, 68f));
    }

    private static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        if (rect == null) return; rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size;
    }

    private void ShowProfile() { if (owner != null) PlayerProfileUI.Show(owner); }
    private void ShowSpin() { if (owner != null) SpinRewardUI.Show(owner); }
    private void OnDestroy() { PlayerProfileService.Changed -= Refresh; }

    private static Sprite CreateSpinSprite()
    {
        if (spinSprite != null) return spinSprite;
        const int size = 96; Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false); texture.name = "DefaultSpinIcon";
        Color[] colors = { new Color(1f,.24f,.18f), new Color(1f,.73f,.12f), new Color(.24f,.82f,.5f), new Color(.25f,.62f,1f), new Color(.72f,.3f,1f), new Color(1f,.3f,.68f) };
        Vector2 center = new Vector2((size-1)*.5f, (size-1)*.5f);
        for (int y=0;y<size;y++) for (int x=0;x<size;x++)
        {
            Vector2 delta = new Vector2(x,y)-center; float radius=delta.magnitude;
            if(radius>45f){texture.SetPixel(x,y,Color.clear);continue;}
            if(radius<10f){texture.SetPixel(x,y,new Color(1f,.88f,.38f));continue;}
            float angle=Mathf.Atan2(delta.y,delta.x)+Mathf.PI; int segment=Mathf.FloorToInt(angle/(Mathf.PI*2f)*colors.Length)%colors.Length;
            texture.SetPixel(x,y,radius>40f?new Color(1f,.82f,.25f):colors[segment]);
        }
        texture.Apply(); spinSprite = Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100f); spinSprite.name="DefaultSpinIcon"; return spinSprite;
    }
}

public sealed class PortraitMenuTopBarBootstrap : MonoBehaviour
{
    private void Awake() { SceneManager.sceneLoaded += Loaded; StartCoroutine(Install(SceneManager.GetActiveScene())); }
    private void OnDestroy() { SceneManager.sceneLoaded -= Loaded; }
    private void Loaded(Scene scene, LoadSceneMode mode) { StartCoroutine(Install(scene)); }
    private IEnumerator Install(Scene scene)
    {
        if (!PortraitMenuTopBar.Supports(scene.name)) yield break;
        yield return null; yield return null;
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (canvas != null && canvas.isRootCanvas && canvas.gameObject.scene == scene && canvas.renderMode != RenderMode.WorldSpace)
            { PortraitMenuTopBar.Ensure(canvas); yield break; }
        }
    }
}
