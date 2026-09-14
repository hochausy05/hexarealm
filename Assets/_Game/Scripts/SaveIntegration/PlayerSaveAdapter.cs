using System;
using System.Collections.Generic;
using HexaRealm.Combat;
using HexaRealm.Equipment;
using HexaRealm.Player;
using HexaRealm.Progression;
using HexaRealm.Save;
using UnityEngine;

namespace HexaRealm.SaveIntegration
{
    /// <summary>Captures and restores authoritative player state without replaying gameplay actions.</summary>
    public sealed class PlayerSaveAdapter
    {
        internal sealed class PlayerComponents
        {
            public PlayerStats Stats;
            public PlayerHealth PlayerHealth;
            public Health Health;
            public PlayerSoulWallet Wallet;
            public PlayerUpgradeProgression Upgrades;
            public PlayerEquipment Equipment;
            public PlayerEquipmentInventory Inventory;
            public PlayerRegionProgression Regions;
        }

        public sealed class RestorePlan
        {
            internal PlayerComponents Components;
            internal int Souls;
            internal int VitalityCount;
            internal int AttackCount;
            internal int DefenseCount;
            internal int AgilityCount;
            internal int RageCount;
            internal int UpgradeCap;
            internal readonly List<WeaponData> OwnedWeapons = new List<WeaponData>();
            internal readonly List<ArmorData> OwnedArmors = new List<ArmorData>();
            internal WeaponData EquippedWeapon;
            internal ArmorData EquippedArmor;
            internal readonly List<RegionId> CompletedRegions = new List<RegionId>();
            internal readonly List<RegionId> TeleportStoneRegions = new List<RegionId>();
        }

        private readonly EquipmentCatalog catalog;

        public PlayerSaveAdapter(EquipmentCatalog catalog)
        {
            this.catalog = catalog;
        }

        public bool TryCapture(GameObject playerRoot, out GameSaveData data, out string errorMessage)
        {
            data = null;
            if (!TryResolveComponents(playerRoot, out PlayerComponents components, out errorMessage)) return false;
            if (!TryCreateLookup(out EquipmentLookup lookup, out errorMessage)) return false;

            GameSaveData captured = new GameSaveData();
            captured.metadata.hasPlayerProgressionEquipmentState = true;
            captured.player.souls = components.Wallet.CurrentSouls;
            captured.progression.vitalityUpgradeCount = components.Upgrades.GetUpgradeCount(PlayerStatType.Vitality);
            captured.progression.attackUpgradeCount = components.Upgrades.GetUpgradeCount(PlayerStatType.Attack);
            captured.progression.defenseUpgradeCount = components.Upgrades.GetUpgradeCount(PlayerStatType.Defense);
            captured.progression.agilityUpgradeCount = components.Upgrades.GetUpgradeCount(PlayerStatType.Agility);
            captured.progression.rageUpgradeCount = components.Upgrades.GetUpgradeCount(PlayerStatType.Rage);
            captured.progression.upgradeCap = components.Upgrades.CurrentUpgradeCap;

            if (!TryCaptureOwnedWeapons(components.Inventory.OwnedWeapons, lookup, captured.equipment.ownedWeaponIds, out errorMessage) ||
                !TryCaptureOwnedArmors(components.Inventory.OwnedArmors, lookup, captured.equipment.ownedArmorIds, out errorMessage) ||
                !TryCaptureEquippedWeapon(components, lookup, captured, out errorMessage) ||
                !TryCaptureEquippedArmor(components, lookup, captured, out errorMessage))
            {
                return false;
            }

            foreach (RegionId region in components.Regions.CompletedRegions)
            {
                captured.progression.completedRegions.Add((int)region);
            }

            foreach (RegionId region in components.Regions.TeleportStoneRegions)
            {
                captured.progression.teleportStoneRegions.Add((int)region);
            }

            data = captured;
            errorMessage = string.Empty;
            return true;
        }

