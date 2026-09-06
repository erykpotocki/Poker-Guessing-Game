using System;
using UnityEngine;

public static class GameAudioSettings
{
    public static event Action Changed;
    public static bool Muted => PlayerPrefs.GetInt("audio.masterMuted", 0) == 1;
    public static float Music => PlayerPrefs.GetFloat("audio.musicVolume", 1f);
    public static float Effects => PlayerPrefs.GetFloat("audio.effectsVolume", 1f);
    public static bool MusicMuted => PlayerPrefs.GetInt("audio.musicMuted", 0) == 1;
    public static bool EffectsMuted => PlayerPrefs.GetInt("audio.effectsMuted", 0) == 1;
    public static float MusicGain => MusicMuted ? 0f : Music;
    public static float EffectsGain => EffectsMuted ? 0f : Effects;
    public static void ToggleMusicMute() { PlayerPrefs.SetInt("audio.musicMuted", MusicMuted ? 0 : 1); PlayerPrefs.Save(); Changed?.Invoke(); }
    public static void ToggleEffectsMute() { PlayerPrefs.SetInt("audio.effectsMuted", EffectsMuted ? 0 : 1); PlayerPrefs.Save(); Changed?.Invoke(); }
    public static bool Haptics => PlayerPrefs.GetInt("audio.haptics", 1) == 1;
    public static void ToggleHaptics()
    {
        PlayerPrefs.SetInt("audio.haptics", Haptics ? 0 : 1);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize() => AudioListener.volume = Muted ? 0f : 1f;
    public static void ToggleMute()
    {
        PlayerPrefs.SetInt("audio.masterMuted", Muted ? 0 : 1);
        Initialize();
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
    public static void SetMusic(float value) { PlayerPrefs.SetFloat("audio.musicVolume", Mathf.Clamp01(value)); Changed?.Invoke(); }
    public static void SetEffects(float value) { PlayerPrefs.SetFloat("audio.effectsVolume", Mathf.Clamp01(value)); Changed?.Invoke(); }
}
