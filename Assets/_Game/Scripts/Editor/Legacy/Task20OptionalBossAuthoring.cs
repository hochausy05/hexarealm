// LEGACY: Historical Task 20 authoring script. Do not use this file for current scene or prefab generation.

using System;
using HexaRealm.Boss;
using HexaRealm.Boss.UI;
using HexaRealm.Loot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HexaRealm.EditorTools
{
    /// <summary>Authors only the Task 20 optional Boss content and its existing side-area encounter.</summary>
    public static class Task20OptionalBossAuthoring
    {
        private const string ScenePath = "Assets/_Game/Scenes/HumanRealm/HumanRealm.unity";
        private const string DataPath = "Assets/_Game/Data/Bosses/HumanRealm/OptionalBoss_Prototype.asset";
        private const string LootPath = "Assets/_Game/Data/Loot/HumanRealm/OptionalBoss_PrototypeReward.asset";
        private const string PrefabPath = "Assets/_Game/Prefabs/Bosses/HumanRealm/OptionalBoss_Prototype.prefab";
        private const string MarkerPath = "Markers/OptionalEncounterMarker_West";
        private const string SpritePath = "Assets/_Game/Art/Placeholders/SlimePlaceholder.png";

        public static void Build()
        {
            try
            {
                EnsureFolders();
                BossData data = CreateData();
                LootBundleData loot = CreateLoot();
                GameObject prefab = BossPrefabAuthoringUtility.CreateAndSaveCommonPrefab(
                    PrefabPath, data, loot, AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath), new Color(.55f, .18f, .78f),
                    new Vector2(1.25f, 1.45f), new Vector2(0f, -.2f), 7f);

                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                GameObject humanRealm = GameObject.Find("HumanRealm");
                Transform marker = humanRealm != null ? humanRealm.transform.Find(MarkerPath) : null;
                Transform gameplay = humanRealm != null ? humanRealm.transform.Find("Gameplay") : null;
                if (marker == null || gameplay == null) throw new InvalidOperationException("TASK20_BUILD_FAILED: HumanRealm optional side-area marker or Gameplay root is missing.");

                ReplaceEncounter(gameplay, prefab, marker.position, scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                BossPrefabAuthoringUtility.ValidateCommonPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
                Debug.Log("TASK20_BUILD_OK: Optional Boss prefab and side-area encounter were saved and validated.");
            }
            catch (Exception exception)
            {
                Debug.LogError("TASK20_BUILD_FAILED: " + exception.Message);
            }
        }

        private static BossData CreateData()
        {
            BossData data = AssetDatabase.LoadAssetAtPath<BossData>(DataPath);
            if (data == null) { data = ScriptableObject.CreateInstance<BossData>(); AssetDatabase.CreateAsset(data, DataPath); }
            Set(data, "displayName", "Optional Guardian");
            Set(data, "maxHealth", 250f); Set(data, "attack", 16f); Set(data, "defense", 3f); Set(data, "moveSpeed", 2.6f);
            return data;
        }

        private static LootBundleData CreateLoot()
        {
            LootBundleData loot = AssetDatabase.LoadAssetAtPath<LootBundleData>(LootPath);
            if (loot == null) { loot = ScriptableObject.CreateInstance<LootBundleData>(); AssetDatabase.CreateAsset(loot, LootPath); }
            loot.SetAuthoringValues(30);
            EditorUtility.SetDirty(loot);
            return loot;
        }

        private static void ReplaceEncounter(Transform gameplay, GameObject prefab, Vector2 position, Scene scene)
        {
            Transform old = gameplay.Find("OptionalBossEncounter");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            GameObject encounter = new GameObject("OptionalBossEncounter");
            encounter.transform.SetParent(gameplay, false);
            GameObject boss = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            boss.transform.SetParent(encounter.transform);
            boss.transform.position = position;
            CreateArena(encounter.transform, boss.GetComponent<BossController>(), position, new Vector2(18f, 14f));
            CreateHealthBar(encounter.transform, boss.GetComponent<BossController>(), boss.GetComponent<BossHealth>(), "OptionalBossHealthUI", new Color(.75f, .16f, .85f));
            BossPrefabAuthoringUtility.ValidateCommonPrefab(boss);
        }

        private static void CreateArena(Transform parent, BossController boss, Vector2 position, Vector2 size)
        {
            GameObject arena = new GameObject("OptionalBossArena", typeof(BoxCollider2D), typeof(BossArena));
            arena.transform.SetParent(parent, false);
            arena.transform.position = position;
            arena.GetComponent<BoxCollider2D>().isTrigger = true;
            arena.GetComponent<BoxCollider2D>().size = size;
            Set(arena.GetComponent<BossArena>(), "boss", boss);
        }

        private static void CreateHealthBar(Transform parent, BossController boss, BossHealth health, string name, Color color)
        {
            GameObject canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.GetComponent<Canvas>().sortingOrder = 20;
            canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            GameObject bar = new GameObject("Bar", typeof(Image));
            bar.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f); rect.pivot = new Vector2(.5f, 1f); rect.anchoredPosition = new Vector2(0f, -36f); rect.sizeDelta = new Vector2(430f, 24f);
            bar.GetComponent<Image>().color = new Color(.12f, .04f, .16f, .9f);
            GameObject fillObject = new GameObject("Fill", typeof(Image)); fillObject.transform.SetParent(bar.transform, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>(); fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = new Vector2(3f, 3f); fillRect.offsetMax = new Vector2(-3f, -3f);
            Image fill = fillObject.GetComponent<Image>(); fill.color = color; fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            BossHealthBarUI ui = canvasObject.AddComponent<BossHealthBarUI>();
            Set(ui, "boss", boss); Set(ui, "bossHealth", health); Set(ui, "fill", fill); Set(ui, "barRoot", bar);
        }

        private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target); serialized.FindProperty(name).objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(UnityEngine.Object target, string name, string value)
        {
            SerializedObject serialized = new SerializedObject(target); serialized.FindProperty(name).stringValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(UnityEngine.Object target, string name, float value)
        {
            SerializedObject serialized = new SerializedObject(target); serialized.FindProperty(name).floatValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolders()
        {
            foreach (string path in new[] { "Assets/_Game/Data/Bosses/HumanRealm", "Assets/_Game/Data/Loot/HumanRealm", "Assets/_Game/Prefabs/Bosses/HumanRealm" })
            {
                string current = "Assets";
                foreach (string part in path.Substring("Assets/".Length).Split('/')) { string next = current + "/" + part; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part); current = next; }
            }
        }
    }
}
