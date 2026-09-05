using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CasinoAudio : MonoBehaviour
{
    // Keep existing indices stable; the user's recordings are the new defaults.
    public static readonly string[] TrackNames = { "Velvet Room", "Midnight Table", "Blue Chips", "Midnight Ashes 2", "Whispers in the Smoke" };
    private const string MenuTrackKey = "casinoMenuMusicV2";
    private const string GameTrackKey = "casinoGameMusicV2";
    private static int localGameplayScene = -1;
    private static CasinoAudio instance;
    private AudioSource music, effects;
    private readonly AudioClip[] clips = new AudioClip[5];
    private AudioClip click;
    private int currentTrack = -1;
    private float lastClick = -1f;
    private string initializedRoom;
    private Coroutine changeRoutine;
    private float fade = 1f;
    public static bool IsGameplay => SceneManager.GetActiveScene().name == "Game" || SceneManager.GetActiveScene().handle == localGameplayScene;
    public static void BeginLocalGame() => localGameplayScene = SceneManager.GetActiveScene().handle;
    public static bool CanChangeTrack => !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
    public static int SelectedTrack => GetSelectedTrack(IsGameplay);
    public static int GetSelectedTrack(bool gameplay)
    {
        string key = gameplay ? GameTrackKey : MenuTrackKey;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object raw) && raw is int index)
            return Mathf.Clamp(index, 0, TrackNames.Length - 1);
        return GetLocalTrack(gameplay);
    }
    private static int GetLocalTrack(bool gameplay) => Mathf.Clamp(PlayerPrefs.GetInt(
        gameplay ? "audio.gameTrackV2" : "audio.menuTrackV2", gameplay ? 4 : 3), 0, TrackNames.Length - 1);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance != null) return;
        GameObject obj = new GameObject("CasinoAudio");
        DontDestroyOnLoad(obj);
        instance = obj.AddComponent<CasinoAudio>();
    }
    private void Awake()
    {
        music = gameObject.AddComponent<AudioSource>();
        effects = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = effects.playOnAwake = false;
        music.loop = true;
        music.spatialBlend = effects.spatialBlend = 0f;
        click = AudioClip.Create("Soft UI click", 1500, 1, 22050, false);
        float[] data = new float[1500];
        for (int i = 0; i < data.Length; i++)
        {
            double t = i / 22050.0;
            data[i] = (float)(Math.Sin(2 * Math.PI * 680 * t) * Math.Exp(-t * 95) * Math.Min(1, t * 1400) * 0.24);
        }
        click.SetData(data, 0);
        GameAudioSettings.Changed += ApplyVolume;
        ApplyVolume();
    }
    private void Update()
    {
        if (!PhotonNetwork.InRoom) initializedRoom = null;
        else if (PhotonNetwork.IsMasterClient && initializedRoom != PhotonNetwork.CurrentRoom.Name)
        {
            initializedRoom = PhotonNetwork.CurrentRoom.Name;
            var properties = new ExitGames.Client.Photon.Hashtable();
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MenuTrackKey)) properties[MenuTrackKey] = GetLocalTrack(false);
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(GameTrackKey)) properties[GameTrackKey] = GetLocalTrack(true);
            if (properties.Count > 0) PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }
        int selected = SelectedTrack;
        if (selected == currentTrack) return;
        currentTrack = selected;
        if (changeRoutine != null) StopCoroutine(changeRoutine);
        changeRoutine = StartCoroutine(ChangeMusic(selected));
    }
    private IEnumerator ChangeMusic(int selected)
    {
        if (clips[selected] == null)
        {
            if (selected >= 3)
            {
                string resource = selected == 3 ? "MidnightAshes2" : "WhispersInTheSmoke";
                ResourceRequest request = Resources.LoadAsync<AudioClip>("Audio/Music/" + resource);
                yield return request;
                clips[selected] = request.asset as AudioClip;
            }
            else
            {
                float[] samples = OriginalCasinoMusic.Generate(selected);
                clips[selected] = AudioClip.Create(TrackNames[selected], samples.Length, 1, OriginalCasinoMusic.SampleRate, false);
                clips[selected].SetData(samples, 0);
            }
        }
        if (clips[selected] == null)
        {
            Debug.LogError("Missing music asset: " + TrackNames[selected]);
            changeRoutine = null;
            yield break;
        }
        // Keep the previous track playing while the next asset loads in the background.
        while (music.isPlaying && fade > 0f)
        {
            fade = Mathf.MoveTowards(fade, 0f, Time.unscaledDeltaTime / 0.35f);
            ApplyVolume();
            yield return null;
        }
        music.Stop();
        music.clip = clips[selected];
        fade = 0f;
        ApplyVolume();
        music.Play();
        while (fade < 1f)
        {
            fade = Mathf.MoveTowards(fade, 1f, Time.unscaledDeltaTime / 0.6f);
            ApplyVolume();
            yield return null;
        }
        changeRoutine = null;
    }
    private void ApplyVolume()
    {
        music.volume = GameAudioSettings.Music * 0.38f * fade;
        effects.volume = GameAudioSettings.Effects * 0.40f;
    }
    public static void ChangeTrack(int direction)
        => ChangeTrack(direction, IsGameplay);
    public static void ChangeTrack(int direction, bool gameplay)
    {
        if (!CanChangeTrack) return;
        int next = (GetSelectedTrack(gameplay) + direction + TrackNames.Length) % TrackNames.Length;
        if (PhotonNetwork.InRoom)
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { gameplay ? GameTrackKey : MenuTrackKey, next } });
        PlayerPrefs.SetInt(gameplay ? "audio.gameTrackV2" : "audio.menuTrackV2", next);
        PlayerPrefs.Save();
    }
    public static void PlayClick()
    {
        Initialize();
        if (Time.unscaledTime - instance.lastClick < 0.08f || GameAudioSettings.Muted) return;
        instance.lastClick = Time.unscaledTime;
        if (!instance.music.isPlaying && instance.music.clip != null) instance.music.Play();
        instance.effects.PlayOneShot(instance.click);
    }
    private void OnDestroy()
    {
        GameAudioSettings.Changed -= ApplyVolume;
        for (int i = 0; i < 3; i++) if (clips[i] != null) Destroy(clips[i]);
        if (click != null) Destroy(click);
        if (instance == this) instance = null;
    }
}

