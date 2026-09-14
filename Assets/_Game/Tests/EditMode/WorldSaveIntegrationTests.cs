using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HexaRealm.Combat;
using HexaRealm.Equipment;
using HexaRealm.Interaction;
using HexaRealm.Loot;
using HexaRealm.Player;
using HexaRealm.Progression;
using HexaRealm.Save;
using HexaRealm.SaveIntegration;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaRealm.Tests.EditMode
{
    public sealed class WorldSaveIntegrationTests
    {
        private sealed class PlayerFixture
        {
            public GameObject Root;
            public Health Health;
            public PlayerSoulWallet Wallet;
            public PlayerUpgradeProgression Upgrades;
            public PlayerEquipment Equipment;
            public PlayerEquipmentInventory Inventory;
            public PlayerRegionProgression Regions;
            public PlayerInteractor Interactor;
        }

        private readonly List<UnityEngine.Object> createdObjects = new List<UnityEngine.Object>();
        private readonly List<string> testDirectories = new List<string>();

        [TearDown]
        public void TearDown()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                if (createdObjects[index] != null) UnityEngine.Object.DestroyImmediate(createdObjects[index]);
            }

            createdObjects.Clear();
            for (int index = 0; index < testDirectories.Count; index++)
            {
                if (Directory.Exists(testDirectories[index])) Directory.Delete(testDirectories[index], true);
            }

            testDirectories.Clear();
        }

        [Test]
        public void WorldId_EmptyIdIsRejectedBeforeCapture()
        {
            LootChest chest = CreateChest(string.Empty);
            WorldSaveAdapter adapter = CreateWorldAdapter(chest);

            bool succeeded = adapter.TryCapture(new GameSaveData(), out string errorMessage);

            Assert.That(succeeded, Is.False);
            Assert.That(errorMessage, Does.Contain("empty persistent ID"));
        }

        [Test]
        public void WorldId_DuplicateIdIsRejectedBeforeCapture()
        {
            LootChest first = CreateChest("humanrealm.chest.duplicate");
            LootChest second = CreateChest("humanrealm.chest.duplicate");
            WorldSaveAdapter adapter = CreateWorldAdapter(first, second);

            bool succeeded = adapter.TryCapture(new GameSaveData(), out string errorMessage);

            Assert.That(succeeded, Is.False);
            Assert.That(errorMessage, Does.Contain("Duplicate LootChest persistent ID 'humanrealm.chest.duplicate'"));
        }

        [Test]
        public void HumanRealmScene_HasThreeUniqueAuthoredChestIds()
        {
            const string scenePath = "Assets/_Game/Scenes/HumanRealm/HumanRealm.unity";
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForTest = !scene.isLoaded;
            if (openedForTest) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                List<LootChest> chests = new List<LootChest>();
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    chests.AddRange(root.GetComponentsInChildren<LootChest>(true));
                }

                HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
                Assert.That(chests.Count, Is.EqualTo(3));
                foreach (LootChest chest in chests)
                {
                    Assert.That(chest.PersistentId, Is.Not.Empty, chest.name);
                    Assert.That(ids.Add(chest.PersistentId), Is.True, chest.PersistentId);
                }

                Assert.That(ids, Is.EquivalentTo(new[]
                {
                    "humanrealm.chest.northern-side-route.001",
                    "humanrealm.chest.eastern-farm.001",
                    "humanrealm.chest.western-forest.001"
                }));
            }
            finally
            {
                if (openedForTest) EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void WorldJsonRoundTrip_CapturesOnlyOpenedIdsAndOldWorldDefaultsEmpty()
        {
            LootChest unopened = CreateChest("humanrealm.chest.unopened");
            LootChest opened = CreateChest("humanrealm.chest.opened");
            opened.RestoreOpenedState(true);
            WorldSaveAdapter adapter = CreateWorldAdapter(unopened, opened);
            GameSaveData data = new GameSaveData();

            Assert.That(adapter.TryCapture(data, out string captureError), Is.True, captureError);
            Assert.That(data.world.openedChestIds, Is.EqualTo(new[] { "humanrealm.chest.opened" }));

            GameSaveJsonSerializer serializer = new GameSaveJsonSerializer();
            Assert.That(serializer.TrySerialize(data, out string json, out _, out string serializeError), Is.True, serializeError);
            Assert.That(serializer.TryDeserialize(json, out GameSaveData roundTripped, out _, out string deserializeError),
                Is.True, deserializeError);
            Assert.That(roundTripped.world.openedChestIds, Is.EqualTo(new[] { "humanrealm.chest.opened" }));

            const string oldJson = "{\"schemaVersion\":1,\"metadata\":{},\"player\":{},\"progression\":{},\"equipment\":{},\"world\":{}}";
            Assert.That(serializer.TryDeserialize(oldJson, out GameSaveData oldSave, out _, out string oldError), Is.True, oldError);
            Assert.That(oldSave.world.openedChestIds, Is.Empty);
        }

        [Test]
        public void Restore_OpenedUnknownAndNewChestStateAreSafeAndIdempotent()
        {
            LootChest savedChest = CreateChest("humanrealm.chest.saved");
            LootChest newChest = CreateChest("humanrealm.chest.new");
            newChest.RestoreOpenedState(true);
            WorldSaveAdapter adapter = CreateWorldAdapter(savedChest, newChest);
            GameSaveData data = new GameSaveData();
            data.world.openedChestIds.Add("humanrealm.chest.saved");
            data.world.openedChestIds.Add("humanrealm.chest.saved");
            data.world.openedChestIds.Add("humanrealm.chest.removed");
            List<string> warnings = new List<string>();

            Assert.That(adapter.TryPrepareRestore(data, warnings, out WorldSaveAdapter.RestorePlan plan, out string error),
                Is.True, error);
            adapter.ApplyRestore(plan);
            adapter.ApplyRestore(plan);

            Assert.That(savedChest.IsOpened, Is.True);
            Assert.That(newChest.IsOpened, Is.False, "A chest absent from the save must remain/default to unopened.");
            Assert.That(warnings.Count, Is.EqualTo(1));
            Assert.That(warnings[0], Does.Contain("humanrealm.chest.removed"));
        }

        [Test]
        public void RestoreOpenedState_DoesNotGrantLootOrRaiseAutosaveBoundary()
        {
            PlayerFixture player = CreatePlayer();
            LootChest chest = CreateChest("humanrealm.chest.restore", CreateLoot(5));
            int globalOpenCount = 0;
            Action<LootChest> listener = _ => globalOpenCount++;
            LootChest.AnyChestOpened += listener;
            try
            {
                chest.RestoreOpenedState(true);
                chest.RestoreOpenedState(true);
            }
            finally
            {
                LootChest.AnyChestOpened -= listener;
            }

            Assert.That(chest.IsOpened, Is.True);
            Assert.That(player.Wallet.CurrentSouls, Is.Zero);
            Assert.That(globalOpenCount, Is.Zero);
        }

        [Test]
        public void SuccessfulChestCompletion_AutosavesExactlyOnceAfterReward()
        {
            PlayerFixture player = CreatePlayer();
            LootChest chest = CreateChest("humanrealm.chest.autosave", CreateLoot(5));
            SaveFileService files = CreateFileService();
            CreateConfiguredSaveService(files, CreateCatalog(), CreateWorldAdapter(chest));

            chest.Interact(player.Interactor);
            chest.Interact(player.Interactor);

            Assert.That(player.Wallet.CurrentSouls, Is.EqualTo(5));
            Assert.That(chest.IsOpened, Is.True);
            Assert.That(File.Exists(files.PrimarySavePath), Is.True);
            Assert.That(File.Exists(files.BackupSavePath), Is.False, "One successful completion must cause one save.");
            LoadResult saved = files.Load();
            Assert.That(saved.Succeeded, Is.True, saved.Message);
            Assert.That(saved.Data.player.souls, Is.EqualTo(5));
            Assert.That(saved.Data.world.openedChestIds, Is.EqualTo(new[] { chest.PersistentId }));
        }

        [Test]
        public void FailedChestReward_DoesNotOpenOrAutosave()
        {
            PlayerFixture player = CreatePlayer();
            LootChest chest = CreateChest("humanrealm.chest.failed", CreateLoot(0));
            SaveFileService files = CreateFileService();
            CreateConfiguredSaveService(files, CreateCatalog(), CreateWorldAdapter(chest));

            chest.Interact(player.Interactor);

            Assert.That(chest.IsOpened, Is.False);
            Assert.That(player.Wallet.CurrentSouls, Is.Zero);
            Assert.That(files.HasSave(), Is.False);
        }

        [Test]
        public void RegionCompletion_AutosavesAfterRegionStoneAndCapAreConsistent()
        {
            PlayerFixture player = CreatePlayer();
            SaveFileService files = CreateFileService();
            GameSaveService service = CreateConfiguredSaveService(files, CreateCatalog(), CreateWorldAdapter());
            RegionBossProgressionReward reward = CreateRegionBossReward(out _);
            reward.SetRecipient(player.Regions);

            InvokePrivate(reward, "GrantOnce");
            service.FlushPendingRegionAutosaveForTests();

            Assert.That(player.Regions.IsRegionCompleted(RegionId.HumanRealm), Is.True);
            Assert.That(player.Regions.HasTeleportStone(RegionId.HumanRealm), Is.True);
            Assert.That(player.Upgrades.CurrentUpgradeCap, Is.EqualTo(20));
            Assert.That(File.Exists(files.PrimarySavePath), Is.True);
            Assert.That(File.Exists(files.BackupSavePath), Is.False);
            LoadResult saved = files.Load();
            Assert.That(saved.Succeeded, Is.True, saved.Message);
            Assert.That(saved.Data.progression.completedRegions, Is.EqualTo(new[] { (int)RegionId.HumanRealm }));
            Assert.That(saved.Data.progression.teleportStoneRegions, Is.EqualTo(new[] { (int)RegionId.HumanRealm }));
            Assert.That(saved.Data.progression.upgradeCap, Is.EqualTo(20));
            Assert.That(reward.gameObject.activeSelf, Is.True, "Boss suppression is a load-time derived behavior.");
        }

        [Test]
        public void CombinedSaveLoad_RestoresPlayerWorldAndDerivedMainBossStateRepeatedly()
        {
            WeaponData weapon = CreateWeapon("weapon.world-test", 5f);
            ArmorData armor = CreateArmor("armor.world-test", 4f);
            EquipmentCatalog catalog = CreateCatalog(new[] { weapon }, new[] { armor });
            PlayerFixture player = CreatePlayer();
            LootChest chest = CreateChest("humanrealm.chest.combined");
            RegionBossProgressionReward regionBoss = CreateRegionBossReward(out _);
            WorldSaveAdapter world = new WorldSaveAdapter(
                () => new[] { chest },
                () => new[] { regionBoss });
            SaveFileService files = CreateFileService();
            GameSaveService service = CreateConfiguredSaveService(files, catalog, world);

            player.Wallet.RestoreExactSouls(25);
            player.Upgrades.RestoreState(1, 2, 3, 4, 5, 20);
            player.Inventory.RestoreOwnership(new[] { weapon }, new[] { armor });
            player.Equipment.RestoreEquipment(weapon, armor);
            player.Regions.RestoreState(new[] { RegionId.HumanRealm }, new[] { RegionId.HumanRealm });
            chest.RestoreOpenedState(true);
            Assert.That(service.Save().Succeeded, Is.True);

            player.Wallet.RestoreExactSouls(999);
            player.Upgrades.RestoreState(0, 0, 0, 0, 0, 0);
            player.Inventory.RestoreOwnership(null, null);
            player.Equipment.RestoreEquipment(null, null);
            player.Regions.RestoreState(null, null);
            player.Health.ApplyDamage(float.MaxValue);
            chest.RestoreOpenedState(false);

            Assert.That(service.Load().Succeeded, Is.True);
            Assert.That(service.Load().Succeeded, Is.True);

            Assert.That(player.Wallet.CurrentSouls, Is.EqualTo(25));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Vitality), Is.EqualTo(1));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Attack), Is.EqualTo(2));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Defense), Is.EqualTo(3));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Agility), Is.EqualTo(4));
            Assert.That(player.Upgrades.GetUpgradeCount(PlayerStatType.Rage), Is.EqualTo(5));
            Assert.That(player.Upgrades.CurrentUpgradeCap, Is.EqualTo(20));
            Assert.That(player.Inventory.OwnedWeapons, Is.EqualTo(new[] { weapon }));
            Assert.That(player.Inventory.OwnedArmors, Is.EqualTo(new[] { armor }));
            Assert.That(player.Equipment.EquippedWeapon, Is.SameAs(weapon));
            Assert.That(player.Equipment.EquippedArmor, Is.SameAs(armor));
            Assert.That(player.Regions.IsRegionCompleted(RegionId.HumanRealm), Is.True);
            Assert.That(player.Regions.HasTeleportStone(RegionId.HumanRealm), Is.True);
            Assert.That(player.Health.IsDead, Is.False);
            Assert.That(player.Health.CurrentHealth, Is.EqualTo(player.Health.MaxHealth));
            Assert.That(chest.IsOpened, Is.True);
            Assert.That(regionBoss.gameObject.activeSelf, Is.False);
            Assert.That(File.Exists(files.BackupSavePath), Is.False, "Load/restore must not recursively autosave.");
        }

        private WorldSaveAdapter CreateWorldAdapter(params LootChest[] chests)
        {
            return new WorldSaveAdapter(() => chests);
        }

        private PlayerFixture CreatePlayer()
        {
            GameObject root = CreateGameObject("World Save Player");
            root.SetActive(false);
            root.AddComponent<PlayerStats>();
            Health health = root.AddComponent<Health>();
            PlayerSoulWallet wallet = root.AddComponent<PlayerSoulWallet>();
            root.AddComponent<PlayerHealth>();
            PlayerUpgradeProgression upgrades = root.AddComponent<PlayerUpgradeProgression>();
            PlayerEquipment equipment = root.AddComponent<PlayerEquipment>();
            PlayerEquipmentInventory inventory = root.AddComponent<PlayerEquipmentInventory>();
            PlayerRegionProgression regions = root.AddComponent<PlayerRegionProgression>();
            PlayerInteractor interactor = root.AddComponent<PlayerInteractor>();
            root.AddComponent<PlayerLootReceiver>();
            root.SetActive(true);

            return new PlayerFixture
            {
                Root = root,
                Health = health,
                Wallet = wallet,
                Upgrades = upgrades,
                Equipment = equipment,
                Inventory = inventory,
                Regions = regions,
                Interactor = interactor
            };
        }

        private LootChest CreateChest(string persistentId, LootBundleData loot = null)
        {
            GameObject root = CreateGameObject("Chest " + persistentId);
            root.SetActive(false);
            root.AddComponent<BoxCollider2D>().isTrigger = true;
            LootChest chest = root.AddComponent<LootChest>();
            chest.SetPersistentIdForAuthoring(persistentId);
            SetPrivateField(chest, "lootBundle", loot);
            root.SetActive(true);
            return chest;
        }

        private RegionBossProgressionReward CreateRegionBossReward(out Health health)
        {
            GameObject root = CreateGameObject("HumanRealm Main Boss Save Test");
            root.SetActive(false);
            health = root.AddComponent<Health>();
            RegionBossProgressionReward reward = root.AddComponent<RegionBossProgressionReward>();
            RegionProgressionRewardData data = ScriptableObject.CreateInstance<RegionProgressionRewardData>();
            createdObjects.Add(data);
            SetPrivateField(reward, "rewardData", data);
            SetPrivateField(reward, "bossHealth", health);
            root.SetActive(true);
            return reward;
        }

        private GameSaveService CreateConfiguredSaveService(
            SaveFileService files,
            EquipmentCatalog catalog,
            WorldSaveAdapter world)
        {
            GameObject root = CreateGameObject("Game Save Service Test");
            GameSaveService service = root.AddComponent<GameSaveService>();
            service.ConfigureForTests(files, new PlayerSaveAdapter(catalog), world);
            return service;
        }

        private SaveFileService CreateFileService()
        {
            string directory = Path.Combine(Path.GetTempPath(), "HexaRealmWorldSaveTests", Guid.NewGuid().ToString("N"));
            testDirectories.Add(directory);
            return new SaveFileService(directory);
        }

        private EquipmentCatalog CreateCatalog(WeaponData[] weapons = null, ArmorData[] armors = null)
        {
            EquipmentCatalog catalog = ScriptableObject.CreateInstance<EquipmentCatalog>();
            catalog.SetEntriesForAuthoring(weapons ?? Array.Empty<WeaponData>(), armors ?? Array.Empty<ArmorData>());
            createdObjects.Add(catalog);
            return catalog;
        }

        private LootBundleData CreateLoot(int souls)
        {
            LootBundleData loot = ScriptableObject.CreateInstance<LootBundleData>();
            loot.SetAuthoringValues(souls);
            createdObjects.Add(loot);
            return loot;
        }

        private WeaponData CreateWeapon(string id, float damage)
        {
            WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
            weapon.SetPersistentIdForAuthoring(id);
            weapon.SetAuthoringValues(id, damage, 1f, 0f, 0f);
            createdObjects.Add(weapon);
            return weapon;
        }

        private ArmorData CreateArmor(string id, float defense)
        {
            ArmorData armor = ScriptableObject.CreateInstance<ArmorData>();
            armor.SetPersistentIdForAuthoring(id);
            armor.SetAuthoringValues(id, 0f, 0f, defense, 0f, 0f);
            createdObjects.Add(armor);
            return armor;
        }

        private GameObject CreateGameObject(string name)
        {
            GameObject value = new GameObject(name);
            createdObjects.Add(value);
            return value;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, null);
        }
    }
}
