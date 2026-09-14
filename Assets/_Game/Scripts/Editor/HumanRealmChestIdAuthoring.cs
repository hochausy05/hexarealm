#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HexaRealm.Loot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaRealm.EditorTools
{
    /// <summary>One-time, field-only T22.3 migration for the three existing HumanRealm chest instances.</summary>
    public static class HumanRealmChestIdAuthoring
    {
        private const string ScenePath = "Assets/_Game/Scenes/HumanRealm/HumanRealm.unity";

        private static readonly IReadOnlyDictionary<string, string> IdByObjectName =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "NorthernSideRouteChest", "humanrealm.chest.northern-side-route.001" },
                { "EasternFarmChest", "humanrealm.chest.eastern-farm.001" },
                { "WesternForestChest", "humanrealm.chest.western-forest.001" }
            };

        public static void ApplyForBatchMode()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            LootChest[] chests = UnityEngine.Object.FindObjectsByType<LootChest>(FindObjectsInactive.Include);

            if (chests.Length != IdByObjectName.Count)
            {
                throw new InvalidOperationException(
                    $"Expected exactly {IdByObjectName.Count} LootChest instances in HumanRealm, found {chests.Length}.");
            }

            HashSet<string> resultingIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < chests.Length; index++)
            {
                LootChest chest = chests[index];
                if (!IdByObjectName.TryGetValue(chest.name, out string desiredId))
                {
                    throw new InvalidOperationException($"Unexpected HumanRealm LootChest '{chest.name}'.");
                }

                if (!string.IsNullOrWhiteSpace(chest.PersistentId) &&
                    !string.Equals(chest.PersistentId, desiredId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"LootChest '{chest.name}' already has conflicting persistent ID '{chest.PersistentId}'.");
                }

                SerializedObject serializedChest = new SerializedObject(chest);
                SerializedProperty idProperty = serializedChest.FindProperty("persistentId");
                if (idProperty == null) throw new InvalidOperationException("LootChest persistentId field was not found.");
                idProperty.stringValue = desiredId;
                serializedChest.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(chest);
                EditorUtility.SetDirty(chest);

                if (!resultingIds.Add(desiredId))
                {
                    throw new InvalidOperationException($"Planned duplicate LootChest persistent ID '{desiredId}'.");
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Unity could not save the HumanRealm scene.");
            }

            Debug.Log("T22.3 HumanRealm chest IDs authored and validated without changing other component fields.");
        }
    }
}
#endif
