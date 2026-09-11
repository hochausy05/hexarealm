using System.Collections.Generic;
using HexaRealm.Loot;
using HexaRealm.Player;
using UnityEngine;

namespace HexaRealm.Boss
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class BossArena : MonoBehaviour
    {
        [SerializeField] private BossController boss;

        private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();
        private PlayerHealth activePlayer;

        private void Awake()
        {
            if (boss == null) boss = GetComponentInParent<BossController>();
            GetComponent<Collider2D>().isTrigger = true;
            if (boss == null)
            {
                Debug.LogError("BossArena requires a BossController.", this);
                enabled = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null) return;
            if (activePlayer == null && playerColliders.Count > 0) playerColliders.Clear();
            if (activePlayer != null && activePlayer != playerHealth) return;
            if (!playerColliders.Add(other)) return;
            if (activePlayer != null) return;

            activePlayer = playerHealth;
            BossReward reward = boss.GetComponent<BossReward>();
            if (reward != null) reward.SetRecipient(playerHealth.GetComponent<PlayerLootReceiver>());
            boss.Engage(playerHealth.transform);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!playerColliders.Remove(other) || playerColliders.Count > 0) return;
            activePlayer = null;
            boss.Disengage();
        }
    }
}
