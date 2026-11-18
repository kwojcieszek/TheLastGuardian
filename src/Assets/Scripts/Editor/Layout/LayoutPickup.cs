using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Collectible spawn point.
    /// </summary>
    public sealed class LayoutPickup
    {
        public PickupKind Kind { get; set; }

        public Vector2 Position { get; set; }
    }
}
