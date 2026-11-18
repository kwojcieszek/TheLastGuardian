using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Secret chest with a coin reward.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class TreasureChest : MonoBehaviour
    {
        private static readonly Color OpenedColor = new Color(0.6f, 0.55f, 0.5f, 1f);

        [field: SerializeField] public int CoinReward { get; set; } = 10;

        [field: SerializeField] public SpriteRenderer ChestRenderer { get; set; }

        public bool IsOpened { get; private set; }

        public void MarkOpened()
        {
            IsOpened = true;

            if (ChestRenderer != null)
            {
                ChestRenderer.color = OpenedColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsOpened || LevelSession.Current == null || other.GetComponentInParent<PiterController>() == null)
            {
                return;
            }

            MarkOpened();
            LevelSession.Current.OpenChest(CoinReward);
            GameAudio.Play(SoundEffect.Chest);
        }
    }
}
