using System.Collections.Generic;
using UnityEngine;

namespace HexaRealm.Equipment
{
    /// <summary>Runtime ownership for equipment definitions; this is intentionally not a general inventory.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerEquipmentInventory : MonoBehaviour
    {
        [SerializeField] private PlayerEquipment playerEquipment;
        [SerializeField] private List<WeaponData> ownedWeapons = new List<WeaponData>();
        [SerializeField] private List<ArmorData> ownedArmors = new List<ArmorData>();
        private bool hasRegisteredStartingEquipment;

        public IReadOnlyList<WeaponData> OwnedWeapons => ownedWeapons;
        public IReadOnlyList<ArmorData> OwnedArmors => ownedArmors;

        private void Awake()
        {
            ResolveReferences();
            RegisterStartingEquipment();
        }

        private void OnEnable()
        {
            ResolveReferences();
            RegisterStartingEquipment();
        }

        public bool OwnsWeapon(WeaponData weapon) => weapon != null && ownedWeapons.Contains(weapon);
        public bool OwnsArmor(ArmorData armor) => armor != null && ownedArmors.Contains(armor);

        public bool AddWeapon(WeaponData weapon)
        {
            if (weapon == null || OwnsWeapon(weapon)) return false;
            ownedWeapons.Add(weapon);
            return true;
        }

        public bool AddArmor(ArmorData armor)
        {
            if (armor == null || OwnsArmor(armor)) return false;
            ownedArmors.Add(armor);
            return true;
        }

        public void RegisterStartingEquipment()
        {
            if (playerEquipment == null || hasRegisteredStartingEquipment) return;
            AddWeapon(playerEquipment.StartingWeapon);
            AddArmor(playerEquipment.StartingArmor);
            hasRegisteredStartingEquipment = true;
        }

        /// <summary>Replaces ownership as unique set-like state without invoking loot/reward behavior.</summary>
        public void RestoreOwnership(IEnumerable<WeaponData> weapons, IEnumerable<ArmorData> armors)
        {
            ownedWeapons.Clear();
            ownedArmors.Clear();
            hasRegisteredStartingEquipment = true;

            if (weapons != null)
            {
                foreach (WeaponData weapon in weapons) AddWeapon(weapon);
            }

            if (armors != null)
            {
                foreach (ArmorData armor in armors) AddArmor(armor);
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
            RemoveInvalidOrDuplicateEntries(ownedWeapons);
            RemoveInvalidOrDuplicateEntries(ownedArmors);
        }

        private void ResolveReferences()
        {
            if (playerEquipment == null) playerEquipment = GetComponent<PlayerEquipment>();
        }

        private static void RemoveInvalidOrDuplicateEntries<T>(List<T> items) where T : Object
        {
            for (int index = items.Count - 1; index >= 0; index--)
            {
                if (items[index] == null || items.IndexOf(items[index]) != index) items.RemoveAt(index);
            }
        }
    }
}
