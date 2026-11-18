using UnityEngine;

namespace PiterGame.EditorTools
{
    /// <summary>
    /// Art and gameplay parameters of a character prefab.
    /// </summary>
    public sealed class ActorProfile
    {
        public const float FeetPixelsBelowCenter = 189f;

        public string Name { get; set; }

        public float PixelsPerUnit { get; set; } = 240f;

        public Vector2 ColliderSize { get; set; } = new Vector2(0.7f, 1.5f);

        public bool HorizontalCapsule { get; set; }

        public int MaxHealth { get; set; } = 2;

        public EnemyMovement Movement { get; set; } = EnemyMovement.Walker;

        public float MoveSpeed { get; set; } = 1.8f;

        public float ChargeSpeed { get; set; } = 4.5f;

        public float DetectionDistance { get; set; } = 5f;

        public float PatrolDistance { get; set; } = 3f;

        public bool Stompable { get; set; } = true;

        public bool IsFlying => Movement == EnemyMovement.Flyer || Movement == EnemyMovement.Diver;

        public float FeetOffset => FeetPixelsBelowCenter / PixelsPerUnit;

        public float ColliderOffsetY => -FeetOffset + ColliderSize.y * 0.5f;

        public float GroundedRootY(float groundTop)
        {
            return groundTop + FeetOffset + 0.05f;
        }
    }
}
