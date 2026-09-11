using HexaRealm.Boss;
using HexaRealm.Boss.Attacks;
using HexaRealm.Boss.UI;
using HexaRealm.Combat;
using HexaRealm.Core;
using HexaRealm.Loot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HexaRealm.EditorTools
{
    /// <summary>Narrow Task 20 authoring utility: prototype boss, side-area trigger, and its local UI only.</summary>
    public static class Task20OptionalBossAuthoring
    {
        private const string ScenePath = "Assets/_Game/Scenes/HumanRealm/HumanRealm.unity";
        private const string DataPath = "Assets/_Game/Data/Bosses/HumanRealm/OptionalBoss_Prototype.asset";
        private const string LootPath = "Assets/_Game/Data/Loot/HumanRealm/OptionalBoss_PrototypeReward.asset";
        private const string PrefabPath = "Assets/_Game/Prefabs/Bosses/HumanRealm/OptionalBoss_Prototype.prefab";
        private const string SpritePath = "Assets/_Game/Art/Placeholders/SlimePlaceholder.png";

        [MenuItem("HexaRealm/Task 20/Build Optional Boss Prototype")]
        public static void Build()
        {
            EnsureFolder("Assets/_Game/Data/Bosses/HumanRealm"); EnsureFolder("Assets/_Game/Data/Loot/HumanRealm"); EnsureFolder("Assets/_Game/Prefabs/Bosses/HumanRealm");
            BossData data = CreateData(); LootBundleData loot = CreateLoot(); GameObject prefab = CreatePrefab(data, loot);
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find("HumanRealm"); Transform gameplay = root != null ? root.transform.Find("Gameplay") : null;
            if (gameplay == null) { Debug.LogError("TASK20_BUILD_FAILED: HumanRealm/Gameplay is missing."); return; }
            ReplaceEncounter(gameplay, prefab, scene);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("TASK20_BUILD_OK: Optional boss encounter authored in HumanRealm side area.");
        }

        private static BossData CreateData()
        {
            BossData asset = AssetDatabase.LoadAssetAtPath<BossData>(DataPath);
            if (asset == null) { asset = ScriptableObject.CreateInstance<BossData>(); AssetDatabase.CreateAsset(asset, DataPath); }
            SerializedObject so = new SerializedObject(asset); so.FindProperty("displayName").stringValue = "Optional Guardian"; so.FindProperty("maxHealth").floatValue = 250f; so.FindProperty("attack").floatValue = 16f; so.FindProperty("defense").floatValue = 3f; so.FindProperty("moveSpeed").floatValue = 2.6f; so.ApplyModifiedPropertiesWithoutUndo(); return asset;
        }
        private static LootBundleData CreateLoot()
        {
            LootBundleData asset = AssetDatabase.LoadAssetAtPath<LootBundleData>(LootPath);
            if (asset == null) { asset = ScriptableObject.CreateInstance<LootBundleData>(); AssetDatabase.CreateAsset(asset, LootPath); }
            asset.SetAuthoringValues(30); EditorUtility.SetDirty(asset); return asset;
        }
        private static GameObject CreatePrefab(BossData data, LootBundleData loot)
        {
            GameObject root = new GameObject("OptionalBoss_Prototype", typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(SortingGroup), typeof(TopDownSorting), typeof(BossRuntime), typeof(Health), typeof(BossHealth), typeof(BossController), typeof(BossCombatController), typeof(BossReward), typeof(BossMeleeAttack), typeof(BossChargeAttack));
            root.layer = LayerMask.NameToLayer("Enemy"); Rigidbody2D body = root.GetComponent<Rigidbody2D>(); body.gravityScale = 0f; body.freezeRotation = true; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; body.interpolation = RigidbodyInterpolation2D.Interpolate;
            CapsuleCollider2D collider = root.GetComponent<CapsuleCollider2D>(); collider.size = new Vector2(1.25f, 1.45f); collider.offset = new Vector2(0f, -.2f);
            root.GetComponent<SortingGroup>().sortingLayerName = "Characters";
            GameObject visual = new GameObject("Body", typeof(SpriteRenderer)); visual.transform.SetParent(root.transform, false); visual.transform.localPosition = new Vector3(0f, .25f, 0f); visual.transform.localScale = new Vector3(7f, 7f, 1f); SpriteRenderer visualRenderer = visual.GetComponent<SpriteRenderer>(); visualRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath); visualRenderer.color = new Color(.55f, .18f, .78f, 1f); visualRenderer.sortingLayerName = "Characters";
            GameObject telegraphs = new GameObject("Telegraphs"); telegraphs.transform.SetParent(root.transform, false);
            GameObject melee = CreateTelegraph("MeleeTelegraph", telegraphs.transform, new Color(1f, .25f, .1f, .5f), new Vector3(11f, 7f, 1f));
            GameObject charge = CreateTelegraph("ChargeTelegraph", telegraphs.transform, new Color(1f, .85f, .1f, .45f), new Vector3(13f, 5f, 1f));
            Configure(root, data, loot, melee, charge); GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); Object.DestroyImmediate(root); return prefab;
        }
        private static GameObject CreateTelegraph(string name, Transform parent, Color color, Vector3 scale)
        {
            GameObject go = new GameObject(name, typeof(SpriteRenderer)); go.transform.SetParent(parent, false); go.transform.localScale = scale; SpriteRenderer sr = go.GetComponent<SpriteRenderer>(); sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"); sr.color = color; sr.sortingLayerName = "Characters"; go.SetActive(false); return go;
        }
        private static void Configure(GameObject root, BossData data, LootBundleData loot, GameObject melee, GameObject charge)
        {
            SerializedObject runtime = new SerializedObject(root.GetComponent<BossRuntime>()); runtime.FindProperty("data").objectReferenceValue = data; runtime.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject reward = new SerializedObject(root.GetComponent<BossReward>()); reward.FindProperty("reward").objectReferenceValue = loot; reward.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject sweep = new SerializedObject(root.GetComponent<BossMeleeAttack>()); sweep.FindProperty("telegraph").objectReferenceValue = melee; sweep.FindProperty("activeDuration").floatValue = .12f; sweep.FindProperty("damageMultiplier").floatValue = 1f; sweep.ApplyModifiedPropertiesWithoutUndo();
            SerializedObject rush = new SerializedObject(root.GetComponent<BossChargeAttack>()); rush.FindProperty("telegraph").objectReferenceValue = charge; rush.FindProperty("activeDuration").floatValue = .55f; rush.FindProperty("damageMultiplier").floatValue = .8f; rush.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void ReplaceEncounter(Transform gameplay, GameObject prefab, Scene scene)
        {
            Transform old = gameplay.Find("OptionalBossEncounter"); if (old != null) Object.DestroyImmediate(old.gameObject);
            GameObject encounter = new GameObject("OptionalBossEncounter"); encounter.transform.SetParent(gameplay, false);
            Vector2 bossPosition = new Vector2(45f, -18f); GameObject boss = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene); boss.transform.SetParent(encounter.transform); boss.transform.position = bossPosition;
            GameObject arena = new GameObject("OptionalBossArena", typeof(BoxCollider2D), typeof(BossArena)); arena.transform.SetParent(encounter.transform); arena.transform.position = bossPosition; BoxCollider2D trigger = arena.GetComponent<BoxCollider2D>(); trigger.isTrigger = true; trigger.size = new Vector2(18f, 14f); SerializedObject arenaSo = new SerializedObject(arena.GetComponent<BossArena>()); arenaSo.FindProperty("boss").objectReferenceValue = boss.GetComponent<BossController>(); arenaSo.ApplyModifiedPropertiesWithoutUndo();
            CreateHealthBar(encounter.transform, boss.GetComponent<BossController>(), boss.GetComponent<BossHealth>());
        }
        private static void CreateHealthBar(Transform parent, BossController boss, BossHealth health)
        {
            GameObject canvasObject = new GameObject("OptionalBossHealthUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); canvasObject.transform.SetParent(parent, false); Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20; canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            GameObject background = new GameObject("Bar", typeof(Image)); background.transform.SetParent(canvasObject.transform, false); RectTransform bg = background.GetComponent<RectTransform>(); bg.anchorMin = new Vector2(.5f, 1f); bg.anchorMax = new Vector2(.5f, 1f); bg.pivot = new Vector2(.5f, 1f); bg.anchoredPosition = new Vector2(0f, -36f); bg.sizeDelta = new Vector2(430f, 24f); background.GetComponent<Image>().color = new Color(.12f, .04f, .16f, .9f);
            GameObject fillObject = new GameObject("Fill", typeof(Image)); fillObject.transform.SetParent(background.transform, false); RectTransform fillRect = fillObject.GetComponent<RectTransform>(); fillRect.anchorMin = new Vector2(0f, 0f); fillRect.anchorMax = new Vector2(1f, 1f); fillRect.offsetMin = new Vector2(3f, 3f); fillRect.offsetMax = new Vector2(-3f, -3f); Image fill = fillObject.GetComponent<Image>(); fill.color = new Color(.75f, .16f, .85f, 1f); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            BossHealthBarUI ui = canvasObject.AddComponent<BossHealthBarUI>(); SerializedObject uiSo = new SerializedObject(ui); uiSo.FindProperty("boss").objectReferenceValue = boss; uiSo.FindProperty("bossHealth").objectReferenceValue = health; uiSo.FindProperty("fill").objectReferenceValue = fill; uiSo.FindProperty("barRoot").objectReferenceValue = background; uiSo.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void EnsureFolder(string path) { string[] parts = path.Split('/'); string current = parts[0]; for (int i = 1; i < parts.Length; i++) { string next = current + "/" + parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]); current = next; } }
    }
}
