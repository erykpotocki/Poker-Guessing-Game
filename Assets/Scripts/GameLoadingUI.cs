using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLoadingUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text tipText;
    private RawImage artwork;
    private RectTransform spinner;
    private Texture2D ringTexture;
    private Sprite ringSprite;
    private bool showing;
    private float elapsed;
    private static int lastArtwork = -1;

    private void Start() => ShowLoading();

    public void ShowLoading(string message = "")
    {
        if (loadingPanel == null) return;
        BuildVisuals();
        loadingPanel.SetActive(true);
        loadingPanel.transform.SetAsLastSibling();
        if (!showing)
        {
            LoadingArtworkLibrary library = Resources.Load<LoadingArtworkLibrary>("UI/LoadingArtwork");
            if (library != null && library.backgrounds != null && library.backgrounds.Length > 0)
            {
                int count = library.backgrounds.Length;
                int index = lastArtwork < 0 || lastArtwork >= count
                    ? Random.Range(0, count)
                    : (lastArtwork + Random.Range(1, Mathf.Max(2, count))) % count;
                lastArtwork = index;
                artwork.texture = library.backgrounds[index];
            }
            elapsed = 0f;
        }
        showing = true;
    }

    public void HideLoading()
    {
        showing = false;
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    private void BuildVisuals()
    {
        if (loadingText != null) loadingText.gameObject.SetActive(false);
        if (tipText != null) tipText.gameObject.SetActive(false);
        if (artwork != null) return;

        RectTransform panel = loadingPanel.GetComponent<RectTransform>();
        Canvas canvas = loadingPanel.GetComponentInParent<Canvas>();
        if (canvas != null && loadingPanel != canvas.rootCanvas.gameObject)
            panel.SetParent(canvas.rootCanvas.transform, false);
        Stretch(panel);
        Image blocker = loadingPanel.GetComponent<Image>();
        if (blocker == null) blocker = loadingPanel.AddComponent<Image>();
        blocker.color = new Color(0.015f, 0.025f, 0.025f, 1f);
        blocker.raycastTarget = true;

        GameObject background = new GameObject("LoadingArtwork", typeof(RectTransform), typeof(RawImage));
        background.transform.SetParent(panel, false);
        artwork = background.GetComponent<RawImage>();
        Stretch(artwork.rectTransform);
        artwork.color = new Color(0.68f, 0.68f, 0.68f, 1f);
        artwork.raycastTarget = false;

        CreateRingSprite();
        GameObject ring = new GameObject("LoadingSpinner", typeof(RectTransform), typeof(Image));
        ring.transform.SetParent(panel, false);
        spinner = ring.GetComponent<RectTransform>();
        spinner.anchorMin = spinner.anchorMax = new Vector2(0.5f, 0.16f);
        spinner.anchoredPosition = Vector2.zero;
        Image image = ring.GetComponent<Image>();
        image.sprite = ringSprite;
        image.color = new Color(1f, 0.78f, 0.30f);
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillAmount = 0.72f;
        image.raycastTarget = false;
    }

    private void LateUpdate()
    {
        if (!showing || loadingPanel == null || !loadingPanel.activeSelf || artwork == null) return;
        loadingPanel.transform.SetAsLastSibling();
        elapsed += Time.unscaledDeltaTime;
        spinner.localRotation = Quaternion.Euler(0f, 0f, -elapsed * 210f);
        Rect rect = artwork.rectTransform.rect;
        float spinnerSize = Mathf.Clamp(Mathf.Min(rect.width, rect.height) * 0.10f, 48f, 100f);
        spinner.sizeDelta = new Vector2(spinnerSize, spinnerSize);
        if (artwork.texture == null || rect.height <= 0f) return;
        float screenAspect = rect.width / rect.height;
        float artAspect = (float)artwork.texture.width / artwork.texture.height;
        // Center-crop to cover any phone aspect ratio without stretching or bars.
        artwork.uvRect = screenAspect > artAspect
            ? new Rect(0f, (1f - artAspect / screenAspect) * 0.5f, 1f, artAspect / screenAspect)
            : new Rect((1f - screenAspect / artAspect) * 0.5f, 0f, screenAspect / artAspect, 1f);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private void CreateRingSprite()
    {
        const int size = 128;
        ringTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        ringTexture.filterMode = FilterMode.Bilinear;
        ringTexture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float distance = new Vector2(x + 0.5f - size / 2f, y + 0.5f - size / 2f).magnitude;
            float alpha = Mathf.Clamp01(59f - distance) * Mathf.Clamp01(distance - 52f);
            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
        }
        ringTexture.SetPixels(pixels);
        ringTexture.Apply(false, true);
        ringSprite = Sprite.Create(ringTexture, new Rect(0f, 0f, size, size), Vector2.one * 0.5f);
    }

    private void OnDestroy()
    {
        if (ringSprite != null) Destroy(ringSprite);
        if (ringTexture != null) Destroy(ringTexture);
    }
}
