using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Fireball or magic orb. Piter can reflect reflectable projectiles with his sword.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Projectile : MonoBehaviour, IAttackable
    {
        private const float ReflectSpeedMultiplier = 1.4f;

        private static readonly Color ReflectedTint = new Color(0.55f, 1f, 1f, 1f);

        private Rigidbody2D body;
        private SpriteRenderer display;
        private ProjectileOwner owner = ProjectileOwner.Enemy;
        private float speed;
        private float expiresAt;

        [field: SerializeField] public int Damage { get; set; } = 1;

        [field: SerializeField] public int ReflectedDamage { get; set; } = 2;

        [field: SerializeField] public float Lifetime { get; set; } = 6f;

        [field: SerializeField] public bool Reflectable { get; set; } = true;

        [field: SerializeField] public float SpinSpeed { get; set; } = 360f;

        public ProjectileOwner Owner => owner;

        public void Launch(Vector2 direction, float launchSpeed, ProjectileOwner projectileOwner)
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            owner = projectileOwner;
            speed = launchSpeed;
            expiresAt = Time.time + Lifetime;
            body.linearVelocity = direction.normalized * speed;
        }

        public void ReceiveAttack(int damage, Vector2 origin)
        {
            if (owner == ProjectileOwner.Player)
            {
                return;
            }

            if (!Reflectable)
            {
                Destroy(gameObject);

                return;
            }

            Vector2 direction = -body.linearVelocity;
            LevelSession session = LevelSession.Current;

            if (session != null && session.Guardian != null && !session.Guardian.IsDead)
            {
                direction = (Vector2)session.Guardian.transform.position + new Vector2(0f, 0.4f) - body.position;
            }

            owner = ProjectileOwner.Player;
            expiresAt = Time.time + Lifetime;
            body.linearVelocity = direction.normalized * speed * ReflectSpeedMultiplier;

            if (display != null)
            {
                display.color = ReflectedTint;
            }

            GameAudio.Play(SoundEffect.Reflect);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            display = GetComponentInChildren<SpriteRenderer>();
            expiresAt = Time.time + Lifetime;
        }

        private void Update()
        {
            if (display != null)
            {
                display.transform.Rotate(0f, 0f, SpinSpeed * Time.deltaTime);
            }

            if (Time.time >= expiresAt)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.isTrigger)
            {
                return;
            }

            if (other.gameObject.layer == GameLayers.Ground)
            {
                Destroy(gameObject);

                return;
            }

            if (owner == ProjectileOwner.Enemy)
            {
                PiterController player = other.GetComponentInParent<PiterController>();

                if (player != null)
                {
                    player.TakeHit(Damage, transform.position);
                    Destroy(gameObject);
                }

                return;
            }

            EnemyController enemy = other.GetComponentInParent<EnemyController>();

            if (enemy != null && !enemy.IsDead)
            {
                enemy.Health.ApplyDamage(ReflectedDamage);
                Destroy(gameObject);
            }
        }
    }
}
