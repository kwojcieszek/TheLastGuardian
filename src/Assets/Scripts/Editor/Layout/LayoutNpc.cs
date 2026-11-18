using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Story character staged in a level.
    /// </summary>
    public sealed class LayoutNpc
    {
        public string Actor { get; set; }

        public float X { get; set; }

        public float HeightAboveGround { get; set; }

        public string Speaker { get; set; }

        public string Line { get; set; }

        public Color Tint { get; set; } = Color.white;

        public bool VanishAfterLine { get; set; }

        public bool Floating { get; set; }

        public float Scale { get; set; } = 1f;
    }
}
