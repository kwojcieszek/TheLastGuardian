using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// A named sequence of sprite frames.
    /// </summary>
    [Serializable]
    public struct SpriteClip
    {
        [SerializeField] private string name;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond;
        [SerializeField] private bool loop;
        [SerializeField] private bool holdLastFrame;

        public string Name => name;

        public Sprite[] Frames => frames;

        public float FramesPerSecond => framesPerSecond;

        public bool Loop => loop;

        public bool HoldLastFrame => holdLastFrame;

        public bool HasFrames => frames != null && frames.Length > 0;

        public SpriteClip(string name, Sprite[] frames, float framesPerSecond, bool loop, bool holdLastFrame)
        {
            this.name = name;
            this.frames = frames;
            this.framesPerSecond = framesPerSecond;
            this.loop = loop;
            this.holdLastFrame = holdLastFrame;
        }
    }
}