        public bool TryRestore(
            GameObject playerRoot,
            GameSaveData data,
            out IReadOnlyList<string> warnings,
            out string errorMessage)
        {
            List<string> warningList = new List<string>();
            warnings = warningList;

            if (!TryPrepareRestore(playerRoot, data, warningList, out RestorePlan plan, out errorMessage))
            {
                return false;
            }

            ApplyRestore(plan);
            errorMessage = string.Empty;
            return true;
        }

        public bool TryPrepareRestore(
            GameObject playerRoot,
            GameSaveData data,
            List<string> warnings,
            out RestorePlan plan,
            out string errorMessage)
        {
            if (warnings == null)
            {
                plan = null;
                errorMessage = "A warning destination is required.";
                return false;
            }

            return TryBuildRestorePlan(playerRoot, data, warnings, out plan, out errorMessage);
        }

        public void ApplyRestore(RestorePlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            // Deterministic replace order: upgrades/cap -> ownership -> equipped slots/modifiers ->
            // Soul -> regions/stones -> full-health playable state.
            plan.Components.Upgrades.RestoreState(
                plan.VitalityCount,
                plan.AttackCount,
                plan.DefenseCount,
                plan.AgilityCount,
                plan.RageCount,
                plan.UpgradeCap);
            plan.Components.Inventory.RestoreOwnership(plan.OwnedWeapons, plan.OwnedArmors);
            plan.Components.Equipment.RestoreEquipment(plan.EquippedWeapon, plan.EquippedArmor);
            plan.Components.Upgrades.SyncUpgradeModifiers();
            plan.Components.Wallet.RestoreExactSouls(plan.Souls);
            plan.Components.Regions.RestoreState(plan.CompletedRegions, plan.TeleportStoneRegions);
            plan.Components.Health.ResetToMaxHealth(plan.Components.Stats.GetFinalStat(PlayerStatType.Vitality));
        }

        private bool TryBuildRestorePlan(
            GameObject playerRoot,
            GameSaveData data,
            List<string> warnings,
            out RestorePlan plan,
            out string errorMessage)
        {
            plan = null;
            if (data == null)
            {
                errorMessage = "Save data is null.";
                return false;
            }

            data.EnsureSections();
            if (!data.metadata.hasPlayerProgressionEquipmentState)
            {
                errorMessage = "This save does not contain Task 22.2 player/progression/equipment state.";
                return false;
            }

            if (!TryResolveComponents(playerRoot, out PlayerComponents components, out errorMessage)) return false;
            if (!TryCreateLookup(out EquipmentLookup lookup, out errorMessage)) return false;

            RestorePlan candidate = new RestorePlan
            {
                Components = components,
                Souls = Mathf.Max(0, data.player.souls),
                VitalityCount = Mathf.Max(0, data.progression.vitalityUpgradeCount),
                AttackCount = Mathf.Max(0, data.progression.attackUpgradeCount),
                DefenseCount = Mathf.Max(0, data.progression.defenseUpgradeCount),
                AgilityCount = Mathf.Max(0, data.progression.agilityUpgradeCount),
                RageCount = Mathf.Max(0, data.progression.rageUpgradeCount),
                UpgradeCap = Mathf.Max(0, data.progression.upgradeCap)
            };

            ResolveOwnedWeapons(data.equipment.ownedWeaponIds, lookup, candidate.OwnedWeapons, warnings);
            ResolveOwnedArmors(data.equipment.ownedArmorIds, lookup, candidate.OwnedArmors, warnings);
            candidate.EquippedWeapon = ResolveEquippedWeapon(
                data.equipment.equippedWeaponId, lookup, candidate.OwnedWeapons, warnings);
            candidate.EquippedArmor = ResolveEquippedArmor(
                data.equipment.equippedArmorId, lookup, candidate.OwnedArmors, warnings);
            ResolveRegions(data.progression.completedRegions, "completed Region", candidate.CompletedRegions, warnings);
            ResolveRegions(data.progression.teleportStoneRegions, "Teleport Stone Region", candidate.TeleportStoneRegions, warnings);

            plan = candidate;
            errorMessage = string.Empty;
            return true;
        }

