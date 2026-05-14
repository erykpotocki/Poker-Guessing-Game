using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FireRandomAnimator : MonoBehaviour
{
    [Header("Fire Layers")]
    [SerializeField] private Image imageA;
    [SerializeField] private Image imageB;
    [SerializeField] private Image imageC;

    [Header("Frames")]
    [SerializeField] private Sprite[] fireFrames;

    [Header("Timing")]
    [SerializeField] private float changeTimeMin = 0.18f;
    [SerializeField] private float changeTimeMax = 0.40f;

    [Header("Start Delays")]
    [SerializeField] private float startDelayA = 0f;
    [SerializeField] private float startDelayB = 0.10f;
    [SerializeField] private float startDelayC = 0.20f;

    [Header("Alpha")]
    [SerializeField] private float alphaA = 0.35f;
    [SerializeField] private float alphaB = 0.25f;
    [SerializeField] private float alphaC = 0.18f;

    private Coroutine routineA;
    private Coroutine routineB;
    private Coroutine routineC;

    private void Start()
    {
        if (!IsReady())
            return;

        SetAlpha(imageA, alphaA);
        SetAlpha(imageB, alphaB);
        SetAlpha(imageC, alphaC);

        routineA = StartCoroutine(AnimateLayer(imageA, startDelayA));
        routineB = StartCoroutine(AnimateLayer(imageB, startDelayB));
        routineC = StartCoroutine(AnimateLayer(imageC, startDelayC));
    }

    private bool IsReady()
    {
        if (imageA == null || imageB == null || imageC == null)
            return false;

        if (fireFrames == null || fireFrames.Length == 0)
            return false;

        return true;
    }

    private IEnumerator AnimateLayer(Image targetImage, float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        int lastFrameIndex = -1;

        while (true)
        {
            int nextFrameIndex = GetRandomNextFrameIndex(lastFrameIndex);
            lastFrameIndex = nextFrameIndex;

            targetImage.sprite = fireFrames[nextFrameIndex];

            float wait = Random.Range(changeTimeMin, changeTimeMax);
            yield return new WaitForSeconds(wait);
        }
    }

    private int GetRandomNextFrameIndex(int currentIndex)
    {
        if (fireFrames.Length == 1)
            return 0;

        int nextIndex = currentIndex;

        while (nextIndex == currentIndex)
            nextIndex = Random.Range(0, fireFrames.Length);

        return nextIndex;
    }

    private void SetAlpha(Image targetImage, float alpha)
    {
        Color c = targetImage.color;
        c.a = alpha;
        targetImage.color = c;
    }

    public void StopAllFire()
    {
        if (routineA != null) StopCoroutine(routineA);
        if (routineB != null) StopCoroutine(routineB);
        if (routineC != null) StopCoroutine(routineC);
    }
}