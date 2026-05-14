using UnityEngine;

public class CardBackDatabase : MonoBehaviour
{
    [Header("Card back sprites")]
    [SerializeField] private Sprite[] backSprites;

    public Sprite GetBackSprite(int index = 0)
    {
        if (backSprites == null || backSprites.Length == 0)
        {
            Debug.LogError("CardBackDatabase: brak przypisanych rewersów kart.");
            return null;
        }

        if (index < 0 || index >= backSprites.Length)
        {
            Debug.LogWarning($"CardBackDatabase: index {index} poza zakresem. Zwracam pierwszy rewers.");
            return backSprites[0];
        }

        return backSprites[index];
    }
}