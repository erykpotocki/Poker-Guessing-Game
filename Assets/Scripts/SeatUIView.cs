using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeatUIView : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text nickText;
    [SerializeField] private RectTransform activeTurnHighlight;

    [Header("Highlight Pulse")]
    [SerializeField] private float pulseSpeed = 1.6f;
    [SerializeField] private float pulseScaleAmount = 0.04f;

    [Header("Eliminated Visual")]
    [SerializeField] private Color eliminatedAvatarTint = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color eliminatedNickTint = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("Disconnected Visual")]
    [SerializeField] private Color disconnectedAvatarTint = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color disconnectedNickTint = new Color(0.4f, 0.4f, 0.4f, 1f);

    private bool isActiveTurn = false;
    private bool isEliminated = false;
    private bool isDisconnected = false;
    private Vector3 highlightBaseScale = Vector3.one;

    private Color defaultAvatarColor = Color.white;
    private Color defaultNickColor = Color.white;

    private void Awake()
    {
        if (avatarImage != null)
            defaultAvatarColor = avatarImage.color;

        if (nickText != null)
            defaultNickColor = nickText.color;

        if (activeTurnHighlight != null)
        {
            highlightBaseScale = activeTurnHighlight.localScale;
            activeTurnHighlight.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isActiveTurn || isEliminated || activeTurnHighlight == null)
            return;

        float pulse = 1f + Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseScaleAmount;
        activeTurnHighlight.localScale = highlightBaseScale * pulse;
    }

    public void Set(string nick, Sprite avatar)
    {
        if (nickText != null)
            nickText.text = nick;

        if (avatarImage != null)
            avatarImage.sprite = avatar;
    }

    public void SetActiveTurnHighlight(bool isActive)
    {
        if (isEliminated)
            isActive = false;

        isActiveTurn = isActive;

        if (activeTurnHighlight == null)
            return;

        activeTurnHighlight.gameObject.SetActive(isActive);
        activeTurnHighlight.localScale = highlightBaseScale;
    }

    public void SetEliminatedVisual(bool value)
    {
        isEliminated = value;

        if (isEliminated)
        {
            isActiveTurn = false;

            if (activeTurnHighlight != null)
            {
                activeTurnHighlight.gameObject.SetActive(false);
                activeTurnHighlight.localScale = highlightBaseScale;
            }
        }

        ApplyCurrentVisualState();
    }

    public void SetDisconnectedVisual(bool value)
    {
        isDisconnected = value;

        if (isDisconnected)
        {
            isActiveTurn = false;

            if (activeTurnHighlight != null)
            {
                activeTurnHighlight.gameObject.SetActive(false);
                activeTurnHighlight.localScale = highlightBaseScale;
            }
        }

        ApplyCurrentVisualState();
    }

    public string GetDisplayedNick()
    {
        return nickText != null ? nickText.text : string.Empty;
    }

    private void ApplyCurrentVisualState()
    {
        if (avatarImage != null)
        {
            if (isEliminated)
                avatarImage.color = eliminatedAvatarTint;
            else if (isDisconnected)
                avatarImage.color = disconnectedAvatarTint;
            else
                avatarImage.color = defaultAvatarColor;
        }

        if (nickText != null)
        {
            if (isEliminated)
                nickText.color = eliminatedNickTint;
            else if (isDisconnected)
                nickText.color = disconnectedNickTint;
            else
                nickText.color = defaultNickColor;
        }
    }
}