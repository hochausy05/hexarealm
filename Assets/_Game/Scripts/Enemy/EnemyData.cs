using UnityEngine;

namespace HexaRealm.Enemy
{
    /// <summary>
    /// Authoring data for one regular enemy. Runtime systems should initialize from this asset
    /// instead of maintaining duplicate base-stat values.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "HexaRealm/Enemy Data")]
    public sealed class EnemyData : ScriptableObject
    {
        [SerializeField] private string displayName = "New Enemy";
        [SerializeField] private EnemyRank rank = EnemyRank.F;
        [SerializeField, Min(1f)] private float maxHealth = 1f;
        [SerializeField, Min(0f)] private float attack;
        [SerializeField, Min(0f)] private float defense;
        [SerializeField, Min(0f)] private float speed;
        [SerializeField, Min(0)] private int soulReward;

        public string DisplayName => displayName;
        public EnemyRank Rank => rank;
        public float MaxHealth => maxHealth;
        public float Attack => attack;
        public float Defense => defense;
        public float Speed => speed;
        public int SoulReward => soulReward;

        public bool IsValid => Validate() == EnemyDataValidationResult.Valid;

        public EnemyDataValidationResult Validate()
        {
            if (!IsFinite(maxHealth) || maxHealth < 1f)
            {
                return EnemyDataValidationResult.InvalidMaxHealth;
            }

            if (!IsFinite(attack) || attack < 0f)
            {
                return EnemyDataValidationResult.InvalidAttack;
            }

            if (!IsFinite(defense) || defense < 0f)
            {
                return EnemyDataValidationResult.InvalidDefense;
            }

            if (!IsFinite(speed) || speed < 0f)
            {
                return EnemyDataValidationResult.InvalidSpeed;
            }

            return soulReward < 0
                ? EnemyDataValidationResult.InvalidSoulReward
                : EnemyDataValidationResult.Valid;
        }

        // This explicit authoring API also makes in-memory data setup possible for EditMode tests.
        public void SetAuthoringValues(
            string newDisplayName,
            EnemyRank newRank,
            float newMaxHealth,
            float newAttack,
            float newDefense,
            float newSpeed,
            int newSoulReward)
        {
            displayName = newDisplayName;
            rank = newRank;
            maxHealth = newMaxHealth;
            attack = newAttack;
            defense = newDefense;
            speed = newSpeed;
            soulReward = newSoulReward;
        }

        private void OnValidate()
        {
            maxHealth = SanitizeAtLeast(maxHealth, 1f);
            attack = SanitizeAtLeast(attack, 0f);
            defense = SanitizeAtLeast(defense, 0f);
            speed = SanitizeAtLeast(speed, 0f);
            soulReward = Mathf.Max(0, soulReward);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float SanitizeAtLeast(float value, float minimum)
        {
            return IsFinite(value) ? Mathf.Max(minimum, value) : minimum;
        }
    }

    public enum EnemyDataValidationResult
    {
        Valid,
        InvalidMaxHealth,
        InvalidAttack,
        InvalidDefense,
        InvalidSpeed,
        InvalidSoulReward
    }
}