        private bool TryCreateLookup(out EquipmentLookup lookup, out string errorMessage)
        {
            if (catalog == null)
            {
                lookup = null;
                errorMessage = "EquipmentCatalog is missing.";
                return false;
            }

            return catalog.TryCreateLookup(out lookup, out errorMessage);
        }

        private static bool TryResolveComponents(GameObject root, out PlayerComponents components, out string errorMessage)
        {
            components = null;
            if (root == null)
            {
                errorMessage = "Player root was not found.";
                return false;
            }

            PlayerComponents candidate = new PlayerComponents
            {
                Stats = root.GetComponent<PlayerStats>(),
                PlayerHealth = root.GetComponent<PlayerHealth>(),
                Health = root.GetComponent<Health>(),
                Wallet = root.GetComponent<PlayerSoulWallet>(),
                Upgrades = root.GetComponent<PlayerUpgradeProgression>(),
                Equipment = root.GetComponent<PlayerEquipment>(),
                Inventory = root.GetComponent<PlayerEquipmentInventory>(),
                Regions = root.GetComponent<PlayerRegionProgression>()
            };

            if (candidate.Stats == null || candidate.PlayerHealth == null || candidate.Health == null ||
                candidate.Wallet == null || candidate.Upgrades == null || candidate.Equipment == null ||
                candidate.Inventory == null || candidate.Regions == null)
            {
                errorMessage = "Player is missing one or more required save components: PlayerStats, PlayerHealth, Health, " +
                               "PlayerSoulWallet, PlayerUpgradeProgression, PlayerEquipment, PlayerEquipmentInventory, " +
                               "or PlayerRegionProgression.";
                return false;
            }

            components = candidate;
            errorMessage = string.Empty;
            return true;
        }

