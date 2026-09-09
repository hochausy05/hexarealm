using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Combat;
using HexaRealm.Enemy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HexaRealm.Tests.EditMode
{
    public sealed class EnemyRuntimeTests
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
        public void EnemyHealth_InitializesFromEnemyDataOnce()
        {
            EnemyData data = CreateEnemyData();
            EnemyHealth enemyHealth = CreateEnemy(data, out Health health, out _);

            Assert.That(enemyHealth.Health, Is.SameAs(health));
            Assert.That(health.MaxHealth, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void EnemyHealth_UsesDamageCalculatorAndDoesNotMutateData()
        {
            EnemyData data = CreateEnemyData();
            EnemyHealth enemyHealth = CreateEnemy(data, out Health health, out _);
            float maxHealth = data.MaxHealth;
            float attack = data.Attack;
            float defense = data.Defense;
            float speed = data.Speed;

            float appliedDamage = enemyHealth.TakeRawDamage(10f);

            Assert.That(appliedDamage, Is.EqualTo(DamageCalculator.CalculateFinalDamage(10f, 3f)).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(23f).Within(Tolerance));
            Assert.That(data.MaxHealth, Is.EqualTo(maxHealth).Within(Tolerance));
            Assert.That(data.Attack, Is.EqualTo(attack).Within(Tolerance));
            Assert.That(data.Defense, Is.EqualTo(defense).Within(Tolerance));
            Assert.That(data.Speed, Is.EqualTo(speed).Within(Tolerance));
        }

        [Test]
        public void EnemyHealth_DeathTransitionOccursOnlyOnce()
        {
            EnemyData data = CreateEnemyData();
            EnemyHealth enemyHealth = CreateEnemy(data, out Health health, out _);
            int deathCount = 0;
            health.Died += () => deathCount++;

            enemyHealth.TakeRawDamage(100f);
            enemyHealth.TakeRawDamage(100f);

            Assert.That(health.IsDead, Is.True);
            Assert.That(deathCount, Is.EqualTo(1));
        }

        [Test]
        public void EnemyMeleeAttack_ReadsAttackFromEnemyData()
        {
            EnemyData data = CreateEnemyData();
            CreateEnemy(data, out _, out EnemyMeleeAttack meleeAttack);

            Assert.That(meleeAttack.AttackValue, Is.EqualTo(data.Attack).Within(Tolerance));
        }

        [Test]
        public void SlimePrefab_HasEnemyRuntimeFoundation()
        {
            const string PrefabPath = "Assets/_Game/Prefabs/Enemies/HumanRealm/Slime_F.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyRuntime>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyRuntime>().Data, Is.Not.Null);
            Assert.That(prefab.GetComponent<Health>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyHealth>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<BasicEnemyAI>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyMeleeAttack>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<Rigidbody2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<SortingGroup>(), Is.Not.Null);

            Transform damageHitbox = prefab.transform.Find("DamageHitbox");
            Assert.That(damageHitbox, Is.Not.Null);
            Assert.That(damageHitbox.gameObject.layer, Is.EqualTo(LayerMask.NameToLayer("EnemyHitbox")));
            Assert.That(damageHitbox.GetComponent<Collider2D>(), Is.Not.Null);
        }

        private EnemyData CreateEnemyData()
        {
            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
            data.SetAuthoringValues("Test Slime", EnemyRank.F, 30f, 5f, 3f, 2f, 1);
            createdObjects.Add(data);
            return data;
        }

        private EnemyHealth CreateEnemy(EnemyData data, out Health health, out EnemyMeleeAttack meleeAttack)
        {
            GameObject gameObject = new GameObject("Enemy Test");
            gameObject.SetActive(false);
            createdObjects.Add(gameObject);

            EnemyRuntime runtime = gameObject.AddComponent<EnemyRuntime>();
            health = gameObject.AddComponent<Health>();
            EnemyHealth enemyHealth = gameObject.AddComponent<EnemyHealth>();
            meleeAttack = gameObject.AddComponent<EnemyMeleeAttack>();
            SetPrivateField(runtime, "data", data);
            InvokeLifecycle(runtime, "Awake");
            InvokeLifecycle(enemyHealth, "Awake");
            InvokeLifecycle(meleeAttack, "Awake");
            return enemyHealth;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(instance, value);
        }

        private static void InvokeLifecycle(object instance, string methodName)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(instance, null);
        }
    }
}
