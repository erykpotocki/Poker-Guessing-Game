using UnityEngine;
using UnityEngine.UI;

public class DealtCardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    private Outline reviewOutline;

    public void SetReviewHighlight(bool visible, Color color)
    {
        if (cardImage == null) cardImage = GetComponent<Image>();
        if (cardImage == null) return;
        if (reviewOutline == null) reviewOutline = cardImage.gameObject.AddComponent<Outline>();
        reviewOutline.effectColor = color;
        reviewOutline.effectDistance = new Vector2(5f, -5f);
        reviewOutline.enabled = visible;
    }

    public void ShowFront(Sprite frontSprite)
    {
        if (cardImage == null)
            cardImage = GetComponent<Image>();

        if (frontSprite == null)
        {
            Debug.LogWarning("DealtCardView: frontSprite jest null.");
            return;
        }

        cardImage.sprite = frontSprite;
        cardImage.color = Color.white;
        cardImage.preserveAspect = true;
        cardImage.enabled = true;
    }
}
