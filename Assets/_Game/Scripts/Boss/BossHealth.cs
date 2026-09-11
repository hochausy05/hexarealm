using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Boss
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(BossRuntime), typeof(Health))]
    public sealed class BossHealth : MonoBehaviour, IRawDamageReceiver
    {
        [SerializeField] private BossRuntime bossRuntime;
        [SerializeField] private Health health;
        public Health Health => health;

        private void Awake()
        {
            ResolveReferences();
            if (bossRuntime == null || bossRuntime.Data == null || health == null)
            {
                Debug.LogError("BossHealth requires BossRuntime with BossData and Health.", this);
                enabled = false;
                return;
            }
            health.Initialize(bossRuntime.Data.MaxHealth);
        }

        public float TakeRawDamage(float rawDamage) => bossRuntime == null || bossRuntime.Data == null || health == null
            ? 0f : health.ApplyDamage(DamageCalculator.CalculateFinalDamage(rawDamage, bossRuntime.Data.Defense));

        public void RestoreFullHealth()
        {
            if (health != null && !health.IsDead) health.Heal(health.MaxHealth - health.CurrentHealth);
        }

        private void ResolveReferences()
        {
            if (bossRuntime == null) bossRuntime = GetComponent<BossRuntime>();
            if (health == null) health = GetComponent<Health>();
        }
    }
}
