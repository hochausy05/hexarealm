using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexaRealm.Equipment
{
    [CreateAssetMenu(fileName = "EquipmentCatalog", menuName = "HexaRealm/Equipment/Equipment Catalog")]
    public sealed class EquipmentCatalog : ScriptableObject
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
        [SerializeField] private List<ArmorData> armors = new List<ArmorData>();

        public IReadOnlyList<WeaponData> Weapons => weapons;
        public IReadOnlyList<ArmorData> Armors => armors;

        public bool TryCreateLookup(out EquipmentLookup lookup, out string errorMessage)
        {
            if (weapons == null || armors == null)
            {
                lookup = null;
                errorMessage = "Equipment catalog collections are missing.";
                return false;
            }

            Dictionary<string, WeaponData> weaponById = new Dictionary<string, WeaponData>(StringComparer.Ordinal);
            Dictionary<string, ArmorData> armorById = new Dictionary<string, ArmorData>(StringComparer.Ordinal);
            HashSet<string> allIds = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < weapons.Count; index++)
            {
                WeaponData weapon = weapons[index];
                if (weapon == null || string.IsNullOrWhiteSpace(weapon.PersistentId))
                {
                    lookup = null;
                    errorMessage = $"Weapon catalog entry {index} is null or has an empty persistent ID.";
                    return false;
                }

                if (!allIds.Add(weapon.PersistentId))
                {
                    lookup = null;
                    errorMessage = $"Duplicate equipment persistent ID '{weapon.PersistentId}'.";
                    return false;
                }

                weaponById.Add(weapon.PersistentId, weapon);
            }

            for (int index = 0; index < armors.Count; index++)
            {
                ArmorData armor = armors[index];
                if (armor == null || string.IsNullOrWhiteSpace(armor.PersistentId))
                {
                    lookup = null;
                    errorMessage = $"Armor catalog entry {index} is null or has an empty persistent ID.";
                    return false;
                }

                if (!allIds.Add(armor.PersistentId))
                {
                    lookup = null;
                    errorMessage = $"Duplicate equipment persistent ID '{armor.PersistentId}'.";
                    return false;
                }

                armorById.Add(armor.PersistentId, armor);
            }

            lookup = new EquipmentLookup(weaponById, armorById);
            errorMessage = string.Empty;
            return true;
        }

#if UNITY_EDITOR
        public void SetEntriesForAuthoring(IEnumerable<WeaponData> weaponEntries, IEnumerable<ArmorData> armorEntries)
        {
            weapons = weaponEntries != null ? new List<WeaponData>(weaponEntries) : new List<WeaponData>();
            armors = armorEntries != null ? new List<ArmorData>(armorEntries) : new List<ArmorData>();
        }
#endif
    }

    public sealed class EquipmentLookup
    {
        private readonly Dictionary<string, WeaponData> weaponById;
        private readonly Dictionary<string, ArmorData> armorById;

        internal EquipmentLookup(Dictionary<string, WeaponData> weaponById, Dictionary<string, ArmorData> armorById)
        {
            this.weaponById = weaponById;
            this.armorById = armorById;
        }

        public bool TryGetWeapon(string persistentId, out WeaponData weapon)
        {
            return weaponById.TryGetValue(persistentId ?? string.Empty, out weapon);
        }

        public bool TryGetArmor(string persistentId, out ArmorData armor)
        {
            return armorById.TryGetValue(persistentId ?? string.Empty, out armor);
        }
    }
}
