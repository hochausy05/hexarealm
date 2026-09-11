using HexaRealm.Player;
using HexaRealm.Loot;
using UnityEngine;

namespace HexaRealm.Boss
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BossArena : MonoBehaviour
    {
        [SerializeField] private BossController boss;
        private void Awake() { GetComponent<Collider2D>().isTrigger = true; }
        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player == null) return;
            boss?.GetComponent<BossReward>()?.SetRecipient(player.GetComponent<PlayerLootReceiver>());
            boss?.Engage(player.transform);
        }
        private void OnTriggerExit2D(Collider2D other) { if (other.GetComponentInParent<PlayerHealth>() != null) boss?.Disengage(); }
    }
}
