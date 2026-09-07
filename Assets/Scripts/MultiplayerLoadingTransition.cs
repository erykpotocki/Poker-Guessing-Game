using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Shows the match loader before leaving the portrait lobby.</summary>
public sealed class MultiplayerLoadingTransition : MonoBehaviourPunCallbacks
{
    private static MultiplayerLoadingTransition instance;
    private GameLoadingUI loadingUI;
    public static bool IsActive => instance != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState() => instance = null;

    public static void Begin()
    {
        if (instance != null) return;

        GameObject root = new GameObject("MultiplayerLoadingTransition",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(root);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        instance = root.AddComponent<MultiplayerLoadingTransition>();
        GameObject panel = new GameObject("MatchLoadingPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        TMP_Text label = new GameObject("LoadingStatus", typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TMP_Text>();
        label.transform.SetParent(panel.transform, false);
        instance.loadingUI = root.AddComponent<GameLoadingUI>();
        instance.loadingUI.InitializeStandalone(panel, label);
        instance.loadingUI.ShowLoading("Przygotowywanie rozgrywki…");
        SceneManager.sceneLoaded += instance.SceneLoaded;
        HotSeatOrientationLock.LockLandscape();
    }

    public static GameLoadingUI UseForGame(GameLoadingUI sceneLoader)
    {
        if (instance == null) return sceneLoader;
        if (sceneLoader != null) sceneLoader.HideLoading();
        return instance.loadingUI;
    }

    public static void Finish()
    {
        if (instance == null) return;
        MultiplayerLoadingTransition previous = instance;
        instance = null;
        previous.gameObject.SetActive(false);
        Destroy(previous.gameObject);
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Returning to any menu also removes an interrupted match transition.
        if (mode == LoadSceneMode.Single && scene.name != "Game") Finish();
    }

    public override void OnLeftRoom() => CancelBeforeGame();

    public override void OnDisconnected(DisconnectCause cause) => CancelBeforeGame();

    private static void CancelBeforeGame()
    {
        // If joining fails before the scene change, do not leave an opaque
        // persistent loader above the lobby. Game handles its own reconnection.
        if (SceneManager.GetActiveScene().name == "Game") return;
        Finish();
        HotSeatOrientationLock.LockPortrait();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        if (instance == this) instance = null;
    }
}
