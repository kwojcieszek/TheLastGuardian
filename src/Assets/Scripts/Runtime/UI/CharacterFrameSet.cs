using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Idle animation frames of a character, used by animated UI portraits.
    /// </summary>
    [Serializable]
    public struct CharacterFrameSet
    {
        [SerializeField] private string character;
        [SerializeField] private Sprite[] frames;

        public string Character => character;

        public Sprite[] Frames => frames ?? Array.Empty<Sprite>();

        public CharacterFrameSet(string character, Sprite[] frames)
        {
            this.character = character;
            this.frames = frames;
        }
    }
}
