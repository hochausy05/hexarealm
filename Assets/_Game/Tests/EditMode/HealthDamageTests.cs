using System.Collections.Generic;
using System.Reflection;
using HexaRealm.Combat;
using HexaRealm.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HexaRealm.Tests.EditMode
{
    public sealed class HealthDamageTests
    {
        private const float Tolerance = 0.0001f;
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public void Initialize_SetsFullHealthAndAliveState()
        {
            Health health = CreateHealth(100f);

            Assert.That(health.MaxHealth, Is.EqualTo(100f).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(100f).Within(Tolerance));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void ApplyDamage_ReducesCurrentHealth()
        {
            Health health = CreateHealth(100f);

            health.ApplyDamage(30f);

            Assert.That(health.CurrentHealth, Is.EqualTo(70f).Within(Tolerance));
        }

        [Test]
        public void LethalDamage_ClampsAtZeroAndKills()
        {
            Health health = CreateHealth(20f);

            health.ApplyDamage(50f);

            Assert.That(health.CurrentHealth, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void DamageAfterDeath_DoesNotFireDiedAgain()
        {
            Health health = CreateHealth(5f);
            int deathCount = 0;
            health.Died += () => deathCount++;

            health.ApplyDamage(10f);
            health.ApplyDamage(10f);
            health.ApplyDamage(10f);

            Assert.That(health.CurrentHealth, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(deathCount, Is.EqualTo(1));
        }

        [Test]
        public void Heal_ClampsAtMaxHealth()
        {
            Health health = CreateHealth(100f);
            health.ApplyDamage(50f);

            health.Heal(100f);

            Assert.That(health.CurrentHealth, Is.EqualTo(100f).Within(Tolerance));
        }

        [Test]
        public void Heal_WhenDead_DoesNotRevive()
        {
            Health health = CreateHealth(100f);
            health.ApplyDamage(100f);

            health.Heal(50f);

            Assert.That(health.CurrentHealth, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void IncreaseMaxHealth_PreservesAbsoluteCurrentHealth()
        {
            Health health = CreateHealth(100f);
            health.ApplyDamage(50f);

            health.SetMaxHealth(120f);

            Assert.That(health.MaxHealth, Is.EqualTo(120f).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(50f).Within(Tolerance));
        }

        [Test]
        public void DecreaseMaxHealth_ClampsCurrentHealth()
        {
            Health health = CreateHealth(120f);
            health.ApplyDamage(10f);

            health.SetMaxHealth(100f);

            Assert.That(health.MaxHealth, Is.EqualTo(100f).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(100f).Within(Tolerance));
        }

        [Test]
        public void RepeatedMaxHealthChanges_DoNotHeal()
        {
            Health health = CreateHealth(100f);
            health.ApplyDamage(50f);

            health.SetMaxHealth(120f);
            Assert.That(health.CurrentHealth, Is.EqualTo(50f).Within(Tolerance));
            health.SetMaxHealth(100f);
            Assert.That(health.CurrentHealth, Is.EqualTo(50f).Within(Tolerance));
            health.SetMaxHealth(120f);

            Assert.That(health.CurrentHealth, Is.EqualTo(50f).Within(Tolerance));
        }

        [Test]
        public void ChangeMaxHealth_WhenDead_DoesNotRevive()
        {
            Health health = CreateHealth(100f);
            health.ApplyDamage(100f);

            health.SetMaxHealth(150f);

            Assert.That(health.MaxHealth, Is.EqualTo(150f).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(health.IsDead, Is.True);
        }

        [TestCase(10f, 3f, 7f)]
        [TestCase(10f, 100f, 1f)]
        [TestCase(10f, -5f, 10f)]
        [TestCase(0f, 3f, 0f)]
        [TestCase(-10f, 3f, 0f)]
        public void DamageCalculator_ReturnsExpectedPrototypeDamage(float rawDamage, float defense, float expected)
        {
            float result = DamageCalculator.CalculateFinalDamage(rawDamage, defense);

            Assert.That(result, Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void NegativeDamageAndHealing_DoNotChangeHealth()
        {
            Health health = CreateHealth(100f);
            health.ApplyDamage(30f);

            health.ApplyDamage(-10f);
            health.Heal(-10f);

            Assert.That(health.CurrentHealth, Is.EqualTo(70f).Within(Tolerance));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void Initialize_WhenAlreadyInitialized_DoesNotRefill()
        {
            Health health = CreateHealth(100f);
            health.ApplyDamage(50f);

            health.Initialize(120f);

            Assert.That(health.MaxHealth, Is.EqualTo(100f).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(50f).Within(Tolerance));
        }

        [Test]
        public void VitalityChange_UpdatesMaxHealthWithoutHealing()
        {
            CreatePlayerHealth(out PlayerStats stats, out Health health, out PlayerHealth playerHealth);
            playerHealth.TakeRawDamage(55f);
            int vitalityNotificationCount = 0;
            float callbackObservedFinalValue = 0f;

            stats.StatChanged += (type, oldFinalValue, newFinalValue) =>
            {
                if (type != PlayerStatType.Vitality)
                {
                    return;
                }

                vitalityNotificationCount++;
                callbackObservedFinalValue = stats.GetFinalStat(type);
            };

            stats.SetEquipmentModifier(PlayerStatType.Vitality, 20f);
            stats.SetEquipmentModifier(PlayerStatType.Vitality, 20f);

            Assert.That(health.MaxHealth, Is.EqualTo(120f).Within(Tolerance));
            Assert.That(health.CurrentHealth, Is.EqualTo(50f).Within(Tolerance));
            Assert.That(vitalityNotificationCount, Is.EqualTo(1));
            Assert.That(callbackObservedFinalValue, Is.EqualTo(120f).Within(Tolerance));
        }

        [Test]
        public void TakeRawDamage_ReadsCurrentFinalDefense()
        {
            CreatePlayerHealth(out PlayerStats stats, out Health health, out PlayerHealth playerHealth);

            playerHealth.TakeRawDamage(10f);
            stats.SetEquipmentModifier(PlayerStatType.Defense, 4f);
            playerHealth.TakeRawDamage(10f);

            Assert.That(health.CurrentHealth, Is.EqualTo(94f).Within(Tolerance));
        }

        [Test]
        public void PlayerPrefab_HasHealthIntegrationWithoutRemovingPlayerFoundation()
        {
            const string PlayerPrefabPath = "Assets/_Game/Prefabs/Player/Player.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerController>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerDash>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerStats>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<Rigidbody2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<Collider2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<SortingGroup>(), Is.Not.Null);
            Assert.That(prefab.transform.Find("Body"), Is.Not.Null);

            foreach (Component component in prefab.GetComponents<Component>())
            {
                Assert.That(component, Is.Not.Null, "Player prefab contains a missing component script.");
            }

            Health health = prefab.GetComponent<Health>();
            PlayerHealth playerHealth = prefab.GetComponent<PlayerHealth>();

            Assert.That(health, Is.Not.Null);
            Assert.That(playerHealth, Is.Not.Null);
            Assert.That(playerHealth.Health, Is.SameAs(health));
        }

        private Health CreateHealth(float maxHealth)
        {
            GameObject gameObject = CreateInactiveGameObject("Health Test");
            Health health = gameObject.AddComponent<Health>();
            health.Initialize(maxHealth);
            gameObject.SetActive(true);
            return health;
        }

        private void CreatePlayerHealth(
            out PlayerStats stats,
            out Health health,
            out PlayerHealth playerHealth)
        {
            GameObject gameObject = CreateInactiveGameObject("Player Health Test");
            stats = gameObject.AddComponent<PlayerStats>();
            health = gameObject.AddComponent<Health>();
            playerHealth = gameObject.AddComponent<PlayerHealth>();
            InvokeLifecycle(playerHealth, "Awake");
            InvokeLifecycle(playerHealth, "OnEnable");
        }

        private static void InvokeLifecycle(PlayerHealth playerHealth, string methodName)
        {
            MethodInfo method = typeof(PlayerHealth).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            method.Invoke(playerHealth, null);
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
