using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Spikes, crystals and bottomless pits.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Hazard : MonoBehaviour
    {
        [field: SerializeField] public int Damage { get; set; } = 1;

        [field: SerializeField] public bool Lethal { get; set; }

        private void OnTriggerStay2D(Collider2D other)
        {
            PiterController player = other.GetComponentInParent<PiterController>();

            if (player == null || player.IsDead)
            {
                return;
            }

            if (Lethal)
            {
                player.Health.Kill();

                return;
            }

            player.TakeHit(Damage, transform.position);
        }
    }
}
