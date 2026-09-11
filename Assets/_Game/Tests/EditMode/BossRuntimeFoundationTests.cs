using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Boss;
using HexaRealm.Boss.Attacks;
using HexaRealm.Boss.UI;
using HexaRealm.Loot;
using HexaRealm.Player;
using HexaRealm.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HexaRealm.Tests.EditMode
{
    public sealed class BossRuntimeFoundationTests
    {
        private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = created.Count - 1; index >= 0; index--) UnityEngine.Object.DestroyImmediate(created[index]);
            created.Clear();
        }

        [Test]
        public void BossController_DoesNotAutomaticallyAddCombatController()
        {
            GameObject root = CreateInactiveObject("Boss Controller Dependency Test");

            root.AddComponent<BossController>();

            Assert.That(root.GetComponents<BossCombatController>(), Is.Empty);
        }

        [Test]
        public void BossCombatController_ResolvesOneAttackFromComponentOrder()
        {
            BossController controller = CreateValidBoss(out BossCombatController combat);

            Assert.That(combat.AttackCount, Is.EqualTo(1));
            Assert.That(combat.HasResolvedReferences, Is.True);
            Assert.That(controller.HasResolvedReferences, Is.True);
        }

        [Test]
        public void BossData_RejectsEveryNonFiniteNumericStat()
        {
            string[] fields = { "maxHealth", "attack", "defense", "moveSpeed" };
            float[] invalidValues = { float.NaN, float.PositiveInfinity, float.NegativeInfinity };

            foreach (string field in fields)
            {
                foreach (float invalidValue in invalidValues)
                {
                    BossData data = ScriptableObject.CreateInstance<BossData>();
                    created.Add(data);
                    SetPrivateField(data, field, invalidValue);
                    Assert.That(data.IsValid, Is.False, $"{field} accepted {invalidValue}.");
                }
            }
        }

        [Test]
        public void BossReward_FailedDeliveryDoesNotMarkGranted()
        {
            GameObject boss = CreateInactiveObject("Boss Reward Failure Test");
            BossReward reward = boss.AddComponent<BossReward>();
            LootBundleData loot = ScriptableObject.CreateInstance<LootBundleData>();
            created.Add(loot);
            loot.SetAuthoringValues(5);
            SetPrivateField(reward, "reward", loot);

            LogAssert.Expect(LogType.Error, "BossReward requires a PlayerLootReceiver recipient before it can grant loot.");
            InvokePrivate(reward, "GrantOnce");

            Assert.That(reward.Granted, Is.False);
        }

        [Test]
        public void BossReward_SuccessfulDeliveryCannotDuplicateSouls()
        {
            GameObject player = CreateInactiveObject("Boss Reward Player");
            PlayerSoulWallet wallet = player.AddComponent<PlayerSoulWallet>();
            PlayerLootReceiver receiver = player.AddComponent<PlayerLootReceiver>();
            player.SetActive(true);

            GameObject boss = CreateInactiveObject("Boss Reward Success Test");
            BossReward reward = boss.AddComponent<BossReward>();
            LootBundleData loot = ScriptableObject.CreateInstance<LootBundleData>();
            created.Add(loot);
            loot.SetAuthoringValues(5);
            SetPrivateField(reward, "reward", loot);
            reward.SetRecipient(receiver);

            InvokePrivate(reward, "GrantOnce");
            InvokePrivate(reward, "GrantOnce");

            Assert.That(reward.Granted, Is.True);
            Assert.That(wallet.CurrentSouls, Is.EqualTo(5));
        }

        [Test]
        public void BossHealthBar_InvalidSelfRootKeepsHostActive()
        {
            GameObject host = CreateInactiveObject("Boss Health Bar Host");
            BossHealthBarUI ui = host.AddComponent<BossHealthBarUI>();
            SetPrivateField(ui, "barRoot", host);

            LogAssert.Expect(
                LogType.Error,
                "BossHealthBarUI requires a BossController, its BossHealth with Health, an Image fill, and a separate visual barRoot that does not contain the UI host.");
            host.SetActive(true);

            Assert.That(host.activeSelf, Is.True);
            Assert.That(ui.enabled, Is.False);
        }

        [Test]
        public void BossArena_DisengagesOnlyAfterAllPlayerCollidersExit()
        {
            BossController boss = CreateValidBoss(out _);
            GameObject player = CreateInactiveObject("Multi Collider Player");
            player.AddComponent<PlayerHealth>();
            BoxCollider2D firstCollider = CreateChildCollider(player.transform, "First Collider");
            BoxCollider2D secondCollider = CreateChildCollider(player.transform, "Second Collider");
            player.SetActive(true);

            GameObject arenaObject = CreateInactiveObject("Boss Arena Test");
            arenaObject.AddComponent<BoxCollider2D>();
            BossArena arena = arenaObject.AddComponent<BossArena>();
            SetPrivateField(arena, "boss", boss);
            arenaObject.SetActive(true);

            int engagedCount = 0;
            int disengagedCount = 0;
            boss.Engaged += () => engagedCount++;
            boss.Disengaged += () => disengagedCount++;

            InvokeTrigger(arena, "OnTriggerEnter2D", firstCollider);
            InvokeTrigger(arena, "OnTriggerEnter2D", firstCollider);
            InvokeTrigger(arena, "OnTriggerEnter2D", secondCollider);
            InvokeTrigger(arena, "OnTriggerExit2D", firstCollider);
            InvokeTrigger(arena, "OnTriggerExit2D", firstCollider);

            Assert.That(engagedCount, Is.EqualTo(1));
            Assert.That(disengagedCount, Is.Zero);
            Assert.That(boss.CurrentState, Is.EqualTo(BossController.BossState.Engaged));

            InvokeTrigger(arena, "OnTriggerExit2D", secondCollider);
            InvokeTrigger(arena, "OnTriggerExit2D", secondCollider);

            Assert.That(disengagedCount, Is.EqualTo(1));
            Assert.That(boss.CurrentState, Is.EqualTo(BossController.BossState.Returning));
        }

        private BossController CreateValidBoss(out BossCombatController combat)
        {
            GameObject root = CreateInactiveObject("Valid Boss Test");
            BossController controller = root.AddComponent<BossController>();
            root.AddComponent<BossMeleeAttack>();
            combat = root.AddComponent<BossCombatController>();

            BossData data = ScriptableObject.CreateInstance<BossData>();
            created.Add(data);
            SetPrivateField(root.GetComponent<BossRuntime>(), "data", data);
            root.SetActive(true);
            return controller;
        }

        private GameObject CreateInactiveObject(string name)
        {
            var instance = new GameObject(name);
            instance.SetActive(false);
            created.Add(instance);
            return instance;
        }

        private static BoxCollider2D CreateChildCollider(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent);
            return child.AddComponent<BoxCollider2D>();
        }

        private static void InvokeTrigger(BossArena arena, string methodName, Collider2D collider)
        {
            MethodInfo method = typeof(BossArena).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(arena, new object[] { collider });
        }

        private static void InvokePrivate(object instance, string methodName)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(instance, null);
        }

        private static void SetPrivateField(object instance, string name, object value)
        {
            FieldInfo field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(instance, value);
        }
    }
}
