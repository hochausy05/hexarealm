using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Combat;
using HexaRealm.Enemy;
using HexaRealm.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HexaRealm.Tests.EditMode
{
    public sealed class SoulWalletTests
    {
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
        public void Wallet_AddSpendAndAffordabilityFollowExpectedRules()
        {
            PlayerSoulWallet wallet = CreateWallet();

            Assert.That(wallet.CurrentSouls, Is.EqualTo(0));
            wallet.AddSouls(5);
            wallet.AddSouls(3);
            wallet.AddSouls(0);
            wallet.AddSouls(-10);

            Assert.That(wallet.CurrentSouls, Is.EqualTo(8));
            Assert.That(wallet.CanAfford(5), Is.True);
            Assert.That(wallet.CanAfford(9), Is.False);
            Assert.That(wallet.TrySpendSouls(4), Is.True);
            Assert.That(wallet.CurrentSouls, Is.EqualTo(4));
            Assert.That(wallet.TrySpendSouls(4), Is.True);
            Assert.That(wallet.CurrentSouls, Is.EqualTo(0));
            Assert.That(wallet.TrySpendSouls(1), Is.False);
            Assert.That(wallet.TrySpendSouls(0), Is.False);
            Assert.That(wallet.TrySpendSouls(-5), Is.False);
        }

        [Test]
        public void Wallet_AddSoulsClampsAtIntegerMaximum()
        {
            PlayerSoulWallet wallet = CreateWallet();
            SetPrivateField(wallet, "currentSouls", int.MaxValue - 1);

            wallet.AddSouls(10);

            Assert.That(wallet.CurrentSouls, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void EnemySoulReward_AwardsDataValueOnceWithoutMutatingData()
        {
            PlayerSoulWallet wallet = CreateWallet();
            EnemyData data = CreateEnemyData(3);
            CreateEnemy(data, wallet, out EnemyHealth enemyHealth);

            enemyHealth.TakeRawDamage(100f);
            enemyHealth.TakeRawDamage(100f);

            Assert.That(wallet.CurrentSouls, Is.EqualTo(3));
            Assert.That(data.SoulReward, Is.EqualTo(3));
        }

        [Test]
        public void EnemySoulReward_ZeroRewardDoesNotChangeWallet()
        {
            PlayerSoulWallet wallet = CreateWallet();
            CreateEnemy(CreateEnemyData(0), wallet, out EnemyHealth enemyHealth);

            enemyHealth.TakeRawDamage(100f);

            Assert.That(wallet.CurrentSouls, Is.EqualTo(0));
        }

        [Test]
        public void EnemySoulReward_EnableDisableBeforeDeathDoesNotDuplicateSubscription()
        {
            PlayerSoulWallet wallet = CreateWallet();
            EnemySoulReward reward = CreateEnemy(CreateEnemyData(2), wallet, out EnemyHealth enemyHealth);

            InvokeLifecycle(reward, "OnDisable");
            InvokeLifecycle(reward, "OnEnable");
            InvokeLifecycle(reward, "OnDisable");
            InvokeLifecycle(reward, "OnEnable");
            enemyHealth.TakeRawDamage(100f);

            Assert.That(wallet.CurrentSouls, Is.EqualTo(2));
        }

        [Test]
        public void Prefabs_HaveSoulWalletAndEnemySoulReward()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Player/Player.prefab");
            GameObject slimePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Enemies/HumanRealm/Slime_F.prefab");

            Assert.That(playerPrefab, Is.Not.Null);
            Assert.That(slimePrefab, Is.Not.Null);
            Assert.That(playerPrefab.GetComponent<PlayerSoulWallet>(), Is.Not.Null);
            Assert.That(slimePrefab.GetComponent<EnemySoulReward>(), Is.Not.Null);
        }

        private PlayerSoulWallet CreateWallet()
        {
            GameObject gameObject = CreateInactiveGameObject("Wallet Test");
            PlayerSoulWallet wallet = gameObject.AddComponent<PlayerSoulWallet>();
            gameObject.SetActive(true);
            return wallet;
        }

        private EnemyData CreateEnemyData(int soulReward)
        {
            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
            data.SetAuthoringValues("Test Slime", EnemyRank.F, 30f, 5f, 3f, 2f, soulReward);
            createdObjects.Add(data);
            return data;
        }

        private EnemySoulReward CreateEnemy(EnemyData data, PlayerSoulWallet wallet, out EnemyHealth enemyHealth)
        {
            GameObject gameObject = CreateInactiveGameObject("Enemy Soul Reward Test");
            EnemyRuntime runtime = gameObject.AddComponent<EnemyRuntime>();
            Health health = gameObject.AddComponent<Health>();
            enemyHealth = gameObject.AddComponent<EnemyHealth>();
            EnemySoulReward reward = gameObject.AddComponent<EnemySoulReward>();
            SetPrivateField(runtime, "data", data);
            SetPrivateField(reward, "playerSoulWallet", wallet);
            InvokeLifecycle(runtime, "Awake");
            InvokeLifecycle(health, "Awake");
            InvokeLifecycle(enemyHealth, "Awake");
            InvokeLifecycle(reward, "Awake");
            InvokeLifecycle(reward, "OnEnable");
            return reward;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private static void InvokeLifecycle(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(target, null);
        }

        private GameObject CreateInactiveGameObject(string name)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.SetActive(false);
            createdObjects.Add(gameObject);
            return gameObject;
        }
    }
}
