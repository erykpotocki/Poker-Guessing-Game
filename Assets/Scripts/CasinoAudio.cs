using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CasinoAudio : MonoBehaviour
{
    public static readonly string[] TrackNames = { "Midnight Ashes 2", "Whispers in the Smoke" };
    private static readonly string[] TrackPaths = { "MidnightAshes2", "WhispersInTheSmoke" };
    private static CasinoAudio instance;
    private static int localGameplayScene = -1;
    private AudioSource music, effects;
    private readonly AudioClip[] clips = new AudioClip[2];
    private AudioClip click, turnSignal;
    private int currentTrack = -1;
    private float fade = 1f, lastClick = -1f;
    private Coroutine changeRoutine;
    public static bool IsGameplay => SceneManager.GetActiveScene().name == "Game" || SceneManager.GetActiveScene().handle == localGameplayScene;
    public static int SelectedTrack => IsGameplay ? 1 : 0;
    public static void BeginLocalGame() => localGameplayScene = SceneManager.GetActiveScene().handle;

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
            ResourceRequest request = Resources.LoadAsync<AudioClip>("Audio/Music/" + TrackPaths[selected]);
            yield return request;
            clips[selected] = request.asset as AudioClip;
        }
        if (clips[selected] == null)
        {
            Debug.LogError("Missing music asset: " + TrackNames[selected]);
            changeRoutine = null;
            yield break;
        }
        while (music.isPlaying && fade > 0f)
        {
            fade = Mathf.MoveTowards(fade, 0f, Time.unscaledDeltaTime / .35f);
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
            fade = Mathf.MoveTowards(fade, 1f, Time.unscaledDeltaTime / .6f);
            ApplyVolume();
            yield return null;
        }
        changeRoutine = null;
    }
    private void ApplyVolume()
    {
        music.volume = GameAudioSettings.MusicGain * .38f * fade;
        effects.volume = GameAudioSettings.EffectsGain * .40f;
    }
    public static void PlayClick()
    {
        Initialize();
        if (Time.unscaledTime - instance.lastClick < .08f || GameAudioSettings.Muted) return;
        instance.lastClick = Time.unscaledTime;
        if (!instance.music.isPlaying && instance.music.clip != null) instance.music.Play();
        instance.effects.PlayOneShot(instance.click);
    }
    public static void PlayLocalTurn()
    {
        Initialize();
        if (GameAudioSettings.Muted || GameAudioSettings.EffectsGain <= 0f) return;
        if (instance.turnSignal == null) instance.turnSignal = Resources.Load<AudioClip>("Audio/SFX/LocalTurn");
        if (instance.turnSignal != null) instance.effects.PlayOneShot(instance.turnSignal);
    }
    private void OnDestroy()
    {
        GameAudioSettings.Changed -= ApplyVolume;
        if (click != null) Destroy(click);
        if (instance == this) instance = null;
    }
}
