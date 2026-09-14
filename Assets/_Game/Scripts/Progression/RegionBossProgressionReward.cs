using System;
using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Progression
{
    /// <summary>Adapts a boss death to its region reward without owning boss combat or player stat state.</summary>
    [RequireComponent(typeof(Health))]
    public sealed class RegionBossProgressionReward : MonoBehaviour
    {
        [SerializeField] private Health bossHealth;
        [SerializeField] private RegionProgressionRewardData rewardData;
        [SerializeField] private PlayerRegionProgression playerProgression;
        [SerializeField] private PlayerUpgradeProgression playerUpgradeProgression;

        private bool granted;
        private bool configurationErrorLogged;

        public static event Action<RegionBossProgressionReward> AnyProgressionCompleted;
        public RegionId CompletedRegion => rewardData != null ? rewardData.CompletedRegion : default;
        public bool HasValidRewardData => rewardData != null && rewardData.IsValid;

        private void Awake()
        {
            if (bossHealth == null) bossHealth = GetComponent<Health>();
            ValidateStaticConfiguration();
            ResolveRecipientReferences();
        }

        private void OnEnable()
        {
            if (bossHealth == null) bossHealth = GetComponent<Health>();
            ResolveRecipientReferences();
            if (bossHealth != null) bossHealth.Died += GrantOnce;
        }

        private void OnDisable()
        {
            if (bossHealth != null) bossHealth.Died -= GrantOnce;
        }

        public void SetRecipient(PlayerRegionProgression progression)
        {
            if (granted) return;
            if (progression == null)
            {
                LogConfigurationError("RegionBossProgressionReward could not find PlayerRegionProgression on the arena player.");
                return;
            }
            playerProgression = progression;
            playerUpgradeProgression = progression.GetComponent<PlayerUpgradeProgression>();
            ValidateEncounterConfiguration();
        }

        private void GrantOnce()
        {
            if (granted || !ValidateEncounterConfiguration()) return;

            playerProgression.GrantRegionCompletion(rewardData.CompletedRegion, rewardData.TeleportStoneRegion);
            playerUpgradeProgression.IncreaseUpgradeCapTo(rewardData.UnlockedUpgradeCap);
            granted = true;
            Debug.Log($"Region complete: {rewardData.CompletedRegion}. Teleport Stone acquired; Upgrade Cap: {playerUpgradeProgression.CurrentUpgradeCap}.", this);
            AnyProgressionCompleted?.Invoke(this);
        }

        /// <summary>Applies the world consequence of persisted completion without replaying rewards.</summary>
        public void RestoreCompletedWorldState()
        {
            granted = true;
            gameObject.SetActive(false);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents()
        {
            AnyProgressionCompleted = null;
        }

        private bool ValidateStaticConfiguration()
        {
            if (bossHealth != null && rewardData != null && rewardData.IsValid) return true;
            LogConfigurationError("RegionBossProgressionReward requires Health and a valid RegionProgressionRewardData.");
            return false;
        }

        private bool ValidateEncounterConfiguration()
        {
            if (!ValidateStaticConfiguration()) return false;
            ResolveRecipientReferences();
            if (playerProgression == null)
            {
                LogConfigurationError("RegionBossProgressionReward could not resolve the active PlayerRegionProgression before the boss reward was granted.");
                return false;
            }

            PlayerUpgradeProgression samePlayerUpgrades = playerProgression.GetComponent<PlayerUpgradeProgression>();
            if (playerUpgradeProgression == null || playerUpgradeProgression != samePlayerUpgrades)
            {
                LogConfigurationError("RegionBossProgressionReward requires PlayerUpgradeProgression on the same Player object as PlayerRegionProgression.");
                return false;
            }

            return true;
        }

        private void ResolveRecipientReferences()
        {
            if (playerProgression == null && playerUpgradeProgression != null)
            {
                playerProgression = playerUpgradeProgression.GetComponent<PlayerRegionProgression>();
            }

            if (playerProgression == null)
            {
                playerProgression = FindAnyObjectByType<PlayerRegionProgression>();
            }

            if (playerUpgradeProgression == null && playerProgression != null)
            {
                playerUpgradeProgression = playerProgression.GetComponent<PlayerUpgradeProgression>();
            }
        }

        private void LogConfigurationError(string message)
        {
            if (configurationErrorLogged) return;
            configurationErrorLogged = true;
            Debug.LogError(message, this);
        }
    }
}
