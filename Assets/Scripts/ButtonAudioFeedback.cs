using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ButtonAudioFeedback : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, ISubmitHandler
{
    private Button button;
    private bool pressed;
    private static float lastHaptic;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void PokerLightHaptic();
#endif
    public static void Ensure(Button button)
    {
        if (Application.isPlaying && button != null && button.GetComponent<ButtonAudioFeedback>() == null)
            button.gameObject.AddComponent<ButtonAudioFeedback>();
    }
    private void Awake() => button = GetComponent<Button>();
    public void OnPointerDown(PointerEventData data) => pressed = data.button == PointerEventData.InputButton.Left && button != null && button.IsInteractable();
    public void OnPointerClick(PointerEventData data) { if (pressed) Play(); pressed = false; }
    public void OnSubmit(BaseEventData data) { if (button != null && button.IsInteractable()) Play(); }
    private void Play()
    {
        CasinoAudio.PlayClick();
        bool important = name == "CheckButton" || name == "RoundReady" || name.Contains("Start");
        if (!important || !GameAudioSettings.Haptics || Time.unscaledTime - lastHaptic < 0.5f) return;
        lastHaptic = Time.unscaledTime;
#if UNITY_WEBGL && !UNITY_EDITOR
        PokerLightHaptic();
#endif
    }
}
