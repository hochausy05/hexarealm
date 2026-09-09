using System.Collections.Generic;
using HexaRealm.Combat;
using HexaRealm.Equipment;
using HexaRealm.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HexaRealm.Tests.EditMode
{
    public sealed class PlayerEquipmentTests
    {
        private const float Tolerance = 0.0001f;
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in createdObjects) Object.DestroyImmediate(createdObject);
            createdObjects.Clear();
        }

        [Test]
        public void NoWeapon_PreservesBaseAndUpgradeModifiers()
        {
            PlayerEquipment equipment = CreateEquipment(out PlayerStats stats);
            stats.SetUpgradeModifier(PlayerStatType.Attack, 3f);
            equipment.UnequipWeapon();
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(stats.GetFinalStat(PlayerStatType.Attack), Is.EqualTo(13f).Within(Tolerance));
        }

        [Test]
        public void EquipSwitchUnequip_RebuildsContributionWithoutMutatingOtherStatLayers()
        {
            PlayerEquipment equipment = CreateEquipment(out PlayerStats stats);
            WeaponData swordA = CreateWeapon("Sword A", 5f);
            WeaponData swordB = CreateWeapon("Sword B", 9f);
            float baseAttack = stats.GetBaseStat(PlayerStatType.Attack);
            stats.SetUpgradeModifier(PlayerStatType.Attack, 4f);
            equipment.EquipWeapon(swordA);
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(5f).Within(Tolerance));
            equipment.EquipWeapon(swordA);
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(5f).Within(Tolerance));
            equipment.EquipWeapon(swordB);
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(9f).Within(Tolerance));
            Assert.That(stats.GetBaseStat(PlayerStatType.Attack), Is.EqualTo(baseAttack).Within(Tolerance));
            Assert.That(stats.GetUpgradeModifier(PlayerStatType.Attack), Is.EqualTo(4f).Within(Tolerance));
            equipment.UnequipWeapon();
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(stats.GetUpgradeModifier(PlayerStatType.Attack), Is.EqualTo(4f).Within(Tolerance));
            Assert.That(stats.GetFinalStat(PlayerStatType.Attack), Is.EqualTo(baseAttack + 4f).Within(Tolerance));
        }

        [Test]
        public void WeaponDamage_ContributesOnceToFinalAttackAndRawDamage()
        {
            PlayerEquipment equipment = CreateEquipment(out PlayerStats stats);
            equipment.EquipWeapon(CreateWeapon("Sword", 5f));

            float finalAttack = stats.GetFinalStat(PlayerStatType.Attack);
            Assert.That(finalAttack, Is.EqualTo(15f).Within(Tolerance));
            Assert.That(CombatMath.CalculateRawDamage(finalAttack, false, 2f), Is.EqualTo(15f).Within(Tolerance));
        }

        [Test]
        public void RecalculateAndEnableDisable_AreIdempotent()
        {
            PlayerEquipment equipment = CreateEquipment(out PlayerStats stats, out GameObject player);
            equipment.EquipWeapon(CreateWeapon("Sword", 5f));
            for (int index = 0; index < 20; index++) equipment.RecalculateEquipmentModifiers();
            player.SetActive(false); player.SetActive(true); player.SetActive(false); player.SetActive(true);
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void ArmorEquipSwitchAndUnequip_RebuildsAggregateWithoutOverwritingWeapon()
        {
            PlayerEquipment equipment = CreateEquipment(out PlayerStats stats);
            WeaponData sword = CreateWeapon("Sword", 3f);
            ArmorData armorA = CreateArmor("Armor A", defense: 3f);
            ArmorData armorB = CreateArmor("Armor B", defense: 7f);
            stats.SetUpgradeModifier(PlayerStatType.Defense, 2f);

            equipment.EquipWeapon(sword);
            equipment.EquipArmor(armorA);
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(3f).Within(Tolerance));
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Defense), Is.EqualTo(3f).Within(Tolerance));
            Assert.That(stats.GetFinalStat(PlayerStatType.Defense), Is.EqualTo(10f).Within(Tolerance));

            equipment.EquipArmor(armorA);
            equipment.EquipArmor(armorB);
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Defense), Is.EqualTo(7f).Within(Tolerance));
            Assert.That(stats.GetUpgradeModifier(PlayerStatType.Defense), Is.EqualTo(2f).Within(Tolerance));
            equipment.UnequipArmor();
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(3f).Within(Tolerance));
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Defense), Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ArmorAttackAndAllStatBonuses_AggregateOnceAndPreserveBaseStats()
        {
            PlayerEquipment equipment = CreateEquipment(out PlayerStats stats);
            float baseVitality = stats.GetBaseStat(PlayerStatType.Vitality);
            float baseAttack = stats.GetBaseStat(PlayerStatType.Attack);
            ArmorData armor = CreateArmor("Full Armor", vitality: 20f, attack: 2f, defense: 4f, agility: 5f, rage: 6f);
            equipment.EquipWeapon(CreateWeapon("Sword", 3f));
            equipment.EquipArmor(armor);
            for (int index = 0; index < 20; index++) equipment.RecalculateEquipmentModifiers();

            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Vitality), Is.EqualTo(20f).Within(Tolerance));
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(5f).Within(Tolerance));
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Defense), Is.EqualTo(4f).Within(Tolerance));
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Agility), Is.EqualTo(5f).Within(Tolerance));
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Rage), Is.EqualTo(6f).Within(Tolerance));
            Assert.That(stats.GetBaseStat(PlayerStatType.Vitality), Is.EqualTo(baseVitality).Within(Tolerance));
            Assert.That(stats.GetBaseStat(PlayerStatType.Attack), Is.EqualTo(baseAttack).Within(Tolerance));
        }

        [Test]
        public void ArmorBodyVisual_SwitchesRestoresDefaultAndFallsBackWhenSpriteIsNull()
        {
            PlayerEquipment equipment = CreateEquipment(out _, out GameObject player, out SpriteRenderer bodyRenderer, out Sprite defaultBody);
            Sprite armorSpriteA = CreateSprite(Color.red);
            Sprite armorSpriteB = CreateSprite(Color.green);
            equipment.EquipArmor(CreateArmor("Armor A", defense: 3f, bodySprite: armorSpriteA));
            Assert.That(bodyRenderer.sprite, Is.SameAs(armorSpriteA));
            equipment.EquipArmor(CreateArmor("Armor B", defense: 7f, bodySprite: armorSpriteB));
            Assert.That(bodyRenderer.sprite, Is.SameAs(armorSpriteB));
            equipment.EquipArmor(CreateArmor("No Visual Armor", defense: 2f));
            Assert.That(bodyRenderer.sprite, Is.SameAs(defaultBody));
            player.SetActive(false); player.SetActive(true);
            Assert.That(bodyRenderer.sprite, Is.SameAs(defaultBody));
            equipment.UnequipArmor();
            Assert.That(bodyRenderer.sprite, Is.SameAs(defaultBody));
            Assert.That(bodyRenderer.enabled, Is.True);
        }

        [Test]
        public void ArmorData_IsNotMutatedByEquipmentOperations()
        {
            PlayerEquipment equipment = CreateEquipment(out _);
            ArmorData armor = CreateArmor("Immutable Armor", vitality: 1f, attack: 2f, defense: 3f, agility: 4f, rage: 5f);
            equipment.EquipArmor(armor);
            equipment.RecalculateEquipmentModifiers();
            equipment.UnequipArmor();
            Assert.That(armor.VitalityBonus, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(armor.AttackBonus, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(armor.DefenseBonus, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(armor.AgilityBonus, Is.EqualTo(4f).Within(Tolerance));
            Assert.That(armor.RageBonus, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void WeaponCombatModifiers_UseExpectedFormulaAndSafeFallbacks()
        {
            WeaponData weapon = CreateWeapon("Swift Longsword", 5f, 1.2f, 0.05f, 0.4f);
            Assert.That(CombatMath.CalculateAttackInterval(0.6f, 0f, 0.05f, 1.2f), Is.LessThan(0.6f));
            Assert.That(CombatMath.CalculateCriticalChance(0.1f, 0f, 0.01f, weapon.CritBonus), Is.EqualTo(0.15f).Within(Tolerance));
            Assert.That(CombatMath.CalculateCriticalChance(0.9f, 100f, 0.01f, weapon.CritBonus), Is.EqualTo(1f));
            Assert.That(CombatMath.CalculateMeleeReach(0.75f, weapon.RangeBonus), Is.EqualTo(1.15f).Within(Tolerance));
            Assert.That(CombatMath.CalculateAttackInterval(0.6f, 0f, 0.05f, 0f), Is.EqualTo(0.6f).Within(Tolerance));
            Assert.That(CombatMath.CalculateAttackInterval(0.6f, 0f, 0.05f, float.NaN), Is.EqualTo(0.6f).Within(Tolerance));
        }

        [Test]
        public void WeaponData_IsNotMutatedByEquipOrCombatFormula()
        {
            PlayerEquipment equipment = CreateEquipment(out _);
            WeaponData weapon = CreateWeapon("Immutable Sword", 5f, 1.1f, 0.2f, 0.3f);
            float damage = weapon.Damage; float speed = weapon.AttackSpeedMultiplier; float crit = weapon.CritBonus; float range = weapon.RangeBonus;
            equipment.EquipWeapon(weapon);
            CombatMath.CalculateAttackInterval(0.6f, 5f, 0.05f, weapon.AttackSpeedMultiplier);
            CombatMath.CalculateCriticalChance(0.05f, 5f, 0.01f, weapon.CritBonus);
            CombatMath.CalculateMeleeReach(0.75f, weapon.RangeBonus);
            equipment.UnequipWeapon();
            Assert.That(weapon.Damage, Is.EqualTo(damage).Within(Tolerance));
            Assert.That(weapon.AttackSpeedMultiplier, Is.EqualTo(speed).Within(Tolerance));
            Assert.That(weapon.CritBonus, Is.EqualTo(crit).Within(Tolerance));
            Assert.That(weapon.RangeBonus, Is.EqualTo(range).Within(Tolerance));
        }

        [Test]
        public void PlayerPrefab_HasEquipmentStartingWeaponArmorAndVisualReferences()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player/Player.prefab");
            WeaponData basicSword = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/_Game/Data/Weapons/BasicSword.asset");
            ArmorData trainingArmor = AssetDatabase.LoadAssetAtPath<ArmorData>("Assets/_Game/Data/Armor/TrainingArmor.asset");
            Assert.That(prefab.GetComponent<PlayerEquipment>(), Is.Not.Null);
            Assert.That(basicSword, Is.Not.Null);
            Assert.That(basicSword.WeaponSprite, Is.Not.Null);
            Assert.That(trainingArmor, Is.Not.Null);
            Assert.That(trainingArmor.BodySprite, Is.Not.Null);
            Assert.That(prefab.transform.Find("Body").GetComponent<SpriteRenderer>(), Is.Not.Null);
            Assert.That(prefab.transform.Find("WeaponSprite").GetComponent<SpriteRenderer>(), Is.Not.Null);
        }

        private PlayerEquipment CreateEquipment(out PlayerStats stats) => CreateEquipment(out stats, out _);

        private PlayerEquipment CreateEquipment(out PlayerStats stats, out GameObject player)
        {
            return CreateEquipment(out stats, out player, out _, out _);
        }

        private PlayerEquipment CreateEquipment(
            out PlayerStats stats,
            out GameObject player,
            out SpriteRenderer bodyRenderer,
            out Sprite defaultBody)
        {
            player = new GameObject("Player Equipment Test"); player.SetActive(false); createdObjects.Add(player);
            GameObject body = new GameObject("Body"); body.transform.SetParent(player.transform);
            bodyRenderer = body.AddComponent<SpriteRenderer>();
            defaultBody = CreateSprite(Color.blue);
            bodyRenderer.sprite = defaultBody;
            stats = player.AddComponent<PlayerStats>();
            PlayerEquipment equipment = player.AddComponent<PlayerEquipment>();
            player.SetActive(true);
            return equipment;
        }

        private WeaponData CreateWeapon(string name, float damage, float speed = 1f, float crit = 0f, float range = 0f)
        {
            WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
            weapon.SetAuthoringValues(name, damage, speed, crit, range); createdObjects.Add(weapon); return weapon;
        }

        private ArmorData CreateArmor(string name, float vitality = 0f, float attack = 0f, float defense = 0f, float agility = 0f, float rage = 0f, Sprite bodySprite = null)
        {
            ArmorData armor = ScriptableObject.CreateInstance<ArmorData>();
            armor.SetAuthoringValues(name, vitality, attack, defense, agility, rage, bodySprite);
            createdObjects.Add(armor);
            return armor;
        }

        private Sprite CreateSprite(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            createdObjects.Add(texture);
            createdObjects.Add(sprite);
            return sprite;
        }
    }
}
