using System.Reflection;
using HexaRealm.Combat;
using HexaRealm.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaRealm.Tests.EditMode
{
    public sealed class CombatMathTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void RawDamage_WithoutCritical_EqualsFinalAttack()
        {
            Assert.That(CombatMath.CalculateRawDamage(10f, false, 2f), Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void RawDamage_WithCritical_UsesMultiplier()
        {
            Assert.That(CombatMath.CalculateRawDamage(10f, true, 2f), Is.EqualTo(20f).Within(Tolerance));
        }

        [Test]
        public void CriticalChance_ClampsBelowZero()
        {
            Assert.That(CombatMath.CalculateCriticalChance(-0.5f, 0f, 0.01f), Is.EqualTo(0f));
        }

        [Test]
        public void CriticalChance_ClampsAboveOne()
        {
            Assert.That(CombatMath.CalculateCriticalChance(0.5f, 100f, 0.01f), Is.EqualTo(1f));
        }

        [Test]
        public void AttackInterval_DecreasesAsRageIncreases()
        {
            float noRage = CombatMath.CalculateAttackInterval(0.6f, 0f, 0.05f);
            float withRage = CombatMath.CalculateAttackInterval(0.6f, 10f, 0.05f);

            Assert.That(withRage, Is.LessThan(noRage));
        }

        [Test]
        public void AttackInterval_WithZeroRage_EqualsBaseInterval()
        {
            Assert.That(
                CombatMath.CalculateAttackInterval(0.6f, 0f, 0.05f),
                Is.EqualTo(0.6f).Within(Tolerance));
        }

        [TestCase(float.NaN, 0f, 0.05f)]
        [TestCase(float.PositiveInfinity, 0f, 0.05f)]
        [TestCase(-1f, 0f, 0.05f)]
        [TestCase(0f, 0f, 0.05f)]
        [TestCase(0.6f, float.PositiveInfinity, 0.05f)]
        [TestCase(0.6f, 10f, float.PositiveInfinity)]
        public void AttackInterval_InvalidValuesRemainFiniteAndPositive(
            float baseInterval,
            float rage,
            float speedPerRage)
        {
            float interval = CombatMath.CalculateAttackInterval(baseInterval, rage, speedPerRage);

            Assert.That(float.IsNaN(interval), Is.False);
            Assert.That(float.IsInfinity(interval), Is.False);
            Assert.That(interval, Is.GreaterThan(0f));
        }

        [Test]
        public void PlayerHealth_ImplementsRawDamageReceiver()
        {
            Assert.That(typeof(IRawDamageReceiver).IsAssignableFrom(typeof(PlayerHealth)), Is.True);
        }

        [Test]
        public void PlayerPrefab_HasCombatAndExpectedVisualHierarchy()
        {
            const string PlayerPrefabPath = "Assets/_Game/Prefabs/Player/Player.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerCombat>(), Is.Not.Null);

            Transform body = prefab.transform.Find("Body");
            Transform weaponSprite = prefab.transform.Find("WeaponSprite");
            Transform slashVFX = prefab.transform.Find("SlashVFX");

            Assert.That(body, Is.Not.Null);
            Assert.That(weaponSprite, Is.Not.Null);
            Assert.That(slashVFX, Is.Not.Null);
            Assert.That(weaponSprite.GetComponent<SpriteRenderer>().sortingOrder, Is.EqualTo(1));
            Assert.That(slashVFX.GetComponent<SpriteRenderer>().sortingOrder, Is.EqualTo(2));
            Assert.That(slashVFX.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void TechnicalTest_HasTwoColliderCombatDummyOnEnemyHitboxLayer()
        {
            const string ScenePath = "Assets/_Game/Scenes/Test/TechnicalTest.unity";
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            try
            {
                GameObject dummy = null;

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == "CombatDummy")
                    {
                        dummy = root;
                        break;
                    }
                }

                Assert.That(dummy, Is.Not.Null);
                Assert.That(dummy.layer, Is.EqualTo(LayerMask.NameToLayer("EnemyHitbox")));
                Assert.That(dummy.GetComponent<Health>(), Is.Not.Null);
                Assert.That(dummy.GetComponent<CombatDummyDamageReceiver>(), Is.Not.Null);
                Assert.That(dummy.GetComponentsInChildren<Collider2D>(), Has.Length.EqualTo(2));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void OneReceiverWithTwoColliders_IsDamagedOnce()
        {
            GameObject source = new GameObject("Source");
            GameObject target = new GameObject("Target");
            GameObject secondHitbox = new GameObject("Second Hitbox");

            try
            {
                Health health = target.AddComponent<Health>();
                health.Initialize(100f);
                target.AddComponent<CombatDummyDamageReceiver>();
                BoxCollider2D firstCollider = target.AddComponent<BoxCollider2D>();
                secondHitbox.transform.SetParent(target.transform);
                BoxCollider2D secondCollider = secondHitbox.AddComponent<BoxCollider2D>();

                MethodInfo method = typeof(PlayerCombat).GetMethod(
                    "ApplyDamageToUniqueReceivers",
                    BindingFlags.Static | BindingFlags.NonPublic);

                Assert.That(method, Is.Not.Null);
                int receiverCount = (int)method.Invoke(
                    null,
                    new object[] { new Collider2D[] { firstCollider, secondCollider }, 20f, source.transform });

                Assert.That(receiverCount, Is.EqualTo(1));
                Assert.That(health.CurrentHealth, Is.EqualTo(80f).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(target);

                if (secondHitbox != null)
                {
                    Object.DestroyImmediate(secondHitbox);
                }
            }
        }
    }
}
