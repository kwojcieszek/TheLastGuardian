using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Patrolling, chasing, flying and diving enemy with contact damage and stomp support.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(SpriteFrameAnimator))]
    public sealed class EnemyController : MonoBehaviour, IAttackable
    {
        private const float ActivationDistance = 24f;
        private const float LedgeProbeOffset = 0.55f;
        private const float LedgeProbeLength = 1.1f;
        private const float HoverAmplitude = 0.35f;
        private const float HoverFrequency = 2f;
        private const float DiveDuration = 1.1f;
        private const float DiveCooldown = 2.4f;
        private const float AttackAnimationRange = 1.6f;
        private const float AttackAnimationCooldown = 1.1f;
        private const float KnockbackSeconds = 0.2f;
        private const float KnockbackSpeed = 3.5f;
        private const float KillHeight = -12f;
        private const float WalkerGravity = 2.5f;
        private const float VerticalSightGround = 3f;
        private const float VerticalSightAir = 6f;

        private Rigidbody2D body;
        private Health health;
        private SpriteFrameAnimator animator;
        private SpriteRenderer display;
        private Collider2D bodyCollider;
        private Vector2 origin;
        private Vector2 diveTarget;
        private float direction = 1f;
        private float hoverPhase;
        private float diveEndTime;
        private float nextDiveTime;
        private float nextAttackAnimationTime;
        private float knockbackUntil;
        private bool diving;

        [field: SerializeField] public string DisplayName { get; set; } = "Enemy";

        [field: SerializeField] public EnemyMovement Movement { get; set; } = EnemyMovement.Walker;

        [field: SerializeField] public float MoveSpeed { get; set; } = 1.8f;

        [field: SerializeField] public float ChargeSpeed { get; set; } = 4.5f;

        [field: SerializeField] public float PatrolDistance { get; set; } = 3f;

        [field: SerializeField] public float DetectionDistance { get; set; } = 5f;

        [field: SerializeField] public int ContactDamage { get; set; } = 1;

        [field: SerializeField] public bool Stompable { get; set; } = true;

        [field: SerializeField] public bool IsBoss { get; set; }

        public PiterController Target { get; private set; }

        public Health Health => health;

        public bool IsDead => health == null || health.IsDead;

        public bool IsFlying => Movement == EnemyMovement.Flyer || Movement == EnemyMovement.Diver;

        public Vector2 Origin => origin;

        public void ReceiveAttack(int damage, Vector2 attackOrigin)
        {
            if (!health.ApplyDamage(damage) || health.IsDead || IsBoss)
            {
                return;
            }

            float push = transform.position.x >= attackOrigin.x ? 1f : -1f;
            knockbackUntil = Time.time + KnockbackSeconds;
            body.linearVelocity = new Vector2(push * KnockbackSpeed, IsFlying ? 0f : 2f);
        }

        public void HandleContact(PiterController player)
        {
            if (IsDead || player == null || player.IsDead)
            {
                return;
            }

            Bounds bounds = bodyCollider.bounds;
            float stompHeight = bounds.center.y + bounds.extents.y * 0.25f;
            bool falling = player.Body.linearVelocity.y < -0.5f;

            if (Stompable && falling && player.FeetY > stompHeight)
            {
                ReceiveAttack(1, player.transform.position);
                player.Bounce();
                GameAudio.Play(SoundEffect.Stomp);

                return;
            }

            player.TakeHit(ContactDamage, transform.position);
        }

        public void PlayAttack()
        {
            if (!IsDead)
            {
                animator.PlayOnce(SpriteFrameAnimator.Attack);
            }
        }

        public void FaceTowards(float x)
        {
            float facing = x >= transform.position.x ? 1f : -1f;
            display.flipX = facing < 0f;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            animator = GetComponent<SpriteFrameAnimator>();
            display = GetComponent<SpriteRenderer>();
            bodyCollider = GetComponent<Collider2D>();
            origin = body.position;
            hoverPhase = Random.Range(0f, Mathf.PI * 2f);
            body.gravityScale = IsFlying ? 0f : WalkerGravity;
            body.freezeRotation = true;

            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
                health.Died -= OnDied;
            }
        }

        private void Start()
        {
            ResolveTarget();
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            if (Target == null)
            {
                ResolveTarget();
            }

            UpdateAnimation();
        }

        private void FixedUpdate()
        {
            if (IsDead)
            {
                if (!IsFlying)
                {
                    body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                }

                return;
            }

            if (body.position.y < KillHeight)
            {
                health.Kill();

                return;
            }

            if (!IsActive())
            {
                body.linearVelocity = IsFlying ? Vector2.zero : new Vector2(0f, body.linearVelocity.y);

                return;
            }

            if (Time.time < knockbackUntil)
            {
                return;
            }

            switch (Movement)
            {
                case EnemyMovement.Walker:
                case EnemyMovement.Charger:
                    UpdateWalker();
                    break;
                case EnemyMovement.Flyer:
                case EnemyMovement.Diver:
                    UpdateFlyer();
                    break;
                default:
                    body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                    break;
            }
        }

        private void ResolveTarget()
        {
            LevelSession session = LevelSession.Current;
            Target = session != null ? session.Player : FindFirstObjectByType<PiterController>();
        }

        private bool IsActive()
        {
            return Target != null && Mathf.Abs(Target.transform.position.x - body.position.x) < ActivationDistance;
        }

        private bool CanSeeTarget(float verticalRange)
        {
            if (Target == null || Target.IsDead)
            {
                return false;
            }

            Vector2 offset = (Vector2)Target.transform.position - body.position;

            return Mathf.Abs(offset.x) < DetectionDistance && Mathf.Abs(offset.y) < verticalRange;
        }

        private void UpdateWalker()
        {
            float left = origin.x - PatrolDistance;
            float right = origin.x + PatrolDistance;
            bool chasing = CanSeeTarget(VerticalSightGround);
            float targetX;

            if (chasing)
            {
                targetX = Mathf.Clamp(Target.transform.position.x, left - PatrolDistance * 0.5f, right + PatrolDistance * 0.5f);
            }
            else
            {
                if (body.position.x >= right - 0.1f)
                {
                    direction = -1f;
                }
                else if (body.position.x <= left + 0.1f)
                {
                    direction = 1f;
                }

                targetX = direction > 0f ? right : left;
            }

            float speed = chasing ? (Movement == EnemyMovement.Charger ? ChargeSpeed : MoveSpeed * 1.3f) : MoveSpeed;
            float delta = targetX - body.position.x;
            float velocityX = Mathf.Abs(delta) < 0.08f ? 0f : Mathf.Sign(delta) * speed;

            if (!Mathf.Approximately(velocityX, 0f) && !HasGroundAhead(Mathf.Sign(velocityX)))
            {
                velocityX = 0f;
                direction = -Mathf.Sign(delta);
            }

            body.linearVelocity = new Vector2(velocityX, body.linearVelocity.y);
        }

        private void UpdateFlyer()
        {
            float hoverY = origin.y + Mathf.Sin(Time.time * HoverFrequency + hoverPhase) * HoverAmplitude;
            Vector2 target;
            float speed = MoveSpeed;

            if (Movement == EnemyMovement.Diver && diving)
            {
                target = diveTarget;
                speed = ChargeSpeed;

                if (Time.time >= diveEndTime)
                {
                    diving = false;
                }
            }
            else if (Movement == EnemyMovement.Diver && Time.time >= nextDiveTime && CanSeeTarget(VerticalSightAir))
            {
                diving = true;
                diveTarget = (Vector2)Target.transform.position + new Vector2(0f, 0.3f);
                diveEndTime = Time.time + DiveDuration;
                nextDiveTime = diveEndTime + DiveCooldown;
                target = diveTarget;
                speed = ChargeSpeed;
            }
            else
            {
                float left = origin.x - PatrolDistance;
                float right = origin.x + PatrolDistance;

                if (body.position.x >= right - 0.1f)
                {
                    direction = -1f;
                }
                else if (body.position.x <= left + 0.1f)
                {
                    direction = 1f;
                }

                target = new Vector2(direction > 0f ? right : left, hoverY);
            }

            Vector2 desired = (target - body.position) / Time.fixedDeltaTime;
            body.linearVelocity = Vector2.ClampMagnitude(desired, speed);
        }

        private bool HasGroundAhead(float sign)
        {
            Bounds bounds = bodyCollider.bounds;
            Vector2 probe = new Vector2(bounds.center.x + sign * (bounds.extents.x + LedgeProbeOffset), bounds.min.y + 0.2f);

            return Physics2D.Raycast(probe, Vector2.down, LedgeProbeLength, GameLayers.GroundMask).collider != null;
        }

        private void UpdateAnimation()
        {
            float velocityX = body.linearVelocity.x;

            if (Target != null && CanSeeTarget(IsFlying ? VerticalSightAir : VerticalSightGround))
            {
                FaceTowards(Target.transform.position.x);
            }
            else if (Mathf.Abs(velocityX) > 0.1f)
            {
                display.flipX = velocityX < 0f;
            }

            bool moving = Mathf.Abs(velocityX) > 0.2f || IsFlying;
            animator.PlayLoop(moving ? SpriteFrameAnimator.Run : SpriteFrameAnimator.Idle);

            if (Target == null || Time.time < nextAttackAnimationTime)
            {
                return;
            }

            float distance = Vector2.Distance(Target.transform.position, transform.position);

            if (distance < AttackAnimationRange)
            {
                nextAttackAnimationTime = Time.time + AttackAnimationCooldown;
                animator.PlayOnce(SpriteFrameAnimator.Attack);
            }
        }

        private void OnDamaged()
        {
            if (!health.IsDead)
            {
                animator.PlayOnce(SpriteFrameAnimator.Hit);
                GameAudio.Play(SoundEffect.EnemyHit);
            }
        }

        private void OnDied()
        {
            animator.PlayOnce(SpriteFrameAnimator.Death);
            GameAudio.Play(SoundEffect.EnemyDefeat);

            foreach (EnemyContactZone zone in GetComponentsInChildren<EnemyContactZone>())
            {
                zone.gameObject.SetActive(false);
            }

            if (IsFlying)
            {
                body.gravityScale = 1.5f;
            }

            body.linearVelocity = new Vector2(0f, Mathf.Min(0f, body.linearVelocity.y));

            if (LevelSession.Current != null)
            {
                LevelSession.Current.NotifyEnemyDefeated(this);
            }
        }
    }
}
