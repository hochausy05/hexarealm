using HexaRealm.Equipment;
using HexaRealm.Progression;
using UnityEngine;

namespace HexaRealm.Loot
{
    [DisallowMultipleComponent]
    public sealed class PlayerLootReceiver : MonoBehaviour
    {
        [SerializeField] private PlayerSoulWallet playerSoulWallet;
        [SerializeField] private PlayerEquipmentInventory equipmentInventory;
        [SerializeField] private PlayerEquipment playerEquipment;

        public bool CanReceive(LootBundleData loot)
        {
            if (loot == null || !loot.HasAnyReward) return false;
            bool needsEquipmentInventory = loot.WeaponReward != null || loot.ArmorReward != null;
            return (loot.SoulAmount == 0 || playerSoulWallet != null)
                && (!needsEquipmentInventory || equipmentInventory != null);
        }

        public bool TryReceive(LootBundleData loot, bool autoEquipWeapon, bool autoEquipArmor)
        {
            if (!CanReceive(loot)) return false;

            if (loot.SoulAmount > 0) playerSoulWallet.AddSouls(loot.SoulAmount);
            bool weaponAdded = loot.WeaponReward != null && equipmentInventory.AddWeapon(loot.WeaponReward);
            bool armorAdded = loot.ArmorReward != null && equipmentInventory.AddArmor(loot.ArmorReward);

            if (autoEquipWeapon && weaponAdded && playerEquipment != null) playerEquipment.EquipWeapon(loot.WeaponReward);
            if (autoEquipArmor && armorAdded && playerEquipment != null) playerEquipment.EquipArmor(loot.ArmorReward);
            return true;
        }

        private void Awake() => ResolveReferences();
        private void OnValidate() => ResolveReferences();

        private void ResolveReferences()
        {
            if (playerSoulWallet == null) playerSoulWallet = GetComponent<PlayerSoulWallet>();
            if (equipmentInventory == null) equipmentInventory = GetComponent<PlayerEquipmentInventory>();
            if (playerEquipment == null) playerEquipment = GetComponent<PlayerEquipment>();
        }
    }
}
