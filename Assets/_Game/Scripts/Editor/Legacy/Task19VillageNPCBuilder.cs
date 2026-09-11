// LEGACY: Historical Task 19 authoring script. Do not use this file for current scene or prefab generation.

using HexaRealm.Core;
using HexaRealm.NPC;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace HexaRealm.EditorTools
{
    /// <summary>
    /// Narrow Task 19 authoring utility. It owns only the villager prefab and HumanRealm patrol placement.
    /// </summary>
    public static class Task19VillageNPCBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/HumanRealm/HumanRealm.unity";
        private const string PrefabFolder = "Assets/_Game/Prefabs/NPC/HumanRealm";
        private const string PrefabPath = PrefabFolder + "/Villager_Prototype.prefab";
        private const string PlaceholderSpritePath = "Assets/_Game/Art/Placeholders/SlimePlaceholder.png";

        public static void Build()
        {
            EnsureFolder("Assets/_Game/Prefabs");
            EnsureFolder("Assets/_Game/Prefabs/NPC");
            EnsureFolder(PrefabFolder);

            GameObject prefab = CreateVillagerPrefab();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find("HumanRealm");
            if (root == null)
            {
                Debug.LogError("TASK19_BUILD_FAILED: HumanRealm root is missing.");
                return;
            }

            Transform gameplay = root.transform.Find("Gameplay");
            if (gameplay == null)
            {
                Debug.LogError("TASK19_BUILD_FAILED: HumanRealm/Gameplay is missing.");
                return;
            }

            ReplaceVillageNPCContent(gameplay, prefab, scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("TASK19_BUILD_OK: 3 villagers and 3 village patrol paths authored.");
        }

        private static GameObject CreateVillagerPrefab()
        {
            GameObject root = new GameObject("Villager_Prototype", typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SortingGroup), typeof(TopDownSorting), typeof(VillageNPCMovement));
            root.layer = LayerMask.NameToLayer("NPC");

            Rigidbody2D body = root.GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D collider = root.GetComponent<CircleCollider2D>();
            collider.offset = new Vector2(0f, -0.14f);
            collider.radius = 0.28f;

            SortingGroup sortingGroup = root.GetComponent<SortingGroup>();
            sortingGroup.sortingLayerName = "Characters";

            GameObject visual = new GameObject("Body", typeof(SpriteRenderer));
            visual.layer = root.layer;
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            visual.transform.localScale = new Vector3(3.5f, 4f, 1f);
            SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderSpritePath);
            renderer.sortingLayerName = "Characters";
            renderer.color = new Color(0.78f, 0.55f, 0.3f, 1f);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void ReplaceVillageNPCContent(Transform gameplay, GameObject prefab, Scene scene)
        {
            DestroyChildIfPresent(gameplay, "VillageNPCs");
            DestroyChildIfPresent(gameplay, "VillageNPCPaths");

            GameObject pathsRoot = new GameObject("VillageNPCPaths");
            pathsRoot.transform.SetParent(gameplay, false);
            GameObject npcsRoot = new GameObject("VillageNPCs");
            npcsRoot.transform.SetParent(gameplay, false);

            CreateVillager("Villager_A", prefab, npcsRoot.transform, scene, 0, 1.25f, new[]
            {
                new Vector2(-14f, -43f), new Vector2(-8f, -45f), new Vector2(-3f, -43f),
                new Vector2(-3f, -38f), new Vector2(-12f, -36f)
            }, pathsRoot.transform);
            CreateVillager("Villager_B", prefab, npcsRoot.transform, scene, 1, 1.5f, new[]
            {
                new Vector2(-15f, -45f), new Vector2(-13f, -40f), new Vector2(-13f, -35f),
                new Vector2(-17f, -33f), new Vector2(-22f, -34f)
            }, pathsRoot.transform);
            CreateVillager("Villager_C", prefab, npcsRoot.transform, scene, 2, 1.1f, new[]
            {
                new Vector2(-1f, -45f), new Vector2(8f, -43f), new Vector2(8f, -39f),
                new Vector2(0f, -34f), new Vector2(-3f, -36f)
            }, pathsRoot.transform);
        }

        private static void CreateVillager(string name, GameObject prefab, Transform parent, Scene scene, int startingWaypointIndex, float idleDuration, Vector2[] points, Transform pathsRoot)
        {
            GameObject pathObject = new GameObject(name + "_Path", typeof(NPCPatrolPath));
            pathObject.transform.SetParent(pathsRoot, false);
            Transform[] waypoints = new Transform[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GameObject waypoint = new GameObject("Point_" + i.ToString("00"));
                waypoint.transform.SetParent(pathObject.transform, false);
                waypoint.transform.position = points[i];
                waypoints[i] = waypoint.transform;
            }

            SerializedObject pathSerialized = new SerializedObject(pathObject.GetComponent<NPCPatrolPath>());
            SerializedProperty waypointProperty = pathSerialized.FindProperty("waypoints");
            waypointProperty.arraySize = waypoints.Length;
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypointProperty.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];
            }
            pathSerialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject villager = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            villager.name = name;
            villager.transform.SetParent(parent);
            villager.transform.position = points[0];
            SerializedObject movementSerialized = new SerializedObject(villager.GetComponent<VillageNPCMovement>());
            movementSerialized.FindProperty("patrolPath").objectReferenceValue = pathObject.GetComponent<NPCPatrolPath>();
            movementSerialized.FindProperty("idleDuration").floatValue = idleDuration;
            movementSerialized.FindProperty("startingWaypointIndex").intValue = startingWaypointIndex;
            movementSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DestroyChildIfPresent(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
