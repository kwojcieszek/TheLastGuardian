using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Tints the sprite after a hit and optionally blinks while invulnerable.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DamageFlash : MonoBehaviour
    {
        private const float FlashSeconds = 0.12f;
        private const float BlinkFrequency = 16f;
        private const float BlinkAlpha = 0.35f;

        private static readonly Color FlashTint = new Color(1f, 0.4f, 0.4f, 1f);

        private SpriteRenderer display;
        private Health health;
        private Color baseColor;
        private float flashUntil;

        [field: SerializeField] public bool BlinkWhileInvulnerable { get; set; }

        private void Awake()
        {
            display = GetComponent<SpriteRenderer>();
            health = GetComponentInParent<Health>();
            baseColor = display.color;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged += OnDamaged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
            }

            if (display != null)
            {
                display.color = baseColor;
            }
        }

        private void LateUpdate()
        {
            Color color = Time.time < flashUntil ? baseColor * FlashTint : baseColor;

            bool blinking = BlinkWhileInvulnerable && health != null && !health.IsDead && health.IsInvulnerable && Time.time >= flashUntil;

            if (blinking && Mathf.FloorToInt(Time.time * BlinkFrequency) % 2 == 0)
            {
                color.a = baseColor.a * BlinkAlpha;
            }

            display.color = color;
        }

        private void OnDamaged()
        {
            flashUntil = Time.time + FlashSeconds;
        }
    }
}
