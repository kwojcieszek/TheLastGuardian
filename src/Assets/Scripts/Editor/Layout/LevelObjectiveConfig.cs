using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Story objective items required to open the exit of a level.
    /// </summary>
    public sealed class LevelObjectiveConfig
    {
        public string Label { get; set; }

        public int Count { get; set; }

        public string Sprite { get; set; }

        public Color Tint { get; set; } = Color.white;

        public Vector2 Size { get; set; } = new Vector2(0.8f, 1f);

        public bool RequiresAttack { get; set; }

        public bool HideWhenCollected { get; set; } = true;

        public bool Floating { get; set; } = true;
    }
}
