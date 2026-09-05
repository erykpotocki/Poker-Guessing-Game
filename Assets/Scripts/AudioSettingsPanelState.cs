using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AudioSettingsPanelState : MonoBehaviour
{
    private RectTransform box;
    private TMP_Text track, permission, haptics, context, mute;
    private Button previous, next;
    private bool gameplay;
    public void ToggleContext() { gameplay = !gameplay; Update(); }
    public void ChangeTrack(int direction) => CasinoAudio.ChangeTrack(direction, gameplay);
    public void Initialize(RectTransform panel, TMP_Text trackLabel, TMP_Text permissionLabel,
        Button previousButton, Button nextButton, TMP_Text hapticsLabel, TMP_Text contextLabel, TMP_Text muteLabel)
    {
        box = panel; track = trackLabel; permission = permissionLabel;
        previous = previousButton; next = nextButton; haptics = hapticsLabel;
        context = contextLabel; mute = muteLabel; gameplay = CasinoAudio.IsGameplay;
        Update();
    }
    private void Update()
    {
        if (box == null) return;
        track.text = CasinoAudio.TrackNames[CasinoAudio.GetSelectedTrack(gameplay)];
        context.text = gameplay ? "MUZYKA: ROZGRYWKA  ›" : "MUZYKA: MENU I POCZEKALNIA  ›";
        bool canChange = CasinoAudio.CanChangeTrack;
        permission.text = canChange ? "Osobny wybór dla menu i rozgrywki" : "Utwór wybiera host";
        previous.interactable = next.interactable = canChange;
        haptics.text = GameAudioSettings.Haptics ? "WIBRACJE: WŁĄCZONE" : "WIBRACJE: WYŁĄCZONE";
        mute.text = GameAudioSettings.Muted ? "DŹWIĘK: WYCISZONY" : "DŹWIĘK: WŁĄCZONY";
        RectTransform root = GetComponentInParent<Canvas>().rootCanvas.transform as RectTransform;
        if (Screen.width <= 0 || Screen.height <= 0) return;
        Rect safe = Screen.safeArea;
        float scale = Mathf.Min(1f, (safe.width * root.rect.width / Screen.width - 48f) / 620f,
            (safe.height * root.rect.height / Screen.height - 48f) / 780f);
        box.localScale = Vector3.one * Mathf.Max(0.1f, scale);
        box.anchoredPosition = new Vector2((safe.center.x / Screen.width - 0.5f) * root.rect.width,
            (safe.center.y / Screen.height - 0.5f) * root.rect.height);
    }
}
