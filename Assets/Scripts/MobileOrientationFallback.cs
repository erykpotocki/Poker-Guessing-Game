using UnityEngine;

// Kept for existing scene/script references. HotSeatOrientationLock and the PWA
// viewport handle orientation without interrupting the user with a modal.
public sealed class MobileOrientationFallback : MonoBehaviour
{
    private void OnEnable()
    {
        Transform previousBlocker = transform.Find("RotatePhone");
        if (previousBlocker != null) previousBlocker.gameObject.SetActive(false);
        enabled = false;
    }
}
