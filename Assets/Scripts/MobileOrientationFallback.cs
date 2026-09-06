using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MobileOrientationFallback : MonoBehaviour
{
    private GameObject blocker;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (!Application.isMobilePlatform && !Application.isEditor) return;
        GameObject owner = new GameObject("MobileOrientationFallback");
        DontDestroyOnLoad(owner);
        owner.AddComponent<MobileOrientationFallback>();
#endif
    }
    private void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        gameObject.AddComponent<GraphicRaycaster>();
        blocker = new GameObject("RotatePhone", typeof(RectTransform), typeof(Image));
        blocker.transform.SetParent(transform, false);
        RectTransform rect = blocker.transform as RectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        blocker.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.04f);
        TMP_Text text = new GameObject("Instruction", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
        text.transform.SetParent(rect, false);
        text.rectTransform.anchorMin = new Vector2(.08f,.2f); text.rectTransform.anchorMax = new Vector2(.92f,.8f);
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
        text.text = "OBRÓĆ TELEFON\n\nRozgrywka działa poziomo.";
        text.alignment = TextAlignmentOptions.Center; text.fontSize = 32;
        text.color = new Color(1f,.9f,.68f);
        text.raycastTarget = false;
        blocker.SetActive(false);
    }
    private void Update()
    {
        string scene = SceneManager.GetActiveScene().name;
        blocker.SetActive((scene == "Game" || scene == "BootLoading") && Screen.height > Screen.width);
    }
}
