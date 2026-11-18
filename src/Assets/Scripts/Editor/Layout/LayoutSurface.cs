using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Solid terrain piece: ground, step, island, barrel or moving platform.
    /// </summary>
    public sealed class LayoutSurface
    {
        public string Kind { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 Size { get; set; }

        public Vector2 Destination { get; set; }

        public float Left => Position.x - Size.x * 0.5f;

        public float Right => Position.x + Size.x * 0.5f;

        public float Top => Position.y + Size.y * 0.5f;
    }
}
