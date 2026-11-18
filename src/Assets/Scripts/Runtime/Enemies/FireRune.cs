using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Floor rune that blinks as a warning and then burns for a short time.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class FireRune : MonoBehaviour
    {
        private const float BlinkFrequency = 10f;

        private SpriteRenderer display;
        private Collider2D trigger;
        private float spawnTime;
        private Vector3 baseScale;

        [field: SerializeField] public float WarningSeconds { get; set; } = 0.9f;

        [field: SerializeField] public float ActiveSeconds { get; set; } = 1.3f;

        [field: SerializeField] public int Damage { get; set; } = 1;

        private void Awake()
        {
            display = GetComponentInChildren<SpriteRenderer>();
            trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
            trigger.enabled = false;
            spawnTime = Time.time;
            baseScale = display != null ? display.transform.localScale : Vector3.one;
        }

        private void Update()
        {
            float age = Time.time - spawnTime;

            if (age >= WarningSeconds + ActiveSeconds)
            {
                Destroy(gameObject);

                return;
            }

            bool active = age >= WarningSeconds;
            trigger.enabled = active;

            if (display == null)
            {
                return;
            }

            Color color = display.color;

            if (active)
            {
                color.a = 1f;
                float pulse = 1f + Mathf.Sin(age * 20f) * 0.08f;
                display.transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y * pulse * 1.6f, baseScale.z);
            }
            else
            {
                color.a = Mathf.FloorToInt(age * BlinkFrequency) % 2 == 0 ? 0.25f : 0.6f;
            }

            display.color = color;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            PiterController player = other.GetComponentInParent<PiterController>();

            if (player != null)
            {
                player.TakeHit(Damage, transform.position);
            }
        }
    }
}
