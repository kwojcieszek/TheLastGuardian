using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Wrapper that lets JsonUtility read the top-level array of dialogues.json.
    /// </summary>
    [Serializable]
    public sealed class DialogueCollection
    {
        [SerializeField] private DialogueEntry[] items;

        public DialogueEntry[] Items => items ?? Array.Empty<DialogueEntry>();
    }
}
