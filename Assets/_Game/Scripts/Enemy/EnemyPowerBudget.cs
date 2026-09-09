namespace HexaRealm.Enemy
{
    /// <summary>
    /// Pure power-budget calculation and declared-rank validation. Only EnemyData's four combat
    /// authoring stats participate; current health, SoulReward, and runtime behavior do not.
    /// </summary>
    public static class EnemyPowerBudget
    {
        public static bool CalculateScore(
            EnemyData enemyData,
            EnemyPowerBudgetProfile profile,
            out float score)
        {
            score = 0f;

            if (enemyData == null || profile == null || !enemyData.IsValid || !profile.HasValidScoreWeights)
            {
                return false;
            }

            float calculatedScore =
                enemyData.MaxHealth * profile.HealthWeight +
                enemyData.Attack * profile.AttackWeight +
                enemyData.Defense * profile.DefenseWeight +
                enemyData.Speed * profile.SpeedWeight;

            if (float.IsNaN(calculatedScore) || float.IsInfinity(calculatedScore) || calculatedScore < 0f)
            {
                return false;
            }

            score = calculatedScore;
            return true;
        }

        public static EnemyRankBudgetValidationResult ValidateDeclaredRank(
            EnemyData enemyData,
            EnemyPowerBudgetProfile profile)
        {
            if (enemyData == null || !enemyData.IsValid)
            {
                return EnemyRankBudgetValidationResult.InvalidEnemyData;
            }

            if (profile == null || !profile.IsValidationConfiguredFor(enemyData.Rank))
            {
                return EnemyRankBudgetValidationResult.NotConfigured;
            }

            if (!CalculateScore(enemyData, profile, out float score) ||
                !profile.TryGetRankBudgetRange(enemyData.Rank, out EnemyRankBudgetRange range))
            {
                return EnemyRankBudgetValidationResult.NotConfigured;
            }

            if (score < range.MinimumScore)
            {
                return EnemyRankBudgetValidationResult.BelowExpectedBudget;
            }

            return score > range.MaximumScore
                ? EnemyRankBudgetValidationResult.AboveExpectedBudget
                : EnemyRankBudgetValidationResult.Valid;
        }
    }

    public enum EnemyRankBudgetValidationResult
    {
        Valid,
        BelowExpectedBudget,
        AboveExpectedBudget,
        NotConfigured,
        InvalidEnemyData
    }
}
