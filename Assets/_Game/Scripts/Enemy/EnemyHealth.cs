using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Enemy
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(EnemyRuntime), typeof(Health))]
    public sealed class EnemyHealth : MonoBehaviour, IRawDamageReceiver
    {
        [SerializeField] private EnemyRuntime enemyRuntime;
        [SerializeField] private Health health;

        public Health Health => health;

        private void Awake()
        {
            ResolveReferences();

            if (enemyRuntime == null || enemyRuntime.Data == null || health == null)
            {
                Debug.LogError("EnemyHealth requires EnemyRuntime with EnemyData and Health.", this);
                enabled = false;
                return;
            }

            health.Initialize(enemyRuntime.Data.MaxHealth);
        }

        public float TakeRawDamage(float rawDamage)
        {
            if (enemyRuntime == null || enemyRuntime.Data == null || health == null)
            {
                return 0f;
            }

            float finalDamage = DamageCalculator.CalculateFinalDamage(rawDamage, enemyRuntime.Data.Defense);
            return health.ApplyDamage(finalDamage);
        }

        private void ResolveReferences()
        {
            if (enemyRuntime == null)
            {
                enemyRuntime = GetComponent<EnemyRuntime>();
            }

            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }
    }
}
