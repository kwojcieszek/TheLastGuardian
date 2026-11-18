using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Enemy spawn point.
    /// </summary>
    public sealed class LayoutEnemy
    {
        public string Kind { get; set; }

        public Vector2 Position { get; set; }

        public bool IsGuardian { get; set; }
    }
}
