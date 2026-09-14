using System;
using System.IO;
using System.Security;
using System.Text;
using UnityEngine;

namespace HexaRealm.Save
{
    /// <summary>Owns JSON serialization and safe, same-directory save-file operations.</summary>
    public sealed class SaveFileService
    {
        public const string DefaultSaveFileName = "save_slot_0.json";
        public const string DefaultBackupFileName = "save_slot_0.backup.json";

        private readonly GameSaveJsonSerializer serializer;

        public SaveFileService(
            string saveDirectory,
            string saveFileName = DefaultSaveFileName,
            string backupFileName = DefaultBackupFileName,
            GameSaveJsonSerializer serializer = null)
        {
            if (string.IsNullOrWhiteSpace(saveDirectory)) throw new ArgumentException("Save directory is required.", nameof(saveDirectory));
            ValidateFileName(saveFileName, nameof(saveFileName));
            ValidateFileName(backupFileName, nameof(backupFileName));

            SaveDirectory = Path.GetFullPath(saveDirectory);
            PrimarySavePath = Path.Combine(SaveDirectory, saveFileName);
            BackupSavePath = Path.Combine(SaveDirectory, backupFileName);
            TemporarySavePath = PrimarySavePath + ".tmp";
            this.serializer = serializer ?? new GameSaveJsonSerializer();
        }

        public string SaveDirectory { get; }
        public string PrimarySavePath { get; }
        public string BackupSavePath { get; }
        public string TemporarySavePath { get; }

        public static SaveFileService CreateDefault()
        {
            return new SaveFileService(Application.persistentDataPath);
        }

        public bool HasSave()
        {
            return File.Exists(PrimarySavePath) || File.Exists(BackupSavePath);
        }

        public SaveResult Save(GameSaveData data)
        {
            if (!serializer.TrySerialize(data, out string json, out SaveError serializationError, out string serializationMessage))
            {
                return SaveResult.Failure(serializationError, PrimarySavePath, serializationMessage);
            }

            try
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            catch (UnauthorizedAccessException exception)
            {
                return SaveResult.Failure(SaveError.DirectoryAccessFailed, SaveDirectory, exception.Message);
            }
            catch (SecurityException exception)
            {
                return SaveResult.Failure(SaveError.DirectoryAccessFailed, SaveDirectory, exception.Message);
            }
            catch (IOException exception)
            {
                return SaveResult.Failure(SaveError.DirectoryAccessFailed, SaveDirectory, exception.Message);
            }
            catch (NotSupportedException exception)
            {
                return SaveResult.Failure(SaveError.DirectoryAccessFailed, SaveDirectory, exception.Message);
            }

            try
            {
                WriteCompleteTemporaryFile(json);

                if (!File.Exists(PrimarySavePath))
                {
                    File.Move(TemporarySavePath, PrimarySavePath);
                    return SaveResult.Success(PrimarySavePath);
                }

                LoadResult existingPrimary = LoadFile(PrimarySavePath, SaveLoadSource.Primary);
                if (existingPrimary.Error == SaveError.UnsupportedFutureSchema)
                {
                    return SaveResult.Failure(existingPrimary.Error, PrimarySavePath,
                        "Refusing to overwrite a save created by a newer schema.");
                }

                if (existingPrimary.Error == SaveError.ReadFailed)
                {
                    return SaveResult.Failure(existingPrimary.Error, PrimarySavePath,
                        "The existing save could not be read safely and was not overwritten: " + existingPrimary.Message);
                }

                if (existingPrimary.Succeeded)
                {
                    PromoteWithBackup();
                }
                else
                {
                    PromoteWithoutReplacingBackup();
                }

                return SaveResult.Success(PrimarySavePath);
            }
            catch (UnauthorizedAccessException exception)
            {
                return SaveResult.Failure(SaveError.WriteFailed, PrimarySavePath, exception.Message);
            }
            catch (SecurityException exception)
            {
                return SaveResult.Failure(SaveError.WriteFailed, PrimarySavePath, exception.Message);
            }
            catch (IOException exception)
            {
                return SaveResult.Failure(SaveError.WriteFailed, PrimarySavePath, exception.Message);
            }
            catch (NotSupportedException exception)
            {
                return SaveResult.Failure(SaveError.WriteFailed, PrimarySavePath, exception.Message);
            }
        }

