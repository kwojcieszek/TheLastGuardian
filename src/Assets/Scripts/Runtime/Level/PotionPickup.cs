using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Health potion that heals immediately or is stored for later.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class PotionPickup : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (LevelSession.Current == null || other.GetComponentInParent<PiterController>() == null)
            {
                return;
            }

            if (LevelSession.Current.TryCollectPotion())
            {
                GameAudio.Play(SoundEffect.Potion);
                gameObject.SetActive(false);
            }
        }
    }
}
