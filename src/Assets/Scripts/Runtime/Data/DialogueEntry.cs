using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Dialogue lines assigned to one level.
    /// </summary>
    [Serializable]
    public sealed class DialogueEntry
    {
        [SerializeField] private int level;
        [SerializeField] private DialogueLine[] lines;

        public int Level => level;

        public DialogueLine[] Lines => lines ?? Array.Empty<DialogueLine>();
    }
}
