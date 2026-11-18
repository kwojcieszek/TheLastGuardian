using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Story objective: a lever, crystal, egg or seal collected by touch or destroyed by the sword.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ObjectiveItem : MonoBehaviour, IAttackable
    {
        private static readonly Color UsedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        [field: SerializeField] public bool RequiresAttack { get; set; }

        [field: SerializeField] public bool HideWhenCollected { get; set; } = true;

        [field: SerializeField] public SpriteRenderer ItemRenderer { get; set; }

        public bool IsCollected { get; private set; }

        public void ReceiveAttack(int damage, Vector2 origin)
        {
            if (RequiresAttack)
            {
                Collect();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (RequiresAttack || other.GetComponentInParent<PiterController>() == null)
            {
                return;
            }

            Collect();
        }

        private void Collect()
        {
            if (IsCollected || LevelSession.Current == null)
            {
                return;
            }

            IsCollected = true;
            LevelSession.Current.CollectObjective();
            GameAudio.Play(SoundEffect.Objective);

            if (HideWhenCollected)
            {
                gameObject.SetActive(false);

                return;
            }

            if (ItemRenderer != null)
            {
                ItemRenderer.flipX = !ItemRenderer.flipX;
                ItemRenderer.color = UsedColor;
            }
        }
    }
}
