using System.Collections.Generic;

namespace HexaRealm.Save
{
    public interface IGameSaveMigration
    {
        int SourceVersion { get; }
        int TargetVersion { get; }
        bool TryMigrate(GameSaveData data, out string errorMessage);
    }

    /// <summary>Runs explicit, ordered schema migrations. Version 1 has no historical migrations.</summary>
    public sealed class SaveMigrationPipeline
    {
        private readonly List<IGameSaveMigration> migrations;

        public SaveMigrationPipeline(IEnumerable<IGameSaveMigration> migrations = null)
        {
            this.migrations = migrations != null
                ? new List<IGameSaveMigration>(migrations)
                : new List<IGameSaveMigration>();
        }

        public bool TryMigrate(GameSaveData data, out string errorMessage)
        {
            while (data.schemaVersion < GameSaveData.CurrentSchemaVersion)
            {
                IGameSaveMigration migration = FindMigration(data.schemaVersion);
                if (migration == null)
                {
                    errorMessage = $"No migration is registered for schema version {data.schemaVersion}.";
                    return false;
                }

                int sourceVersion = data.schemaVersion;
                if (!migration.TryMigrate(data, out errorMessage))
                {
                    return false;
                }

                if (migration.TargetVersion <= sourceVersion || data.schemaVersion != migration.TargetVersion)
                {
                    errorMessage = $"Migration {sourceVersion} -> {migration.TargetVersion} produced an invalid schema version.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return data.schemaVersion == GameSaveData.CurrentSchemaVersion;
        }

        private IGameSaveMigration FindMigration(int sourceVersion)
        {
            for (int index = 0; index < migrations.Count; index++)
            {
                IGameSaveMigration migration = migrations[index];
                if (migration != null && migration.SourceVersion == sourceVersion)
                {
                    return migration;
                }
            }

            return null;
        }
    }
}
