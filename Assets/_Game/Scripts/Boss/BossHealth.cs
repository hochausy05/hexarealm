using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Boss
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BossRuntime), typeof(Health))]
    public sealed class BossHealth : MonoBehaviour, IRawDamageReceiver
    {
        [SerializeField] private BossRuntime bossRuntime;
        [SerializeField] private Health health;
        public Health Health => health;
        public bool HasResolvedReferences => bossRuntime != null && bossRuntime.HasValidData && health != null;

        private void Awake()
        {
            if (!ResolveAndValidateReferences())
            {
                Debug.LogError("BossHealth requires BossRuntime with BossData and Health.", this);
                enabled = false;
                return;
            }

            // Health may already have run Awake. This explicit spawn operation is therefore
            // independent of component Awake order and makes BossData authoritative.
            health.ResetToMaxHealth(bossRuntime.Data.MaxHealth);
        }

        public float TakeRawDamage(float rawDamage) => !HasResolvedReferences
            ? 0f : health.ApplyDamage(DamageCalculator.CalculateFinalDamage(rawDamage, bossRuntime.Data.Defense));

        private void ResolveReferences()
        {
            if (bossRuntime == null || bossRuntime.gameObject != gameObject) bossRuntime = GetComponent<BossRuntime>();
            if (health == null || health.gameObject != gameObject) health = GetComponent<Health>();
        }

        internal bool ResolveAndValidateReferences()
        {
            ResolveReferences();
            return HasResolvedReferences;
        }
    }
}
