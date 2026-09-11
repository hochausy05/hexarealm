using UnityEngine;

namespace HexaRealm.Progression
{
    [CreateAssetMenu(fileName = "RegionProgressionReward", menuName = "HexaRealm/Progression/Region Progression Reward")]
    public sealed class RegionProgressionRewardData : ScriptableObject
    {
        [SerializeField] private RegionId completedRegion = RegionId.HumanRealm;
        [SerializeField] private RegionId teleportStoneRegion = RegionId.HumanRealm;
        [SerializeField, Min(0)] private int unlockedUpgradeCap = 20;

        public RegionId CompletedRegion => completedRegion;
        public RegionId TeleportStoneRegion => teleportStoneRegion;
        public int UnlockedUpgradeCap => unlockedUpgradeCap;
        public bool IsValid => completedRegion == teleportStoneRegion && unlockedUpgradeCap >= 0;
    }
}