        public LoadResult Load()
        {
            bool hasPrimary = File.Exists(PrimarySavePath);
            bool hasBackup = File.Exists(BackupSavePath);
            if (!hasPrimary && !hasBackup)
            {
                return LoadResult.Failure(SaveError.NoSaveFound, PrimarySavePath, "No primary or backup save exists.");
            }

            LoadResult primaryResult = hasPrimary
                ? LoadFile(PrimarySavePath, SaveLoadSource.Primary)
                : LoadResult.Failure(SaveError.NoSaveFound, PrimarySavePath, "Primary save does not exist.");

            if (primaryResult.Succeeded || primaryResult.Error == SaveError.UnsupportedFutureSchema)
            {
                return primaryResult;
            }

            if (hasBackup)
            {
                LoadResult backupResult = LoadFile(BackupSavePath, SaveLoadSource.Backup);
                if (backupResult.Succeeded || backupResult.Error == SaveError.UnsupportedFutureSchema)
                {
                    return backupResult;
                }

                return LoadResult.Failure(backupResult.Error, backupResult.Path,
                    $"Primary save failed ({primaryResult.Message}); backup also failed ({backupResult.Message}).");
            }

            return primaryResult;
        }

        public SaveResult DeleteSave()
        {
            try
            {
                DeleteIfPresent(PrimarySavePath);
                DeleteIfPresent(BackupSavePath);
                DeleteIfPresent(TemporarySavePath);
                return SaveResult.Success(PrimarySavePath);
            }
            catch (UnauthorizedAccessException exception)
            {
                return SaveResult.Failure(SaveError.DeleteFailed, PrimarySavePath, exception.Message);
            }
            catch (SecurityException exception)
            {
                return SaveResult.Failure(SaveError.DeleteFailed, PrimarySavePath, exception.Message);
            }
            catch (IOException exception)
            {
                return SaveResult.Failure(SaveError.DeleteFailed, PrimarySavePath, exception.Message);
            }
            catch (NotSupportedException exception)
            {
                return SaveResult.Failure(SaveError.DeleteFailed, PrimarySavePath, exception.Message);
            }
        }

        private LoadResult LoadFile(string path, SaveLoadSource source)
        {
            string json;
            try
            {
                json = File.ReadAllText(path, Encoding.UTF8);
            }
            catch (UnauthorizedAccessException exception)
            {
                return LoadResult.Failure(SaveError.ReadFailed, path, exception.Message);
            }
            catch (SecurityException exception)
            {
                return LoadResult.Failure(SaveError.ReadFailed, path, exception.Message);
            }
            catch (IOException exception)
            {
                return LoadResult.Failure(SaveError.ReadFailed, path, exception.Message);
            }
            catch (NotSupportedException exception)
            {
                return LoadResult.Failure(SaveError.ReadFailed, path, exception.Message);
            }

            if (!serializer.TryDeserialize(json, out GameSaveData data, out SaveError error, out string errorMessage))
            {
                return LoadResult.Failure(error, path, errorMessage);
            }

            return LoadResult.Success(data, source, path);
        }

        private void WriteCompleteTemporaryFile(string json)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(json);
            using (FileStream stream = new FileStream(TemporarySavePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }

        private void PromoteWithBackup()
        {
            try
            {
                File.Replace(TemporarySavePath, PrimarySavePath, BackupSavePath, true);
            }
            catch (PlatformNotSupportedException)
            {
                PromoteWithCopyFallback();
            }
            catch (IOException)
            {
                PromoteWithCopyFallback();
            }
        }

        private void PromoteWithCopyFallback()
        {
            File.Copy(PrimarySavePath, BackupSavePath, true);
            File.Copy(TemporarySavePath, PrimarySavePath, true);
            File.Delete(TemporarySavePath);
        }

        private void PromoteWithoutReplacingBackup()
        {
            try
            {
                File.Replace(TemporarySavePath, PrimarySavePath, null, true);
            }
            catch (PlatformNotSupportedException)
            {
                PromoteCorruptPrimaryWithCopyFallback();
            }
            catch (IOException)
            {
                PromoteCorruptPrimaryWithCopyFallback();
            }
        }

        private void PromoteCorruptPrimaryWithCopyFallback()
        {
            File.Copy(TemporarySavePath, PrimarySavePath, true);
            File.Delete(TemporarySavePath);
        }

        private static void DeleteIfPresent(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private static void ValidateFileName(string fileName, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
            {
                throw new ArgumentException("A plain file name without directory components is required.", parameterName);
            }
        }
    }
}
