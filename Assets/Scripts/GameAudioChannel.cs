using UnityEngine;

// Attach to each future music/effect source; the authored volume remains its relative gain.
[RequireComponent(typeof(AudioSource))]
public sealed class GameAudioChannel : MonoBehaviour
{
    public enum Channel { Music, Effects }
    [SerializeField] private Channel channel = Channel.Effects;
    private AudioSource source;
    private float gain;
    private void Awake() { source = GetComponent<AudioSource>(); gain = source.volume; }
    private void OnEnable() { GameAudioSettings.Changed += Apply; Apply(); }
    private void OnDisable() { GameAudioSettings.Changed -= Apply; }
    private void Apply() { if (source != null) source.volume = gain * (channel == Channel.Music ? GameAudioSettings.Music : GameAudioSettings.Effects); }
}
