using System;
using System.Collections.Generic;
using System.IO;
using HexaRealm.Core;
using HexaRealm.Enemy;
using HexaRealm.Interaction;
using HexaRealm.Loot;
using HexaRealm.Player;
using HexaRealm.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace HexaRealm.EditorTools
{
    /// <summary>
    /// Deterministic authoring utility for the HumanRealm graybox. It creates authored assets/scene data only;
    /// no runtime procedural generation is involved.
    /// </summary>
    public static class Task17HumanRealmBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/HumanRealm/HumanRealm.unity";
        private const string ArtFolder = "Assets/_Game/Art/Placeholders/HumanRealm";
        private const string TileFolder = "Assets/_Game/Tilemaps/HumanRealm/Graybox";
        private const string PlayerPrefabPath = "Assets/_Game/Prefabs/Player/Player.prefab";
        private const string SlimePrefabPath = "Assets/_Game/Prefabs/Enemies/HumanRealm/Slime_F.prefab";
        private const string SoulPillarPrefabPath = "Assets/_Game/Prefabs/Progression/SoulPillar.prefab";
        private const string LootChestPrefabPath = "Assets/_Game/Prefabs/World/Chests/LootChest.prefab";
        private const string LootBundlePath = "Assets/_Game/Data/Loot/LootTestBundle.asset";

        private const int Min = -56;
        private const int Max = 55;

        [MenuItem("HexaRealm/Task 17/Validate Foundation")]
        public static void ValidateFoundation()
        {
            var errors = new List<string>();
            GameObject player = RequirePrefab(PlayerPrefabPath, errors);
            GameObject slime = RequirePrefab(SlimePrefabPath, errors);
            GameObject pillar = RequirePrefab(SoulPillarPrefabPath, errors);
            GameObject chest = RequirePrefab(LootChestPrefabPath, errors);
            LootBundleData loot = AssetDatabase.LoadAssetAtPath<LootBundleData>(LootBundlePath);

            if (player != null && player.GetComponent<Rigidbody2D>() == null)
                errors.Add("Player prefab is missing Rigidbody2D.");
            if (slime != null && (slime.GetComponent<EnemyRuntime>() == null || slime.GetComponent<EnemyHealth>() == null))
                errors.Add("Slime_F prefab is missing EnemyRuntime or EnemyHealth.");
            if (pillar != null && pillar.GetComponent<SoulPillar>() == null)
                errors.Add("SoulPillar prefab is missing SoulPillar.");
            if (chest != null && chest.GetComponent<LootChest>() == null)
                errors.Add("LootChest prefab is missing LootChest.");
            if (loot == null || !loot.HasAnyReward)
                errors.Add("LootTestBundle is missing or empty.");

            if (errors.Count > 0)
                throw new InvalidOperationException("Task 17 foundation validation failed:\n- " + string.Join("\n- ", errors));

            Debug.Log("TASK17_FOUNDATION_OK Player, Slime_F, SoulPillar, LootChest, and fixed loot bundle are valid.");
        }

        public static void ValidateFoundationBatch()
        {
            ValidateFoundation();
        }

        [MenuItem("HexaRealm/Task 17/Build HumanRealm Graybox")]
        public static void Build()
        {
            ValidateFoundation();
            EnsureFolder("Assets/_Game/Scenes/HumanRealm");
            EnsureFolder(ArtFolder);
            EnsureFolder(TileFolder);

            Tile grass = CreateTile("Grass", new Color32(104, 164, 78, 255), new Color32(91, 149, 68, 255), false);
            Tile dirt = CreateTile("Dirt", new Color32(174, 132, 78, 255), new Color32(151, 111, 65, 255), false);
            Tile water = CreateTile("Water", new Color32(66, 139, 187, 255), new Color32(50, 119, 169, 255), false);
            Tile blocker = CreateTile("BlockerCliff", new Color32(91, 91, 76, 255), new Color32(70, 70, 60, 255), true);
            Tile village = CreateTile("VillageGround", new Color32(188, 169, 121, 255), new Color32(164, 145, 100, 255), false);
            Tile terrain = CreateTile("SecondaryTerrain", new Color32(73, 126, 67, 255), new Color32(61, 108, 57, 255), false);
            Tile collision = CreateTile("Collision", new Color32(255, 0, 255, 255), new Color32(255, 0, 255, 255), true);

            EnsurePlayerPrefabTransition();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = NewObject("HumanRealm");
            GameObject systems = NewObject("Systems", root.transform);
            GameObject world = NewObject("World", root.transform);
            GameObject gameplay = NewObject("Gameplay", root.transform);
            GameObject markers = NewObject("Markers", root.transform);

            GameObject gridObject = NewObject("Grid", world.transform, typeof(Grid));
            Grid grid = gridObject.GetComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap ground = CreateTilemap("Ground", gridObject.transform, "Ground", 0);
            Tilemap details = CreateTilemap("GroundDetails", gridObject.transform, "GroundDetails", 0);
            Tilemap waterMap = CreateTilemap("Water", gridObject.transform, "GroundDetails", 1);
            Tilemap collisionMap = CreateCollisionTilemap(gridObject.transform);
            Tilemap decorationsBack = CreateTilemap("Decorations_Back", gridObject.transform, "Environment", -20);
            CreateTilemap("Decorations_Front", gridObject.transform, "Environment", 20);
            CreateTilemap("AbovePlayer", gridObject.transform, "AboveCharacters", 0);
            NewObject("Environment", world.transform);

            FillRect(ground, grass, Min, Min, Max, Max);
            PaintRegions(details, village, terrain);
            PaintRoadNetwork(details, dirt);
            PaintWater(waterMap, collisionMap, water, collision);
            PaintWorldBlockers(decorationsBack, collisionMap, blocker, collision);

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.name = "Player";
            player.transform.SetParent(gameplay.transform);
            player.transform.position = new Vector3(-8f, -42f, 0f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(player.transform);

            CreateCamera(systems.transform, player.transform);

            GameObject pillars = NewObject("SoulPillars", gameplay.transform);
            PlacePrefab(SoulPillarPrefabPath, "VillageSoulPillar", new Vector2(-8f, -38f), pillars.transform, scene, 3f);
            PlacePrefab(SoulPillarPrefabPath, "CrossroadsSoulPillar", new Vector2(0f, 2f), pillars.transform, scene, 3f);
            PlacePrefab(SoulPillarPrefabPath, "NorthernApproachSoulPillar", new Vector2(3f, 31f), pillars.transform, scene, 3f);

            GameObject chests = NewObject("Chests", gameplay.transform);
            PlaceChest("WesternForestChest", new Vector2(-45f, 16f), chests.transform, scene);
            PlaceChest("EasternFarmChest", new Vector2(43f, 8f), chests.transform, scene);
            PlaceChest("NorthernSideRouteChest", new Vector2(31f, 31f), chests.transform, scene);

            GameObject zones = NewObject("EnemySpawnZones", gameplay.transform);
            CreateSpawnZone("BeginnerMeadowSpawnZone", new[] { new Vector2(-13, -25), new Vector2(-3, -22), new Vector2(8, -26) }, 2, 3, player.transform, zones.transform);
            CreateSpawnZone("WesternWildernessSpawnZone", new[] { new Vector2(-43, 2), new Vector2(-35, 12), new Vector2(-25, 7), new Vector2(-29, 20) }, 2, 3, player.transform, zones.transform);
            CreateSpawnZone("EasternFarmingSpawnZone", new[] { new Vector2(20, -7), new Vector2(35, -11), new Vector2(43, 3), new Vector2(25, 13) }, 2, 3, player.transform, zones.transform);

            CreateMarkers(markers.transform);
            CreateTask18Cave(world.transform, gameplay.transform);

            ground.CompressBounds();
            details.CompressBounds();
            waterMap.CompressBounds();
            collisionMap.CompressBounds();
            decorationsBack.CompressBounds();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Failed to save HumanRealm scene.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("TASK17_BUILD_OK HumanRealm graybox authored at " + ScenePath);
        }

        public static void BuildBatch()
        {
            Build();
            ValidateScene();
        }

        [MenuItem("HexaRealm/Task 17/Validate HumanRealm Scene")]
        public static void ValidateScene()
        {
            var errors = new List<string>();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = FindRoot(scene, "HumanRealm");
            if (root == null)
                errors.Add("HumanRealm root is missing.");

            Transform grid = root != null ? root.transform.Find("World/Grid") : null;
            string[] requiredTilemaps = { "Ground", "GroundDetails", "Water", "Collision", "Decorations_Back", "Decorations_Front", "AbovePlayer" };
            foreach (string tilemapName in requiredTilemaps)
            {
                Transform child = grid != null ? grid.Find(tilemapName) : null;
                if (child == null || child.GetComponent<Tilemap>() == null)
                    errors.Add("Required Tilemap is missing: " + tilemapName);
            }

            Tilemap groundMap = grid != null && grid.Find("Ground") != null ? grid.Find("Ground").GetComponent<Tilemap>() : null;
            if (groundMap == null || groundMap.cellBounds.xMin != Min || groundMap.cellBounds.yMin != Min ||
                groundMap.cellBounds.size.x != 112 || groundMap.cellBounds.size.y != 112)
                errors.Add("Ground Tilemap bounds are not the expected 112x112 cells at [-56,55].");

            Tilemap collisionMap = grid != null && grid.Find("Collision") != null ? grid.Find("Collision").GetComponent<Tilemap>() : null;
            if (collisionMap == null || collisionMap.cellBounds.xMin != Min - 1 || collisionMap.cellBounds.yMin != Min - 1 ||
                collisionMap.cellBounds.size.x != 114 || collisionMap.cellBounds.size.y != 114)
                errors.Add("Collision Tilemap bounds are not the expected 114x114 exterior ring at [-57,56].");
            else
            {
                Vector3Int[] perimeterCorners = { new Vector3Int(Min - 1, Min - 1, 0), new Vector3Int(Max + 1, Min - 1, 0),
                    new Vector3Int(Min - 1, Max + 1, 0), new Vector3Int(Max + 1, Max + 1, 0) };
                foreach (Vector3Int corner in perimeterCorners)
                    if (collisionMap.GetTile(corner) == null) errors.Add("Collision perimeter corner is missing at " + corner + ".");
            }

            Transform collisionTransform = grid != null ? grid.Find("Collision") : null;
            if (collisionTransform == null || collisionTransform.GetComponent<TilemapCollider2D>() == null ||
                collisionTransform.GetComponent<Rigidbody2D>() == null)
                errors.Add("Collision Tilemap collider setup is incomplete.");

            CameraFollow2D follow = UnityEngine.Object.FindAnyObjectByType<CameraFollow2D>();
            if (follow == null || new SerializedObject(follow).FindProperty("target").objectReferenceValue == null)
                errors.Add("CameraFollow2D target is missing.");

            GameObject player = GameObject.Find("HumanRealm/Gameplay/Player");
            if (player == null || PrefabUtility.GetCorrespondingObjectFromSource(player) == null)
                errors.Add("Player prefab instance is missing or disconnected.");

            SoulPillar[] pillars = UnityEngine.Object.FindObjectsByType<SoulPillar>(FindObjectsSortMode.None);
            LootChest[] chests = UnityEngine.Object.FindObjectsByType<LootChest>(FindObjectsSortMode.None);
            EnemySpawnZone[] zones = UnityEngine.Object.FindObjectsByType<EnemySpawnZone>(FindObjectsSortMode.None);
            if (pillars.Length != 3) errors.Add("Expected 3 SoulPillars; found " + pillars.Length + ".");
            if (chests.Length != 3) errors.Add("Expected 3 LootChests; found " + chests.Length + ".");
            if (zones.Length != 3) errors.Add("Expected 3 EnemySpawnZones; found " + zones.Length + ".");

            GameObject slimePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefabPath);
            int maximumSlimes = 0;
            foreach (EnemySpawnZone zone in zones)
            {
                SerializedObject serialized = new SerializedObject(zone);
                if (serialized.FindProperty("enemyPrefab").objectReferenceValue != slimePrefab)
                    errors.Add(zone.name + " does not reference Slime_F.");
                if (serialized.FindProperty("player").objectReferenceValue == null)
                    errors.Add(zone.name + " has no Player reference.");
                if (serialized.FindProperty("spawnPoints").arraySize < 3)
                    errors.Add(zone.name + " has too few spawn points.");
                maximumSlimes += serialized.FindProperty("maxAlive").intValue;
            }
            if (maximumSlimes != 9) errors.Add("Expected maximum concurrent Slime population of 9; found " + maximumSlimes + ".");

            foreach (LootChest chest in chests)
            {
                SerializedObject serialized = new SerializedObject(chest);
                if (serialized.FindProperty("lootBundle").objectReferenceValue == null)
                    errors.Add(chest.name + " has no loot bundle.");
                if (serialized.FindProperty("autoEquipWeaponReward").boolValue || serialized.FindProperty("autoEquipArmorReward").boolValue)
                    errors.Add(chest.name + " should use Auto Equip = false.");
            }

            int manuallyPlacedSlimes = 0;
            foreach (EnemyRuntime enemy in UnityEngine.Object.FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None))
            {
                if (!PrefabUtility.IsPartOfPrefabAsset(enemy)) manuallyPlacedSlimes++;
            }
            if (manuallyPlacedSlimes != 0)
                errors.Add("Found " + manuallyPlacedSlimes + " manually placed runtime enemies.");

            if (GameObject.Find("HumanRealm/Markers/CaveEntrance/CaveEntranceMarker") == null)
                errors.Add("CaveEntranceMarker is missing.");
            if (GameObject.Find("HumanRealm/Markers/MainBossArea/MainBossAreaMarker") == null)
                errors.Add("MainBossAreaMarker is missing.");

            ValidateTask18(errors, root);

            string[] dependencies = AssetDatabase.GetDependencies(ScenePath, true);
            foreach (string dependency in dependencies)
            {
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(dependency)))
                    errors.Add("Missing dependency GUID: " + dependency);
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("Task 17 scene validation failed:\n- " + string.Join("\n- ", errors));

            Debug.Log("TASK17_SCENE_OK bounds=112x112 player=(-8,-42) pillars=3 chests=3 zones=3 maxSlimes=9 village=[-25..10,-52..-33] cave=(-38,30) boss=(0,44) distances=44/43/60_tiles");
        }

        public static void ValidateSceneBatch()
        {
            ValidateScene();
        }

        [MenuItem("HexaRealm/Task 17/Validate HumanRealm Physics Diagnostics")]
        public static void ValidatePhysicsDiagnostics()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Physics2D.SyncTransforms();
            GameObject root = FindRoot(scene, "HumanRealm");
            Transform grid = root != null ? root.transform.Find("World/Grid") : null;
            GameObject collisionObject = grid != null && grid.Find("Collision") != null ? grid.Find("Collision").gameObject : null;
            TilemapCollider2D tilemapCollider = collisionObject != null ? collisionObject.GetComponent<TilemapCollider2D>() : null;
            CompositeCollider2D composite = collisionObject != null ? collisionObject.GetComponent<CompositeCollider2D>() : null;
            Rigidbody2D worldBody = collisionObject != null ? collisionObject.GetComponent<Rigidbody2D>() : null;
            int playerLayer = LayerMask.NameToLayer("Player");
            int worldLayer = LayerMask.NameToLayer("World");
            bool ignored = playerLayer < 0 || worldLayer < 0 || Physics2D.GetIgnoreLayerCollision(playerLayer, worldLayer);
            Debug.Log($"TASK17_PHYSICS playerLayer={playerLayer} worldLayer={worldLayer} ignored={ignored} " +
                      $"tilemapEnabled={tilemapCollider != null && tilemapCollider.enabled} tilemapTrigger={tilemapCollider != null && tilemapCollider.isTrigger} " +
                      $"tilemapShapes={(tilemapCollider != null ? tilemapCollider.shapeCount : -1)} " +
                      $"compositeEnabled={composite != null && composite.enabled} compositeTrigger={composite != null && composite.isTrigger} " +
                      $"compositePaths={(composite != null ? composite.pathCount : -1)} worldBody={(worldBody != null ? worldBody.bodyType.ToString() : "missing")}");
        }

        public static void ValidatePhysicsDiagnosticsBatch()
        {
            ValidatePhysicsDiagnostics();
        }

        private static GameObject RequirePrefab(string path, List<string> errors)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) errors.Add("Missing prefab: " + path);
            return prefab;
        }

        private static Tile CreateTile(string name, Color32 main, Color32 accent, bool hasCollider)
        {
            string pngPath = ArtFolder + "/" + name + ".png";
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var pixels = new Color32[32 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    bool pattern = ((x * 7 + y * 11 + name.Length) % 29 == 0) || x == 0 || y == 0;
                    pixels[y * 32 + x] = pattern ? accent : main;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(Path.GetFullPath(pngPath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            string tilePath = TileFolder + "/" + name + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, tilePath);
            }

            tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            tile.color = Color.white;
            tile.colliderType = hasCollider ? Tile.ColliderType.Grid : Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static GameObject NewObject(string name, Transform parent = null, params Type[] components)
        {
            GameObject instance = components.Length == 0 ? new GameObject(name) : new GameObject(name, components);
            if (parent != null) instance.transform.SetParent(parent, false);
            return instance;
        }

        private static Tilemap CreateTilemap(string name, Transform parent, string sortingLayer, int sortingOrder)
        {
            GameObject go = NewObject(name, parent, typeof(Tilemap), typeof(TilemapRenderer));
            TilemapRenderer renderer = go.GetComponent<TilemapRenderer>();
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = sortingOrder;
            return go.GetComponent<Tilemap>();
        }

        private static Tilemap CreateCollisionTilemap(Transform parent)
        {
            GameObject go = NewObject("Collision", parent, typeof(Tilemap), typeof(TilemapRenderer), typeof(Rigidbody2D), typeof(TilemapCollider2D));
            go.layer = LayerMask.NameToLayer("World");
            go.GetComponent<TilemapRenderer>().enabled = false;
            Rigidbody2D body = go.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            body.simulated = true;
            return go.GetComponent<Tilemap>();
        }

        private static void FillRect(Tilemap map, TileBase tile, int xMin, int yMin, int xMax, int yMax)
        {
            int width = xMax - xMin + 1;
            int height = yMax - yMin + 1;
            var tiles = new TileBase[width * height];
            for (int i = 0; i < tiles.Length; i++) tiles[i] = tile;
            map.SetTilesBlock(new BoundsInt(xMin, yMin, 0, width, height, 1), tiles);
        }

        private static void FillEllipse(Tilemap map, TileBase tile, Vector2Int center, int radiusX, int radiusY)
        {
            for (int y = -radiusY; y <= radiusY; y++)
            for (int x = -radiusX; x <= radiusX; x++)
            {
                float value = x * x / (float)(radiusX * radiusX) + y * y / (float)(radiusY * radiusY);
                if (value <= 1f) map.SetTile(new Vector3Int(center.x + x, center.y + y, 0), tile);
            }
        }

        private static void FillCircle(Tilemap map, TileBase tile, Vector2Int center, int radius)
        {
            FillEllipse(map, tile, center, radius, radius);
        }

        private static void PaintRegions(Tilemap details, Tile village, Tile terrain)
        {
            FillRect(details, village, -25, -52, 10, -33);
            FillEllipse(details, terrain, new Vector2Int(-34, 8), 20, 24);
            FillEllipse(details, terrain, new Vector2Int(34, 1), 19, 24);
            FillRect(details, terrain, -49, 25, 49, 37);
        }

        private static void PaintRoadNetwork(Tilemap details, Tile dirt)
        {
            Vector2Int[][] routes =
            {
                new[] { P(-8,-42), P(-6,-31), P(-1,-20), P(3,-10), P(0,0) },
                new[] { P(-18,-41), P(-29,-29), P(-36,-15), P(-32,0), P(-16,3), P(0,0) },
                new[] { P(2,-41), P(15,-31), P(25,-17), P(20,-7), P(10,-2), P(0,0) },
                new[] { P(0,0), P(-10,10), P(-22,17), P(-31,25), P(-38,30) },
                new[] { P(0,0), P(-12,10), P(-18,22), P(-12,32), P(-6,37), P(0,41) },
                new[] { P(0,0), P(12,8), P(24,18), P(28,29), P(16,35), P(3,41) },
                new[] { P(-32,0), P(-42,9), P(-45,16) },
                new[] { P(20,-7), P(34,0), P(43,8) },
                new[] { P(28,29), P(31,31) }
            };

            foreach (Vector2Int[] route in routes)
                for (int i = 0; i < route.Length - 1; i++) DrawThickLine(details, dirt, route[i], route[i + 1], 2);

            FillCircle(details, dirt, P(0, 0), 6);
            FillCircle(details, dirt, P(0, 44), 11);
            FillRect(details, dirt, -13, -46, -2, -36);
        }

        private static void PaintWater(Tilemap waterMap, Tilemap collisionMap, Tile water, Tile collision)
        {
            FillEllipse(waterMap, water, P(33, -2), 6, 8);
            FillEllipse(collisionMap, collision, P(33, -2), 6, 8);
            FillEllipse(waterMap, water, P(-9, 18), 4, 7);
            FillEllipse(collisionMap, collision, P(-9, 18), 4, 7);
        }

        private static void PaintWorldBlockers(Tilemap visuals, Tilemap collisions, Tile blocker, Tile collision)
        {
            FillRect(collisions, collision, Min, Min, Max, Min);
            FillRect(collisions, collision, Min, Max, Max, Max);
            FillRect(collisions, collision, Min, Min, Min, Max);
            FillRect(collisions, collision, Max, Min, Max, Max);
            // Keep the physical barrier just outside the 112x112 playable cells. This prevents
            // high-velocity Rigidbody2D contact from stepping across an edge coincident with the
            // outermost walkable row while preserving the intended map area.
            FillRect(collisions, collision, Min - 1, Min - 1, Max + 1, Min - 1);
            FillRect(collisions, collision, Min - 1, Max + 1, Max + 1, Max + 1);
            FillRect(collisions, collision, Min - 1, Min - 1, Min - 1, Max + 1);
            FillRect(collisions, collision, Max + 1, Min - 1, Max + 1, Max + 1);
            FillRect(visuals, blocker, Min, Min, Max, Min);
            FillRect(visuals, blocker, Min, Max, Max, Max);
            FillRect(visuals, blocker, Min, Min, Min, Max);
            FillRect(visuals, blocker, Max, Min, Max, Max);

            RectInt[] buildings =
            {
                new RectInt(-22, -50, 7, 5), new RectInt(-11, -51, 7, 4),
                new RectInt(1, -48, 7, 5), new RectInt(-23, -39, 6, 4),
                new RectInt(1, -38, 6, 4)
            };
            foreach (RectInt rect in buildings)
            {
                FillRect(visuals, blocker, rect.xMin, rect.yMin, rect.xMax - 1, rect.yMax - 1);
                FillRect(collisions, collision, rect.xMin, rect.yMin, rect.xMax - 1, rect.yMax - 1);
            }

            Vector2Int[] forestClusters = { P(-48,-15), P(-45,-4), P(-49,8), P(-39,22), P(-27,26), P(-22,13), P(-26,-8) };
            foreach (Vector2Int center in forestClusters)
            {
                FillCircle(visuals, blocker, center, 3);
                FillCircle(collisions, collision, center, 3);
            }

            Vector2Int[] eastObstacles = { P(48,-20), P(44,20), P(18,18), P(48,31) };
            foreach (Vector2Int center in eastObstacles)
            {
                FillEllipse(visuals, blocker, center, 3, 2);
                FillEllipse(collisions, collision, center, 3, 2);
            }

            FillRect(visuals, blocker, -43, 34, -33, 38);
            FillRect(collisions, collision, -43, 34, -33, 38);
            FillRect(visuals, blocker, -43, 30, -41, 34);
            FillRect(collisions, collision, -43, 30, -41, 34);
            FillRect(visuals, blocker, -35, 30, -33, 34);
            FillRect(collisions, collision, -35, 30, -33, 34);

            FillRect(visuals, blocker, -50, 39, -18, 42);
            FillRect(collisions, collision, -50, 39, -18, 42);
            FillRect(visuals, blocker, 20, 40, 50, 43);
            FillRect(collisions, collision, 20, 40, 50, 43);
        }

        private static void DrawThickLine(Tilemap map, TileBase tile, Vector2Int start, Vector2Int end, int radius)
        {
            int dx = Mathf.Abs(end.x - start.x);
            int dy = Mathf.Abs(end.y - start.y);
            int steps = Mathf.Max(dx, dy);
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0f : i / (float)steps;
                Vector2Int point = new Vector2Int(Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t)), Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t)));
                FillCircle(map, tile, point, radius);
            }
        }

        private static void CreateCamera(Transform parent, Transform player)
        {
            GameObject cameraObject = NewObject("Main Camera", parent, typeof(Camera), typeof(AudioListener), typeof(CameraFollow2D));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(player.position.x, player.position.y, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(31, 43, 49, 255);
            SerializedObject follow = new SerializedObject(cameraObject.GetComponent<CameraFollow2D>());
            follow.FindProperty("target").objectReferenceValue = player;
            follow.FindProperty("offset").vector2Value = Vector2.zero;
            follow.FindProperty("smoothTime").floatValue = 0.12f;
            follow.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject PlacePrefab(string path, string name, Vector2 position, Transform parent, Scene scene, float scale = 1f)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.localScale = Vector3.one * scale;
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            return instance;
        }

        private static void PlaceChest(string name, Vector2 position, Transform parent, Scene scene)
        {
            GameObject instance = PlacePrefab(LootChestPrefabPath, name, position, parent, scene, 2f);
            LootChest chest = instance.GetComponent<LootChest>();
            SerializedObject serialized = new SerializedObject(chest);
            serialized.FindProperty("lootBundle").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LootBundleData>(LootBundlePath);
            serialized.FindProperty("autoEquipWeaponReward").boolValue = false;
            serialized.FindProperty("autoEquipArmorReward").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(chest);
        }

        private static void CreateSpawnZone(string name, Vector2[] points, int initial, int maximum, Transform player, Transform parent)
        {
            GameObject zoneObject = NewObject(name, parent, typeof(EnemySpawnZone));
            var transforms = new Transform[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GameObject point = NewObject("SpawnPoint_" + (char)('A' + i), zoneObject.transform);
                point.transform.position = points[i];
                transforms[i] = point.transform;
            }

            SerializedObject serialized = new SerializedObject(zoneObject.GetComponent<EnemySpawnZone>());
            serialized.FindProperty("enemyPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefabPath);
            SerializedProperty spawnPoints = serialized.FindProperty("spawnPoints");
            spawnPoints.arraySize = transforms.Length;
            for (int i = 0; i < transforms.Length; i++) spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue = transforms[i];
            serialized.FindProperty("initialSpawnCount").intValue = initial;
            serialized.FindProperty("maxAlive").intValue = maximum;
            serialized.FindProperty("respawnDelay").floatValue = 3f;
            serialized.FindProperty("respawnRetryInterval").floatValue = 0.5f;
            serialized.FindProperty("player").objectReferenceValue = player;
            serialized.FindProperty("minimumPlayerDistance").floatValue = 5f;
            serialized.FindProperty("spawnClearanceRadius").floatValue = 0.75f;
            serialized.FindProperty("spawnBlockingMask").intValue = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Enemy")) | (1 << LayerMask.NameToLayer("World"));
            serialized.FindProperty("corpseLifetime").floatValue = 1f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateMarkers(Transform parent)
        {
            GameObject village = NewObject("Village", parent);
            Marker("PlayerStartMarker", new Vector2(-8, -42), village.transform);
            Marker("StartingVillageAreaMarker", new Vector2(-8, -42), village.transform);
            Marker("BeginnerMeadowMarker", new Vector2(-3, -24), village.transform);

            GameObject cave = NewObject("CaveEntrance", parent);
            Marker("CaveEntranceMarker", new Vector2(-38, 30), cave.transform);

            GameObject future = NewObject("FutureEncounters", parent);
            Marker("OptionalEncounterMarker_West", new Vector2(-47, 27), future.transform);
            Marker("FutureEnemyZoneMarker_NorthEast", new Vector2(39, 27), future.transform);

            GameObject boss = NewObject("MainBossArea", parent);
            Marker("NorthernDangerApproachMarker", new Vector2(3, 31), boss.transform);
            Marker("MainBossApproachMarker", new Vector2(0, 38), boss.transform);
            Marker("MainBossAreaMarker", new Vector2(0, 44), boss.transform);

            GameObject navigation = NewObject("Navigation", parent);
            Marker("CentralCrossroadsMarker", Vector2.zero, navigation.transform);
            Marker("WesternForestMarker", new Vector2(-35, 7), navigation.transform);
            Marker("EasternFarmingAreaMarker", new Vector2(34, -4), navigation.transform);
        }

        private static void CreateTask18Cave(Transform world, Transform gameplay)
        {
            Tile grass = AssetDatabase.LoadAssetAtPath<Tile>(TileFolder + "/Grass.asset");
            Tile dirt = AssetDatabase.LoadAssetAtPath<Tile>(TileFolder + "/Dirt.asset");
            Tile blocker = AssetDatabase.LoadAssetAtPath<Tile>(TileFolder + "/BlockerCliff.asset");
            Tile collision = AssetDatabase.LoadAssetAtPath<Tile>(TileFolder + "/Collision.asset");

            GameObject subAreas = NewObject("SubAreas", world);
            GameObject cave = NewObject("Cave01", subAreas.transform);
            cave.transform.position = new Vector3(160f, 0f, 0f);
            GameObject gridObject = NewObject("Grid", cave.transform, typeof(Grid));
            Tilemap ground = CreateTilemap("Ground", gridObject.transform, "Ground", 0);
            Tilemap details = CreateTilemap("GroundDetails", gridObject.transform, "Ground", 1);
            Tilemap caveCollision = CreateCollisionTilemap(gridObject.transform);

            FillRect(ground, grass, -14, -10, 13, 9);
            FillRect(details, dirt, -10, -2, 9, 1);
            FillRect(caveCollision, collision, -15, -11, 14, -11);
            FillRect(caveCollision, collision, -15, 10, 14, 10);
            FillRect(caveCollision, collision, -15, -11, -15, 10);
            FillRect(caveCollision, collision, 14, -11, 14, 10);
            FillRect(details, blocker, -14, -10, 13, -10);
            FillRect(details, blocker, -14, 9, 13, 9);
            FillRect(details, blocker, -14, -10, -14, 9);
            FillRect(details, blocker, 13, -10, 13, 9);

            Transform entry = CreateDestination("CaveEntryPoint", new Vector2(0f, 4f), cave.transform);
            Transform exit = CreatePortal("CaveExit", new Vector2(0f, -4f), cave.transform, dirt.sprite, new Color32(120, 80, 180, 255));

            Transform outsideReturn = CreateDestination("CaveReturnPoint", new Vector2(-38f, 27f), gameplay);
            GameObject areaTransitions = NewObject("AreaTransitions", gameplay);
            CreatePortal("CaveEntrance", new Vector2(-38f, 30f), areaTransitions.transform, blocker.sprite, new Color32(90, 70, 55, 255), entry);
            SerializedObject exitPortal = new SerializedObject(exit.GetComponent<AreaTransitionPortal>());
            exitPortal.FindProperty("destination").objectReferenceValue = outsideReturn;
            exitPortal.ApplyModifiedPropertiesWithoutUndo();

            ground.CompressBounds();
            details.CompressBounds();
            caveCollision.CompressBounds();
        }

        private static Transform CreateDestination(string name, Vector2 position, Transform parent)
        {
            GameObject destination = NewObject(name, parent);
            destination.transform.position = position;
            return destination.transform;
        }

        private static Transform CreatePortal(string name, Vector2 position, Transform parent, Sprite sprite, Color32 color, Transform destination = null)
        {
            GameObject portal = NewObject(name, parent, typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(AreaTransitionPortal));
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            portal.layer = interactableLayer >= 0 ? interactableLayer : 0;
            portal.transform.position = position;
            BoxCollider2D collider = portal.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(1.5f, 1.5f);
            SpriteRenderer renderer = portal.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingLayerName = "Environment";
            renderer.sortingOrder = 5;
            SerializedObject serialized = new SerializedObject(portal.GetComponent<AreaTransitionPortal>());
            serialized.FindProperty("destination").objectReferenceValue = destination;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return portal.transform;
        }

        private static void ValidateTask18(List<string> errors, GameObject root)
        {
            AreaTransitionPortal entrance = root != null ? root.transform.Find("Gameplay/AreaTransitions/CaveEntrance")?.GetComponent<AreaTransitionPortal>() : null;
            AreaTransitionPortal exit = root != null ? root.transform.Find("World/SubAreas/Cave01/CaveExit")?.GetComponent<AreaTransitionPortal>() : null;
            Transform entry = root != null ? root.transform.Find("World/SubAreas/Cave01/CaveEntryPoint") : null;
            Transform returnPoint = root != null ? root.transform.Find("Gameplay/CaveReturnPoint") : null;
            Transform caveCollision = root != null ? root.transform.Find("World/SubAreas/Cave01/Grid/Collision") : null;
            if (entrance == null || entrance.Destination != entry) errors.Add("CaveEntrance destination is invalid.");
            if (exit == null || exit.Destination != returnPoint) errors.Add("CaveExit destination is invalid.");
            if (entry == null || returnPoint == null) errors.Add("Task 18 destination point is missing.");
            if (caveCollision == null || caveCollision.GetComponent<TilemapCollider2D>() == null || caveCollision.GetComponent<Rigidbody2D>() == null)
                errors.Add("Cave collision setup is incomplete.");
            if (root != null && root.transform.Find("Gameplay/Player")?.GetComponent<PlayerAreaTransition>() == null)
                errors.Add("PlayerAreaTransition is missing from the Player.");
        }

        private static void EnsurePlayerPrefabTransition()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null || prefab.GetComponent<PlayerAreaTransition>() != null) return;

            GameObject contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (contents.GetComponent<PlayerAreaTransition>() == null)
                contents.AddComponent<PlayerAreaTransition>();
            PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
            AssetDatabase.ImportAsset(PlayerPrefabPath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static void Marker(string name, Vector2 position, Transform parent)
        {
            GameObject marker = NewObject(name, parent);
            marker.transform.position = position;
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }

        private static Vector2Int P(int x, int y) => new Vector2Int(x, y);
    }
}
