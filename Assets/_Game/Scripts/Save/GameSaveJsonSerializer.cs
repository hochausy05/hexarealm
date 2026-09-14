using System;
using UnityEngine;

namespace HexaRealm.Save
{
    public sealed class GameSaveJsonSerializer
    {
        [Serializable]
        private sealed class SaveVersionHeader
        {
            public int schemaVersion;
        }

        private readonly SaveMigrationPipeline migrationPipeline;

        public GameSaveJsonSerializer(SaveMigrationPipeline migrationPipeline = null)
        {
            this.migrationPipeline = migrationPipeline ?? new SaveMigrationPipeline();
        }

        public bool TrySerialize(GameSaveData data, out string json, out SaveError error, out string errorMessage)
        {
            json = null;
            if (data == null)
            {
                error = SaveError.InvalidData;
                errorMessage = "Save data is null.";
                return false;
            }

            if (data.schemaVersion != GameSaveData.CurrentSchemaVersion)
            {
                error = SaveError.InvalidSchemaVersion;
                errorMessage = $"Only schema version {GameSaveData.CurrentSchemaVersion} can be written; received {data.schemaVersion}.";
                return false;
            }

            try
            {
                data.EnsureSections();
                json = JsonUtility.ToJson(data, true);
                if (string.IsNullOrWhiteSpace(json))
                {
                    error = SaveError.SerializationFailed;
                    errorMessage = "JSON serialization returned no data.";
                    return false;
                }
            }
            catch (ArgumentException exception)
            {
                error = SaveError.SerializationFailed;
                errorMessage = exception.Message;
                return false;
            }

            error = SaveError.None;
            errorMessage = string.Empty;
            return true;
        }

        public bool TryDeserialize(string json, out GameSaveData data, out SaveError error, out string errorMessage)
        {
            data = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                error = SaveError.EmptyFile;
                errorMessage = "Save file is empty.";
                return false;
            }

            SaveVersionHeader header;
            try
            {
                header = JsonUtility.FromJson<SaveVersionHeader>(json);
            }
            catch (ArgumentException exception)
            {
                error = SaveError.MalformedJson;
                errorMessage = exception.Message;
                return false;
            }

            if (header == null || header.schemaVersion <= 0)
            {
                error = SaveError.InvalidSchemaVersion;
                errorMessage = "Save file has no valid positive schemaVersion.";
                return false;
            }

            if (header.schemaVersion > GameSaveData.CurrentSchemaVersion)
            {
                error = SaveError.UnsupportedFutureSchema;
                errorMessage = $"Save schema {header.schemaVersion} is newer than supported schema {GameSaveData.CurrentSchemaVersion}.";
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (ArgumentException exception)
            {
                error = SaveError.MalformedJson;
                errorMessage = exception.Message;
                return false;
            }

            if (data == null)
            {
                error = SaveError.MalformedJson;
                errorMessage = "JSON did not contain a save object.";
                return false;
            }

            if (data.schemaVersion < GameSaveData.CurrentSchemaVersion &&
                !migrationPipeline.TryMigrate(data, out errorMessage))
            {
                data = null;
                error = SaveError.MigrationUnavailable;
                return false;
            }

            if (data.schemaVersion != GameSaveData.CurrentSchemaVersion)
            {
                data = null;
                error = SaveError.InvalidSchemaVersion;
                errorMessage = "Save migration did not produce the current schema version.";
                return false;
            }

            data.EnsureSections();
            error = SaveError.None;
            errorMessage = string.Empty;
            return true;
        }
    }
}
