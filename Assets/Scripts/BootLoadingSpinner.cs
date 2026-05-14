using UnityEngine;

public class BootLoadingSpinner : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;

    private float animationMultiplier = 1f;

    private void Update()
    {
        transform.Rotate(0f, 0f, -rotationSpeed * animationMultiplier * Time.unscaledDeltaTime);
    }

    public void SetAnimationMultiplier(float value)
    {
        animationMultiplier = Mathf.Clamp01(value);
    }
}