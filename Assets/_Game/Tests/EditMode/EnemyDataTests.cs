using System;
using HexaRealm.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace HexaRealm.Tests.EditMode
{
    public sealed class EnemyDataTests
    {
        private const float Tolerance = 0.0001f;
        private EnemyData enemyData;
        private EnemyPowerBudgetProfile profile;

        [SetUp]
        public void SetUp()
        {
            enemyData = ScriptableObject.CreateInstance<EnemyData>();
            profile = ScriptableObject.CreateInstance<EnemyPowerBudgetProfile>();
            ConfigureValidEnemy();
            ConfigureProfile(
                new EnemyRankBudgetRange(EnemyRank.F, 0f, 100f),
                new EnemyRankBudgetRange(EnemyRank.E, 101f, 200f));
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(enemyData);
            UnityEngine.Object.DestroyImmediate(profile);
        }

        [Test]
        public void EnemyRank_ContainsExactlyFThroughS()
        {
            CollectionAssert.AreEqual(
                new[] { EnemyRank.F, EnemyRank.E, EnemyRank.D, EnemyRank.C, EnemyRank.B, EnemyRank.A, EnemyRank.S },
                (EnemyRank[])Enum.GetValues(typeof(EnemyRank)));
        }

        [Test]
        public void ValidAuthoringData_PassesValidation()
        {
            Assert.That(enemyData.Validate(), Is.EqualTo(EnemyDataValidationResult.Valid));
        }

        [Test]
        public void InvalidMaxHealth_FailsValidation()
        {
            enemyData.SetAuthoringValues("Slime", EnemyRank.F, 0f, 3f, 1f, 2f, 5);

            Assert.That(enemyData.Validate(), Is.EqualTo(EnemyDataValidationResult.InvalidMaxHealth));
        }

        [Test]
        public void CalculateScore_IncreasesWhenHealthIncreases()
        {
            float originalScore = CalculateScore();
            enemyData.SetAuthoringValues("Slime", EnemyRank.F, 20f, 3f, 1f, 2f, 5);

            Assert.That(CalculateScore(), Is.GreaterThan(originalScore));
        }

        [Test]
        public void CalculateScore_IncreasesWhenAttackIncreases()
        {
            float originalScore = CalculateScore();
            enemyData.SetAuthoringValues("Slime", EnemyRank.F, 10f, 6f, 1f, 2f, 5);

            Assert.That(CalculateScore(), Is.GreaterThan(originalScore));
        }

        [Test]
        public void CalculateScore_IncreasesWhenDefenseIncreases()
        {
            float originalScore = CalculateScore();
            enemyData.SetAuthoringValues("Slime", EnemyRank.F, 10f, 3f, 4f, 2f, 5);

            Assert.That(CalculateScore(), Is.GreaterThan(originalScore));
        }

        [Test]
        public void CalculateScore_IncreasesWhenSpeedIncreases()
        {
            float originalScore = CalculateScore();
            enemyData.SetAuthoringValues("Slime", EnemyRank.F, 10f, 3f, 1f, 5f, 5);

            Assert.That(CalculateScore(), Is.GreaterThan(originalScore));
        }

        [Test]
        public void CalculateScore_DoesNotUseSoulReward()
        {
            float originalScore = CalculateScore();
            enemyData.SetAuthoringValues("Slime", EnemyRank.F, 10f, 3f, 1f, 2f, 5000);

            Assert.That(CalculateScore(), Is.EqualTo(originalScore).Within(Tolerance));
        }

        [Test]
        public void CalculateScore_DoesNotUseDeclaredRank()
        {
            float originalScore = CalculateScore();
            enemyData.SetAuthoringValues("Slime", EnemyRank.S, 10f, 3f, 1f, 2f, 5);

            Assert.That(CalculateScore(), Is.EqualTo(originalScore).Within(Tolerance));
        }

        [Test]
        public void ValidateDeclaredRank_WithoutConfiguredProfile_ReturnsNotConfigured()
        {
            profile.SetConfiguration(false, 1f, 1f, 1f, 1f);

            Assert.That(
                EnemyPowerBudget.ValidateDeclaredRank(enemyData, profile),
                Is.EqualTo(EnemyRankBudgetValidationResult.NotConfigured));
        }

        [TestCase(-1f, 1f, 1f, 1f)]
        [TestCase(0f, 1f, 1f, 1f)]
        [TestCase(1f, -1f, 1f, 1f)]
        public void ValidationEnabled_InvalidWeight_IsNotConfigured(float health, float attack, float defense, float speed)
        {
            profile.SetConfiguration(true, health, attack, defense, speed, new EnemyRankBudgetRange(EnemyRank.F, 0f, 100f));

            Assert.That(profile.HasValidValidationWeights, Is.False);
            Assert.That(
                EnemyPowerBudget.ValidateDeclaredRank(enemyData, profile),
                Is.EqualTo(EnemyRankBudgetValidationResult.NotConfigured));
        }

        [Test]
        public void ValidateDeclaredRank_WithinDeclaredRange_IsValid()
        {
            Assert.That(
                EnemyPowerBudget.ValidateDeclaredRank(enemyData, profile),
                Is.EqualTo(EnemyRankBudgetValidationResult.Valid));
        }

        [Test]
        public void ValidateDeclaredRank_AboveDeclaredRange_DoesNotMutateRank()
        {
            EnemyRank declaredRank = enemyData.Rank;
            enemyData.SetAuthoringValues("Slime", declaredRank, 150f, 3f, 1f, 2f, 5);

            Assert.That(
                EnemyPowerBudget.ValidateDeclaredRank(enemyData, profile),
                Is.EqualTo(EnemyRankBudgetValidationResult.AboveExpectedBudget));
            Assert.That(enemyData.Rank, Is.EqualTo(declaredRank));
        }

        [Test]
        public void ValidateDeclaredRank_BelowDeclaredRange_DoesNotMutateRank()
        {
            enemyData.SetAuthoringValues("Slime", EnemyRank.E, 10f, 3f, 1f, 2f, 5);
            EnemyRank declaredRank = enemyData.Rank;

            Assert.That(
                EnemyPowerBudget.ValidateDeclaredRank(enemyData, profile),
                Is.EqualTo(EnemyRankBudgetValidationResult.BelowExpectedBudget));
            Assert.That(enemyData.Rank, Is.EqualTo(declaredRank));
        }

        [Test]
        public void ValidateDeclaredRank_IsIdempotentAndDoesNotMutateData()
        {
            EnemyRank declaredRank = enemyData.Rank;
            float maxHealth = enemyData.MaxHealth;
            EnemyRankBudgetValidationResult first = EnemyPowerBudget.ValidateDeclaredRank(enemyData, profile);
            EnemyRankBudgetValidationResult second = EnemyPowerBudget.ValidateDeclaredRank(enemyData, profile);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(enemyData.Rank, Is.EqualTo(declaredRank));
            Assert.That(enemyData.MaxHealth, Is.EqualTo(maxHealth).Within(Tolerance));
        }

        private void ConfigureValidEnemy()
        {
            enemyData.SetAuthoringValues("Slime", EnemyRank.F, 10f, 3f, 1f, 2f, 5);
        }

        private void ConfigureProfile(params EnemyRankBudgetRange[] ranges)
        {
            profile.SetConfiguration(true, 1f, 1f, 1f, 1f, ranges);
        }

        private float CalculateScore()
        {
            Assert.That(EnemyPowerBudget.CalculateScore(enemyData, profile, out float score), Is.True);
            return score;
        }
    }
}
