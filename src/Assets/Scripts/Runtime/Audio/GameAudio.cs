using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Persistent player of synthesized sound effects. No external audio files are required.
    /// </summary>
    public sealed class GameAudio : MonoBehaviour
    {
        private const int SampleRate = 22050;
        private const float TwoPi = Mathf.PI * 2f;

        private static GameAudio instance;

        private readonly Dictionary<SoundEffect, AudioClip> clips = new Dictionary<SoundEffect, AudioClip>();
        private readonly System.Random noise = new System.Random(1337);
        private AudioSource source;

        public static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            GameObject host = new GameObject("GameAudio");
            DontDestroyOnLoad(host);

            instance = host.AddComponent<GameAudio>();
        }

        public static void Play(SoundEffect effect, float volume = 1f)
        {
            if (instance == null)
            {
                return;
            }

            if (instance.clips.TryGetValue(effect, out AudioClip clip))
            {
                instance.source.PlayOneShot(clip, volume);
            }
        }

        private static float Wave(Waveform waveform, float phase)
        {
            float cycle = phase - Mathf.Floor(phase);

            switch (waveform)
            {
                case Waveform.Square:
                    return cycle < 0.5f ? 1f : -1f;
                case Waveform.Triangle:
                    return 1f - 4f * Mathf.Abs(cycle - 0.5f);
                default:
                    return Mathf.Sin(cycle * TwoPi);
            }
        }

        private static float Envelope(float progress)
        {
            float attack = Mathf.Clamp01(progress / 0.04f);
            float release = 1f - progress;

            return attack * release * release;
        }

        private static AudioClip CreateClip(string clipName, float[] samples)
        {
            AudioClip clip = AudioClip.Create(clipName, Math.Max(1, samples.Length), 1, SampleRate, false);
            clip.SetData(samples, 0);

            return clip;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);

                return;
            }

            instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;

            BuildClips();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void BuildClips()
        {
            clips[SoundEffect.Click] = Tone("Click", 0.04f, 1100f, 1100f, Waveform.Sine, 0.35f);
            clips[SoundEffect.Jump] = Tone("Jump", 0.14f, 320f, 640f, Waveform.Square, 0.18f);
            clips[SoundEffect.DoubleJump] = Tone("DoubleJump", 0.14f, 480f, 900f, Waveform.Square, 0.16f);
            clips[SoundEffect.Attack] = Noise("Attack", 0.12f, 0.35f, 0.25f);
            clips[SoundEffect.Coin] = Sequence("Coin", new[] { 988f, 1319f }, new[] { 0.06f, 0.14f }, Waveform.Sine, 0.3f);
            clips[SoundEffect.Hurt] = Tone("Hurt", 0.22f, 260f, 110f, Waveform.Square, 0.25f);
            clips[SoundEffect.Death] = Tone("Death", 0.7f, 420f, 70f, Waveform.Square, 0.25f);
            clips[SoundEffect.EnemyHit] = Tone("EnemyHit", 0.1f, 200f, 120f, Waveform.Square, 0.22f);
            clips[SoundEffect.EnemyDefeat] = Tone("EnemyDefeat", 0.32f, 520f, 90f, Waveform.Triangle, 0.35f);
            clips[SoundEffect.Stomp] = Tone("Stomp", 0.12f, 170f, 60f, Waveform.Sine, 0.6f);
            clips[SoundEffect.Checkpoint] = Sequence("Checkpoint", new[] { 523f, 659f, 784f, 1047f }, new[] { 0.08f, 0.08f, 0.08f, 0.2f }, Waveform.Triangle, 0.35f);
            clips[SoundEffect.Potion] = Tone("Potion", 0.35f, 400f, 950f, Waveform.Sine, 0.35f);
            clips[SoundEffect.Chest] = Sequence("Chest", new[] { 392f, 523f, 659f, 784f, 1047f }, new[] { 0.07f, 0.07f, 0.07f, 0.07f, 0.25f }, Waveform.Triangle, 0.35f);
            clips[SoundEffect.Objective] = Sequence("Objective", new[] { 659f, 880f, 1175f }, new[] { 0.09f, 0.09f, 0.22f }, Waveform.Sine, 0.4f);
            clips[SoundEffect.Blocked] = Tone("Blocked", 0.16f, 180f, 160f, Waveform.Square, 0.18f);
            clips[SoundEffect.Fireball] = Noise("Fireball", 0.3f, 0.12f, 0.3f);
            clips[SoundEffect.Reflect] = Tone("Reflect", 0.14f, 1300f, 650f, Waveform.Square, 0.16f);
            clips[SoundEffect.StageComplete] = Sequence("StageComplete", new[] { 523f, 659f, 784f, 659f, 784f, 1047f }, new[] { 0.12f, 0.12f, 0.12f, 0.12f, 0.12f, 0.5f }, Waveform.Triangle, 0.4f);
        }

        private AudioClip Tone(string clipName, float duration, float startFrequency, float endFrequency, Waveform waveform, float volume)
        {
            int length = Mathf.CeilToInt(duration * SampleRate);
            float[] samples = new float[length];
            float phase = 0f;

            for (int index = 0; index < length; index++)
            {
                float progress = (float)index / length;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                phase += frequency / SampleRate;

                samples[index] = Wave(waveform, phase) * Envelope(progress) * volume;
            }

            return CreateClip(clipName, samples);
        }

        private AudioClip Sequence(string clipName, float[] frequencies, float[] durations, Waveform waveform, float volume)
        {
            List<float> samples = new List<float>();

            for (int note = 0; note < frequencies.Length; note++)
            {
                int length = Mathf.CeilToInt(durations[note] * SampleRate);
                float phase = 0f;

                for (int index = 0; index < length; index++)
                {
                    float progress = (float)index / length;
                    phase += frequencies[note] / SampleRate;

                    samples.Add(Wave(waveform, phase) * Envelope(progress) * volume);
                }
            }

            return CreateClip(clipName, samples.ToArray());
        }

        private AudioClip Noise(string clipName, float duration, float smoothing, float volume)
        {
            int length = Mathf.CeilToInt(duration * SampleRate);
            float[] samples = new float[length];
            float filtered = 0f;

            for (int index = 0; index < length; index++)
            {
                float progress = (float)index / length;
                float white = (float)(noise.NextDouble() * 2.0 - 1.0);
                filtered = Mathf.Lerp(filtered, white, smoothing);

                samples[index] = filtered * Envelope(progress) * volume * 2f;
            }

            return CreateClip(clipName, samples);
        }
    }
}
