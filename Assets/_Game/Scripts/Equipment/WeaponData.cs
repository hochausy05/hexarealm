using UnityEngine;

namespace HexaRealm.Equipment
{
    /// <summary>Shared authoring data for a melee-slash weapon.</summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "HexaRealm/Equipment/Weapon Data")]
    public sealed class WeaponData : ScriptableObject
    {
        [SerializeField] private string displayName = "New Weapon";
        [SerializeField, Min(0f)] private float damage;
        [SerializeField] private float attackSpeedMultiplier = 1f;
        [SerializeField, Min(0f)] private float critBonus;
        [SerializeField, Min(0f)] private float rangeBonus;
        [SerializeField] private Sprite weaponSprite;
        [SerializeField] private Sprite slashVFXSprite;

        public string DisplayName => displayName;
        public float Damage => damage;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;
        public float CritBonus => critBonus;
        public float RangeBonus => rangeBonus;
        public Sprite WeaponSprite => weaponSprite;
        public Sprite SlashVFXSprite => slashVFXSprite;

        // This authoring helper supports isolated EditMode tests; gameplay never mutates a WeaponData.
        public void SetAuthoringValues(
            string newDisplayName,
            float newDamage,
            float newAttackSpeedMultiplier,
            float newCritBonus,
            float newRangeBonus,
            Sprite newWeaponSprite = null,
            Sprite newSlashVFXSprite = null)
        {
            displayName = newDisplayName;
            damage = newDamage;
            attackSpeedMultiplier = newAttackSpeedMultiplier;
            critBonus = newCritBonus;
            rangeBonus = newRangeBonus;
            weaponSprite = newWeaponSprite;
            slashVFXSprite = newSlashVFXSprite;
        }

        private void OnValidate()
        {
            damage = SanitizeNonNegative(damage);
            attackSpeedMultiplier = IsValidAttackSpeedMultiplier(attackSpeedMultiplier)
                ? attackSpeedMultiplier
                : 1f;
            critBonus = SanitizeNonNegative(critBonus);
            rangeBonus = SanitizeNonNegative(rangeBonus);
        }

        private static bool IsValidAttackSpeedMultiplier(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }

        private static float SanitizeNonNegative(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) ? Mathf.Max(0f, value) : 0f;
        }
    }
}
