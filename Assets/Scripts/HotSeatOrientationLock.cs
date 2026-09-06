using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class HotSeatOrientationLock : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void PokerSetOrientation(int landscape);
    [DllImport("__Internal")] private static extern float PokerKeyboardFraction();
    [DllImport("__Internal")] private static extern void PokerRefreshViewport();
#endif
    public static float KeyboardFraction
    {
        get {
#if UNITY_WEBGL && !UNITY_EDITOR
            return PokerKeyboardFraction();
#else
            return Screen.height > 0 ? TouchScreenKeyboard.area.height / Screen.height : 0f;
#endif
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LockPortraitBeforeFirstScene()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.mobileKeyboardSupport = true;
#endif
        LockPortrait();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Awake()
    {
        LockPortrait();
    }

    private void Start()
    {
        LockPortrait();
    }

    public static void LockPortrait()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PokerSetOrientation(0);
#endif
        Screen.orientation = ScreenOrientation.Portrait;

        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
    }

    public static void LockLandscape()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PokerSetOrientation(1);
#endif
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool landscape = scene.name == "Game" || scene.name == "BootLoading";
        if (landscape)
            LockLandscape();
        else
            LockPortrait();

        ConfigureCanvases(landscape);
    }

    private static void ConfigureCanvases(bool landscape)
    {
        CanvasScaler[] scalers = Object.FindObjectsByType<CanvasScaler>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CanvasScaler scaler in scalers)
        {
            if (scaler == null)
                continue;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = landscape
                ? new Vector2(1920f, 1080f)
                : new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = landscape ? 1f : 0f;
        }

        OverscanFullScreenBackgrounds();
    }

    private static void OverscanFullScreenBackgrounds()
    {
        Image[] images = Object.FindObjectsByType<Image>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Image image in images)
        {
            if (image == null ||
                (image.name != "Background" && image.name != "BackgroundGame" && image.name != "Czarne"))
                continue;

            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            if (image.sprite != null)
            {
                AspectRatioFitter cover = image.GetComponent<AspectRatioFitter>();
                if (cover == null) cover = image.gameObject.AddComponent<AspectRatioFitter>();
                cover.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
                cover.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            }
            image.preserveAspect = true;
        }
    }
}
