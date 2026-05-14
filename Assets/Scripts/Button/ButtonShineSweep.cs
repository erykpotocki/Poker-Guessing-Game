using UnityEngine;
using UnityEngine.UI;

public class ButtonShineSweep : MonoBehaviour
{
    [SerializeField] private RectTransform shineRect;
    [SerializeField] private Image shineImage;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float interval = 3.5f;
    [SerializeField] private float sweepDuration = 0.8f;
    [SerializeField] private float maxAlpha = 0.28f;

    private RectTransform parentRect;
    private float timer;
    private Color baseColor;

    private void Awake()
    {
        if (shineRect == null)
            shineRect = GetComponent<RectTransform>();

        if (shineImage == null)
            shineImage = GetComponent<Image>();

        if (shineRect != null && shineRect.parent != null)
            parentRect = shineRect.parent as RectTransform;

        if (shineImage != null)
            baseColor = shineImage.color;
    }

    private void OnEnable()
    {
        timer = -startDelay;
        MoveToStart();
        SetAlpha(0f);
    }

    private void Update()
    {
        if (shineRect == null || parentRect == null)
            return;

        timer += Time.unscaledDeltaTime;

        float cycleLength = sweepDuration + interval;

        if (timer < 0f)
        {
            MoveToStart();
            SetAlpha(0f);
            return;
        }

        float cycleTime = timer % cycleLength;

        if (cycleTime <= sweepDuration)
        {
            float t = cycleTime / sweepDuration;

            float startX = GetStartX();
            float endX = GetEndX();

            Vector2 pos = shineRect.anchoredPosition;
            pos.x = Mathf.Lerp(startX, endX, t);
            shineRect.anchoredPosition = pos;

            float edgeFade = Mathf.Sin(t * Mathf.PI);
            SetAlpha(edgeFade * maxAlpha);
        }
        else
        {
            MoveToStart();
            SetAlpha(0f);
        }
    }

    private void MoveToStart()
    {
        if (shineRect == null)
            return;

        Vector2 pos = shineRect.anchoredPosition;
        pos.x = GetStartX();
        shineRect.anchoredPosition = pos;
    }

    private float GetStartX()
    {
        float parentWidth = parentRect.rect.width;
        float shineWidth = shineRect.rect.width;
        return -(parentWidth * 0.5f) - shineWidth;
    }

    private float GetEndX()
    {
        float parentWidth = parentRect.rect.width;
        float shineWidth = shineRect.rect.width;
        return (parentWidth * 0.5f) + shineWidth;
    }

    private void SetAlpha(float alpha)
    {
        if (shineImage == null)
            return;

        Color color = baseColor;
        color.a = alpha;
        shineImage.color = color;
    }
}