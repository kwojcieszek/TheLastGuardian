using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Plays frame-by-frame sprite clips such as Idle, Run, Attack, Jump, Hit and Death.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFrameAnimator : MonoBehaviour
    {
        public const string Idle = "Idle";
        public const string Run = "Run";
        public const string Attack = "Attack";
        public const string Jump = "Jump";
        public const string Hit = "Hit";
        public const string Death = "Death";

        [SerializeField] private SpriteClip[] clips = Array.Empty<SpriteClip>();

        private SpriteRenderer display;
        private int currentIndex = -1;
        private float elapsed;
        private bool oneShotPlaying;
        private bool terminal;

        public string CurrentClip => currentIndex >= 0 && currentIndex < clips.Length ? clips[currentIndex].Name : null;

        public bool IsPlayingOneShot => oneShotPlaying;

        public int ClipCount => clips != null ? clips.Length : 0;

        public void SetClips(SpriteClip[] value)
        {
            clips = value ?? Array.Empty<SpriteClip>();
            currentIndex = -1;
        }

        public void PlayLoop(string clipName)
        {
            if (oneShotPlaying || terminal || CurrentClip == clipName)
            {
                return;
            }

            Play(clipName, false);
        }

        public void PlayOnce(string clipName)
        {
            if (terminal)
            {
                return;
            }

            Play(clipName, true);
        }

        public void ResetState()
        {
            terminal = false;
            oneShotPlaying = false;
            currentIndex = -1;

            PlayLoop(Idle);
        }

        private void Awake()
        {
            display = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (currentIndex < 0)
            {
                PlayLoop(Idle);
            }
        }

        private void Update()
        {
            if (currentIndex < 0 || clips == null || currentIndex >= clips.Length)
            {
                return;
            }

            SpriteClip clip = clips[currentIndex];

            if (!clip.HasFrames)
            {
                return;
            }

            elapsed += Time.deltaTime;

            int frame = Mathf.FloorToInt(elapsed * Mathf.Max(clip.FramesPerSecond, 1f));

            if (frame >= clip.Frames.Length)
            {
                if (clip.Loop && !oneShotPlaying && !terminal)
                {
                    frame %= clip.Frames.Length;
                }
                else
                {
                    frame = clip.Frames.Length - 1;
                    oneShotPlaying = false;
                }
            }

            display.sprite = clip.Frames[frame];
        }

        private void Play(string clipName, bool oneShot)
        {
            if (clips == null)
            {
                return;
            }

            for (int index = 0; index < clips.Length; index++)
            {
                if (clips[index].Name != clipName || !clips[index].HasFrames)
                {
                    continue;
                }

                currentIndex = index;
                elapsed = 0f;
                oneShotPlaying = oneShot;
                terminal = clips[index].HoldLastFrame;

                if (display == null)
                {
                    display = GetComponent<SpriteRenderer>();
                }

                display.sprite = clips[index].Frames[0];

                return;
            }
        }
    }
}
