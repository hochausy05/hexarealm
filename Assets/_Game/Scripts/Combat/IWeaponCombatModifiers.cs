using UnityEngine;

namespace HexaRealm.Combat
{
    /// <summary>Read-only weapon values consumed by the combat formula layer.</summary>
    public interface IWeaponCombatModifiers
    {
        float AttackSpeedMultiplier { get; }
        float CritBonus { get; }
        float RangeBonus { get; }
        Sprite SlashVFXSprite { get; }
    }
}
