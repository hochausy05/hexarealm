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
        private bool granted;

        public LootBundleData Reward => reward;
        public bool Granted => granted;

        private void Awake()
        {
            if (bossHealth == null) bossHealth = GetComponent<BossHealth>();
        }

        private void OnEnable()
        {
            if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.Died += GrantOnce;
        }

        private void OnDisable()
        {
            if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.Died -= GrantOnce;
        }

        public void SetRecipient(PlayerLootReceiver value)
        {
            if (!granted && value != null) recipient = value;
        }

        private void GrantOnce()
        {
            if (granted) return;

            if (reward == null)
            {
                Debug.LogError("BossReward requires a LootBundleData reward before it can grant loot.", this);
                return;
            }

            if (recipient == null)
            {
                Debug.LogError("BossReward requires a PlayerLootReceiver recipient before it can grant loot.", this);
                return;
            }

            if (!recipient.TryReceive(reward, false, false))
            {
                Debug.LogError("BossReward failed to deliver its configured LootBundleData to the PlayerLootReceiver.", this);
                return;
            }

            granted = true;
        }
    }
}
