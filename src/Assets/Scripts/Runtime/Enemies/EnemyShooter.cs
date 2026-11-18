using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Periodically fires projectiles at Piter, used by dragons.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemyShooter : MonoBehaviour
    {
        private const float SpawnDistance = 0.9f;

        private EnemyController enemy;
        private float nextFireTime;

        [field: SerializeField] public Projectile ProjectilePrefab { get; set; }

        [field: SerializeField] public float FireInterval { get; set; } = 3f;

        [field: SerializeField] public float Range { get; set; } = 9f;

        [field: SerializeField] public float ProjectileSpeed { get; set; } = 6f;

        private void Awake()
        {
            enemy = GetComponent<EnemyController>();
            nextFireTime = Time.time + FireInterval;
        }

        private void Update()
        {
            PiterController target = enemy.Target;

            if (enemy.IsDead || target == null || target.IsDead || ProjectilePrefab == null || Time.time < nextFireTime)
            {
                return;
            }

            Vector2 toTarget = (Vector2)target.transform.position + new Vector2(0f, 0.2f) - (Vector2)transform.position;

            if (toTarget.magnitude > Range)
            {
                return;
            }

            nextFireTime = Time.time + FireInterval;

            Fire(toTarget.normalized);
        }

        private void Fire(Vector2 direction)
        {
            Vector3 position = transform.position + (Vector3)(direction * SpawnDistance);
            Projectile projectile = Instantiate(ProjectilePrefab, position, Quaternion.identity);
            projectile.Launch(direction, ProjectileSpeed, ProjectileOwner.Enemy);

            enemy.FaceTowards(enemy.Target.transform.position.x);
            enemy.PlayAttack();
            GameAudio.Play(SoundEffect.Fireball);
        }
    }
}
