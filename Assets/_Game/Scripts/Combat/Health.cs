using System;
using UnityEngine;

namespace HexaRealm.Combat
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float maxHealth = 100f;

        [SerializeField] private float currentHealth;
        private bool isDead;
        private bool isInitialized;

        public event Action<float, float> HealthChanged;
        public event Action<float, float> MaxHealthChanged;
        public event Action Died;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsDead => isDead;
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            Initialize(maxHealth);
        }

        public void Initialize(float initialMaxHealth)
        {
            if (isInitialized)
            {
                return;
            }

            maxHealth = SanitizeMaxHealth(initialMaxHealth);
            currentHealth = maxHealth;
            isDead = currentHealth <= 0f;
            isInitialized = true;
        }

        public void SetMaxHealth(float value)
        {
            if (!isInitialized)
            {
                Initialize(value);
                return;
            }

            float newMaxHealth = SanitizeMaxHealth(value);

            if (Mathf.Approximately(maxHealth, newMaxHealth))
            {
                return;
            }

            float oldMaxHealth = maxHealth;
            float oldCurrentHealth = currentHealth;

            maxHealth = newMaxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            bool diedNow = !isDead && currentHealth <= 0f;

            if (diedNow)
            {
                isDead = true;
            }

            MaxHealthChanged?.Invoke(oldMaxHealth, maxHealth);

            if (!Mathf.Approximately(oldCurrentHealth, currentHealth))
            {
                HealthChanged?.Invoke(oldCurrentHealth, currentHealth);
            }

            if (diedNow)
            {
                Died?.Invoke();
            }
        }

        /// <summary>
        /// Sets a new maximum and begins a fresh life at that maximum. This is intended for
        /// explicit spawn/reset boundaries; ordinary healing must continue to use <see cref="Heal"/>.
        /// </summary>
        public void ResetToMaxHealth(float value)
        {
            float newMaxHealth = SanitizeMaxHealth(value);

            if (!isInitialized)
            {
                Initialize(newMaxHealth);
                return;
            }

            float oldMaxHealth = maxHealth;
            float oldCurrentHealth = currentHealth;
            maxHealth = newMaxHealth;
            currentHealth = maxHealth;
            isDead = currentHealth <= 0f;

            if (!Mathf.Approximately(oldMaxHealth, maxHealth))
            {
                MaxHealthChanged?.Invoke(oldMaxHealth, maxHealth);
            }

            if (!Mathf.Approximately(oldCurrentHealth, currentHealth))
            {
                HealthChanged?.Invoke(oldCurrentHealth, currentHealth);
            }
        }

        public float ApplyDamage(float finalDamage)
        {
            if (!isInitialized || isDead || finalDamage <= 0f || float.IsNaN(finalDamage))
            {
                return 0f;
            }

            float oldCurrentHealth = currentHealth;
            float newCurrentHealth = Mathf.Max(0f, currentHealth - finalDamage);

            if (Mathf.Approximately(oldCurrentHealth, newCurrentHealth))
            {
                return 0f;
            }

            currentHealth = newCurrentHealth;
            bool diedNow = currentHealth <= 0f;

            if (diedNow)
            {
                isDead = true;
            }

            HealthChanged?.Invoke(oldCurrentHealth, currentHealth);

            if (diedNow)
            {
                Died?.Invoke();
            }

            return oldCurrentHealth - currentHealth;
        }

        public float Heal(float amount)
        {
            if (!isInitialized || isDead || amount <= 0f || float.IsNaN(amount))
            {
                return 0f;
            }

            float oldCurrentHealth = currentHealth;
            float newCurrentHealth = Mathf.Min(maxHealth, currentHealth + amount);

            if (Mathf.Approximately(oldCurrentHealth, newCurrentHealth))
            {
                return 0f;
            }

            currentHealth = newCurrentHealth;
            HealthChanged?.Invoke(oldCurrentHealth, currentHealth);
            return currentHealth - oldCurrentHealth;
        }

        private static float SanitizeMaxHealth(float value)
        {
            return float.IsNaN(value) ? 0f : Mathf.Max(0f, value);
        }
    }
}
