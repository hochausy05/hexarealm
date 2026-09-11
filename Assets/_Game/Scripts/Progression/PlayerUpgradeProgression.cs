using System;
using HexaRealm.Player;
using UnityEngine;

namespace HexaRealm.Progression
{
    public enum UpgradePurchaseResult
    {
        Success,
        CapReached,
        InsufficientSouls,
        InvalidStat,
        InvalidConfiguration
    }

    [RequireComponent(typeof(PlayerStats), typeof(PlayerSoulWallet))]
    public sealed class PlayerUpgradeProgression : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerSoulWallet soulWallet;

        [Header("Prototype Upgrade Cap (not final balance)")]
        [SerializeField, Min(0)] private int currentUpgradeCap = 10;

        [Header("Prototype Soul Cost (not final balance)")]
        [SerializeField, Min(1)] private int baseSoulCost = 1;
        [SerializeField, Min(0)] private int costGrowthPerUpgrade = 1;

        [Header("Prototype Stat Value Per Upgrade (not final balance)")]
        [SerializeField, Min(0f)] private float vitalityPerUpgrade = 1f;
        [SerializeField, Min(0f)] private float attackPerUpgrade = 1f;
        [SerializeField, Min(0f)] private float defensePerUpgrade = 1f;
        [SerializeField, Min(0f)] private float agilityPerUpgrade = 1f;
        [SerializeField, Min(0f)] private float ragePerUpgrade = 1f;

        [Header("Runtime Upgrade Counts")]
        [SerializeField, Min(0)] private int vitalityUpgradeCount;
        [SerializeField, Min(0)] private int attackUpgradeCount;
        [SerializeField, Min(0)] private int defenseUpgradeCount;
        [SerializeField, Min(0)] private int agilityUpgradeCount;
        [SerializeField, Min(0)] private int rageUpgradeCount;

        public event Action<PlayerStatType> UpgradePurchased;

        public PlayerStats PlayerStats => playerStats;
        public PlayerSoulWallet SoulWallet => soulWallet;
        public int CurrentUpgradeCap => currentUpgradeCap;
        public int TotalUpgradeCount => SaturatingTotal(vitalityUpgradeCount, attackUpgradeCount, defenseUpgradeCount, agilityUpgradeCount, rageUpgradeCount);
        public int NextUpgradeCost => SoulUpgradeCostCalculator.CalculateNextCost(baseSoulCost, costGrowthPerUpgrade, TotalUpgradeCount);

        private void Awake()
        {
            ResolveReferences();
            SyncUpgradeModifiers();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SyncUpgradeModifiers();
        }

        private void OnValidate()
        {
            currentUpgradeCap = Mathf.Max(0, currentUpgradeCap);
            baseSoulCost = Mathf.Max(1, baseSoulCost);
            costGrowthPerUpgrade = Mathf.Max(0, costGrowthPerUpgrade);
            vitalityUpgradeCount = Mathf.Max(0, vitalityUpgradeCount);
            attackUpgradeCount = Mathf.Max(0, attackUpgradeCount);
            defenseUpgradeCount = Mathf.Max(0, defenseUpgradeCount);
            agilityUpgradeCount = Mathf.Max(0, agilityUpgradeCount);
            rageUpgradeCount = Mathf.Max(0, rageUpgradeCount);
        }

        public int GetUpgradeCount(PlayerStatType stat)
        {
            switch (stat)
            {
                case PlayerStatType.Vitality: return vitalityUpgradeCount;
                case PlayerStatType.Attack: return attackUpgradeCount;
                case PlayerStatType.Defense: return defenseUpgradeCount;
                case PlayerStatType.Agility: return agilityUpgradeCount;
                case PlayerStatType.Rage: return rageUpgradeCount;
                default: throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown player stat type.");
            }
        }

        public float GetValuePerUpgrade(PlayerStatType stat)
        {
            switch (stat)
            {
                case PlayerStatType.Vitality: return vitalityPerUpgrade;
                case PlayerStatType.Attack: return attackPerUpgrade;
                case PlayerStatType.Defense: return defensePerUpgrade;
                case PlayerStatType.Agility: return agilityPerUpgrade;
                case PlayerStatType.Rage: return ragePerUpgrade;
                default: throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown player stat type.");
            }
        }

