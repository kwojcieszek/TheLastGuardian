using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Physics layer names, masks and the collision matrix used by the game.
    /// </summary>
    public static class GameLayers
    {
        public const string GroundName = "Ground";
        public const string PlayerName = "Player";
        public const string EnemyName = "Enemy";
        public const string InteractableName = "Interactable";
        public const string ProjectileName = "Projectile";

        public static int Ground => LayerMask.NameToLayer(GroundName);

        public static int Player => LayerMask.NameToLayer(PlayerName);

        public static int Enemy => LayerMask.NameToLayer(EnemyName);

        public static int Interactable => LayerMask.NameToLayer(InteractableName);

        public static int Projectile => LayerMask.NameToLayer(ProjectileName);

        public static int GroundMask => 1 << Ground;

        public static int AttackMask => (1 << Enemy) | (1 << Interactable) | (1 << Projectile);

        public static void ConfigureCollisions()
        {
            int[] layers = { Ground, Player, Enemy, Interactable, Projectile };

            foreach (int layer in layers)
            {
                if (layer < 0)
                {
                    Debug.LogError("Missing physics layers. Expected: Ground, Player, Enemy, Interactable, Projectile.");

                    return;
                }
            }

            SetCollision(Player, Enemy, false);
            SetCollision(Enemy, Enemy, false);
            SetCollision(Enemy, Interactable, false);
            SetCollision(Interactable, Interactable, false);
            SetCollision(Interactable, Ground, false);
            SetCollision(Projectile, Projectile, false);
            SetCollision(Projectile, Interactable, false);
        }

        private static void SetCollision(int first, int second, bool enabled)
        {
            Physics2D.IgnoreLayerCollision(first, second, !enabled);
        }
    }
}
