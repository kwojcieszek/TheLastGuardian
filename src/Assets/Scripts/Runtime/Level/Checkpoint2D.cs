using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Flag that stores the respawn point and saves the game when Piter touches it.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Checkpoint2D : MonoBehaviour
    {
        private static readonly Color InactiveColor = new Color(0.55f, 0.55f, 0.6f, 1f);

        [field: SerializeField] public Vector2 SpawnPoint { get; set; }

        [field: SerializeField] public SpriteRenderer FlagRenderer { get; set; }

        public bool IsActivated { get; private set; }

        public void MarkActivated()
        {
            IsActivated = true;

            if (FlagRenderer != null)
            {
                FlagRenderer.color = Color.white;
            }
        }

        private void Start()
        {
            if (FlagRenderer != null && !IsActivated)
            {
                FlagRenderer.color = InactiveColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsActivated || LevelSession.Current == null)
            {
                return;
            }

            if (other.GetComponentInParent<PiterController>() != null)
            {
                LevelSession.Current.ActivateCheckpoint(this);
            }
        }
    }
}
