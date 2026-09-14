using System;
using System.Collections.Generic;
using HexaRealm.Loot;
using HexaRealm.Progression;
using HexaRealm.Save;
using UnityEngine;

namespace HexaRealm.SaveIntegration
{
    /// <summary>Captures and restores authoritative scene-world state without replaying gameplay.</summary>
    public sealed class WorldSaveAdapter
    {
        public sealed class RestorePlan
        {
            internal readonly List<LootChest> Chests = new List<LootChest>();
            internal readonly HashSet<string> OpenedChestIds = new HashSet<string>(StringComparer.Ordinal);
            internal readonly List<RegionBossProgressionReward> CompletedRegionBosses =
                new List<RegionBossProgressionReward>();
        }

        private readonly Func<IReadOnlyList<LootChest>> chestProvider;
        private readonly Func<IReadOnlyList<RegionBossProgressionReward>> regionBossProvider;

        public WorldSaveAdapter()
            : this(FindCurrentChests, FindCurrentRegionBosses)
        {
        }

        public WorldSaveAdapter(Func<IReadOnlyList<LootChest>> chestProvider)
            : this(chestProvider, () => Array.Empty<RegionBossProgressionReward>())
        {
        }

        public WorldSaveAdapter(
            Func<IReadOnlyList<LootChest>> chestProvider,
            Func<IReadOnlyList<RegionBossProgressionReward>> regionBossProvider)
        {
            this.chestProvider = chestProvider ?? throw new ArgumentNullException(nameof(chestProvider));
            this.regionBossProvider = regionBossProvider ?? throw new ArgumentNullException(nameof(regionBossProvider));
        }

        public bool TryCapture(GameSaveData data, out string errorMessage)
        {
            if (data == null)
            {
                errorMessage = "Save data is null.";
                return false;
            }

            if (!TryBuildChestLookup(out List<LootChest> chests, out _, out errorMessage)) return false;

            List<string> openedIds = new List<string>();
            for (int index = 0; index < chests.Count; index++)
            {
                if (chests[index].IsOpened) openedIds.Add(chests[index].PersistentId);
            }

            openedIds.Sort(StringComparer.Ordinal);
            data.EnsureSections();
            data.world.openedChestIds.Clear();
            data.world.openedChestIds.AddRange(openedIds);
            return true;
        }

        public bool TryPrepareRestore(
            GameSaveData data,
            List<string> warnings,
            out RestorePlan plan,
            out string errorMessage)
        {
            plan = null;
            if (data == null)
            {
                errorMessage = "Save data is null.";
                return false;
            }

            if (warnings == null)
            {
                errorMessage = "A warning destination is required.";
                return false;
            }

            data.EnsureSections();
            if (!TryBuildChestLookup(out List<LootChest> chests, out Dictionary<string, LootChest> chestById,
                    out errorMessage))
            {
                return false;
            }

            RestorePlan candidate = new RestorePlan();
            candidate.Chests.AddRange(chests);

            HashSet<string> uniqueSavedIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < data.world.openedChestIds.Count; index++)
            {
                string id = NormalizeId(data.world.openedChestIds[index]);
                if (!uniqueSavedIds.Add(id)) continue;
                if (id.Length == 0)
                {
                    warnings.Add("Saved opened LootChest ID is empty; skipped.");
                    continue;
                }

                if (!chestById.ContainsKey(id))
                {
                    warnings.Add($"Unknown saved LootChest ID '{id}'; current scene has no matching chest, so it was skipped.");
                    continue;
                }

                candidate.OpenedChestIds.Add(id);
            }

            HashSet<int> completedRegions = new HashSet<int>();
            for (int index = 0; index < data.progression.completedRegions.Count; index++)
            {
                int value = data.progression.completedRegions[index];
                if (Enum.IsDefined(typeof(RegionId), value)) completedRegions.Add(value);
            }

            IReadOnlyList<RegionBossProgressionReward> regionBosses = regionBossProvider();
            if (regionBosses != null)
            {
                for (int index = 0; index < regionBosses.Count; index++)
                {
                    RegionBossProgressionReward reward = regionBosses[index];
                    if (reward != null && reward.HasValidRewardData &&
                        completedRegions.Contains((int)reward.CompletedRegion))
                    {
                        candidate.CompletedRegionBosses.Add(reward);
                    }
                }
            }

            plan = candidate;
            errorMessage = string.Empty;
            return true;
        }

        public void ApplyRestore(RestorePlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            for (int index = 0; index < plan.Chests.Count; index++)
            {
                LootChest chest = plan.Chests[index];
                if (chest != null)
                {
                    chest.RestoreOpenedState(plan.OpenedChestIds.Contains(NormalizeId(chest.PersistentId)));
                }
            }

            for (int index = 0; index < plan.CompletedRegionBosses.Count; index++)
            {
                RegionBossProgressionReward reward = plan.CompletedRegionBosses[index];
                if (reward != null) reward.RestoreCompletedWorldState();
            }
        }

        private bool TryBuildChestLookup(
            out List<LootChest> chests,
            out Dictionary<string, LootChest> chestById,
            out string errorMessage)
        {
            chests = new List<LootChest>();
            chestById = new Dictionary<string, LootChest>(StringComparer.Ordinal);
            IReadOnlyList<LootChest> currentChests = chestProvider();
            if (currentChests == null)
            {
                errorMessage = "LootChest provider returned no collection.";
                return false;
            }

            for (int index = 0; index < currentChests.Count; index++)
            {
                LootChest chest = currentChests[index];
                if (chest == null) continue;

                string id = NormalizeId(chest.PersistentId);
                if (id.Length == 0)
                {
                    errorMessage = $"LootChest '{chest.name}' has an empty persistent ID.";
                    return false;
                }

                if (chestById.TryGetValue(id, out LootChest duplicate))
                {
                    errorMessage = $"Duplicate LootChest persistent ID '{id}' on '{duplicate.name}' and '{chest.name}'.";
                    return false;
                }

                chestById.Add(id, chest);
                chests.Add(chest);
            }

            errorMessage = string.Empty;
            return true;
        }

        private static IReadOnlyList<LootChest> FindCurrentChests()
        {
            return UnityEngine.Object.FindObjectsByType<LootChest>(FindObjectsInactive.Include);
        }

        private static IReadOnlyList<RegionBossProgressionReward> FindCurrentRegionBosses()
        {
            return UnityEngine.Object.FindObjectsByType<RegionBossProgressionReward>(FindObjectsInactive.Include);
        }

        private static string NormalizeId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
