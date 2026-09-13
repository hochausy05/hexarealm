using System;
using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Boss;
using HexaRealm.Boss.UI;
using HexaRealm.Combat;
using HexaRealm.Loot;
using HexaRealm.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HexaRealm.Tests.EditMode
{
    public sealed class BossPresentationRewardTests
    {
        private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = created.Count - 1; index >= 0; index--)
            {
                if (created[index] != null) UnityEngine.Object.DestroyImmediate(created[index]);
            }

            created.Clear();
        }

        [Test]
        public void BossHealthBar_MissingBossFailsSafelyWithSpecificMessage()
        {
            BossHealthBarUI ui = CreateUnconfiguredUI(out GameObject host);

            LogAssert.Expect(LogType.Error, "BossHealthBarUI: Boss is missing.");
            Assert.That(InvokePrivate<bool>(ui, "ValidateConfiguration"), Is.False);
            host.SetActive(true);

            Assert.That(host.activeSelf, Is.True);
            Assert.That(ui.enabled, Is.False);
        }

        [Test]
        public void BossHealthBar_DestroyedBossFailsSafelyWithSpecificMessage()
        {
            BossHealthBarUI ui = CreateConfiguredUI(out _, out BossController boss, out _, out _);
            UnityEngine.Object.DestroyImmediate(boss);

            LogAssert.Expect(LogType.Error, "BossHealthBarUI: Boss has been destroyed.");
            Assert.That(InvokePrivate<bool>(ui, "ValidateConfiguration"), Is.False);
        }

        [Test]
        public void BossHealthBar_DestroyedBossHealthFailsSafelyWithSpecificMessage()
        {
            BossHealthBarUI ui = CreateConfiguredUI(out _, out _, out _, out _);
            GameObject doomedBossHealthObject = CreateInactiveObject("Destroyed Boss Health Reference");
            BossHealth doomedBossHealth = doomedBossHealthObject.AddComponent<BossHealth>();
            SetPrivateField(ui, "bossHealth", doomedBossHealth);
            UnityEngine.Object.DestroyImmediate(doomedBossHealthObject);

            LogAssert.Expect(LogType.Error, "BossHealthBarUI: Boss Health has been destroyed.");
            Assert.That(InvokePrivate<bool>(ui, "ValidateConfiguration"), Is.False);
        }

        [Test]
        public void BossHealthBar_DestroyedSubscriptionsDoNotThrowDuringCleanup()
        {
            BossHealthBarUI ui = CreateConfiguredUI(out GameObject host, out BossController boss, out _, out _);
            InvokePrivate(ui, "Awake");
            InvokePrivate(ui, "OnEnable");
            UnityEngine.Object.DestroyImmediate(boss);

            Assert.DoesNotThrow(() => InvokePrivate(ui, "OnDisable"));
            Assert.That(ui, Is.Not.Null);
            Assert.That(host.activeSelf, Is.False);
        }

        [Test]
        public void BossHealthBar_DestroyedBossHealthDoesNotThrowDuringCleanup()
        {
            BossHealthBarUI ui = CreateConfiguredUI(out GameObject host, out BossController boss, out BossHealth bossHealth, out _);
            SetPrivateField(ui, "subscribedBoss", boss);
            SetPrivateField(ui, "subscribedHealth", bossHealth.Health);
            GameObject doomedBossHealthObject = CreateInactiveObject("Destroyed Boss Health Cleanup Reference");
            BossHealth doomedBossHealth = doomedBossHealthObject.AddComponent<BossHealth>();
            SetPrivateField(ui, "bossHealth", doomedBossHealth);
            UnityEngine.Object.DestroyImmediate(doomedBossHealthObject);

            Assert.DoesNotThrow(() => InvokePrivate(ui, "OnDisable"));
            Assert.That(ui, Is.Not.Null);
            Assert.That(host.activeSelf, Is.False);
        }

        [Test]
        public void BossHealthBar_FillCalculationUsesCurrentAndMaximumHealth()
        {
            BossHealthBarUI ui = CreateConfiguredUI(out _, out _, out BossHealth bossHealth, out Image fill);
            bossHealth.Health.ResetToMaxHealth(200f);

            InvokePrivate(ui, "UpdateFill", 0f, 50f);

            Assert.That(fill.fillAmount, Is.EqualTo(.25f).Within(.0001f));
        }

        [Test]
        public void BossHealthBar_TracksEngageHealthDisengageAndDeathWithoutPolling()
        {
            BossHealthBarUI ui = CreateConfiguredUI(
                out GameObject host,
                out BossController boss,
                out BossHealth bossHealth,
                out Image fill);
            GameObject barRoot = fill.transform.parent.gameObject;
            InvokePrivate(ui, "Awake");
            InvokePrivate(ui, "OnEnable");

            Assert.That(barRoot.activeSelf, Is.False);

            RaiseEvent(boss, "Engaged");
            Assert.That(barRoot.activeSelf, Is.True);
            Assert.That(fill.fillAmount, Is.EqualTo(1f).Within(.0001f));

            bossHealth.Health.ApplyDamage(25f);
            Assert.That(fill.fillAmount,
                Is.EqualTo(bossHealth.Health.CurrentHealth / bossHealth.Health.MaxHealth).Within(.0001f));

            RaiseEvent(boss, "Disengaged");
            Assert.That(barRoot.activeSelf, Is.False);

            RaiseEvent(boss, "Engaged");
            RaiseEvent(boss, "ResetToDormant");
            Assert.That(barRoot.activeSelf, Is.False);

            RaiseEvent(boss, "Engaged");
            RaiseEvent(boss, "Died");
            Assert.That(barRoot.activeSelf, Is.False);
            Assert.That(ui.enabled, Is.True);
        }

        [Test]
        public void BossReward_MissingReceiverDoesNotMarkGranted()
        {
            BossReward bossReward = CreateBossReward(CreateLoot(5));

            LogAssert.Expect(LogType.Error, "BossReward: PlayerLootReceiver is missing; reward was not granted.");
            InvokePrivate(bossReward, "GrantOnce");

            Assert.That(bossReward.Granted, Is.False);
        }

        [Test]
        public void BossReward_RejectedDeliveryDoesNotMarkGranted()
        {
            BossReward bossReward = CreateBossReward(CreateLoot(5));
            GameObject player = CreateInactiveObject("Incomplete Loot Receiver");
            PlayerLootReceiver receiver = player.AddComponent<PlayerLootReceiver>();
            player.SetActive(true);
            bossReward.SetRecipient(receiver);

            LogAssert.Expect(
                LogType.Error,
                "BossReward: PlayerLootReceiver rejected the reward; check that Reward Data is non-empty and the Player has the required Soul wallet/equipment inventory.");
            InvokePrivate(bossReward, "GrantOnce");

            Assert.That(bossReward.Granted, Is.False);
        }

        [Test]
        public void BossReward_SuccessfulDeliveryMarksGrantedAndOccursOnlyOnce()
        {
            BossReward bossReward = CreateBossReward(CreateLoot(5));
            PlayerLootReceiver receiver = CreateSoulReceiver(out PlayerSoulWallet wallet);
            bossReward.SetRecipient(receiver);

            InvokePrivate(bossReward, "GrantOnce");
            LogAssert.Expect(LogType.Warning, "BossReward: Reward was already granted; duplicate delivery was ignored.");
            InvokePrivate(bossReward, "GrantOnce");

            Assert.That(bossReward.Granted, Is.True);
            Assert.That(wallet.CurrentSouls, Is.EqualTo(5));
        }

        [Test]
        public void BossReward_DestroyedReceiverDoesNotMarkGranted()
        {
            BossReward bossReward = CreateBossReward(CreateLoot(5));
            PlayerLootReceiver receiver = CreateSoulReceiver(out _);
            bossReward.SetRecipient(receiver);
            UnityEngine.Object.DestroyImmediate(receiver.gameObject);

            LogAssert.Expect(LogType.Error, "BossReward: PlayerLootReceiver has been destroyed; reward was not granted.");
            Assert.DoesNotThrow(() => InvokePrivate(bossReward, "GrantOnce"));
            Assert.That(bossReward.Granted, Is.False);
        }

        [Test]
        public void BossReward_DestroyedRewardDataDoesNotMarkGranted()
        {
            LootBundleData loot = CreateLoot(5);
            BossReward bossReward = CreateBossReward(loot);
            bossReward.SetRecipient(CreateSoulReceiver(out _));
            UnityEngine.Object.DestroyImmediate(loot);

            LogAssert.Expect(LogType.Error, "BossReward: Reward data has been destroyed; reward was not granted.");
            Assert.DoesNotThrow(() => InvokePrivate(bossReward, "GrantOnce"));
            Assert.That(bossReward.Granted, Is.False);
        }

        private BossHealthBarUI CreateUnconfiguredUI(out GameObject host)
        {
            host = new GameObject("Boss Health UI Host", typeof(RectTransform));
            host.SetActive(false);
            created.Add(host);
            return host.AddComponent<BossHealthBarUI>();
        }

        private BossHealthBarUI CreateConfiguredUI(
            out GameObject host,
            out BossController boss,
            out BossHealth bossHealth,
            out Image fill)
        {
            GameObject bossObject = CreateInactiveObject("Boss UI Reference");
            boss = bossObject.AddComponent<BossController>();
            bossHealth = bossObject.GetComponent<BossHealth>();
            Health health = bossObject.GetComponent<Health>();
            SetPrivateField(bossHealth, "health", health);
            health.Initialize(100f);
            return CreateConfiguredUI(boss, out host, out _, out fill);
        }

        private BossHealthBarUI CreateConfiguredUI(
            BossController boss,
            out GameObject host,
            out GameObject barRoot,
            out Image fill)
        {
            host = new GameObject("Boss Health UI Host", typeof(RectTransform));
            host.SetActive(false);
            created.Add(host);
            barRoot = new GameObject("BarRoot", typeof(RectTransform));
            barRoot.transform.SetParent(host.transform);
            GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObject.transform.SetParent(barRoot.transform);
            fill = fillObject.GetComponent<Image>();

            BossHealthBarUI ui = host.AddComponent<BossHealthBarUI>();
            SetPrivateField(ui, "boss", boss);
            SetPrivateField(ui, "bossHealth", boss.GetComponent<BossHealth>());
            SetPrivateField(ui, "fill", fill);
            SetPrivateField(ui, "barRoot", barRoot);
            return ui;
        }

        private BossReward CreateBossReward(LootBundleData loot)
        {
            GameObject boss = CreateInactiveObject("Boss Reward Test");
            BossReward bossReward = boss.AddComponent<BossReward>();
            SetPrivateField(bossReward, "reward", loot);
            return bossReward;
        }

        private LootBundleData CreateLoot(int souls)
        {
            LootBundleData loot = ScriptableObject.CreateInstance<LootBundleData>();
            loot.SetAuthoringValues(souls);
            created.Add(loot);
            return loot;
        }

        private PlayerLootReceiver CreateSoulReceiver(out PlayerSoulWallet wallet)
        {
            GameObject player = CreateInactiveObject("Boss Reward Player");
            wallet = player.AddComponent<PlayerSoulWallet>();
            PlayerLootReceiver receiver = player.AddComponent<PlayerLootReceiver>();
            player.SetActive(true);
            return receiver;
        }

        private GameObject CreateInactiveObject(string name)
        {
            GameObject instance = new GameObject(name);
            instance.SetActive(false);
            created.Add(instance);
            return instance;
        }

        private static void InvokePrivate(object instance, string methodName, params object[] arguments)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(instance, arguments);
        }

        private static T InvokePrivate<T>(object instance, string methodName, params object[] arguments)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (T)method.Invoke(instance, arguments);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(instance, value);
        }

        private static void RaiseEvent(object instance, string eventName)
        {
            FieldInfo field = instance.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            MulticastDelegate callback = field.GetValue(instance) as MulticastDelegate;
            Assert.That(callback, Is.Not.Null);
            callback.DynamicInvoke();
        }
    }
}