// Original, synthesized lounge sketches: soft electric piano, bass and very light brushes.
// No downloaded recordings or third-party samples.
public static class OriginalCasinoMusic
{
    public const int SampleRate = 22050;
    public static float[] Generate(int variant)
    {
        variant = Math.Max(0, Math.Min(2, variant));
        double beat = 60.0 / (82 + variant * 5);
        float[] samples = new float[(int)(beat * 32 * SampleRate)];
        int[][] chords = {
            new[] { 60, 64, 67, 71, 74 }, new[] { 57, 60, 64, 67, 71 },
            new[] { 62, 65, 69, 72, 76 }, new[] { 55, 59, 62, 65, 69 }
        };
        System.Random random = new System.Random(9827 + variant);
        int transpose = variant == 1 ? -2 : variant == 2 ? 3 : 0;
        for (int bar = 0; bar < 8; bar++)
        {
            int[] chord = chords[(bar + variant) % chords.Length];
            double start = bar * 4 * beat;
            for (int note = 1; note < chord.Length; note++)
            {
                AddNote(samples, start + beat * 0.12 + note * 0.012, beat * 2.4, chord[note] + transpose, 0.054, false);
                AddNote(samples, start + beat * 2.65 + note * 0.008, beat * 1.3, chord[note] + transpose, 0.036, false);
            }
            for (int b = 0; b < 4; b++)
            {
                int bass = chord[0] - 24 + (b == 2 ? 7 : 0) + transpose;
                AddNote(samples, start + b * beat, beat * 0.84, bass, 0.12, true);
                int noiseStart = (int)((start + (b + 0.65) * beat) * SampleRate);
                for (int n = 0; n < 900; n++)
                    samples[(noiseStart + n) % samples.Length] += (float)((random.NextDouble() * 2 - 1) * Math.Exp(-n / 160.0) * 0.010);
            }
            if (bar % 2 == 1)
            {
                AddNote(samples, start + beat * 1.65, beat * 0.8, chord[4] + 12 + transpose, 0.035, false);
                AddNote(samples, start + beat * 3, beat * 0.8, chord[3] + 12 + transpose, 0.028, false);
            }
        }
        for (int i = 0; i < samples.Length; i++) samples[i] = Math.Max(-0.8f, Math.Min(0.8f, samples[i]));
        return samples;
    }
    private static void AddNote(float[] samples, double start, double duration, int midi, double gain, bool bass)
    {
        int first = (int)(start * SampleRate), length = (int)(duration * SampleRate);
        double frequency = 440 * Math.Pow(2, (midi - 69) / 12.0);
        for (int i = 0; i < length; i++)
        {
            double t = (double)i / SampleRate;
            double phase = 2 * Math.PI * frequency * t;
            double envelope = Math.Min(1, t / 0.009) * Math.Exp(-t * (bass ? 3.4 : 2.3)) * Math.Min(1, (duration-t) / 0.07);
            double tone = Math.Sin(phase) + Math.Sin(phase * 2) * (bass ? 0.18 : 0.24) * Math.Exp(-t * 4) + Math.Sin(phase * 3) * 0.06;
            samples[(first+i) % samples.Length] += (float)(tone * envelope * gain);
        }
    }
}
