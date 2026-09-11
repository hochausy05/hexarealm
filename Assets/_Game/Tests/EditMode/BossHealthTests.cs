using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Boss;
using HexaRealm.Combat;
using NUnit.Framework;
using UnityEngine;

namespace HexaRealm.Tests.EditMode
{
    public sealed class BossHealthTests
    {
        private const float Tolerance = .0001f;
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object item in created) Object.DestroyImmediate(item);
            created.Clear();
        }

        [Test]
        public void RawDamage_UsesBossDefenseWithoutMutatingBossData()
        {
            BossData data = CreateData(25f, 3f);
            BossHealth bossHealth = CreateBoss(data, out Health health);

            float applied = bossHealth.TakeRawDamage(10f);

            Assert.That(applied, Is.EqualTo(DamageCalculator.CalculateFinalDamage(10f, 3f)).Within(Tolerance));
            Assert.That(health.MaxHealth, Is.EqualTo(25f).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(18f).Within(Tolerance));
            Assert.That(data.MaxHealth, Is.EqualTo(25f).Within(Tolerance));
            Assert.That(data.Defense, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Death_FiresOnceAndLaterDamageCannotRewardOrRevive()
        {
            BossHealth bossHealth = CreateBoss(CreateData(5f, 0f), out Health health);
            int deaths = 0;
            health.Died += () => deaths++;

            Assert.That(bossHealth.TakeRawDamage(5f), Is.EqualTo(5f).Within(Tolerance));
            Assert.That(bossHealth.TakeRawDamage(5f), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(health.IsDead, Is.True);
            Assert.That(health.CurrentHealth, Is.Zero.Within(Tolerance));
            Assert.That(deaths, Is.EqualTo(1));
        }

        private BossHealth CreateBoss(BossData data, out Health health)
        {
            GameObject root = new GameObject("Boss Health Test");
            root.SetActive(false);
            created.Add(root);
            BossRuntime runtime = root.AddComponent<BossRuntime>();
            health = root.AddComponent<Health>();
            BossHealth bossHealth = root.AddComponent<BossHealth>();
            SetPrivateField(runtime, "data", data);
            root.SetActive(true);
            return bossHealth;
        }

        private BossData CreateData(float maxHealth, float defense)
        {
            BossData data = ScriptableObject.CreateInstance<BossData>();
            created.Add(data);
            SetPrivateField(data, "maxHealth", maxHealth);
            SetPrivateField(data, "defense", defense);
            SetPrivateField(data, "attack", 1f);
            SetPrivateField(data, "moveSpeed", 1f);
            return data;
        }

        private static void SetPrivateField(object instance, string name, object value)
        {
            FieldInfo field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(instance, value);
        }
    }
}
