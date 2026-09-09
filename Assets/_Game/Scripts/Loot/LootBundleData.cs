using HexaRealm.Equipment;
using UnityEngine;

namespace HexaRealm.Loot
{
    [CreateAssetMenu(fileName = "LootBundle", menuName = "HexaRealm/Loot/Loot Bundle")]
    public sealed class LootBundleData : ScriptableObject
    {
        [SerializeField, Min(0)] private int soulAmount;
        [SerializeField] private WeaponData weaponReward;
        [SerializeField] private ArmorData armorReward;

        public int SoulAmount => soulAmount;
        public WeaponData WeaponReward => weaponReward;
        public ArmorData ArmorReward => armorReward;
        public bool HasAnyReward => soulAmount > 0 || weaponReward != null || armorReward != null;

        // Supports isolated EditMode tests. Runtime loot transactions never mutate this asset.
        public void SetAuthoringValues(int newSoulAmount, WeaponData newWeaponReward = null, ArmorData newArmorReward = null)
        {
            soulAmount = Mathf.Max(0, newSoulAmount);
            weaponReward = newWeaponReward;
            armorReward = newArmorReward;
        }

        private void OnValidate()
        {
            soulAmount = Mathf.Max(0, soulAmount);
        }
    }
}
