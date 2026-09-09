using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Combat;
using HexaRealm.Enemy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HexaRealm.Tests.EditMode
{
    public sealed class EnemySpawnZoneTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    Object.DestroyImmediate(createdObject);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void InitialPopulation_SpawnsConfiguredCountWithoutExceedingMaxAlive()
        {
            EnemySpawnZone zone = CreateZone(initialSpawnCount: 1, maxAlive: 3);

            Initialize(zone);

            Assert.That(zone.AliveCount, Is.EqualTo(1));
            Assert.That(zone.PendingRespawnCount, Is.EqualTo(0));
        }

        [Test]
        public void InitialPopulation_ClampsToMaxAlive()
        {
            EnemySpawnZone zone = CreateZone(initialSpawnCount: 5, maxAlive: 2);

            Initialize(zone);

            Assert.That(zone.AliveCount, Is.EqualTo(2));
        }

        [Test]
        public void Death_RemovesAliveEnemyAndSchedulesOneRespawn()
        {
            EnemySpawnZone zone = CreateZone(initialSpawnCount: 1, maxAlive: 1);
            Initialize(zone);
            Health spawnedHealth = GetOnlyAliveHealth(zone);

            spawnedHealth.ApplyDamage(999f);
            spawnedHealth.ApplyDamage(999f);

            Assert.That(zone.AliveCount, Is.EqualTo(0));
            Assert.That(zone.PendingRespawnCount, Is.EqualTo(1));
        }

        [Test]
        public void SpawnPointNearPlayer_IsRejectedAndRemainsPending()
        {
            EnemySpawnZone zone = CreateZone(initialSpawnCount: 1, maxAlive: 1, playerPosition: Vector2.zero,
                spawnPointPositions: new[] { Vector2.one });

            Initialize(zone);

            Assert.That(zone.AliveCount, Is.EqualTo(0));
            Assert.That(zone.PendingRespawnCount, Is.EqualTo(1));
        }

        [Test]
        public void BlockedSpawnPoint_IsRejectedAndRemainsPending()
        {
            EnemySpawnZone zone = CreateZone(initialSpawnCount: 1, maxAlive: 1);
            Transform spawnPoint = GetPrivateField<Transform[]>(zone, "spawnPoints")[0];
            GameObject blocker = new GameObject("Spawn Blocker");
            blocker.layer = LayerMask.NameToLayer("Enemy");
            blocker.transform.position = spawnPoint.position;
            blocker.AddComponent<CircleCollider2D>().radius = 1f;
            createdObjects.Add(blocker);
            SetPrivateField(zone, "spawnClearanceRadius", 0.5f);
            SetPrivateField(zone, "spawnBlockingMask", LayerMask.GetMask("Enemy"));
            Physics2D.SyncTransforms();

            Initialize(zone);

            Assert.That(zone.AliveCount, Is.EqualTo(0));
            Assert.That(zone.PendingRespawnCount, Is.EqualTo(1));
        }

        [Test]
        public void InitializePopulation_IsIdempotentAfterDisableEnable()
        {
            EnemySpawnZone zone = CreateZone(initialSpawnCount: 1, maxAlive: 3);
            Initialize(zone);
            zone.enabled = false;
            zone.enabled = true;
            Initialize(zone);

            Assert.That(zone.AliveCount, Is.EqualTo(1));
        }

        [Test]
        public void SpawnedEnemy_UsesSpawnPointAsAiHomePosition()
        {
            Vector2 spawnPosition = new Vector2(-4f, 2f);
            EnemySpawnZone zone = CreateZone(initialSpawnCount: 1, maxAlive: 1,
                spawnPointPositions: new[] { spawnPosition });
            Initialize(zone);
            BasicEnemyAI ai = GetOnlyAliveHealth(zone).GetComponent<BasicEnemyAI>();

            Assert.That(ai.HomePosition, Is.EqualTo(spawnPosition));
        }

        private EnemySpawnZone CreateZone(
            int initialSpawnCount,
            int maxAlive,
            Vector2? playerPosition = null,
            Vector2[] spawnPointPositions = null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Game/Prefabs/Enemies/HumanRealm/Slime_F.prefab");
            Assert.That(prefab, Is.Not.Null);

            GameObject zoneObject = new GameObject("Enemy Spawn Zone Test");
            createdObjects.Add(zoneObject);
            EnemySpawnZone zone = zoneObject.AddComponent<EnemySpawnZone>();

            GameObject player = new GameObject("Player Test");
            player.transform.position = playerPosition ?? new Vector2(10f, 0f);
            createdObjects.Add(player);

            Vector2[] positions = spawnPointPositions ?? new[] { Vector2.zero, new Vector2(2f, 0f), new Vector2(4f, 0f) };
            Transform[] points = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject point = new GameObject($"Spawn Point {i}");
                point.transform.position = positions[i];
                createdObjects.Add(point);
                points[i] = point.transform;
            }

            SetPrivateField(zone, "enemyPrefab", prefab);
            SetPrivateField(zone, "spawnPoints", points);
            SetPrivateField(zone, "initialSpawnCount", initialSpawnCount);
            SetPrivateField(zone, "maxAlive", maxAlive);
            SetPrivateField(zone, "player", player.transform);
            SetPrivateField(zone, "minimumPlayerDistance", 3f);
            SetPrivateField(zone, "spawnClearanceRadius", 0f);
            SetPrivateField(zone, "spawnBlockingMask", (LayerMask)0);
            return zone;
        }

        private void Initialize(EnemySpawnZone zone)
        {
            MethodInfo start = typeof(EnemySpawnZone).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(start, Is.Not.Null);
            start.Invoke(zone, null);

            foreach (Health health in GetAliveHealths(zone))
            {
                createdObjects.Add(health.gameObject);
            }
        }

        private static Health GetOnlyAliveHealth(EnemySpawnZone zone)
        {
            List<Health> healths = GetAliveHealths(zone);
            Assert.That(healths, Has.Count.EqualTo(1));
            return healths[0];
        }

        private static List<Health> GetAliveHealths(EnemySpawnZone zone)
        {
            FieldInfo field = typeof(EnemySpawnZone).GetField("aliveEnemies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            IDictionary enemies = field.GetValue(zone) as IDictionary;
            Assert.That(enemies, Is.Not.Null);

            var healths = new List<Health>();
            foreach (Health health in enemies.Keys)
            {
                healths.Add(health);
            }

            return healths;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(instance, value);
        }

        private static T GetPrivateField<T>(object instance, string fieldName)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (T)field.GetValue(instance);
        }
    }
}
