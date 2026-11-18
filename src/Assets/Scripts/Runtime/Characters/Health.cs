using System;
using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Hit points with temporary invulnerability after each hit.
    /// </summary>
    public sealed class Health : MonoBehaviour
    {
        private int currentHealth;
        private float lastDamageTime = -999f;

        [field: SerializeField] public int MaxHealth { get; set; } = 3;

        [field: SerializeField] public float InvulnerabilitySeconds { get; set; } = 0.5f;

        [field: SerializeField] public bool DestroyOnDeath { get; set; } = true;

        [field: SerializeField] public float DestroyDelay { get; set; } = 0.9f;

        public int CurrentHealth => currentHealth;

        public bool IsDead => currentHealth <= 0;

        public bool IsInvulnerable => Time.time - lastDamageTime < InvulnerabilitySeconds;

        public float Fraction => MaxHealth > 0 ? (float)currentHealth / MaxHealth : 0f;

        public event Action Damaged;

        public event Action Died;

        public event Action Changed;

        public bool ApplyDamage(int amount)
        {
            if (IsDead || amount <= 0 || IsInvulnerable)
            {
                return false;
            }

            lastDamageTime = Time.time;
            currentHealth = Mathf.Max(0, currentHealth - amount);

            Changed?.Invoke();
            Damaged?.Invoke();

            if (currentHealth == 0)
            {
                Died?.Invoke();

                if (DestroyOnDeath)
                {
                    Destroy(gameObject, DestroyDelay);
                }
            }

            return true;
        }

        public bool Heal(int amount)
        {
            if (IsDead || amount <= 0 || currentHealth >= MaxHealth)
            {
                return false;
            }

            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);

            Changed?.Invoke();

            return true;
        }

        public void Kill()
        {
            if (IsDead)
            {
                return;
            }

            lastDamageTime = -999f;

            ApplyDamage(currentHealth);
        }

        public void Restore()
        {
            currentHealth = MaxHealth;
            lastDamageTime = Time.time;

            Changed?.Invoke();
        }

        private void Awake()
        {
            currentHealth = MaxHealth;
        }
    }
}
