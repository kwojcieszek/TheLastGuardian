using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Non-colliding scenery prop standing on the ground (crate, torch, barrel, crystal).
    /// </summary>
    public sealed class LayoutDecoration
    {
        public string Sprite { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 Size { get; set; }

        public Color Tint { get; set; } = Color.white;

        public bool Mirrored { get; set; }
    }
}
