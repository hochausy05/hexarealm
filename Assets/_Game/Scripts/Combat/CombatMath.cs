using UnityEngine;

namespace HexaRealm.Combat
{
    public static class CombatMath
    {
        // TECHNICAL SAFETY only. This prevents invalid timing and is not a balance cap.
        public const float MinimumAttackInterval = 0.001f;

        public static float CalculateRawDamage(float finalAttack, bool isCritical, float criticalMultiplier)
        {
            float attack = SanitizeNonNegative(finalAttack);

            if (!isCritical)
            {
                return attack;
            }

            float multiplier = IsFinite(criticalMultiplier)
                ? Mathf.Max(1f, criticalMultiplier)
                : 1f;
            double criticalDamage = (double)attack * multiplier;
            return criticalDamage >= float.MaxValue ? float.MaxValue : (float)criticalDamage;
        }

        public static float CalculateCriticalChance(
            float baseCriticalChance,
            float finalRage,
            float criticalChancePerRage)
        {
            double baseChance = IsFinite(baseCriticalChance) ? baseCriticalChance : 0f;
            double rage = SanitizeNonNegative(finalRage);
            double chancePerRage = SanitizeNonNegative(criticalChancePerRage);
            double chance = baseChance + rage * chancePerRage;

            if (double.IsNaN(chance))
            {
                return 0f;
            }

            return (float)System.Math.Max(0d, System.Math.Min(1d, chance));
        }

        public static float CalculateAttackInterval(
            float baseAttackInterval,
            float finalRage,
            float attackSpeedPerRage)
        {
            if (!IsFinite(baseAttackInterval) || baseAttackInterval <= 0f)
            {
                return MinimumAttackInterval;
            }

            double rage = SanitizeNonNegative(finalRage);
            double speedPerRage = SanitizeNonNegative(attackSpeedPerRage);
            double denominator = 1d + rage * speedPerRage;
            double interval = baseAttackInterval / denominator;

            if (double.IsNaN(interval) || double.IsInfinity(interval) || interval <= 0d)
            {
                return MinimumAttackInterval;
            }

            return Mathf.Max(MinimumAttackInterval, (float)interval);
        }

        private static float SanitizeNonNegative(float value)
        {
            return IsFinite(value) ? Mathf.Max(0f, value) : 0f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
