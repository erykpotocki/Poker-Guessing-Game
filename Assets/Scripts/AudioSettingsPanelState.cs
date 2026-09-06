using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AudioSettingsPanelState : MonoBehaviour
{
    private RectTransform box;
    private TMP_Text track, musicMute, effectsMute, haptics, mute;
    public void Initialize(RectTransform panel, TMP_Text trackLabel, TMP_Text musicMuteLabel,
        TMP_Text effectsMuteLabel, TMP_Text hapticsLabel, TMP_Text muteLabel)
    {
        box = panel; track = trackLabel; musicMute = musicMuteLabel; effectsMute = effectsMuteLabel;
        haptics = hapticsLabel; mute = muteLabel;
        Update();
    }
    private void Update()
    {
        if (box == null) return;
        track.text = CasinoAudio.TrackNames[CasinoAudio.SelectedTrack];
        musicMute.text = GameAudioSettings.MusicMuted ? "WŁĄCZ MUZYKĘ" : "WYCISZ MUZYKĘ";
        effectsMute.text = GameAudioSettings.EffectsMuted ? "WŁĄCZ SFX" : "WYCISZ SFX";
        haptics.text = GameAudioSettings.Haptics ? "WIBRACJE: WŁĄCZONE" : "WIBRACJE: WYŁĄCZONE";
        mute.text = GameAudioSettings.Muted ? "DŹWIĘK: WYCISZONY" : "DŹWIĘK: WŁĄCZONY";
        RectTransform root = GetComponentInParent<Canvas>().rootCanvas.transform as RectTransform;
        if (Screen.width <= 0 || Screen.height <= 0) return;
        Rect safe = Screen.safeArea;
        float scale = Mathf.Min(1.65f, (safe.width * root.rect.width / Screen.width - 48f) / 620f,
            (safe.height * root.rect.height / Screen.height - 48f) / 720f);
        box.localScale = Vector3.one * Mathf.Max(0.1f, scale);
        box.anchoredPosition = new Vector2((safe.center.x / Screen.width - 0.5f) * root.rect.width,
            (safe.center.y / Screen.height - 0.5f) * root.rect.height);
    }
}
