using UnityEngine;

namespace HexaRealm.Combat
{
    public static class DamageCalculator
    {
        private const float MinimumPositiveDamage = 1f;

        public static float CalculateFinalDamage(float rawDamage, float defense)
        {
            if (rawDamage <= 0f || float.IsNaN(rawDamage))
            {
                return 0f;
            }

            float clampedDefense = float.IsNaN(defense) ? 0f : Mathf.Max(0f, defense);
            return Mathf.Max(MinimumPositiveDamage, rawDamage - clampedDefense);
        }
    }
}
