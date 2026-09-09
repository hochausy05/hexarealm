using HexaRealm.Progression;
using UnityEngine;

namespace HexaRealm.Enemy
{
    [RequireComponent(typeof(EnemyRuntime), typeof(EnemyHealth))]
    public sealed class EnemySoulReward : MonoBehaviour
    {
        [SerializeField] private EnemyRuntime enemyRuntime;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private PlayerSoulWallet playerSoulWallet;

        private bool hasAwarded;
        private bool hasWarnedMissingWallet;
        private HexaRealm.Combat.Health subscribedHealth;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToDeath();
        }

        private void OnDisable()
        {
            UnsubscribeFromDeath();
        }

        private void HandleDied()
        {
            if (hasAwarded)
            {
                return;
            }

            hasAwarded = true;

            if (enemyRuntime == null || enemyRuntime.Data == null)
            {
                return;
            }

            int soulReward = enemyRuntime.Data.SoulReward;

            if (soulReward <= 0)
            {
                return;
            }

            if (playerSoulWallet == null)
            {
                playerSoulWallet = FindFirstObjectByType<PlayerSoulWallet>();
            }

            if (playerSoulWallet == null)
            {
                if (!hasWarnedMissingWallet)
                {
                    Debug.LogWarning("EnemySoulReward could not find a PlayerSoulWallet to receive Souls.", this);
                    hasWarnedMissingWallet = true;
                }

                return;
            }

            playerSoulWallet.AddSouls(soulReward);
        }

        private void SubscribeToDeath()
        {
            if (subscribedHealth != null || enemyHealth == null || enemyHealth.Health == null)
            {
                return;
            }

            subscribedHealth = enemyHealth.Health;
            subscribedHealth.Died += HandleDied;
        }

        private void UnsubscribeFromDeath()
        {
            if (subscribedHealth == null)
            {
                return;
            }

            subscribedHealth.Died -= HandleDied;
            subscribedHealth = null;
        }

        private void ResolveReferences()
        {
            if (enemyRuntime == null)
            {
                enemyRuntime = GetComponent<EnemyRuntime>();
            }

            if (enemyHealth == null)
            {
                enemyHealth = GetComponent<EnemyHealth>();
            }
        }
    }
}