        public bool IncreaseUpgradeCapTo(int newCap)
        {
            if (newCap < currentUpgradeCap)
            {
                return false;
            }

            currentUpgradeCap = newCap;
            return true;
        }

        /// <summary>Unlocks an absolute cap value. Existing upgrades, stats, and Soul remain unchanged.</summary>
        public bool UnlockUpgradeCap(int newCap) => IncreaseUpgradeCapTo(newCap);

        public UpgradePurchaseResult TryPurchaseUpgrade(PlayerStatType stat)
        {
            if (!IsKnownStat(stat))
            {
                return UpgradePurchaseResult.InvalidStat;
            }

            ResolveReferences();
            if (!HasValidConfiguration())
            {
                return UpgradePurchaseResult.InvalidConfiguration;
            }

            if (TotalUpgradeCount >= currentUpgradeCap)
            {
                return UpgradePurchaseResult.CapReached;
            }

            int cost = NextUpgradeCost;
            if (!soulWallet.CanAfford(cost))
            {
                return UpgradePurchaseResult.InsufficientSouls;
            }

            if (!soulWallet.TrySpendSouls(cost))
            {
                return UpgradePurchaseResult.InsufficientSouls;
            }

            IncrementCount(stat);
            SyncUpgradeModifier(stat);
            UpgradePurchased?.Invoke(stat);
            return UpgradePurchaseResult.Success;
        }

        /// <summary>Rebuilds aggregate modifiers from counts; safe to call repeatedly.</summary>
        public void SyncUpgradeModifiers()
        {
            ResolveReferences();
            if (playerStats == null || !HasValidStatValues())
            {
                return;
            }

            SyncUpgradeModifier(PlayerStatType.Vitality);
            SyncUpgradeModifier(PlayerStatType.Attack);
            SyncUpgradeModifier(PlayerStatType.Defense);
            SyncUpgradeModifier(PlayerStatType.Agility);
            SyncUpgradeModifier(PlayerStatType.Rage);
        }

        private void SyncUpgradeModifier(PlayerStatType stat)
        {
            playerStats.SetUpgradeModifier(stat, CalculateUpgradeModifier(GetUpgradeCount(stat), GetValuePerUpgrade(stat)));
        }

        private bool HasValidConfiguration()
        {
            return playerStats != null && soulWallet != null && currentUpgradeCap >= 0 && baseSoulCost >= 1 &&
                   costGrowthPerUpgrade >= 0 && HasValidStatValues();
        }

        private bool HasValidStatValues()
        {
            return IsValidUpgradeValue(vitalityPerUpgrade) && IsValidUpgradeValue(attackPerUpgrade) &&
                   IsValidUpgradeValue(defensePerUpgrade) && IsValidUpgradeValue(agilityPerUpgrade) &&
                   IsValidUpgradeValue(ragePerUpgrade);
        }

        private static bool IsValidUpgradeValue(float value)
        {
            return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float CalculateUpgradeModifier(int count, float valuePerUpgrade)
        {
            double result = Math.Max(0, count) * (double)valuePerUpgrade;
            return result >= float.MaxValue ? float.MaxValue : (float)result;
        }

        private void IncrementCount(PlayerStatType stat)
        {
            switch (stat)
            {
                case PlayerStatType.Vitality: vitalityUpgradeCount++; break;
                case PlayerStatType.Attack: attackUpgradeCount++; break;
                case PlayerStatType.Defense: defenseUpgradeCount++; break;
                case PlayerStatType.Agility: agilityUpgradeCount++; break;
                case PlayerStatType.Rage: rageUpgradeCount++; break;
            }
        }

        private void ResolveReferences()
        {
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (soulWallet == null) soulWallet = GetComponent<PlayerSoulWallet>();
        }

        private static bool IsKnownStat(PlayerStatType stat)
        {
            return stat == PlayerStatType.Vitality || stat == PlayerStatType.Attack || stat == PlayerStatType.Defense ||
                   stat == PlayerStatType.Agility || stat == PlayerStatType.Rage;
        }

        private static int SaturatingTotal(int vitality, int attack, int defense, int agility, int rage)
        {
            long total = (long)Math.Max(0, vitality) + Math.Max(0, attack) + Math.Max(0, defense) + Math.Max(0, agility) + Math.Max(0, rage);
            return total >= int.MaxValue ? int.MaxValue : (int)total;
        }
    }
}
