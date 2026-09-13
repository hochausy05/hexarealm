using HexaRealm.Combat;
using HexaRealm.Loot;
using UnityEngine;

namespace HexaRealm.Boss
{
    [RequireComponent(typeof(BossHealth))]
    public sealed class BossReward : MonoBehaviour
    {
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private LootBundleData reward;

        private PlayerLootReceiver recipient;
        private Health subscribedHealth;
        private bool granted;
        private bool isGranting;
        private bool alreadyGrantedLogged;

        public LootBundleData Reward => reward;
        public bool Granted => granted;

        private void Awake()
        {
            if (ReferenceEquals(bossHealth, null)) bossHealth = GetComponent<BossHealth>();
        }

        private void OnEnable()
        {
            if (!TryGetBossHealth(out Health health)) return;

            subscribedHealth = health;
            subscribedHealth.Died -= GrantOnce;
            subscribedHealth.Died += GrantOnce;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public void SetRecipient(PlayerLootReceiver value)
        {
            if (!granted) recipient = value;
        }

        private void GrantOnce()
        {
            if (granted)
            {
                if (!alreadyGrantedLogged)
                {
                    alreadyGrantedLogged = true;
                    Debug.LogWarning("BossReward: Reward was already granted; duplicate delivery was ignored.", this);
                }

                return;
            }

            if (isGranting)
            {
                Debug.LogWarning("BossReward: Reward delivery is already in progress; duplicate delivery was ignored.", this);
                return;
            }

            if (ReferenceEquals(reward, null))
            {
                Debug.LogError("BossReward: Reward data is missing; reward was not granted.", this);
                return;
            }

            if (reward == null)
            {
                Debug.LogError("BossReward: Reward data has been destroyed; reward was not granted.", this);
                return;
            }

            if (ReferenceEquals(recipient, null))
            {
                Debug.LogError("BossReward: PlayerLootReceiver is missing; reward was not granted.", this);
                return;
            }

            if (recipient == null)
            {
                Debug.LogError("BossReward: PlayerLootReceiver has been destroyed; reward was not granted.", this);
                return;
            }

            isGranting = true;
            bool accepted;
            try
            {
                accepted = recipient.TryReceive(reward, false, false);
            }
            finally
            {
                isGranting = false;
            }

            if (!accepted)
            {
                Debug.LogError(
                    "BossReward: PlayerLootReceiver rejected the reward; check that Reward Data is non-empty and the Player has the required Soul wallet/equipment inventory.",
                    this);
                return;
            }

            granted = true;
        }

        private bool TryGetBossHealth(out Health health)
        {
            health = null;
            if (ReferenceEquals(bossHealth, null))
            {
                Debug.LogError("BossReward: Boss Health is missing; reward cannot listen for Boss death.", this);
                return false;
            }

            if (bossHealth == null)
            {
                Debug.LogError("BossReward: Boss Health has been destroyed; reward cannot listen for Boss death.", this);
                return false;
            }

            health = bossHealth.Health;
            if (ReferenceEquals(health, null))
            {
                Debug.LogError("BossReward: Boss Health's Health reference is missing; reward cannot listen for Boss death.", this);
                return false;
            }

            if (health == null)
            {
                Debug.LogError("BossReward: Boss Health's Health reference has been destroyed; reward cannot listen for Boss death.", this);
                return false;
            }

            return true;
        }

        private void Unsubscribe()
        {
            if (subscribedHealth != null) subscribedHealth.Died -= GrantOnce;
            subscribedHealth = null;
        }
    }
}
