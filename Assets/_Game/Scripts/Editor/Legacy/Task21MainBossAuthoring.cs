// LEGACY: Historical Task 21 authoring script. Do not use this file for current scene or prefab generation.

using System;
using HexaRealm.Boss;
using HexaRealm.Boss.UI;
using HexaRealm.Loot;
using HexaRealm.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HexaRealm.EditorTools
{
    /// <summary>Authors only Task 21 main-Boss content and scene-local HumanRealm progression references.</summary>
    public static class Task21MainBossAuthoring
    {
        private const string ScenePath = "Assets/_Game/Scenes/HumanRealm/HumanRealm.unity";
        private const string DataPath = "Assets/_Game/Data/Bosses/HumanRealm/MainBoss_Prototype.asset";
        private const string LootPath = "Assets/_Game/Data/Loot/HumanRealm/MainBoss_PrototypeReward.asset";
        private const string ProgressionPath = "Assets/_Game/Data/Progression/HumanRealm/MainBoss_ProgressionReward.asset";
        private const string PrefabPath = "Assets/_Game/Prefabs/Bosses/HumanRealm/MainBoss_Prototype.prefab";
        private const string SpritePath = "Assets/_Game/Art/Placeholders/SlimePlaceholder.png";
        private const string MarkerPath = "Markers/MainBossArea/MainBossAreaMarker";

        public static void Build()
        {
            try
            {
                EnsureFolders();
                BossData data = CreateBossData();
                LootBundleData loot = CreateLoot();
                RegionProgressionRewardData progressionData = CreateProgressionData();
                CreateMainBossPrefab(data, loot, progressionData);

                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                GameObject humanRealm = GameObject.Find("HumanRealm");
                Transform marker = humanRealm != null ? humanRealm.transform.Find(MarkerPath) : null;
                if (marker == null) throw new InvalidOperationException("TASK21_BUILD_FAILED: HumanRealm/Markers/MainBossArea/MainBossAreaMarker is missing.");
                GameObject player = GameObject.Find("Player");
                if (player == null) throw new InvalidOperationException("TASK21_BUILD_FAILED: HumanRealm Player is missing.");
                PlayerRegionProgression regionProgression = player.GetComponent<PlayerRegionProgression>();
                if (regionProgression == null) regionProgression = player.AddComponent<PlayerRegionProgression>();
                PlayerUpgradeProgression upgradeProgression = player.GetComponent<PlayerUpgradeProgression>();
                if (upgradeProgression == null) throw new InvalidOperationException("TASK21_BUILD_FAILED: PlayerUpgradeProgression is missing from Player.");

                ReplaceEncounter(marker.parent, marker.position, scene, regionProgression, upgradeProgression);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                ValidateSavedPrefab(progressionData);
                Debug.Log("TASK21_BUILD_OK: Main Boss prefab, marker-based encounter, and scene progression references were saved and validated.");
            }
            catch (Exception exception)
            {
                Debug.LogError("TASK21_BUILD_FAILED: " + exception.Message);
            }
        }

        private static void CreateMainBossPrefab(BossData data, LootBundleData loot, RegionProgressionRewardData progressionData)
        {
            BossPrefabAuthoringUtility.CreateAndSaveCommonPrefab(
                PrefabPath, data, loot, AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath), new Color(.82f, .2f, .16f),
                new Vector2(1.5f, 1.7f), new Vector2(0f, -.2f), 8.5f);
            GameObject editable = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                RegionBossProgressionReward reward = editable.GetComponent<RegionBossProgressionReward>();
                if (reward == null) reward = editable.AddComponent<RegionBossProgressionReward>();
                Set(reward, "bossHealth", editable.GetComponent<HexaRealm.Combat.Health>());
                Set(reward, "rewardData", progressionData);
                PrefabUtility.SaveAsPrefabAsset(editable, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(editable);
            }
            ValidateSavedPrefab(progressionData);
        }

        private static BossData CreateBossData()
        {
            BossData data = AssetDatabase.LoadAssetAtPath<BossData>(DataPath);
            if (data == null) { data = ScriptableObject.CreateInstance<BossData>(); AssetDatabase.CreateAsset(data, DataPath); }
            Set(data, "displayName", "HumanRealm Warden"); Set(data, "maxHealth", 420f); Set(data, "attack", 22f); Set(data, "defense", 5f); Set(data, "moveSpeed", 3.1f);
            return data;
        }

        private static LootBundleData CreateLoot()
        {
            LootBundleData loot = AssetDatabase.LoadAssetAtPath<LootBundleData>(LootPath);
            if (loot == null) { loot = ScriptableObject.CreateInstance<LootBundleData>(); AssetDatabase.CreateAsset(loot, LootPath); }
            loot.SetAuthoringValues(60); EditorUtility.SetDirty(loot);
            return loot;
        }

        private static RegionProgressionRewardData CreateProgressionData()
        {
            RegionProgressionRewardData data = AssetDatabase.LoadAssetAtPath<RegionProgressionRewardData>(ProgressionPath);
            if (data == null) { data = ScriptableObject.CreateInstance<RegionProgressionRewardData>(); AssetDatabase.CreateAsset(data, ProgressionPath); }
            SerializedObject serialized = new SerializedObject(data);
            serialized.FindProperty("completedRegion").enumValueIndex = (int)RegionId.HumanRealm;
            serialized.FindProperty("teleportStoneRegion").enumValueIndex = (int)RegionId.HumanRealm;
            serialized.FindProperty("unlockedUpgradeCap").intValue = 20;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private static void ReplaceEncounter(Transform mainBossArea, Vector2 position, Scene scene, PlayerRegionProgression region, PlayerUpgradeProgression upgrades)
        {
            Transform old = mainBossArea.Find("MainBossEncounter");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            GameObject encounter = new GameObject("MainBossEncounter"); encounter.transform.SetParent(mainBossArea, false);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            GameObject boss = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            boss.transform.SetParent(encounter.transform); boss.transform.position = position;
            RegionBossProgressionReward progression = boss.GetComponent<RegionBossProgressionReward>();
            Set(progression, "playerProgression", region); Set(progression, "playerUpgradeProgression", upgrades);
            CreateArena(encounter.transform, boss.GetComponent<BossController>(), position);
            CreateHealthBar(encounter.transform, boss.GetComponent<BossController>(), boss.GetComponent<BossHealth>());
            ValidateSceneInstance(boss);
        }

        private static void CreateArena(Transform parent, BossController boss, Vector2 position)
        {
            GameObject arena = new GameObject("MainBossArena", typeof(BoxCollider2D), typeof(BossArena)); arena.transform.SetParent(parent, false); arena.transform.position = position;
            arena.GetComponent<BoxCollider2D>().isTrigger = true; arena.GetComponent<BoxCollider2D>().size = new Vector2(20f, 16f);
            Set(arena.GetComponent<BossArena>(), "boss", boss);
        }

        private static void CreateHealthBar(Transform parent, BossController boss, BossHealth health)
        {
            GameObject canvasObject = new GameObject("MainBossHealthUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); canvasObject.transform.SetParent(parent, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; canvasObject.GetComponent<Canvas>().sortingOrder = 20; canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            GameObject bar = new GameObject("Bar", typeof(Image)); bar.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = bar.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f); rect.pivot = new Vector2(.5f, 1f); rect.anchoredPosition = new Vector2(0f, -70f); rect.sizeDelta = new Vector2(460f, 26f); bar.GetComponent<Image>().color = new Color(.16f, .04f, .03f, .92f);
            GameObject fillObject = new GameObject("Fill", typeof(Image)); fillObject.transform.SetParent(bar.transform, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>(); fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = new Vector2(3f, 3f); fillRect.offsetMax = new Vector2(-3f, -3f);
            Image fill = fillObject.GetComponent<Image>(); fill.color = new Color(.9f, .18f, .08f); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            BossHealthBarUI ui = canvasObject.AddComponent<BossHealthBarUI>(); Set(ui, "boss", boss); Set(ui, "bossHealth", health); Set(ui, "fill", fill); Set(ui, "barRoot", bar);
        }

        private static void ValidateSavedPrefab(RegionProgressionRewardData expectedData)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            BossPrefabAuthoringUtility.ValidateCommonPrefab(prefab);
            RegionBossProgressionReward reward = prefab.GetComponent<RegionBossProgressionReward>();
            if (reward == null || rewardData(reward) != expectedData) throw new InvalidOperationException("TASK21_BUILD_FAILED: Saved Main Boss progression reward is invalid.");
        }

        private static void ValidateSceneInstance(GameObject boss)
        {
            BossPrefabAuthoringUtility.ValidateCommonPrefab(boss);
            SerializedObject serialized = new SerializedObject(boss.GetComponent<RegionBossProgressionReward>());
            if (serialized.FindProperty("playerProgression").objectReferenceValue == null || serialized.FindProperty("playerUpgradeProgression").objectReferenceValue == null)
                throw new InvalidOperationException("TASK21_BUILD_FAILED: Main Boss scene progression references are missing.");
        }

        private static RegionProgressionRewardData rewardData(RegionBossProgressionReward reward)
        {
            return new SerializedObject(reward).FindProperty("rewardData").objectReferenceValue as RegionProgressionRewardData;
        }

        private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target); SerializedProperty property = serialized.FindProperty(name); if (property == null) throw new InvalidOperationException("TASK21_BUILD_FAILED: Missing serialized property " + name + "."); property.objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
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
            foreach (string path in new[] { "Assets/_Game/Data/Bosses/HumanRealm", "Assets/_Game/Data/Loot/HumanRealm", "Assets/_Game/Data/Progression/HumanRealm", "Assets/_Game/Prefabs/Bosses/HumanRealm" })
            {
                string current = "Assets";
                foreach (string part in path.Substring("Assets/".Length).Split('/')) { string next = current + "/" + part; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part); current = next; }
            }
        }
    }
}
