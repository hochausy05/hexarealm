using UnityEngine;

namespace HexaRealm.Boss
{
    [CreateAssetMenu(fileName = "Boss", menuName = "HexaRealm/Boss/Boss Data")]
    public sealed class BossData : ScriptableObject
    {
        [SerializeField] private string displayName = "Optional Guardian";
        [SerializeField, Min(1f)] private float maxHealth = 250f;
        [SerializeField, Min(0f)] private float attack = 16f;
        [SerializeField, Min(0f)] private float defense = 3f;
        [SerializeField, Min(0f)] private float moveSpeed = 2.6f;

        public string DisplayName => displayName;
        public float MaxHealth => maxHealth;
        public float Attack => attack;
        public float Defense => defense;
        public float MoveSpeed => moveSpeed;
        public bool IsValid => maxHealth > 0f && attack >= 0f && defense >= 0f && moveSpeed >= 0f;
    }
}