        private static bool TryCaptureOwnedWeapons(
            IReadOnlyList<WeaponData> weapons,
            EquipmentLookup lookup,
            List<string> destination,
            out string errorMessage)
        {
            HashSet<string> uniqueIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < weapons.Count; index++)
            {
                WeaponData weapon = weapons[index];
                if (!TryValidateCatalogedWeapon(weapon, lookup, out errorMessage)) return false;
                if (uniqueIds.Add(weapon.PersistentId)) destination.Add(weapon.PersistentId);
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool TryCaptureOwnedArmors(
            IReadOnlyList<ArmorData> armors,
            EquipmentLookup lookup,
            List<string> destination,
            out string errorMessage)
        {
            HashSet<string> uniqueIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < armors.Count; index++)
            {
                ArmorData armor = armors[index];
                if (!TryValidateCatalogedArmor(armor, lookup, out errorMessage)) return false;
                if (uniqueIds.Add(armor.PersistentId)) destination.Add(armor.PersistentId);
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool TryCaptureEquippedWeapon(
            PlayerComponents components,
            EquipmentLookup lookup,
            GameSaveData data,
            out string errorMessage)
        {
            WeaponData weapon = components.Equipment.EquippedWeapon;
            if (weapon == null)
            {
                data.equipment.equippedWeaponId = string.Empty;
                errorMessage = string.Empty;
                return true;
            }

            if (!components.Inventory.OwnsWeapon(weapon))
            {
                errorMessage = $"Equipped Weapon ID '{weapon.PersistentId}' is not owned by the player.";
                return false;
            }

            if (!TryValidateCatalogedWeapon(weapon, lookup, out errorMessage)) return false;
            data.equipment.equippedWeaponId = weapon.PersistentId;
            return true;
        }

        private static bool TryCaptureEquippedArmor(
            PlayerComponents components,
            EquipmentLookup lookup,
            GameSaveData data,
            out string errorMessage)
        {
            ArmorData armor = components.Equipment.EquippedArmor;
            if (armor == null)
            {
                data.equipment.equippedArmorId = string.Empty;
                errorMessage = string.Empty;
                return true;
            }

            if (!components.Inventory.OwnsArmor(armor))
            {
                errorMessage = $"Equipped Armor ID '{armor.PersistentId}' is not owned by the player.";
                return false;
            }

            if (!TryValidateCatalogedArmor(armor, lookup, out errorMessage)) return false;
            data.equipment.equippedArmorId = armor.PersistentId;
            return true;
        }

        private static bool TryValidateCatalogedWeapon(WeaponData weapon, EquipmentLookup lookup, out string errorMessage)
        {
            if (weapon == null || string.IsNullOrWhiteSpace(weapon.PersistentId) ||
                !lookup.TryGetWeapon(weapon.PersistentId, out WeaponData cataloged) || cataloged != weapon)
            {
                errorMessage = $"Weapon '{(weapon != null ? weapon.name : "null")}' is not represented uniquely in EquipmentCatalog.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool TryValidateCatalogedArmor(ArmorData armor, EquipmentLookup lookup, out string errorMessage)
        {
            if (armor == null || string.IsNullOrWhiteSpace(armor.PersistentId) ||
                !lookup.TryGetArmor(armor.PersistentId, out ArmorData cataloged) || cataloged != armor)
            {
                errorMessage = $"Armor '{(armor != null ? armor.name : "null")}' is not represented uniquely in EquipmentCatalog.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private static void ResolveOwnedWeapons(
            IEnumerable<string> ids,
            EquipmentLookup lookup,
            List<WeaponData> destination,
            List<string> warnings)
        {
            HashSet<string> uniqueIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string rawId in ids)
            {
                string id = NormalizeSavedId(rawId);
                if (!uniqueIds.Add(id)) continue;
                if (id.Length == 0 || !lookup.TryGetWeapon(id, out WeaponData weapon))
                {
                    warnings.Add($"Unknown Weapon ID '{id}' in saved ownership; skipped.");
                    continue;
                }

                destination.Add(weapon);
            }
        }

        private static void ResolveOwnedArmors(
            IEnumerable<string> ids,
            EquipmentLookup lookup,
            List<ArmorData> destination,
            List<string> warnings)
        {
            HashSet<string> uniqueIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string rawId in ids)
            {
                string id = NormalizeSavedId(rawId);
                if (!uniqueIds.Add(id)) continue;
                if (id.Length == 0 || !lookup.TryGetArmor(id, out ArmorData armor))
                {
                    warnings.Add($"Unknown Armor ID '{id}' in saved ownership; skipped.");
                    continue;
                }

                destination.Add(armor);
            }
        }

        private static WeaponData ResolveEquippedWeapon(
            string rawId,
            EquipmentLookup lookup,
            List<WeaponData> owned,
            List<string> warnings)
        {
            string id = NormalizeSavedId(rawId);
            if (id.Length == 0) return null;
            if (!lookup.TryGetWeapon(id, out WeaponData weapon))
            {
                warnings.Add($"Unknown equipped Weapon ID '{id}'; weapon slot restored empty.");
                return null;
            }

            if (!owned.Contains(weapon))
            {
                warnings.Add($"Equipped Weapon ID '{id}' is not in saved ownership; weapon slot restored empty.");
                return null;
            }

            return weapon;
        }

        private static ArmorData ResolveEquippedArmor(
            string rawId,
            EquipmentLookup lookup,
            List<ArmorData> owned,
            List<string> warnings)
        {
            string id = NormalizeSavedId(rawId);
            if (id.Length == 0) return null;
            if (!lookup.TryGetArmor(id, out ArmorData armor))
            {
                warnings.Add($"Unknown equipped Armor ID '{id}'; armor slot restored empty.");
                return null;
            }

            if (!owned.Contains(armor))
            {
                warnings.Add($"Equipped Armor ID '{id}' is not in saved ownership; armor slot restored empty.");
                return null;
            }

            return armor;
        }

        private static void ResolveRegions(
            IEnumerable<int> values,
            string label,
            List<RegionId> destination,
            List<string> warnings)
        {
            HashSet<int> uniqueValues = new HashSet<int>();
            foreach (int value in values)
            {
                if (!uniqueValues.Add(value)) continue;
                if (!Enum.IsDefined(typeof(RegionId), value))
                {
                    warnings.Add($"Unknown {label} value '{value}'; skipped.");
                    continue;
                }

                destination.Add((RegionId)value);
            }
        }

        private static string NormalizeSavedId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
