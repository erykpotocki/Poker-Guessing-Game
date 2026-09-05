using UnityEngine;

public sealed class MultiplayerTableLayout : MonoBehaviour
{
    public RectTransform Initialize(RectTransform center)
    {
        RectTransform root = center.GetComponentInParent<Canvas>().rootCanvas.transform as RectTransform;
        // Keep the authored sizes, seat radii and status positions.
        Vector2 shift = new Vector2(0f, -38f);
        center.anchoredPosition += shift;
        RectTransform table = root.Find("Table") as RectTransform;
        if (table != null) table.anchoredPosition += shift;
        return root;
    }
}
