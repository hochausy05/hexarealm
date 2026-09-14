using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexaRealm.Progression
{
    /// <summary>Runtime-session ownership for completed regions and their Teleport Stones.</summary>
    public sealed class PlayerRegionProgression : MonoBehaviour
    {
        [SerializeField, HideInInspector] private List<RegionId> completedRegions = new List<RegionId>();
        [SerializeField, HideInInspector] private List<RegionId> teleportStoneRegions = new List<RegionId>();

        public event Action<RegionId> RegionProgressionGranted;

        public IReadOnlyList<RegionId> CompletedRegions => completedRegions;
        public IReadOnlyList<RegionId> TeleportStoneRegions => teleportStoneRegions;
        public int TeleportStoneCount => teleportStoneRegions.Count;
        public bool IsRegionCompleted(RegionId region) => completedRegions.Contains(region);
        public bool HasTeleportStone(RegionId region) => teleportStoneRegions.Contains(region);

        public bool IsRegionUnlocked(RegionId region)
        {
            if (region == RegionId.HumanRealm) return true;
            return IsRegionCompleted((RegionId)((int)region - 1));
        }

        /// <summary>Grants the completion and its stone as one idempotent session-state operation.</summary>
        public bool GrantRegionCompletion(RegionId completedRegion, RegionId teleportStoneRegion)
        {
            bool changed = false;
            if (!completedRegions.Contains(completedRegion))
            {
                completedRegions.Add(completedRegion);
                changed = true;
            }

            if (!teleportStoneRegions.Contains(teleportStoneRegion))
            {
                teleportStoneRegions.Add(teleportStoneRegion);
                changed = true;
            }

            if (changed) RegionProgressionGranted?.Invoke(completedRegion);
            return changed;
        }

        /// <summary>Replaces persistent region state without replaying milestone reward events.</summary>
        public void RestoreState(IEnumerable<RegionId> completed, IEnumerable<RegionId> teleportStones)
        {
            ReplaceWithUniqueValidRegions(completedRegions, completed);
            ReplaceWithUniqueValidRegions(teleportStoneRegions, teleportStones);
        }

        private static void ReplaceWithUniqueValidRegions(List<RegionId> destination, IEnumerable<RegionId> source)
        {
            destination.Clear();
            if (source == null) return;

            foreach (RegionId region in source)
            {
                if (Enum.IsDefined(typeof(RegionId), region) && !destination.Contains(region))
                {
                    destination.Add(region);
                }
            }
        }
    }
}
