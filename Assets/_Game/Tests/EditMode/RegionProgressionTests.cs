using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Combat;
using HexaRealm.Player;
using HexaRealm.Progression;
using NUnit.Framework;
using UnityEngine;

namespace HexaRealm.Tests.EditMode
{
    public sealed class RegionProgressionTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in createdObjects) Object.DestroyImmediate(createdObject);
            createdObjects.Clear();
        }

        [Test]
        public void HumanRealmGrant_IsIdempotentAndUnlocksRegion2()
        {
            PlayerRegionProgression progression = CreatePlayer(out _);

            Assert.That(progression.TeleportStoneCount, Is.EqualTo(0));
            Assert.That(progression.GrantRegionCompletion(RegionId.HumanRealm, RegionId.HumanRealm), Is.True);
            Assert.That(progression.IsRegionCompleted(RegionId.HumanRealm), Is.True);
            Assert.That(progression.HasTeleportStone(RegionId.HumanRealm), Is.True);
            Assert.That(progression.IsRegionUnlocked(RegionId.Region2), Is.True);
            Assert.That(progression.TeleportStoneCount, Is.EqualTo(1));
            Assert.That(progression.GrantRegionCompletion(RegionId.HumanRealm, RegionId.HumanRealm), Is.False);
            Assert.That(progression.TeleportStoneCount, Is.EqualTo(1));
        }

        [Test]
        public void CapUnlock_IsAbsoluteAndPreservesExistingUpgradeState()
        {
            PlayerRegionProgression regionProgression = CreatePlayer(out PlayerUpgradeProgression upgrades);
            SetPrivateField(upgrades, "attackUpgradeCount", 3);
            upgrades.SyncUpgradeModifiers();
            int totalBefore = upgrades.TotalUpgradeCount;
            float attackBefore = upgrades.PlayerStats.GetFinalStat(PlayerStatType.Attack);
            int soulsBefore = upgrades.SoulWallet.CurrentSouls;

            regionProgression.GrantRegionCompletion(RegionId.HumanRealm, RegionId.HumanRealm);
            Assert.That(upgrades.UnlockUpgradeCap(20), Is.True);
            Assert.That(upgrades.CurrentUpgradeCap, Is.EqualTo(20));
            Assert.That(upgrades.TotalUpgradeCount, Is.EqualTo(totalBefore));
            Assert.That(upgrades.PlayerStats.GetFinalStat(PlayerStatType.Attack), Is.EqualTo(attackBefore));
            Assert.That(upgrades.SoulWallet.CurrentSouls, Is.EqualTo(soulsBefore));
            Assert.That(upgrades.UnlockUpgradeCap(20), Is.True);
            Assert.That(upgrades.CurrentUpgradeCap, Is.EqualTo(20));
            Assert.That(upgrades.UnlockUpgradeCap(10), Is.False);
            Assert.That(upgrades.CurrentUpgradeCap, Is.EqualTo(20));
        }

        [Test]
        public void RegionBossReward_ResolvesActivePlayerAndGrantsProgressionOnlyOnce()
        {
            PlayerRegionProgression progression = CreatePlayer(out PlayerUpgradeProgression upgrades);
            GameObject boss = new GameObject("Region Boss Reward Test");
            boss.SetActive(false);
            createdObjects.Add(boss);
            Health health = boss.AddComponent<Health>();
            RegionBossProgressionReward reward = boss.AddComponent<RegionBossProgressionReward>();
            SetPrivateField(reward, "rewardData", CreateRewardData(20));
            boss.SetActive(true);

            health.ApplyDamage(999f);
            InvokePrivate(reward, "GrantOnce");

            Assert.That(progression.IsRegionCompleted(RegionId.HumanRealm), Is.True);
            Assert.That(progression.TeleportStoneCount, Is.EqualTo(1));
            Assert.That(upgrades.CurrentUpgradeCap, Is.EqualTo(20));
        }

        private PlayerRegionProgression CreatePlayer(out PlayerUpgradeProgression upgrades)
        {
            GameObject player = new GameObject("Region Progression Test");
            player.SetActive(false);
            createdObjects.Add(player);
            player.AddComponent<PlayerStats>();
            player.AddComponent<PlayerSoulWallet>();
            upgrades = player.AddComponent<PlayerUpgradeProgression>();
            PlayerRegionProgression progression = player.AddComponent<PlayerRegionProgression>();
            player.SetActive(true);
            return progression;
        }

        private RegionProgressionRewardData CreateRewardData(int cap)
        {
            RegionProgressionRewardData data = ScriptableObject.CreateInstance<RegionProgressionRewardData>();
            createdObjects.Add(data);
            SetPrivateField(data, "completedRegion", RegionId.HumanRealm);
            SetPrivateField(data, "teleportStoneRegion", RegionId.HumanRealm);
            SetPrivateField(data, "unlockedUpgradeCap", cap);
            return data;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(instance, value);
        }

        private static void InvokePrivate(object instance, string methodName)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(instance, null);
        }
    }
}
