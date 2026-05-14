using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WindowCityAnimator : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image imageA;
    [SerializeField] private Image imageB;

    [Header("Frames")]
    [SerializeField] private Sprite[] frames;

    [Header("Timing")]
    [SerializeField] private float visibleTime = 2f;
    [SerializeField] private float fadeTime = 0.6f;
    [SerializeField] private bool playOnStart = true;

    private readonly int[] sequence = { 0, 1, 2, 3, 4, 3, 2, 1, 0 };

    private int sequenceIndex = 0;
    private Coroutine playRoutine;

    private void Start()
    {
        if (imageA == null || imageB == null)
            return;

        if (frames == null || frames.Length < 5)
            return;

        imageA.sprite = frames[sequence[0]];
        imageA.color = new Color(1f, 1f, 1f, 1f);

        imageB.sprite = frames[sequence[0]];
        imageB.color = new Color(1f, 1f, 1f, 0f);

        if (playOnStart)
            playRoutine = StartCoroutine(PlayLoop());
    }

    private IEnumerator PlayLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(visibleTime);

            int nextSequenceIndex = sequenceIndex + 1;
            if (nextSequenceIndex >= sequence.Length)
                nextSequenceIndex = 0;

            int nextFrameIndex = sequence[nextSequenceIndex];
            Sprite nextSprite = frames[nextFrameIndex];

            imageB.sprite = nextSprite;
            imageB.color = new Color(1f, 1f, 1f, 0f);

            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float lerp = Mathf.Clamp01(t / fadeTime);

                imageB.color = new Color(1f, 1f, 1f, lerp);
                yield return null;
            }

            imageB.color = new Color(1f, 1f, 1f, 1f);

            imageA.sprite = nextSprite;
            imageA.color = new Color(1f, 1f, 1f, 1f);

            imageB.color = new Color(1f, 1f, 1f, 0f);

            sequenceIndex = nextSequenceIndex;
        }
    }

    public void Play()
    {
        if (playRoutine == null)
            playRoutine = StartCoroutine(PlayLoop());
    }

    public void Stop()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
    }
}