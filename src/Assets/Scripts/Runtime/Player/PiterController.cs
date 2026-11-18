using System.Collections.Generic;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Side-scrolling movement, jumping and sword combat of Piter.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(PiterInput))]
    [RequireComponent(typeof(Health), typeof(SpriteFrameAnimator))]
    public sealed class PiterController : MonoBehaviour
    {
        private const float GroundProbeWidth = 0.45f;
        private const float GroundProbeHeight = 0.14f;
        private const float RunAnimationThreshold = 0.3f;
        private const float JumpCutMultiplier = 0.5f;
        private const float AirJumpMultiplier = 0.92f;
        private const float BounceMultiplier = 0.8f;
        private const float KnockbackLockSeconds = 0.28f;
        private const float AttackForwardOffset = 0.55f;
        private const float AttackHeightOffset = 0.15f;
        private const float KillHeight = -14f;

        private static PhysicsMaterial2D frictionless;

        private readonly HashSet<IAttackable> attackTargets = new HashSet<IAttackable>();
        private Rigidbody2D body;
        private CapsuleCollider2D capsule;
        private PiterInput input;
        private Health health;
        private SpriteFrameAnimator animator;
        private SpriteRenderer display;
        private float lastGroundedTime = -999f;
        private float lastJumpPressedTime = -999f;
        private float nextAttackTime;
        private float controlLockedUntil;
        private int airJumpsLeft;
        private bool grounded;
        private bool jumpRising;
        private bool frozen;

        [field: SerializeField] public float MoveSpeed { get; set; } = 6f;

        [field: SerializeField] public float GroundAcceleration { get; set; } = 70f;

        [field: SerializeField] public float AirAcceleration { get; set; } = 45f;

        [field: SerializeField] public float JumpSpeed { get; set; } = 11f;

        [field: SerializeField] public float MaxFallSpeed { get; set; } = 18f;

        [field: SerializeField] public float CoyoteTime { get; set; } = 0.1f;

        [field: SerializeField] public float JumpBufferTime { get; set; } = 0.12f;

        [field: SerializeField] public int MaxAirJumps { get; set; } = 1;

        [field: SerializeField] public int AttackDamage { get; set; } = 1;

        [field: SerializeField] public float AttackRadius { get; set; } = 0.8f;

        [field: SerializeField] public float AttackCooldown { get; set; } = 0.42f;

        [field: SerializeField] public Vector2 KnockbackVelocity { get; set; } = new Vector2(6f, 6.5f);

        [field: SerializeField] public Transform GroundCheck { get; set; }

        public bool AirJumpUnlocked { get; set; } = true;

        public bool IsGrounded => grounded;

        public bool IsDead => health != null && health.IsDead;

        public int FacingDirection { get; private set; } = 1;

        public float FeetY => capsule.bounds.min.y;

        public PiterInput Input => input;

        public Health Health => health;

        public Rigidbody2D Body => body;

        public void TakeHit(int damage, Vector2 sourcePosition)
        {
            if (frozen || !health.ApplyDamage(damage) || health.IsDead)
            {
                return;
            }

            float direction = transform.position.x >= sourcePosition.x ? 1f : -1f;
            body.linearVelocity = new Vector2(direction * KnockbackVelocity.x, KnockbackVelocity.y);
            controlLockedUntil = Time.time + KnockbackLockSeconds;
            jumpRising = false;
        }

        public void Bounce()
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, JumpSpeed * BounceMultiplier);
            airJumpsLeft = AirJumpUnlocked ? MaxAirJumps : 0;
            jumpRising = true;
        }

        public void Respawn(Vector2 position)
        {
            frozen = false;
            body.simulated = true;
            body.position = position;
            transform.position = position;
            body.linearVelocity = Vector2.zero;
            controlLockedUntil = 0f;
            lastJumpPressedTime = -999f;
            FacingDirection = 1;
            display.flipX = false;

            health.Restore();
            animator.ResetState();
            input.Clear();
        }

        public void Freeze()
        {
            frozen = true;
            body.linearVelocity = new Vector2(0f, Mathf.Min(0f, body.linearVelocity.y));
            input.Clear();
            animator.PlayLoop(SpriteFrameAnimator.Idle);
        }

        public void Unfreeze()
        {
            frozen = false;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            input = GetComponent<PiterInput>();
            health = GetComponent<Health>();
            animator = GetComponent<SpriteFrameAnimator>();
            display = GetComponent<SpriteRenderer>();

            if (frictionless == null)
            {
                frictionless = new PhysicsMaterial2D("Frictionless") { friction = 0f, bounciness = 0f };
            }

            capsule.sharedMaterial = frictionless;
            airJumpsLeft = MaxAirJumps;
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }

        private void Update()
        {
            if (health.IsDead || frozen)
            {
                return;
            }

            if (input.ConsumeJump())
            {
                lastJumpPressedTime = Time.time;
            }

            if (input.ConsumeAttack())
            {
                TryAttack();
            }

            UpdateFacing();
            UpdateAnimation();
        }

        private void FixedUpdate()
        {
            if (health.IsDead || frozen)
            {
                body.linearVelocity = new Vector2(Mathf.MoveTowards(body.linearVelocity.x, 0f, GroundAcceleration * Time.fixedDeltaTime), body.linearVelocity.y);

                return;
            }

            if (body.position.y < KillHeight)
            {
                health.Kill();

                return;
            }

            grounded = CheckGround();

            if (grounded)
            {
                lastGroundedTime = Time.time;
                airJumpsLeft = AirJumpUnlocked ? MaxAirJumps : 0;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.x = ComputeHorizontalVelocity(velocity.x);
            velocity.y = ComputeVerticalVelocity(velocity.y);

            body.linearVelocity = velocity;
        }

        private float ComputeHorizontalVelocity(float current)
        {
            if (Time.time < controlLockedUntil)
            {
                return current;
            }

            float target = input.Horizontal * MoveSpeed;
            float acceleration = grounded ? GroundAcceleration : AirAcceleration;

            return Mathf.MoveTowards(current, target, acceleration * Time.fixedDeltaTime);
        }

        private float ComputeVerticalVelocity(float current)
        {
            float velocity = current;
            bool jumpBuffered = Time.time - lastJumpPressedTime <= JumpBufferTime;

            if (jumpBuffered && Time.time >= controlLockedUntil)
            {
                if (Time.time - lastGroundedTime <= CoyoteTime)
                {
                    velocity = JumpSpeed;
                    lastGroundedTime = -999f;
                    lastJumpPressedTime = -999f;
                    jumpRising = true;
                    grounded = false;

                    animator.PlayOnce(SpriteFrameAnimator.Jump);
                    GameAudio.Play(SoundEffect.Jump);
                }
                else if (airJumpsLeft > 0)
                {
                    airJumpsLeft--;
                    velocity = JumpSpeed * AirJumpMultiplier;
                    lastJumpPressedTime = -999f;
                    jumpRising = true;

                    animator.PlayOnce(SpriteFrameAnimator.Jump);
                    GameAudio.Play(SoundEffect.DoubleJump);
                }
            }

            if (jumpRising && velocity > 0f && !input.JumpHeld)
            {
                velocity *= JumpCutMultiplier;
                jumpRising = false;
            }

            if (velocity <= 0f)
            {
                jumpRising = false;
            }

            return Mathf.Max(velocity, -MaxFallSpeed);
        }

        private bool CheckGround()
        {
            if (body.linearVelocity.y > 0.05f)
            {
                return false;
            }

            Vector2 probe = GroundCheck != null ? (Vector2)GroundCheck.position : new Vector2(capsule.bounds.center.x, capsule.bounds.min.y);

            return Physics2D.OverlapBox(probe, new Vector2(GroundProbeWidth, GroundProbeHeight), 0f, GameLayers.GroundMask) != null;
        }

        private void TryAttack()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + AttackCooldown;
            animator.PlayOnce(SpriteFrameAnimator.Attack);
            GameAudio.Play(SoundEffect.Attack);

            Vector2 origin = (Vector2)transform.position + new Vector2(FacingDirection * AttackForwardOffset, AttackHeightOffset);
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, AttackRadius, GameLayers.AttackMask);

            attackTargets.Clear();

            foreach (Collider2D hit in hits)
            {
                IAttackable target = hit.GetComponentInParent<IAttackable>();

                if (target != null && attackTargets.Add(target))
                {
                    target.ReceiveAttack(AttackDamage, origin);
                }
            }
        }

        private void UpdateFacing()
        {
            if (Time.time < controlLockedUntil || Mathf.Abs(input.Horizontal) < 0.1f)
            {
                return;
            }

            FacingDirection = input.Horizontal > 0f ? 1 : -1;
            display.flipX = FacingDirection < 0;
        }

        private void UpdateAnimation()
        {
            if (!grounded)
            {
                animator.PlayLoop(SpriteFrameAnimator.Jump);

                return;
            }

            bool running = Mathf.Abs(body.linearVelocity.x) > RunAnimationThreshold || Mathf.Abs(input.Horizontal) > 0.1f;
            animator.PlayLoop(running ? SpriteFrameAnimator.Run : SpriteFrameAnimator.Idle);
        }

        private void OnDamaged()
        {
            if (!health.IsDead)
            {
                animator.PlayOnce(SpriteFrameAnimator.Hit);
                GameAudio.Play(SoundEffect.Hurt);
            }
        }

        private void OnDied()
        {
            body.linearVelocity = new Vector2(0f, Mathf.Min(0f, body.linearVelocity.y));
            input.Clear();
            animator.PlayOnce(SpriteFrameAnimator.Death);
            GameAudio.Play(SoundEffect.Death);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + new Vector3(FacingDirection * AttackForwardOffset, AttackHeightOffset, 0f), AttackRadius);
        }
    }
}
