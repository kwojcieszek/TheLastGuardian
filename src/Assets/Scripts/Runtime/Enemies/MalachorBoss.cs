using System.Collections.Generic;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Three-phase final boss: orb volleys, summoned skeletons and fire runes.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public sealed class MalachorBoss : MonoBehaviour
    {
        private const float PhaseTwoThreshold = 0.66f;
        private const float PhaseThreeThreshold = 0.33f;
        private const float ActivationRange = 13f;
        private const float OrbSpreadDegrees = 16f;
        private const float OrbSpawnDistance = 0.9f;
        private const float RuneSpacing = 2.4f;
        private const float SummonOffset = 4f;
        private const int MaxMinions = 3;

        private readonly List<EnemyController> minions = new List<EnemyController>();
        private EnemyController enemy;
        private float nextVolleyTime;
        private float nextSummonTime;
        private float nextRuneTime;

        [field: SerializeField] public Projectile OrbPrefab { get; set; }

        [field: SerializeField] public EnemyController MinionPrefab { get; set; }

        [field: SerializeField] public FireRune RunePrefab { get; set; }

        [field: SerializeField] public float OrbSpeed { get; set; } = 5.5f;

        [field: SerializeField] public float MinionSpawnY { get; set; } = -0.7f;

        [field: SerializeField] public float RuneY { get; set; } = -1.35f;

        public bool IsActivated { get; private set; }

        public int Phase { get; private set; } = 1;

        private void Awake()
        {
            enemy = GetComponent<EnemyController>();
        }

        private void Update()
        {
            PiterController target = enemy.Target;

            if (enemy.IsDead)
            {
                DefeatMinions();

                return;
            }

            if (target == null || target.IsDead)
            {
                return;
            }

            if (!IsActivated)
            {
                TryActivate(target);

                return;
            }

            UpdatePhase();

            if (Time.time >= nextVolleyTime)
            {
                FireVolley(target, Phase == 3 ? 5 : 3);
                nextVolleyTime = Time.time + (Phase == 1 ? 3f : Phase == 2 ? 3.2f : 2.4f);
            }

            if (Phase >= 2 && Time.time >= nextSummonTime)
            {
                SummonMinions();
                nextSummonTime = Time.time + 8f;
            }

            if (Phase == 3 && Time.time >= nextRuneTime)
            {
                SpawnRunes(target);
                nextRuneTime = Time.time + 4.5f;
            }
        }

        private void TryActivate(PiterController target)
        {
            if (Mathf.Abs(target.transform.position.x - transform.position.x) > ActivationRange)
            {
                return;
            }

            IsActivated = true;
            nextVolleyTime = Time.time + 1.5f;
            nextSummonTime = Time.time + 2f;
            nextRuneTime = Time.time + 2f;

            if (LevelSession.Current != null)
            {
                LevelSession.Current.ShowToast("Malachor: \"You are too late, boy!\"");
            }
        }

        private void UpdatePhase()
        {
            float fraction = enemy.Health.Fraction;
            int phase = fraction > PhaseTwoThreshold ? 1 : fraction > PhaseThreeThreshold ? 2 : 3;

            if (phase == Phase)
            {
                return;
            }

            Phase = phase;

            if (LevelSession.Current != null)
            {
                string message = phase == 2 ? "Malachor summons the dead! Reflect his orbs with your sword!" : "Malachor unleashes fire runes!";
                LevelSession.Current.ShowToast(message);
            }
        }

        private void FireVolley(PiterController target, int count)
        {
            if (OrbPrefab == null)
            {
                return;
            }

            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.4f);
            Vector2 aim = (Vector2)target.transform.position + new Vector2(0f, 0.2f) - origin;
            float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

            for (int index = 0; index < count; index++)
            {
                float angle = (baseAngle + (index - (count - 1) * 0.5f) * OrbSpreadDegrees) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Projectile orb = Instantiate(OrbPrefab, origin + direction * OrbSpawnDistance, Quaternion.identity);

                orb.Launch(direction, OrbSpeed, ProjectileOwner.Enemy);
            }

            enemy.FaceTowards(target.transform.position.x);
            enemy.PlayAttack();
            GameAudio.Play(SoundEffect.Fireball);
        }

        private void SummonMinions()
        {
            if (MinionPrefab == null)
            {
                return;
            }

            minions.RemoveAll(minion => minion == null || minion.IsDead);

            float[] offsets = { -SummonOffset, SummonOffset };

            foreach (float offset in offsets)
            {
                if (minions.Count >= MaxMinions)
                {
                    return;
                }

                Vector3 position = new Vector3(transform.position.x + offset, MinionSpawnY, 0f);
                EnemyController minion = Instantiate(MinionPrefab, position, Quaternion.identity, transform.parent);
                minion.name = "Summoned Skeleton";
                minions.Add(minion);
            }
        }

        private void SpawnRunes(PiterController target)
        {
            if (RunePrefab == null)
            {
                return;
            }

            for (int index = -1; index <= 1; index++)
            {
                Vector3 position = new Vector3(target.transform.position.x + index * RuneSpacing, RuneY, 0f);
                Instantiate(RunePrefab, position, Quaternion.identity, transform.parent);
            }
        }

        private void DefeatMinions()
        {
            if (minions.Count == 0)
            {
                return;
            }

            foreach (EnemyController minion in minions)
            {
                if (minion != null && !minion.IsDead)
                {
                    minion.Health.Kill();
                }
            }

            minions.Clear();
        }
    }
}
