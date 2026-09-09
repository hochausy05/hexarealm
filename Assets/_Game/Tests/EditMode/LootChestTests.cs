using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Equipment;
using HexaRealm.Interaction;
using HexaRealm.Loot;
using HexaRealm.Player;
using HexaRealm.Progression;
using NUnit.Framework;
using UnityEngine;

namespace HexaRealm.Tests.EditMode
{
    public sealed class LootChestTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in createdObjects) Object.DestroyImmediate(createdObject);
            createdObjects.Clear();
        }

        [Test]
        public void EquipmentInventory_StartsEmptyAndAddsUniqueEquipmentOnly()
        {
            PlayerEquipmentInventory inventory = CreateInventory();
            WeaponData weapon = CreateWeapon("Loot Sword", 5f);
            ArmorData armor = CreateArmor("Loot Armor", 5f);

            Assert.That(inventory.OwnedWeapons, Is.Empty);
            Assert.That(inventory.OwnedArmors, Is.Empty);
            Assert.That(inventory.AddWeapon(weapon), Is.True);
            Assert.That(inventory.AddWeapon(weapon), Is.False);
            Assert.That(inventory.AddArmor(armor), Is.True);
            Assert.That(inventory.AddArmor(armor), Is.False);
            Assert.That(inventory.OwnsWeapon(weapon), Is.True);
            Assert.That(inventory.OwnsArmor(armor), Is.True);
            Assert.That(inventory.OwnedWeapons.Count, Is.EqualTo(1));
            Assert.That(inventory.OwnedArmors.Count, Is.EqualTo(1));
        }

        [Test]
        public void EquipmentInventory_RejectsNullEquipment()
        {
            PlayerEquipmentInventory inventory = CreateInventory();
            Assert.That(inventory.AddWeapon(null), Is.False);
            Assert.That(inventory.AddArmor(null), Is.False);
            Assert.That(inventory.OwnedWeapons, Is.Empty);
            Assert.That(inventory.OwnedArmors, Is.Empty);
        }

        [Test]
        public void EquipmentInventory_RegistersStartingEquipmentIdempotently()
        {
            GameObject player = CreateGameObject("Starting Equipment Player");
            player.SetActive(false);
            player.AddComponent<PlayerStats>();
            PlayerEquipment equipment = player.AddComponent<PlayerEquipment>();
            PlayerEquipmentInventory inventory = player.AddComponent<PlayerEquipmentInventory>();
            WeaponData weapon = CreateWeapon("Starting Sword", 3f);
            ArmorData armor = CreateArmor("Starting Armor", 3f);
            SetPrivateField(equipment, "startingWeapon", weapon);
            SetPrivateField(equipment, "startingArmor", armor);

            player.SetActive(true);
            inventory.RegisterStartingEquipment();
            inventory.RegisterStartingEquipment();

            Assert.That(inventory.OwnsWeapon(weapon), Is.True);
            Assert.That(inventory.OwnsArmor(armor), Is.True);
            Assert.That(inventory.OwnedWeapons.Count, Is.EqualTo(1));
            Assert.That(inventory.OwnedArmors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Chest_MixedRewardClaimsOnceAndPreservesLootData()
        {
            PlayerLootReceiver receiver = CreateReceiver(out PlayerInteractor interactor, out PlayerSoulWallet wallet, out PlayerEquipmentInventory inventory, out PlayerEquipment equipment);
            LootBundleData loot = CreateLoot(5, CreateWeapon("Explorer Sword", 5f), CreateArmor("Explorer Armor", 5f));
            LootChest chest = CreateChest(loot);
            int openedCount = 0;
            chest.Opened += () => openedCount++;

            chest.Interact(interactor);
            chest.Interact(interactor);

            Assert.That(receiver, Is.Not.Null);
            Assert.That(wallet.CurrentSouls, Is.EqualTo(5));
            Assert.That(inventory.OwnsWeapon(loot.WeaponReward), Is.True);
            Assert.That(inventory.OwnsArmor(loot.ArmorReward), Is.True);
            Assert.That(equipment.EquippedWeapon, Is.Null);
            Assert.That(equipment.EquippedArmor, Is.Null);
            Assert.That(chest.IsOpened, Is.True);
            Assert.That(openedCount, Is.EqualTo(1));
            Assert.That(loot.SoulAmount, Is.EqualTo(5));
            Assert.That(loot.WeaponReward, Is.Not.Null);
            Assert.That(loot.ArmorReward, Is.Not.Null);
        }

        [Test]
        public void Chest_InvalidLootOrMissingReceiverRemainsClosedWithoutPartialReward()
        {
            PlayerLootReceiver receiver = CreateReceiver(out PlayerInteractor interactor, out PlayerSoulWallet wallet, out PlayerEquipmentInventory inventory, out _);
            LootChest invalidChest = CreateChest(CreateLoot(0));
            invalidChest.Interact(interactor);
            Assert.That(invalidChest.IsOpened, Is.False);
            Assert.That(wallet.CurrentSouls, Is.EqualTo(0));
            Assert.That(inventory.OwnedWeapons, Is.Empty);

            GameObject otherPlayer = CreateGameObject("Interactor Without Receiver");
            PlayerInteractor noReceiverInteractor = otherPlayer.AddComponent<PlayerInteractor>();
            LootChest validChest = CreateChest(CreateLoot(5));
            validChest.Interact(noReceiverInteractor);
            Assert.That(receiver, Is.Not.Null);
            Assert.That(validChest.IsOpened, Is.False);
            Assert.That(wallet.CurrentSouls, Is.EqualTo(0));
        }

        [Test]
        public void Chest_PersistsOpenedStateAcrossDisableEnableAndAutoEquipUsesExistingEquipment()
        {
            PlayerLootReceiver receiver = CreateReceiver(out PlayerInteractor interactor, out PlayerSoulWallet wallet, out PlayerEquipmentInventory inventory, out PlayerEquipment equipment);
            WeaponData explorerSword = CreateWeapon("Explorer Sword", 5f);
            LootChest chest = CreateChest(CreateLoot(5, explorerSword), autoEquipWeapon: true);
            chest.Interact(interactor);
            chest.gameObject.SetActive(false);
            chest.gameObject.SetActive(true);
            chest.Interact(interactor);

            Assert.That(receiver, Is.Not.Null);
            Assert.That(wallet.CurrentSouls, Is.EqualTo(5));
            Assert.That(inventory.OwnedWeapons.Count, Is.EqualTo(1));
            Assert.That(equipment.EquippedWeapon, Is.SameAs(explorerSword));
            Assert.That(chest.IsOpened, Is.True);
        }

        [Test]
        public void Chest_SoulRewardUsesWalletOverflowProtection()
        {
            CreateReceiver(out PlayerInteractor interactor, out PlayerSoulWallet wallet, out _, out _);
            SetPrivateField(wallet, "currentSouls", int.MaxValue - 1);
            LootChest chest = CreateChest(CreateLoot(5));

            chest.Interact(interactor);

            Assert.That(wallet.CurrentSouls, Is.EqualTo(int.MaxValue));
            Assert.That(chest.IsOpened, Is.True);
        }

        private PlayerEquipmentInventory CreateInventory()
        {
            GameObject player = CreateGameObject("Inventory Test Player");
            return player.AddComponent<PlayerEquipmentInventory>();
        }

        private PlayerLootReceiver CreateReceiver(
            out PlayerInteractor interactor,
            out PlayerSoulWallet wallet,
            out PlayerEquipmentInventory inventory,
            out PlayerEquipment equipment)
        {
            GameObject player = CreateGameObject("Loot Test Player");
            player.SetActive(false);
            player.AddComponent<PlayerStats>();
            wallet = player.AddComponent<PlayerSoulWallet>();
            equipment = player.AddComponent<PlayerEquipment>();
            inventory = player.AddComponent<PlayerEquipmentInventory>();
            interactor = player.AddComponent<PlayerInteractor>();
            PlayerLootReceiver receiver = player.AddComponent<PlayerLootReceiver>();
            player.SetActive(true);
            return receiver;
        }

        private LootChest CreateChest(LootBundleData loot, bool autoEquipWeapon = false)
        {
            GameObject chestObject = CreateGameObject("Loot Chest Test");
            chestObject.AddComponent<BoxCollider2D>().isTrigger = true;
            LootChest chest = chestObject.AddComponent<LootChest>();
            SetPrivateField(chest, "lootBundle", loot);
            SetPrivateField(chest, "autoEquipWeaponReward", autoEquipWeapon);
            return chest;
        }

        private LootBundleData CreateLoot(int souls, WeaponData weapon = null, ArmorData armor = null)
        {
            LootBundleData loot = ScriptableObject.CreateInstance<LootBundleData>();
            loot.SetAuthoringValues(souls, weapon, armor);
            createdObjects.Add(loot);
            return loot;
        }

        private WeaponData CreateWeapon(string displayName, float damage)
        {
            WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
            weapon.SetAuthoringValues(displayName, damage, 1f, 0f, 0f);
            createdObjects.Add(weapon);
            return weapon;
        }

        private ArmorData CreateArmor(string displayName, float defense)
        {
            ArmorData armor = ScriptableObject.CreateInstance<ArmorData>();
            armor.SetAuthoringValues(displayName, 0f, 0f, defense, 0f, 0f);
            createdObjects.Add(armor);
            return armor;
        }

        private GameObject CreateGameObject(string name)
        {
            GameObject gameObject = new GameObject(name);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }
    }
}
