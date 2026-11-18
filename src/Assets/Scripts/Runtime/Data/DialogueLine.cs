using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// A single spoken line.
    /// </summary>
    [Serializable]
    public sealed class DialogueLine
    {
        [SerializeField] private string speaker;
        [SerializeField] private string text;

        public string Speaker => speaker;

        public string Text => text;

        public DialogueLine()
        {
        }

        public DialogueLine(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }
}
