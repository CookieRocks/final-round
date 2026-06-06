using System.Collections.Generic;
using UnityEngine;

public enum FinalRoundSoundEvent
{
    UiClick,
    AnswerSelected,
    PrepCardUsed,
    RecoveryChoiceSelected,
    Continue,
    RandomEventAppears,
    StageComplete,
    PressureWarning,
    FinalPositiveOutcome,
    FinalNegativeOutcome,
    BadgeReveal
}

public sealed class FinalRoundAudioManager : MonoBehaviour
{
    private const int SampleRate = 44100;

    private readonly Dictionary<FinalRoundSoundEvent, AudioClip> proceduralClips = new Dictionary<FinalRoundSoundEvent, AudioClip>();
    private readonly Dictionary<FinalRoundSoundEvent, AudioClip> overrideClips = new Dictionary<FinalRoundSoundEvent, AudioClip>();
    private AudioSource audioSource;
    private float masterVolume = 0.65f;
    private float sfxVolume = 0.8f;
    private bool muteAudio;

    public void Configure(AudioSource source, float master, float sfx, bool muted)
    {
        audioSource = source;
        masterVolume = Mathf.Clamp01(master);
        sfxVolume = Mathf.Clamp01(sfx);
        muteAudio = muted;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = masterVolume * sfxVolume;
        }

        EnsureProceduralClips();
    }

    public void SetClipOverride(FinalRoundSoundEvent soundEvent, AudioClip clip)
    {
        if (clip == null)
        {
            overrideClips.Remove(soundEvent);
            return;
        }

        overrideClips[soundEvent] = clip;
    }

    public void Play(FinalRoundSoundEvent soundEvent)
    {
        Play(null, soundEvent);
    }

    public void Play(AudioClip explicitClip, FinalRoundSoundEvent fallbackEvent)
    {
        if (muteAudio || audioSource == null)
        {
            return;
        }

        AudioClip clip = explicitClip;
        if (clip == null && overrideClips.TryGetValue(fallbackEvent, out AudioClip overrideClip))
        {
            clip = overrideClip;
        }

        if (clip == null)
        {
            proceduralClips.TryGetValue(fallbackEvent, out clip);
        }

        if (clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, masterVolume * sfxVolume);
    }

    private void EnsureProceduralClips()
    {
        if (proceduralClips.Count > 0)
        {
            return;
        }

        proceduralClips[FinalRoundSoundEvent.UiClick] = CreateTone("FR UI Click", 0.035f, 720f, 900f, 0.14f, WaveShape.Sine);
        proceduralClips[FinalRoundSoundEvent.Continue] = CreateTone("FR Continue", 0.055f, 520f, 700f, 0.12f, WaveShape.Sine);
        proceduralClips[FinalRoundSoundEvent.AnswerSelected] = CreateTone("FR Answer Selected", 0.09f, 440f, 660f, 0.16f, WaveShape.Sine);
        proceduralClips[FinalRoundSoundEvent.PrepCardUsed] = CreateTone("FR Prep Card", 0.12f, 620f, 920f, 0.13f, WaveShape.Triangle);
        proceduralClips[FinalRoundSoundEvent.RecoveryChoiceSelected] = CreateTone("FR Recovery Choice", 0.11f, 360f, 520f, 0.13f, WaveShape.Sine);
        proceduralClips[FinalRoundSoundEvent.RandomEventAppears] = CreateTone("FR Random Event", 0.18f, 240f, 180f, 0.15f, WaveShape.Triangle);
        proceduralClips[FinalRoundSoundEvent.StageComplete] = CreateArpeggio("FR Stage Complete", new[] { 440f, 660f, 880f }, 0.22f, 0.13f);
        proceduralClips[FinalRoundSoundEvent.PressureWarning] = CreatePulse("FR Pressure Warning", 0.26f, 620f, 0.12f);
        proceduralClips[FinalRoundSoundEvent.FinalPositiveOutcome] = CreateArpeggio("FR Final Positive", new[] { 392f, 523.25f, 659.25f, 783.99f }, 0.42f, 0.15f);
        proceduralClips[FinalRoundSoundEvent.FinalNegativeOutcome] = CreateArpeggio("FR Final Negative", new[] { 330f, 246.94f, 196f }, 0.34f, 0.14f);
        proceduralClips[FinalRoundSoundEvent.BadgeReveal] = CreateTone("FR Badge Reveal", 0.14f, 880f, 1174.66f, 0.11f, WaveShape.Sine);
    }

    private static AudioClip CreateTone(string name, float duration, float startFrequency, float endFrequency, float amplitude, WaveShape shape)
    {
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(SampleRate * duration));
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
            phase += frequency / SampleRate;
            samples[i] = EvaluateWave(phase, shape) * amplitude * Envelope(t);
        }

        return BuildClip(name, samples);
    }

    private static AudioClip CreateArpeggio(string name, float[] frequencies, float duration, float amplitude)
    {
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(SampleRate * duration));
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t * frequencies.Length), 0, frequencies.Length - 1);
            phase += frequencies[noteIndex] / SampleRate;
            samples[i] = Mathf.Sin(phase * Mathf.PI * 2f) * amplitude * Envelope(t);
        }

        return BuildClip(name, samples);
    }

    private static AudioClip CreatePulse(string name, float duration, float frequency, float amplitude)
    {
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(SampleRate * duration));
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float tremolo = 0.55f + 0.45f * Mathf.Sin(t * Mathf.PI * 8f);
            phase += frequency / SampleRate;
            samples[i] = Mathf.Sin(phase * Mathf.PI * 2f) * amplitude * tremolo * Envelope(t);
        }

        return BuildClip(name, samples);
    }

    private static AudioClip BuildClip(string name, float[] samples)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float Envelope(float t)
    {
        float attack = Mathf.Clamp01(t / 0.08f);
        float release = Mathf.Clamp01((1f - t) / 0.18f);
        return Mathf.Min(attack, release);
    }

    private static float EvaluateWave(float phase, WaveShape shape)
    {
        float wrapped = phase - Mathf.Floor(phase);
        if (shape == WaveShape.Triangle)
        {
            return (Mathf.Abs(wrapped - 0.5f) * 4f) - 1f;
        }

        return Mathf.Sin(wrapped * Mathf.PI * 2f);
    }

    private enum WaveShape
    {
        Sine,
        Triangle
    }
}
