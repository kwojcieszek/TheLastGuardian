using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Collectible gold coin.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class CoinPickup : MonoBehaviour
    {
        private bool collected;

        [field: SerializeField] public int Value { get; set; } = 1;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || LevelSession.Current == null || other.GetComponentInParent<PiterController>() == null)
            {
                return;
            }

            collected = true;
            LevelSession.Current.CollectCoin(Value);
            GameAudio.Play(SoundEffect.Coin);

            gameObject.SetActive(false);
        }
    }
}
