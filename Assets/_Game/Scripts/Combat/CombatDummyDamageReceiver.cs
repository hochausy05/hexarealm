using UnityEngine;

namespace HexaRealm.Combat
{
    [RequireComponent(typeof(Health))]
    public sealed class CombatDummyDamageReceiver : MonoBehaviour, IRawDamageReceiver
    {
        [SerializeField] private Health health;
        [SerializeField, Min(0f)] private float defense;

        public Health Health => health;
        public float Defense => defense;

        private void Awake()
        {
            ResolveHealth();
        }

        public float TakeRawDamage(float rawDamage)
        {
            ResolveHealth();
            float finalDamage = DamageCalculator.CalculateFinalDamage(rawDamage, defense);
            return health.ApplyDamage(finalDamage);
        }

        private void ResolveHealth()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }
    }
}
