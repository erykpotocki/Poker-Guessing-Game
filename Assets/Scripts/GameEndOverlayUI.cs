using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameEndOverlayUI : MonoBehaviour
{
    [Header("Winner UI")]
    [SerializeField] private Image winnerAvatarImage;
    [SerializeField] private TMP_Text winnerTitleText;
    [SerializeField] private Button continueButton;

    [Header("FX")]
    [SerializeField] private GameObject fireworksLeft;
    [SerializeField] private GameObject fireworksRight;

    [Header("Animation")]
    [SerializeField] private RectTransform winnerAvatarRoot;
    [SerializeField] private float introDuration = 0.55f;
    [SerializeField] private float startYOffset = -80f;
    [SerializeField] private float startScale = 0.82f;

    private const string WinnerNickColorHex = "#FFD133";

    private Vector2 winnerAvatarRootTargetPos;
    private Vector3 winnerAvatarRootTargetScale;
    private Coroutine introRoutine;

    private void Awake()
    {
        CacheTargets();
    }

    public void ShowWinner(Sprite winnerAvatar, string winnerNick)
    {
        gameObject.SetActive(true);
        CacheTargets();

        if (winnerAvatarImage != null)
        {
            winnerAvatarImage.sprite = winnerAvatar;
            winnerAvatarImage.enabled = winnerAvatar != null;
        }

        if (winnerTitleText != null)
        {
            string safeNick = string.IsNullOrWhiteSpace(winnerNick)
                ? "GRACZ"
                : winnerNick.ToUpperInvariant();

            winnerTitleText.text = $"WYGRYWA <color={WinnerNickColorHex}>{safeNick}</color>";
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        SetFireworksActive(false);
        PrepareIntroState();

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
        }

        introRoutine = StartCoroutine(PlayIntroRoutine());
    }

    public void ShowContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    public void SetContinueAction(UnityAction action)
    {
        if (continueButton == null)
            return;

        continueButton.onClick.RemoveAllListeners();

        if (action != null)
        {
            continueButton.onClick.AddListener(action);
        }
    }

    public void ShowFireworks()
    {
        SetFireworksActive(true);
    }

    public void HideFireworks()
    {
        SetFireworksActive(false);
    }

    public void HideInstant()
    {
        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        SetFireworksActive(false);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    private void CacheTargets()
    {
        if (winnerAvatarRoot == null && winnerAvatarImage != null)
        {
            winnerAvatarRoot = winnerAvatarImage.transform.parent as RectTransform;
        }

        if (winnerAvatarRoot != null)
        {
            winnerAvatarRootTargetPos = winnerAvatarRoot.anchoredPosition;
            winnerAvatarRootTargetScale = Vector3.one;
        }
    }

    private void PrepareIntroState()
    {
        if (winnerAvatarRoot == null)
            return;

        winnerAvatarRoot.anchoredPosition = winnerAvatarRootTargetPos + new Vector2(0f, startYOffset);
        winnerAvatarRoot.localScale = Vector3.one * startScale;

        SetGraphicAlpha(winnerAvatarImage, 0f);
        SetGraphicAlpha(winnerTitleText, 0f);
    }

    private IEnumerator PlayIntroRoutine()
    {
        if (winnerAvatarRoot == null)
            yield break;

        float time = 0f;

        while (time < introDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / introDuration);
            float eased = EaseOutBack(t);

            winnerAvatarRoot.anchoredPosition = Vector2.Lerp(
                winnerAvatarRootTargetPos + new Vector2(0f, startYOffset),
                winnerAvatarRootTargetPos,
                eased
            );

            winnerAvatarRoot.localScale = Vector3.Lerp(
                Vector3.one * startScale,
                winnerAvatarRootTargetScale,
                eased
            );

            float alpha = Mathf.SmoothStep(0f, 1f, t);
            SetGraphicAlpha(winnerAvatarImage, alpha);
            SetGraphicAlpha(winnerTitleText, alpha);

            yield return null;
        }

        winnerAvatarRoot.anchoredPosition = winnerAvatarRootTargetPos;
        winnerAvatarRoot.localScale = winnerAvatarRootTargetScale;
        SetGraphicAlpha(winnerAvatarImage, 1f);
        SetGraphicAlpha(winnerTitleText, 1f);

        introRoutine = null;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void SetGraphicAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null)
            return;

        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private void SetFireworksActive(bool isActive)
    {
        if (fireworksLeft != null)
            fireworksLeft.SetActive(isActive);

        if (fireworksRight != null)
            fireworksRight.SetActive(isActive);
    }
}