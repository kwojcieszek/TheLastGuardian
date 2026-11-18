using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Portrait shown next to a dialogue speaker.
    /// </summary>
    [Serializable]
    public struct SpeakerPortrait
    {
        [SerializeField] private string speaker;
        [SerializeField] private Sprite sprite;

        public string Speaker => speaker;

        public Sprite Sprite => sprite;

        public SpeakerPortrait(string speaker, Sprite sprite)
        {
            this.speaker = speaker;
            this.sprite = sprite;
        }
    }
}
