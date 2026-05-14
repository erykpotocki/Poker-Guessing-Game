using UnityEngine;

public class BootLoadingLogoPulse : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 0.45f;
    [SerializeField] private float pulseAmount = 0.02f;

    private Vector3 baseScale;
    private float animationMultiplier = 1f;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float scale = 1f + (t * pulseAmount * animationMultiplier);
        transform.localScale = baseScale * scale;
    }

    public void SetAnimationMultiplier(float value)
    {
        animationMultiplier = Mathf.Clamp01(value);
    }
}