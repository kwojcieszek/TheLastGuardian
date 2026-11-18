using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// End-of-stage gate. The level session decides whether it may be used.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelExit : MonoBehaviour
    {
        private void OnTriggerStay2D(Collider2D other)
        {
            if (LevelSession.Current == null || other.GetComponentInParent<PiterController>() == null)
            {
                return;
            }

            LevelSession.Current.TryCompleteLevel();
        }
    }
}
