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
        private bool granted;
        private PlayerLootReceiver receiver;
        private void Awake() { if (bossHealth == null) bossHealth = GetComponent<BossHealth>(); }
        private void OnEnable() { if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.Died += GrantOnce; }
        private void OnDisable() { if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.Died -= GrantOnce; }
        private void GrantOnce()
        {
            if (granted || reward == null) return;
            if (receiver != null && receiver.TryReceive(reward, false, false)) granted = true;
        }

        public void SetRecipient(PlayerLootReceiver value)
        {
            if (!granted && value != null) receiver = value;
        }
    }
}
