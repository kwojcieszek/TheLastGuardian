using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Scenery prop that may appear in a biome, with its base size and tint.
    /// </summary>
    public sealed class DecorationStyle
    {
        public string Sprite { get; }

        public Vector2 Size { get; }

        public Color Tint { get; }

        public bool Stackable { get; }

        public DecorationStyle(string sprite, Vector2 size, Color tint, bool stackable)
        {
            Sprite = sprite;
            Size = size;
            Tint = tint;
            Stackable = stackable;
        }
    }
}
