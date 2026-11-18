using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiterGame
{
    /// <summary>
    /// Cycles sprite frames on a UI image using unscaled time, so it also animates in paused menus.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class UiSpriteAnimator : MonoBehaviour
    {
        private Image image;
        private Sprite[] frames = Array.Empty<Sprite>();
        private float framesPerSecond = 8f;
        private float elapsed;

        public void Play(Sprite[] value, float fps, float startOffset)
        {
            frames = value ?? Array.Empty<Sprite>();
            framesPerSecond = Mathf.Max(1f, fps);
            elapsed = startOffset;

            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (frames.Length > 0)
            {
                image.sprite = frames[0];
                image.color = Color.white;
            }
        }

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Update()
        {
            if (frames.Length == 0)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;

            int index = Mathf.FloorToInt(elapsed * framesPerSecond) % frames.Length;
            image.sprite = frames[index];
        }
    }
}
