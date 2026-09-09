using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Player
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(PlayerStats), typeof(Health))]
    public sealed class PlayerHealth : MonoBehaviour, IRawDamageReceiver
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private Health health;

        private bool isSubscribed;

        public Health Health => health;

        private void Awake()
        {
            ResolveReferences();
            health.Initialize(VitalityToMaxHealth(playerStats.GetFinalStat(PlayerStatType.Vitality)));
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToStats();
            SyncMaxHealthFromVitality();
        }

        private void OnDisable()
        {
            UnsubscribeFromStats();
        }

        public float TakeRawDamage(float rawDamage)
        {
            float defense = playerStats.GetFinalStat(PlayerStatType.Defense);
            float finalDamage = DamageCalculator.CalculateFinalDamage(rawDamage, defense);
            return health.ApplyDamage(finalDamage);
        }

        private void HandleStatChanged(PlayerStatType type, float oldFinalValue, float newFinalValue)
        {
            if (type == PlayerStatType.Vitality)
            {
                health.SetMaxHealth(VitalityToMaxHealth(newFinalValue));
            }
        }

        private void SyncMaxHealthFromVitality()
        {
            float maxHealth = VitalityToMaxHealth(playerStats.GetFinalStat(PlayerStatType.Vitality));

            if (health.IsInitialized)
            {
                health.SetMaxHealth(maxHealth);
            }
            else
            {
                health.Initialize(maxHealth);
            }
        }

        private void SubscribeToStats()
        {
            if (isSubscribed)
            {
                return;
            }

            playerStats.StatChanged += HandleStatChanged;
            isSubscribed = true;
        }

        private void UnsubscribeFromStats()
        {
            if (!isSubscribed || playerStats == null)
            {
                return;
            }

            playerStats.StatChanged -= HandleStatChanged;
            isSubscribed = false;
        }

        private void ResolveReferences()
        {
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
            }

            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }

        private static float VitalityToMaxHealth(float vitality)
        {
            return Mathf.Max(0f, vitality);
        }
    }
}
