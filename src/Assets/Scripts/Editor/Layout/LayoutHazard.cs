using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Damaging obstacle such as spikes or a moving crystal.
    /// </summary>
    public sealed class LayoutHazard
    {
        public string Kind { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 Size { get; set; }

        public Vector2 Destination { get; set; }
    }
}
