// LEGACY: Historical one-off Boss authoring support. Do not use this file for current scene or prefab generation.

using System;
using HexaRealm.Boss;
using HexaRealm.Boss.Attacks;
using HexaRealm.Combat;
using HexaRealm.Core;
using HexaRealm.Loot;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HexaRealm.EditorTools
{
    /// <summary>Creates, wires, saves, reloads, and validates the shared Boss prefab contract only.</summary>
    public static class BossPrefabAuthoringUtility
    {
        public static GameObject CreateAndSaveCommonPrefab(
            string prefabPath,
            BossData data,
            LootBundleData loot,
            Sprite sprite,
            Color bodyColor,
            Vector2 physicalSize,
            Vector2 physicalOffset,
            float visualScale)
        {
            if (data == null || !data.IsValid) throw new InvalidOperationException("BOSS_BUILD_FAILED: BossData is missing or invalid.");
            if (loot == null) throw new InvalidOperationException("BOSS_BUILD_FAILED: BossReward requires a LootBundleData asset.");

            GameObject root = new GameObject(
                System.IO.Path.GetFileNameWithoutExtension(prefabPath),
                typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(SortingGroup), typeof(TopDownSorting),
                typeof(BossRuntime), typeof(Health), typeof(BossHealth), typeof(BossController),
                typeof(BossCombatController), typeof(BossReward), typeof(BossMeleeAttack), typeof(BossChargeAttack));

            try
            {
                ConfigurePhysicalRoot(root, physicalSize, physicalOffset);
                CreateDamageHitbox(root.transform, physicalSize, physicalOffset);
                CreatePresentation(root.transform, sprite, bodyColor, visualScale);
                WireCommonComponents(root, data, loot);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            GameObject saved = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (saved == null) throw new InvalidOperationException("BOSS_BUILD_FAILED: Saved prefab could not be reloaded.");
            ValidateCommonPrefab(saved);
            return saved;
        }

        public static void ValidateCommonPrefab(GameObject root)
        {
            if (root == null) throw new InvalidOperationException("BOSS_BUILD_FAILED: Prefab is null after save.");
            int enemyLayer = RequireLayer("Enemy");
            if (root.layer != enemyLayer) throw new InvalidOperationException("BOSS_BUILD_FAILED: Boss root must use Enemy layer.");

            BossRuntime runtime = root.GetComponent<BossRuntime>();
            Health health = root.GetComponent<Health>();
            BossHealth bossHealth = root.GetComponent<BossHealth>();
            BossController controller = root.GetComponent<BossController>();
            BossCombatController combat = root.GetComponent<BossCombatController>();
            BossReward reward = root.GetComponent<BossReward>();
            if (runtime == null || !runtime.HasValidData || health == null || bossHealth == null || !bossHealth.HasResolvedReferences ||
                controller == null || !controller.HasResolvedReferences || combat == null || !combat.HasResolvedReferences ||
                reward == null || reward.Reward == null)
            {
                throw new InvalidOperationException("BOSS_BUILD_FAILED: Common Boss component wiring is incomplete.");
            }

            BossAttackBase[] attacks = root.GetComponents<BossAttackBase>();
            if (combat.AttackCount != 2 || attacks.Length != 2 || root.GetComponent<BossMeleeAttack>() == null || root.GetComponent<BossChargeAttack>() == null)
            {
                throw new InvalidOperationException("BOSS_BUILD_FAILED: BossCombatController must contain exactly the intended melee and charge attacks.");
            }

            ValidateDamageHitbox(root);
        }

        public static void ValidateDamageHitbox(GameObject root)
        {
            Transform hitbox = root != null ? FindDescendant(root.transform, "DamageHitbox") : null;
            if (hitbox == null || hitbox == root.transform || !hitbox.IsChildOf(root.transform) || !hitbox.gameObject.activeSelf || hitbox.gameObject.layer != RequireLayer("EnemyHitbox"))
            {
                throw new InvalidOperationException("BOSS_BUILD_FAILED: DamageHitbox is missing, inactive, misplaced, or not on EnemyHitbox.");
            }

            Collider2D trigger = hitbox.GetComponent<Collider2D>();
            if (trigger == null || !trigger.enabled || !trigger.isTrigger)
            {
                throw new InvalidOperationException("BOSS_BUILD_FAILED: DamageHitbox requires an enabled trigger Collider2D.");
            }

            BossHealth rootBossHealth = root.GetComponent<BossHealth>();
            if (rootBossHealth == null || !(rootBossHealth is IRawDamageReceiver))
            {
                throw new InvalidOperationException("BOSS_BUILD_FAILED: Boss root requires a BossHealth IRawDamageReceiver.");
            }

            // Match PlayerCombat: inspect MonoBehaviours at each parent transform and
            // select the first IRawDamageReceiver found while walking toward the root.
            IRawDamageReceiver resolvedReceiver = FindRawDamageReceiver(hitbox);
            if (!ReferenceEquals(resolvedReceiver, rootBossHealth))
            {
                throw new InvalidOperationException("BOSS_BUILD_FAILED: DamageHitbox cannot resolve the root BossHealth receiver.");
            }
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == name) return child;
                Transform nested = FindDescendant(child, name);
                if (nested != null) return nested;
            }

            return null;
        }

        private static IRawDamageReceiver FindRawDamageReceiver(Transform start)
        {
            for (Transform current = start; current != null; current = current.parent)
            {
                foreach (MonoBehaviour behaviour in current.GetComponents<MonoBehaviour>())
                {
                    if (behaviour is IRawDamageReceiver receiver) return receiver;
                }
            }

            return null;
        }

        private static void ConfigurePhysicalRoot(GameObject root, Vector2 size, Vector2 offset)
        {
            root.layer = RequireLayer("Enemy");
            Rigidbody2D body = root.GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            CapsuleCollider2D physical = root.GetComponent<CapsuleCollider2D>();
            physical.size = size;
            physical.offset = offset;
            root.GetComponent<SortingGroup>().sortingLayerName = "Characters";
        }

        private static void CreateDamageHitbox(Transform parent, Vector2 size, Vector2 localPosition)
        {
            GameObject hitbox = new GameObject("DamageHitbox", typeof(CapsuleCollider2D));
            hitbox.transform.SetParent(parent, false);
            hitbox.transform.localPosition = localPosition;
            hitbox.layer = RequireLayer("EnemyHitbox");
            CapsuleCollider2D collider = hitbox.GetComponent<CapsuleCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
            collider.offset = Vector2.zero;
        }

        private static void CreatePresentation(Transform root, Sprite sprite, Color bodyColor, float visualScale)
        {
            GameObject body = new GameObject("Body", typeof(SpriteRenderer));
            body.transform.SetParent(root, false);
            body.transform.localPosition = new Vector3(0f, .25f, 0f);
            body.transform.localScale = Vector3.one * visualScale;
            SpriteRenderer renderer = body.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = bodyColor;
            renderer.sortingLayerName = "Characters";

            Transform telegraphs = new GameObject("Telegraphs").transform;
            telegraphs.SetParent(root, false);
            CreateTelegraph("MeleeTelegraph", telegraphs, new Color(1f, .25f, .1f, .5f), new Vector3(11f, 7f, 1f));
            CreateTelegraph("ChargeTelegraph", telegraphs, new Color(1f, .8f, .1f, .45f), new Vector3(13f, 5f, 1f));
        }

        private static void CreateTelegraph(string name, Transform parent, Color color, Vector3 scale)
        {
            GameObject telegraph = new GameObject(name, typeof(SpriteRenderer));
            telegraph.transform.SetParent(parent, false);
            telegraph.transform.localScale = scale;
            SpriteRenderer renderer = telegraph.GetComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            renderer.color = color;
            renderer.sortingLayerName = "Characters";
            telegraph.SetActive(false);
        }

        private static void WireCommonComponents(GameObject root, BossData data, LootBundleData loot)
        {
            BossRuntime runtime = root.GetComponent<BossRuntime>();
            BossHealth bossHealth = root.GetComponent<BossHealth>();
            BossController controller = root.GetComponent<BossController>();
            BossCombatController combat = root.GetComponent<BossCombatController>();
            SetReference(runtime, "data", data);
            SetReference(bossHealth, "bossRuntime", runtime);
            SetReference(bossHealth, "health", root.GetComponent<Health>());
            SetReference(controller, "body", root.GetComponent<Rigidbody2D>());
            SetReference(controller, "runtime", runtime);
            SetReference(controller, "bossHealth", bossHealth);
            SetReference(controller, "combat", combat);
            SetReference(combat, "boss", controller);
            SetAttackReferences(combat, root.GetComponent<BossMeleeAttack>(), root.GetComponent<BossChargeAttack>());
            SetReference(root.GetComponent<BossReward>(), "bossHealth", bossHealth);
            SetReference(root.GetComponent<BossReward>(), "reward", loot);
            SetReference(root.GetComponent<BossMeleeAttack>(), "telegraph", root.transform.Find("Telegraphs/MeleeTelegraph").gameObject);
            SetReference(root.GetComponent<BossChargeAttack>(), "telegraph", root.transform.Find("Telegraphs/ChargeTelegraph").gameObject);
        }

        private static void SetAttackReferences(BossCombatController combat, BossMeleeAttack melee, BossChargeAttack charge)
        {
            SerializedObject serialized = new SerializedObject(combat);
            SerializedProperty attacks = serialized.FindProperty("attacks");
            attacks.arraySize = 2;
            attacks.GetArrayElementAtIndex(0).objectReferenceValue = melee;
            attacks.GetArrayElementAtIndex(1).objectReferenceValue = charge;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) throw new InvalidOperationException("BOSS_BUILD_FAILED: Missing serialized property " + propertyName + ".");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static int RequireLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0) throw new InvalidOperationException("BOSS_BUILD_FAILED: Required layer is missing: " + layerName + ".");
            return layer;
        }
    }
}
