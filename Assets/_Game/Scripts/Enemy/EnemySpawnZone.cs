using System;
using System.Collections.Generic;
using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Enemy
{
    /// <summary>
    /// Owns the spawn lifecycle for one enemy prefab type. Each respawn creates a new instance.
    /// </summary>
    public sealed class EnemySpawnZone : MonoBehaviour
    {
        [Header("Enemy")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform[] spawnPoints = Array.Empty<Transform>();

        [Header("Population")]
        [SerializeField, Min(0)] private int initialSpawnCount = 1;
        [SerializeField, Min(1)] private int maxAlive = 1;
        [SerializeField, Min(0f)] private float respawnDelay = 3f;
        [SerializeField, Min(0.01f)] private float respawnRetryInterval = 0.5f;

        [Header("Spawn Safety")]
        [SerializeField] private Transform player;
        [SerializeField, Min(0f)] private float minimumPlayerDistance = 3f;
        [SerializeField, Min(0f)] private float spawnClearanceRadius = 0.5f;
        [SerializeField] private LayerMask spawnBlockingMask;

        [Header("Cleanup")]
        [SerializeField, Min(0f)] private float corpseLifetime = 1f;

        private readonly Dictionary<Health, SpawnedEnemy> aliveEnemies = new Dictionary<Health, SpawnedEnemy>();
        private readonly List<float> pendingRespawnTimes = new List<float>();
        private readonly List<CorpseCleanup> pendingCorpseCleanup = new List<CorpseCleanup>();
        private int nextSpawnPointIndex;
        private bool hasInitialized;
        private bool hasWarnedInvalidPrefab;
        private bool hasWarnedMissingPlayer;

        public int AliveCount => aliveEnemies.Count;
        public int PendingRespawnCount => pendingRespawnTimes.Count;

        private void Start()
        {
            InitializePopulation();
        }

        private void Update()
        {
            CleanupExpiredCorpses();
            ProcessPendingRespawns();
        }

        private void OnDestroy()
        {
            foreach (SpawnedEnemy spawnedEnemy in aliveEnemies.Values)
            {
                if (spawnedEnemy.Health != null)
                {
                    spawnedEnemy.Health.Died -= spawnedEnemy.DeathHandler;
                }
            }

            aliveEnemies.Clear();
        }

        private void InitializePopulation()
        {
            if (hasInitialized)
            {
                return;
            }

            hasInitialized = true;
            int desiredCount = Mathf.Min(initialSpawnCount, maxAlive);

            for (int i = 0; i < desiredCount && AliveCount < maxAlive; i++)
            {
                if (!TrySpawnEnemy())
                {
                    pendingRespawnTimes.Add(Time.time + respawnRetryInterval);
                }
            }
        }

        private void ProcessPendingRespawns()
        {
            for (int i = pendingRespawnTimes.Count - 1; i >= 0; i--)
            {
                if (Time.time < pendingRespawnTimes[i] || AliveCount >= maxAlive)
                {
                    continue;
                }

                if (TrySpawnEnemy())
                {
                    pendingRespawnTimes.RemoveAt(i);
                }
                else
                {
                    pendingRespawnTimes[i] = Time.time + respawnRetryInterval;
                }
            }
        }

        private bool TrySpawnEnemy()
        {
            if (AliveCount >= maxAlive || !HasValidEnemyPrefab() || !TryGetValidSpawnPoint(out Transform spawnPoint))
            {
                return false;
            }

            GameObject instance = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            EnemyHealth enemyHealth = instance.GetComponent<EnemyHealth>();
            Health health = enemyHealth != null ? enemyHealth.Health : null;
            BasicEnemyAI enemyAI = instance.GetComponent<BasicEnemyAI>();

            if (health == null || enemyAI == null)
            {
                Debug.LogError("EnemySpawnZone instantiated an enemy without its required runtime foundation.", this);
                Destroy(instance);
                return false;
            }

            enemyAI.SetHomePosition(spawnPoint.position);
            Action deathHandler = () => HandleEnemyDied(health);
            aliveEnemies.Add(health, new SpawnedEnemy(health, deathHandler));
            health.Died += deathHandler;
            return true;
        }

        private void HandleEnemyDied(Health health)
        {
            if (health == null || !aliveEnemies.TryGetValue(health, out SpawnedEnemy spawnedEnemy))
            {
                return;
            }

            health.Died -= spawnedEnemy.DeathHandler;
            aliveEnemies.Remove(health);
            pendingRespawnTimes.Add(Time.time + respawnDelay);
            pendingCorpseCleanup.Add(new CorpseCleanup(health.gameObject, Time.time + corpseLifetime));
        }

        private void CleanupExpiredCorpses()
        {
            for (int i = pendingCorpseCleanup.Count - 1; i >= 0; i--)
            {
                CorpseCleanup cleanup = pendingCorpseCleanup[i];
                if (Time.time < cleanup.DueTime)
                {
                    continue;
                }

                if (cleanup.Instance != null)
                {
                    Destroy(cleanup.Instance);
                }

                pendingCorpseCleanup.RemoveAt(i);
            }
        }

        private bool TryGetValidSpawnPoint(out Transform spawnPoint)
        {
            spawnPoint = null;
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return false;
            }

            if (player == null)
            {
                if (!hasWarnedMissingPlayer)
                {
                    Debug.LogWarning("EnemySpawnZone is waiting for a Player Transform before spawning.", this);
                    hasWarnedMissingPlayer = true;
                }

                return false;
            }

            for (int offset = 0; offset < spawnPoints.Length; offset++)
            {
                int index = (nextSpawnPointIndex + offset) % spawnPoints.Length;
                Transform candidate = spawnPoints[index];
                if (!IsSpawnPointValid(candidate))
                {
                    continue;
                }

                nextSpawnPointIndex = (index + 1) % spawnPoints.Length;
                spawnPoint = candidate;
                return true;
            }

            return false;
        }

        private bool IsSpawnPointValid(Transform spawnPoint)
        {
            if (spawnPoint == null || Vector2.Distance(player.position, spawnPoint.position) < minimumPlayerDistance)
            {
                return false;
            }

            return spawnClearanceRadius <= 0f ||
                Physics2D.OverlapCircle(spawnPoint.position, spawnClearanceRadius, spawnBlockingMask) == null;
        }

        private bool HasValidEnemyPrefab()
        {
            if (enemyPrefab != null && enemyPrefab.GetComponent<EnemyRuntime>() != null &&
                enemyPrefab.GetComponent<EnemyHealth>() != null &&
                enemyPrefab.GetComponent<Health>() != null && enemyPrefab.GetComponent<BasicEnemyAI>() != null)
            {
                return true;
            }

            if (!hasWarnedInvalidPrefab)
            {
                Debug.LogError("EnemySpawnZone requires an enemy prefab with EnemyRuntime, EnemyHealth, Health, and BasicEnemyAI.", this);
                hasWarnedInvalidPrefab = true;
            }

            return false;
        }

        private void OnValidate()
        {
            initialSpawnCount = Mathf.Max(0, initialSpawnCount);
            maxAlive = Mathf.Max(1, maxAlive);
            initialSpawnCount = Mathf.Min(initialSpawnCount, maxAlive);
            respawnDelay = Mathf.Max(0f, respawnDelay);
            respawnRetryInterval = Mathf.Max(0.01f, respawnRetryInterval);
            minimumPlayerDistance = Mathf.Max(0f, minimumPlayerDistance);
            spawnClearanceRadius = Mathf.Max(0f, spawnClearanceRadius);
            corpseLifetime = Mathf.Max(0f, corpseLifetime);
        }

        private void OnDrawGizmosSelected()
        {
            if (spawnPoints == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, spawnClearanceRadius);
                }
            }
        }

        private readonly struct SpawnedEnemy
        {
            public SpawnedEnemy(Health health, Action deathHandler)
            {
                Health = health;
                DeathHandler = deathHandler;
            }

            public Health Health { get; }
            public Action DeathHandler { get; }
        }

        private readonly struct CorpseCleanup
        {
            public CorpseCleanup(GameObject instance, float dueTime)
            {
                Instance = instance;
                DueTime = dueTime;
            }

            public GameObject Instance { get; }
            public float DueTime { get; }
        }
    }
}
