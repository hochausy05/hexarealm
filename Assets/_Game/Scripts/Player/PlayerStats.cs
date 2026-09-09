using System;
using UnityEngine;

namespace HexaRealm.Player
{
    public enum PlayerStatType
    {
        Vitality,
        Attack,
        Defense,
        Agility,
        Rage
    }

    [Serializable]
    public sealed class PlayerStatValue
    {
        [SerializeField] private float baseValue;
        [SerializeField] private float upgradeModifier;
        [SerializeField] private float equipmentModifier;
        [SerializeField, HideInInspector] private float minimumValue;

        public float BaseValue => baseValue;
        public float UpgradeModifier => upgradeModifier;
        public float EquipmentModifier => equipmentModifier;
        public float FinalValue => Mathf.Max(minimumValue, baseValue + upgradeModifier + equipmentModifier);

        public PlayerStatValue(float baseValue, float minimumValue)
        {
            this.baseValue = baseValue;
            this.minimumValue = minimumValue;
        }

        internal void SetMinimumValue(float value)
        {
            minimumValue = value;
        }

        internal void SetUpgradeModifier(float value)
        {
            upgradeModifier = value;
        }

        internal void SetEquipmentModifier(float value)
        {
            equipmentModifier = value;
        }
    }

    public sealed class PlayerStats : MonoBehaviour
    {
        private const float VitalityMinimum = 1f;
        private const float OtherStatMinimum = 0f;

        [Header("Prototype Base Stats (not final balance)")]
        [SerializeField] private PlayerStatValue vitality = new PlayerStatValue(100f, VitalityMinimum);
        [SerializeField] private PlayerStatValue attack = new PlayerStatValue(10f, OtherStatMinimum);
        [SerializeField] private PlayerStatValue defense = new PlayerStatValue(5f, OtherStatMinimum);
        [SerializeField] private PlayerStatValue agility = new PlayerStatValue(10f, OtherStatMinimum);
        [SerializeField] private PlayerStatValue rage = new PlayerStatValue(0f, OtherStatMinimum);

        public event Action<PlayerStatType, float, float> StatChanged;

        public float GetBaseStat(PlayerStatType type)
        {
            return GetStat(type).BaseValue;
        }

        public float GetUpgradeModifier(PlayerStatType type)
        {
            return GetStat(type).UpgradeModifier;
        }

        public float GetEquipmentModifier(PlayerStatType type)
        {
            return GetStat(type).EquipmentModifier;
        }

        public float GetFinalStat(PlayerStatType type)
        {
            return GetStat(type).FinalValue;
        }

        public void SetUpgradeModifier(PlayerStatType type, float value)
        {
            PlayerStatValue stat = GetStat(type);
            float oldFinalValue = stat.FinalValue;
            stat.SetUpgradeModifier(value);
            NotifyIfFinalValueChanged(type, oldFinalValue, stat.FinalValue);
        }

        public void SetEquipmentModifier(PlayerStatType type, float value)
        {
            PlayerStatValue stat = GetStat(type);
            float oldFinalValue = stat.FinalValue;
            stat.SetEquipmentModifier(value);
            NotifyIfFinalValueChanged(type, oldFinalValue, stat.FinalValue);
        }

        private void Awake()
        {
            EnsureValidStats();
        }

        private void OnValidate()
        {
            EnsureValidStats();
        }

        private PlayerStatValue GetStat(PlayerStatType type)
        {
            EnsureValidStats();

            switch (type)
            {
                case PlayerStatType.Vitality:
                    return vitality;
                case PlayerStatType.Attack:
                    return attack;
                case PlayerStatType.Defense:
                    return defense;
                case PlayerStatType.Agility:
                    return agility;
                case PlayerStatType.Rage:
                    return rage;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown player stat type.");
            }
        }

        private void EnsureValidStats()
        {
            vitality = EnsureStat(vitality, 100f, VitalityMinimum);
            attack = EnsureStat(attack, 10f, OtherStatMinimum);
            defense = EnsureStat(defense, 5f, OtherStatMinimum);
            agility = EnsureStat(agility, 10f, OtherStatMinimum);
            rage = EnsureStat(rage, 0f, OtherStatMinimum);
        }

        private static PlayerStatValue EnsureStat(PlayerStatValue stat, float defaultBaseValue, float minimumValue)
        {
            if (stat == null)
            {
                stat = new PlayerStatValue(defaultBaseValue, minimumValue);
            }

            stat.SetMinimumValue(minimumValue);
            return stat;
        }

        private void NotifyIfFinalValueChanged(PlayerStatType type, float oldFinalValue, float newFinalValue)
        {
            if (!Mathf.Approximately(oldFinalValue, newFinalValue))
            {
                StatChanged?.Invoke(type, oldFinalValue, newFinalValue);
            }
        }
    }
}
