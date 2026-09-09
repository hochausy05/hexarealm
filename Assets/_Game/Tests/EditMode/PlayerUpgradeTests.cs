using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Player;
using HexaRealm.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaRealm.Tests.EditMode
{
    public sealed class PlayerUpgradeTests
    {
        private const float Tolerance = 0.0001f;
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public void InitialState_HasNoUpgrades()
        {
            PlayerUpgradeProgression progression = CreateProgression(0);

            Assert.That(progression.TotalUpgradeCount, Is.EqualTo(0));
            Assert.That(progression.GetUpgradeCount(PlayerStatType.Vitality), Is.EqualTo(0));
            Assert.That(progression.GetUpgradeCount(PlayerStatType.Attack), Is.EqualTo(0));
            Assert.That(progression.GetUpgradeCount(PlayerStatType.Defense), Is.EqualTo(0));
            Assert.That(progression.GetUpgradeCount(PlayerStatType.Agility), Is.EqualTo(0));
            Assert.That(progression.GetUpgradeCount(PlayerStatType.Rage), Is.EqualTo(0));
        }

        [Test]
        public void CostCalculator_UsesPrototypeSequenceAndClampsOverflow()
        {
            Assert.That(SoulUpgradeCostCalculator.CalculateNextCost(1, 1, 0), Is.EqualTo(1));
            Assert.That(SoulUpgradeCostCalculator.CalculateNextCost(1, 1, 5), Is.EqualTo(6));
            Assert.That(SoulUpgradeCostCalculator.CalculateNextCost(int.MaxValue, int.MaxValue, int.MaxValue), Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void SwitchingStats_DoesNotResetTotalBasedCost()
        {
            PlayerUpgradeProgression progression = CreateProgression(100);
            SetPrivateField(progression, "attackUpgradeCount", 5);
            progression.SyncUpgradeModifiers();

            int attackCost = progression.NextUpgradeCost;
            int vitalityCost = progression.NextUpgradeCost;

            Assert.That(attackCost, Is.EqualTo(6));
            Assert.That(vitalityCost, Is.EqualTo(attackCost));
        }

        [Test]
        public void Purchase_IsAtomicAndPreservesEquipmentModifier()
        {
            PlayerUpgradeProgression progression = CreateProgression(10, out PlayerStats stats, out PlayerSoulWallet wallet);
            stats.SetEquipmentModifier(PlayerStatType.Attack, 5f);

            UpgradePurchaseResult result = progression.TryPurchaseUpgrade(PlayerStatType.Attack);

            Assert.That(result, Is.EqualTo(UpgradePurchaseResult.Success));
            Assert.That(wallet.CurrentSouls, Is.EqualTo(9));
            Assert.That(progression.GetUpgradeCount(PlayerStatType.Attack), Is.EqualTo(1));
            Assert.That(progression.TotalUpgradeCount, Is.EqualTo(1));
            Assert.That(stats.GetUpgradeModifier(PlayerStatType.Attack), Is.EqualTo(1f).Within(Tolerance));
            Assert.That(stats.GetEquipmentModifier(PlayerStatType.Attack), Is.EqualTo(5f).Within(Tolerance));
            Assert.That(stats.GetFinalStat(PlayerStatType.Attack), Is.EqualTo(16f).Within(Tolerance));
        }

        [Test]
        public void Purchase_WithInsufficientSouls_DoesNotChangeState()
        {
            PlayerUpgradeProgression progression = CreateProgression(0, out PlayerStats stats, out PlayerSoulWallet wallet);
            float initialAttack = stats.GetFinalStat(PlayerStatType.Attack);

            UpgradePurchaseResult result = progression.TryPurchaseUpgrade(PlayerStatType.Attack);

            Assert.That(result, Is.EqualTo(UpgradePurchaseResult.InsufficientSouls));
            Assert.That(wallet.CurrentSouls, Is.EqualTo(0));
            Assert.That(progression.TotalUpgradeCount, Is.EqualTo(0));
            Assert.That(stats.GetFinalStat(PlayerStatType.Attack), Is.EqualTo(initialAttack).Within(Tolerance));
        }

        [Test]
        public void Cap_AppliesToTotalAndAllowsLastPoint()
        {
            PlayerUpgradeProgression progression = CreateProgression(1000, out _, out PlayerSoulWallet wallet);
            SetPrivateField(progression, "currentUpgradeCap", 3);
            SetPrivateField(progression, "attackUpgradeCount", 2);
            progression.SyncUpgradeModifiers();

            Assert.That(progression.TryPurchaseUpgrade(PlayerStatType.Vitality), Is.EqualTo(UpgradePurchaseResult.Success));
            int soulsAtCap = wallet.CurrentSouls;
            Assert.That(progression.TotalUpgradeCount, Is.EqualTo(3));
            Assert.That(progression.TryPurchaseUpgrade(PlayerStatType.Rage), Is.EqualTo(UpgradePurchaseResult.CapReached));
            Assert.That(wallet.CurrentSouls, Is.EqualTo(soulsAtCap));
            Assert.That(progression.GetUpgradeCount(PlayerStatType.Rage), Is.EqualTo(0));
        }

        [Test]
        public void RepeatedSync_RebuildsInsteadOfDoubleApplying()
        {
            PlayerUpgradeProgression progression = CreateProgression(0, out PlayerStats stats, out _);
            SetPrivateField(progression, "attackUpgradeCount", 3);

            for (int index = 0; index < 20; index++) progression.SyncUpgradeModifiers();

            Assert.That(stats.GetUpgradeModifier(PlayerStatType.Attack), Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void CapCanIncreaseButNeverDecrease()
        {
            PlayerUpgradeProgression progression = CreateProgression(0);

            Assert.That(progression.IncreaseUpgradeCapTo(20), Is.True);
            Assert.That(progression.CurrentUpgradeCap, Is.EqualTo(20));
            Assert.That(progression.IncreaseUpgradeCapTo(10), Is.False);
            Assert.That(progression.CurrentUpgradeCap, Is.EqualTo(20));
        }

        [Test]
        public void InvalidStatAndConfiguration_DoNotSpendOrUpgrade()
        {
            PlayerUpgradeProgression progression = CreateProgression(10, out PlayerStats stats, out PlayerSoulWallet wallet);
            Assert.That(progression.TryPurchaseUpgrade((PlayerStatType)99), Is.EqualTo(UpgradePurchaseResult.InvalidStat));
            SetPrivateField(progression, "attackPerUpgrade", -1f);
            Assert.That(progression.TryPurchaseUpgrade(PlayerStatType.Attack), Is.EqualTo(UpgradePurchaseResult.InvalidConfiguration));
            Assert.That(wallet.CurrentSouls, Is.EqualTo(10));
            Assert.That(progression.TotalUpgradeCount, Is.EqualTo(0));
            Assert.That(stats.GetUpgradeModifier(PlayerStatType.Attack), Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Event_FiresOnceOnlyAfterSuccessfulPurchase()
        {
            PlayerUpgradeProgression progression = CreateProgression(1);
            int eventCount = 0;
            progression.UpgradePurchased += _ => eventCount++;

            Assert.That(progression.TryPurchaseUpgrade(PlayerStatType.Attack), Is.EqualTo(UpgradePurchaseResult.Success));
            Assert.That(progression.TryPurchaseUpgrade(PlayerStatType.Defense), Is.EqualTo(UpgradePurchaseResult.InsufficientSouls));
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void PlayerAndTechnicalScene_HaveUpgradeInteractionFoundation()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player/Player.prefab");
            GameObject pillarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Progression/SoulPillar.prefab");
            Assert.That(playerPrefab.GetComponent<PlayerUpgradeProgression>(), Is.Not.Null);
            Assert.That(pillarPrefab.GetComponent<SoulPillar>(), Is.Not.Null);
            Assert.That(pillarPrefab.layer, Is.EqualTo(LayerMask.NameToLayer("Interactable")));
            Assert.That(pillarPrefab.GetComponent<Collider2D>().isTrigger, Is.True);

            Scene scene = EditorSceneManager.OpenScene("Assets/_Game/Scenes/Test/TechnicalTest.unity", OpenSceneMode.Additive);
            try
            {
                bool foundPillar = false;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foundPillar |= root.GetComponent<SoulPillar>() != null;
                }

                Assert.That(foundPillar, Is.True);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private PlayerUpgradeProgression CreateProgression(int souls)
        {
            return CreateProgression(souls, out _, out _);
        }

        private PlayerUpgradeProgression CreateProgression(int souls, out PlayerStats stats, out PlayerSoulWallet wallet)
        {
            GameObject gameObject = new GameObject("Player Upgrade Test");
            gameObject.SetActive(false);
            createdObjects.Add(gameObject);
            stats = gameObject.AddComponent<PlayerStats>();
            wallet = gameObject.AddComponent<PlayerSoulWallet>();
            PlayerUpgradeProgression progression = gameObject.AddComponent<PlayerUpgradeProgression>();
            wallet.AddSouls(souls);
            gameObject.SetActive(true);
            return progression;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(instance, value);
        }
    }
}
