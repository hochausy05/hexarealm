using System;

namespace HexaRealm.Progression
{
    /// <summary>Prototype-only Soul cost formula for the next player upgrade.</summary>
    public static class SoulUpgradeCostCalculator
    {
        public static int CalculateNextCost(int baseCost, int costGrowthPerUpgrade, int totalUpgradeCount)
        {
            long safeBaseCost = Math.Max(1, baseCost);
            long safeGrowth = Math.Max(0, costGrowthPerUpgrade);
            long safeTotal = Math.Max(0, totalUpgradeCount);
            long cost = safeBaseCost + safeGrowth * safeTotal;
            return cost >= int.MaxValue ? int.MaxValue : (int)cost;
        }
    }
}
