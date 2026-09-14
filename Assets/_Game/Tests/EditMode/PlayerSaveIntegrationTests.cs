using System;
using System.Collections.Generic;
using System.Linq;
using HexaRealm.Combat;
using HexaRealm.Equipment;
using HexaRealm.Player;
using HexaRealm.Progression;
using HexaRealm.Save;
using HexaRealm.SaveIntegration;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HexaRealm.Tests.EditMode
{
    public sealed class PlayerSaveIntegrationTests
    {
        private const float Tolerance = 0.0001f;
        private readonly List<UnityEngine.Object> createdObjects = new List<UnityEngine.Object>();

        private WeaponData basicSword;
        private WeaponData explorerSword;
        private ArmorData trainingArmor;
        private ArmorData explorerArmor;
        private EquipmentCatalog catalog;
        private PlayerSaveAdapter adapter;

        private sealed class PlayerFixture
        {
            public GameObject Root;
            public PlayerStats Stats;
            public Health Health;
            public PlayerSoulWallet Wallet;
            public PlayerUpgradeProgression Upgrades;
            public PlayerEquipment Equipment;
            public PlayerEquipmentInventory Inventory;
            public PlayerRegionProgression Regions;
        }

        [SetUp]
        public void SetUp()
        {
            basicSword = CreateWeapon("weapon.basic", 3f);
            explorerSword = CreateWeapon("weapon.explorer", 5f);
            trainingArmor = CreateArmor("armor.training", defense: 3f);
            explorerArmor = CreateArmor("armor.explorer", vitality: 4f, attack: 2f, defense: 5f, agility: 3f, rage: 2f);
            catalog = ScriptableObject.CreateInstance<EquipmentCatalog>();
            catalog.SetEntriesForAuthoring(
                new[] { basicSword, explorerSword },
                new[] { trainingArmor, explorerArmor });
            createdObjects.Add(catalog);
            adapter = new PlayerSaveAdapter(catalog);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (UnityEngine.Object createdObject in createdObjects)
            {
                UnityEngine.Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public void CombinedJsonRoundTrip_RestoresAllPersistentStateAndFullPlayableHealth()
        {
            PlayerFixture player = CreatePlayer();
            player.Wallet.RestoreExactSouls(25);
            player.Upgrades.RestoreState(1, 2, 3, 4, 5, 30);
            player.Inventory.RestoreOwnership(
                new[] { basicSword, explorerSword },
                new[] { trainingArmor, explorerArmor });
            player.Equipment.RestoreEquipment(explorerSword, explorerArmor);
            player.Regions.RestoreState(
                new[] { RegionId.HumanRealm },
                new[] { RegionId.HumanRealm });
            player.Health.ApplyDamage(50f);

            Assert.That(adapter.TryCapture(player.Root, out GameSaveData captured, out string captureError), Is.True, captureError);
            GameSaveJsonSerializer serializer = new GameSaveJsonSerializer();
            Assert.That(serializer.TrySerialize(captured, out string json, out _, out string serializeError), Is.True, serializeError);
            Assert.That(serializer.TryDeserialize(json, out GameSaveData roundTripped, out _, out string deserializeError), Is.True, deserializeError);

            player.Wallet.RestoreExactSouls(999);
            player.Upgrades.RestoreState(0, 0, 0, 0, 0, 0);
            player.Inventory.RestoreOwnership(null, null);
            player.Equipment.RestoreEquipment(null, null);
            player.Regions.RestoreState(null, null);
            player.Health.ApplyDamage(float.MaxValue);

            Assert.That(adapter.TryRestore(player.Root, roundTripped, out IReadOnlyList<string> warnings, out string restoreError), Is.True, restoreError);

            Assert.That(warnings, Is.Empty);
            Assert.That(player.Wallet.CurrentSouls, Is.EqualTo(25));
            AssertUpgradeState(player, 1, 2, 3, 4, 5, 30);
            Assert.That(player.Inventory.OwnedWeapons, Is.EquivalentTo(new[] { basicSword, explorerSword }));
            Assert.That(player.Inventory.OwnedArmors, Is.EquivalentTo(new[] { trainingArmor, explorerArmor }));
            Assert.That(player.Equipment.EquippedWeapon, Is.SameAs(explorerSword));
            Assert.That(player.Equipment.EquippedArmor, Is.SameAs(explorerArmor));
            Assert.That(player.Regions.IsRegionCompleted(RegionId.HumanRealm), Is.True);
            Assert.That(player.Regions.HasTeleportStone(RegionId.HumanRealm), Is.True);
            Assert.That(player.Regions.IsRegionUnlocked(RegionId.Region2), Is.True);
            Assert.That(player.Health.IsDead, Is.False);
            Assert.That(player.Health.CurrentHealth, Is.EqualTo(player.Health.MaxHealth).Within(Tolerance));
            Assert.That(player.Health.MaxHealth, Is.EqualTo(105f).Within(Tolerance));
        }

        [Test]
        public void SoulRestore_IsExactSanitizedAndRepeatedLoadDoesNotAccumulate()
        {
            PlayerFixture player = CreatePlayer();
            GameSaveData data = CreateRestorableData();
            data.player.souls = 25;
            player.Wallet.RestoreExactSouls(100);

            AssertRestore(player, data);
            AssertRestore(player, data);
            AssertRestore(player, data);
            Assert.That(player.Wallet.CurrentSouls, Is.EqualTo(25));

            data.player.souls = -500;
            AssertRestore(player, data);
            Assert.That(player.Wallet.CurrentSouls, Is.Zero);
        }

        [Test]
        public void UpgradeRestore_ReplacesAllCountsAndCapWithoutSoulCostOrModifierAccumulation()
        {
            PlayerFixture player = CreatePlayer();
            GameSaveData data = CreateRestorableData();
            data.player.souls = 50;
            SetUpgradeData(data, 1, 2, 3, 4, 5, 20);
            int purchaseEvents = 0;
            player.Upgrades.UpgradePurchased += _ => purchaseEvents++;

            AssertRestore(player, data);
            AssertRestore(player, data);
            AssertRestore(player, data);

            AssertUpgradeState(player, 1, 2, 3, 4, 5, 20);
            Assert.That(player.Wallet.CurrentSouls, Is.EqualTo(50));
            Assert.That(player.Upgrades.NextUpgradeCost, Is.EqualTo(16));
            Assert.That(player.Stats.GetUpgradeModifier(PlayerStatType.Vitality), Is.EqualTo(1f).Within(Tolerance));
            Assert.That(player.Stats.GetUpgradeModifier(PlayerStatType.Attack), Is.EqualTo(2f).Within(Tolerance));
            Assert.That(player.Stats.GetUpgradeModifier(PlayerStatType.Defense), Is.EqualTo(3f).Within(Tolerance));
            Assert.That(player.Stats.GetUpgradeModifier(PlayerStatType.Agility), Is.EqualTo(4f).Within(Tolerance));
            Assert.That(player.Stats.GetUpgradeModifier(PlayerStatType.Rage), Is.EqualTo(5f).Within(Tolerance));
            Assert.That(purchaseEvents, Is.Zero);
        }

        [Test]
        public void EquipmentRestore_DeduplicatesOwnershipAndDoesNotDoubleApplyModifiers()
        {
            PlayerFixture player = CreatePlayer();
            GameSaveData data = CreateRestorableData();
            data.equipment.ownedWeaponIds.AddRange(new[] { basicSword.PersistentId, explorerSword.PersistentId, explorerSword.PersistentId });
            data.equipment.ownedArmorIds.AddRange(new[] { explorerArmor.PersistentId, explorerArmor.PersistentId });
            data.equipment.equippedWeaponId = explorerSword.PersistentId;
            data.equipment.equippedArmorId = explorerArmor.PersistentId;

            AssertRestore(player, data);
            AssertRestore(player, data);
            AssertRestore(player, data);
            player.Root.SetActive(false);
            player.Root.SetActive(true);

            Assert.That(player.Inventory.OwnedWeapons.Count, Is.EqualTo(2));
            Assert.That(player.Inventory.OwnedArmors.Count, Is.EqualTo(1));
            Assert.That(player.Equipment.EquippedWeapon, Is.SameAs(explorerSword));
            Assert.That(player.Equipment.EquippedArmor, Is.SameAs(explorerArmor));
            Assert.That(player.Stats.GetEquipmentModifier(PlayerStatType.Vitality), Is.EqualTo(4f).Within(Tolerance));
            Assert.That(player.Stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(7f).Within(Tolerance));
            Assert.That(player.Stats.GetEquipmentModifier(PlayerStatType.Defense), Is.EqualTo(5f).Within(Tolerance));
            Assert.That(player.Stats.GetEquipmentModifier(PlayerStatType.Agility), Is.EqualTo(3f).Within(Tolerance));
            Assert.That(player.Stats.GetEquipmentModifier(PlayerStatType.Rage), Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void UnknownEquipmentIds_AreReportedSkippedAndDoNotBlockOtherState()
        {
            PlayerFixture player = CreatePlayer();
            GameSaveData data = CreateRestorableData();
            data.player.souls = 12;
            data.equipment.ownedWeaponIds.AddRange(new[] { basicSword.PersistentId, "weapon.missing" });
            data.equipment.ownedArmorIds.AddRange(new[] { trainingArmor.PersistentId, "armor.missing" });
            data.equipment.equippedWeaponId = "weapon.missing";
            data.equipment.equippedArmorId = "armor.missing";

            bool succeeded = adapter.TryRestore(player.Root, data, out IReadOnlyList<string> warnings, out string errorMessage);

            Assert.That(succeeded, Is.True, errorMessage);
            Assert.That(player.Wallet.CurrentSouls, Is.EqualTo(12));
            Assert.That(player.Inventory.OwnedWeapons, Is.EquivalentTo(new[] { basicSword }));
            Assert.That(player.Inventory.OwnedArmors, Is.EquivalentTo(new[] { trainingArmor }));
            Assert.That(player.Equipment.EquippedWeapon, Is.Null);
            Assert.That(player.Equipment.EquippedArmor, Is.Null);
            Assert.That(warnings.Any(value => value.Contains("Weapon ID 'weapon.missing'")), Is.True);
            Assert.That(warnings.Any(value => value.Contains("Armor ID 'armor.missing'")), Is.True);
        }

        [Test]
        public void RegionRestore_DeduplicatesWithoutEventsAndRestoresCapAbsolutely()
        {
            PlayerFixture player = CreatePlayer();
            player.Upgrades.RestoreState(0, 0, 0, 0, 0, 99);
            GameSaveData data = CreateRestorableData();
            data.progression.upgradeCap = 20;
            data.progression.completedRegions.AddRange(new[] { (int)RegionId.HumanRealm, (int)RegionId.HumanRealm });
            data.progression.teleportStoneRegions.AddRange(new[] { (int)RegionId.HumanRealm, (int)RegionId.HumanRealm });
            int progressionEvents = 0;
            player.Regions.RegionProgressionGranted += _ => progressionEvents++;

            AssertRestore(player, data);
            AssertRestore(player, data);

            Assert.That(player.Regions.CompletedRegions.Count, Is.EqualTo(1));
            Assert.That(player.Regions.TeleportStoneRegions.Count, Is.EqualTo(1));
            Assert.That(player.Regions.IsRegionUnlocked(RegionId.Region2), Is.True);
            Assert.That(player.Upgrades.CurrentUpgradeCap, Is.EqualTo(20));
            Assert.That(progressionEvents, Is.Zero);
        }

        [Test]
        public void NegativeUpgradeData_IsSanitizedAndHealthReturnsAliveAtFinalMax()
        {
            PlayerFixture player = CreatePlayer();
            GameSaveData data = CreateRestorableData();
            SetUpgradeData(data, -1, -2, -3, -4, -5, -10);
            player.Health.ApplyDamage(float.MaxValue);

            AssertRestore(player, data);

            AssertUpgradeState(player, 0, 0, 0, 0, 0, 0);
            Assert.That(player.Health.IsDead, Is.False);
            Assert.That(player.Health.MaxHealth, Is.EqualTo(100f).Within(Tolerance));
            Assert.That(player.Health.CurrentHealth, Is.EqualTo(100f).Within(Tolerance));
        }

        [Test]
        public void Capture_UsesStableIdsForOwnedAndEquippedEquipment()
        {
            PlayerFixture player = CreatePlayer();
            player.Inventory.RestoreOwnership(new[] { basicSword, explorerSword }, new[] { trainingArmor });
            player.Equipment.RestoreEquipment(explorerSword, trainingArmor);

            bool succeeded = adapter.TryCapture(player.Root, out GameSaveData data, out string errorMessage);

            Assert.That(succeeded, Is.True, errorMessage);
            Assert.That(data.equipment.ownedWeaponIds, Is.EquivalentTo(new[] { basicSword.PersistentId, explorerSword.PersistentId }));
            Assert.That(data.equipment.ownedArmorIds, Is.EquivalentTo(new[] { trainingArmor.PersistentId }));
            Assert.That(data.equipment.equippedWeaponId, Is.EqualTo(explorerSword.PersistentId));
            Assert.That(data.equipment.equippedArmorId, Is.EqualTo(trainingArmor.PersistentId));
        }

        [Test]
        public void InvalidCatalog_FailsBeforeMutatingPlayerState()
        {
            WeaponData duplicate = CreateWeapon(basicSword.PersistentId, 99f);
            EquipmentCatalog invalidCatalog = ScriptableObject.CreateInstance<EquipmentCatalog>();
            invalidCatalog.SetEntriesForAuthoring(new[] { basicSword, duplicate }, new[] { trainingArmor });
            createdObjects.Add(invalidCatalog);
            PlayerSaveAdapter invalidAdapter = new PlayerSaveAdapter(invalidCatalog);
            PlayerFixture player = CreatePlayer();
            player.Wallet.RestoreExactSouls(77);
            GameSaveData data = CreateRestorableData();
            data.player.souls = 1;

            bool succeeded = invalidAdapter.TryRestore(player.Root, data, out _, out string errorMessage);

            Assert.That(succeeded, Is.False);
            Assert.That(errorMessage, Does.Contain("Duplicate equipment persistent ID"));
            Assert.That(player.Wallet.CurrentSouls, Is.EqualTo(77));
        }

        [Test]
        public void RuntimeCatalog_ContainsEveryEquipmentAssetWithUniqueStableId()
        {
            EquipmentCatalog runtimeCatalog = Resources.Load<EquipmentCatalog>("EquipmentCatalog");
            Assert.That(runtimeCatalog, Is.Not.Null);
            Assert.That(runtimeCatalog.TryCreateLookup(out EquipmentLookup lookup, out string errorMessage), Is.True, errorMessage);

            List<WeaponData> allWeapons = AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/_Game" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<WeaponData>(path))
                .Where(value => value != null)
                .ToList();
            List<ArmorData> allArmors = AssetDatabase.FindAssets("t:ArmorData", new[] { "Assets/_Game" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<ArmorData>(path))
                .Where(value => value != null)
                .ToList();

            Assert.That(allWeapons.Count, Is.EqualTo(2));
            Assert.That(allArmors.Count, Is.EqualTo(2));
            foreach (WeaponData weapon in allWeapons)
            {
                Assert.That(weapon.PersistentId, Is.Not.Empty);
                Assert.That(lookup.TryGetWeapon(weapon.PersistentId, out WeaponData resolved), Is.True);
                Assert.That(resolved, Is.SameAs(weapon));
            }

            foreach (ArmorData armor in allArmors)
            {
                Assert.That(armor.PersistentId, Is.Not.Empty);
                Assert.That(lookup.TryGetArmor(armor.PersistentId, out ArmorData resolved), Is.True);
                Assert.That(resolved, Is.SameAs(armor));
            }
        }

        private void AssertRestore(PlayerFixture player, GameSaveData data)
        {
            Assert.That(adapter.TryRestore(player.Root, data, out _, out string errorMessage), Is.True, errorMessage);
        }

        private static GameSaveData CreateRestorableData()
        {
            GameSaveData data = new GameSaveData();
            data.metadata.hasPlayerProgressionEquipmentState = true;
            return data;
        }

        private static void SetUpgradeData(GameSaveData data, int vitality, int attack, int defense, int agility, int rage, int cap)
        {
            data.progression.vitalityUpgradeCount = vitality;
            data.progression.attackUpgradeCount = attack;
            data.progression.defenseUpgradeCount = defense;
            data.progression.agilityUpgradeCount = agility;
            data.progression.rageUpgradeCount = rage;
            data.progression.upgradeCap = cap;
        }

        private static void AssertUpgradeState(
            PlayerFixture player,
            int vitality,
            int attack,
            int defense,
            int agility,
            int rage,
            int cap)
        {
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Vitality), Is.EqualTo(vitality));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Attack), Is.EqualTo(attack));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Defense), Is.EqualTo(defense));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Agility), Is.EqualTo(agility));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Rage), Is.EqualTo(rage));
            Assert.That(player.Upgrades.CurrentUpgradeCap, Is.EqualTo(cap));
        }

        private PlayerFixture CreatePlayer()
        {
            GameObject root = new GameObject("Save Integration Player Test");
            root.SetActive(false);
            createdObjects.Add(root);

            PlayerFixture fixture = new PlayerFixture
            {
                Root = root,
                Stats = root.AddComponent<PlayerStats>(),
                Health = root.AddComponent<Health>(),
                Wallet = root.AddComponent<PlayerSoulWallet>()
            };

            root.AddComponent<PlayerHealth>();
            fixture.Upgrades = root.AddComponent<PlayerUpgradeProgression>();
            fixture.Equipment = root.AddComponent<PlayerEquipment>();
            fixture.Inventory = root.AddComponent<PlayerEquipmentInventory>();
            fixture.Regions = root.AddComponent<PlayerRegionProgression>();
            root.SetActive(true);
            return fixture;
        }

        private WeaponData CreateWeapon(string persistentId, float damage)
        {
            WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
            weapon.name = persistentId;
            weapon.SetPersistentIdForAuthoring(persistentId);
            weapon.SetAuthoringValues(persistentId, damage, 1f, 0f, 0f);
            createdObjects.Add(weapon);
            return weapon;
        }

        private ArmorData CreateArmor(
            string persistentId,
            float vitality = 0f,
            float attack = 0f,
            float defense = 0f,
            float agility = 0f,
            float rage = 0f)
        {
            ArmorData armor = ScriptableObject.CreateInstance<ArmorData>();
            armor.name = persistentId;
            armor.SetPersistentIdForAuthoring(persistentId);
            armor.SetAuthoringValues(persistentId, vitality, attack, defense, agility, rage);
            createdObjects.Add(armor);
            return armor;
        }
    }
}
