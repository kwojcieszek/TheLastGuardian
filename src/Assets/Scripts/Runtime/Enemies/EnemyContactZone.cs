using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Trigger around an enemy that forwards contact with Piter to its controller.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class EnemyContactZone : MonoBehaviour
    {
        private EnemyController owner;

        private void Awake()
        {
            owner = GetComponentInParent<EnemyController>();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (owner == null)
            {
                return;
            }

            PiterController player = other.GetComponentInParent<PiterController>();

            if (player != null)
            {
                owner.HandleContact(player);
            }
        }
    }
}
