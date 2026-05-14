using UnityEngine;

public class CardSpawnTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform cardsParent;
    [SerializeField] private CardBackDatabase cardBackDatabase;

    [Header("Test settings")]
    [SerializeField] private int backIndex = 0;

    private void Start()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("CardSpawnTest: cardPrefab nie jest przypisany.");
            return;
        }

        if (cardsParent == null)
        {
            Debug.LogError("CardSpawnTest: cardsParent nie jest przypisany.");
            return;
        }

        if (cardBackDatabase == null)
        {
            Debug.LogError("CardSpawnTest: cardBackDatabase nie jest przypisany.");
            return;
        }

        CardView spawnedCard = Instantiate(cardPrefab, cardsParent);
        spawnedCard.SetBack(cardBackDatabase, backIndex);

        RectTransform rect = spawnedCard.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }
}