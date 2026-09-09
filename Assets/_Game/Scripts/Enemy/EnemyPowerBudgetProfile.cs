using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexaRealm.Enemy
{
    /// <summary>
    /// Optional, data-driven tuning for enemy power budgets. It deliberately ships disabled so
    /// rank thresholds are not implied before balancing and playtests define them.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyPowerBudgetProfile", menuName = "HexaRealm/Enemy Power Budget Profile")]
    public sealed class EnemyPowerBudgetProfile : ScriptableObject
    {
        [SerializeField] private bool validationEnabled;
        [SerializeField, Min(0f)] private float healthWeight = 1f;
        [SerializeField, Min(0f)] private float attackWeight = 1f;
        [SerializeField, Min(0f)] private float defenseWeight = 1f;
        [SerializeField, Min(0f)] private float speedWeight = 1f;
        [SerializeField] private List<EnemyRankBudgetRange> rankBudgetRanges = new List<EnemyRankBudgetRange>();

        public bool ValidationEnabled => validationEnabled;
        public float HealthWeight => healthWeight;
        public float AttackWeight => attackWeight;
        public float DefenseWeight => defenseWeight;
        public float SpeedWeight => speedWeight;

        public bool HasValidScoreWeights =>
            IsFiniteAndAtLeast(healthWeight, 0f) &&
            IsFiniteAndAtLeast(attackWeight, 0f) &&
            IsFiniteAndAtLeast(defenseWeight, 0f) &&
            IsFiniteAndAtLeast(speedWeight, 0f);

        public bool HasValidValidationWeights =>
            IsFiniteAndGreaterThanZero(healthWeight) &&
            IsFiniteAndGreaterThanZero(attackWeight) &&
            IsFiniteAndGreaterThanZero(defenseWeight) &&
            IsFiniteAndGreaterThanZero(speedWeight);

        public bool IsValidationConfiguredFor(EnemyRank rank)
        {
            return validationEnabled &&
                HasValidValidationWeights &&
                TryGetRankBudgetRange(rank, out _);
        }

        public bool TryGetRankBudgetRange(EnemyRank rank, out EnemyRankBudgetRange result)
        {
            result = default;
            bool found = false;

            foreach (EnemyRankBudgetRange range in rankBudgetRanges)
            {
                if (range.Rank != rank)
                {
                    continue;
                }

                if (found || !range.IsValid)
                {
                    return false;
                }

                result = range;
                found = true;
            }

            return found;
        }

        // Supports explicit configuration in tools and tests without embedding balance values in code.
        public void SetConfiguration(
            bool enabled,
            float newHealthWeight,
            float newAttackWeight,
            float newDefenseWeight,
            float newSpeedWeight,
            params EnemyRankBudgetRange[] ranges)
        {
            validationEnabled = enabled;
            healthWeight = newHealthWeight;
            attackWeight = newAttackWeight;
            defenseWeight = newDefenseWeight;
            speedWeight = newSpeedWeight;
            rankBudgetRanges = ranges == null
                ? new List<EnemyRankBudgetRange>()
                : new List<EnemyRankBudgetRange>(ranges);
        }

        private static bool IsFiniteAndAtLeast(float value, float minimum)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= minimum;
        }

        private static bool IsFiniteAndGreaterThanZero(float value)
        {
            return IsFiniteAndAtLeast(value, 0f) && value > 0f;
        }
    }

    [Serializable]
    public struct EnemyRankBudgetRange
    {
        [SerializeField] private EnemyRank rank;
        [SerializeField] private float minimumScore;
        [SerializeField] private float maximumScore;

        public EnemyRank Rank => rank;
        public float MinimumScore => minimumScore;
        public float MaximumScore => maximumScore;
        public bool IsValid =>
            IsFinite(minimumScore) &&
            IsFinite(maximumScore) &&
            minimumScore >= 0f &&
            maximumScore >= minimumScore;

        public EnemyRankBudgetRange(EnemyRank rank, float minimumScore, float maximumScore)
        {
            this.rank = rank;
            this.minimumScore = minimumScore;
            this.maximumScore = maximumScore;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
