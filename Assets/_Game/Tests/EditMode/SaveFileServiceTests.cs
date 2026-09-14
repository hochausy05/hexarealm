using System;
using System.IO;
using HexaRealm.Save;
using NUnit.Framework;

namespace HexaRealm.Tests.EditMode
{
    public sealed class SaveFileServiceTests
    {
        private string testDirectory;
        private SaveFileService service;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), "HexaRealmSaveTests", Guid.NewGuid().ToString("N"));
            service = new SaveFileService(testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory)) Directory.Delete(testDirectory, true);
        }

        [Test]
        public void Serializer_SerializesValidVersionedRootAndSectionBoundaries()
        {
            GameSaveData data = CreateData("serialize-test");
            GameSaveJsonSerializer serializer = new GameSaveJsonSerializer();

            bool succeeded = serializer.TrySerialize(data, out string json, out SaveError error, out string message);

            Assert.That(succeeded, Is.True, message);
            Assert.That(error, Is.EqualTo(SaveError.None));
            Assert.That(json, Does.Contain("\"schemaVersion\": 1"));
            Assert.That(json, Does.Contain("\"player\""));
            Assert.That(json, Does.Contain("\"progression\""));
            Assert.That(json, Does.Contain("\"equipment\""));
            Assert.That(json, Does.Contain("\"world\""));
        }

        [Test]
        public void Serializer_DeserializesValidDataAndRetainsSchemaVersion()
        {
            GameSaveJsonSerializer serializer = new GameSaveJsonSerializer();
            const string json = "{\"schemaVersion\":1,\"metadata\":{\"savedAtUtc\":\"round-trip\"},\"player\":{},\"progression\":{},\"equipment\":{},\"world\":{}}";

            bool succeeded = serializer.TryDeserialize(json, out GameSaveData data, out SaveError error, out string message);

            Assert.That(succeeded, Is.True, message);
            Assert.That(error, Is.EqualTo(SaveError.None));
            Assert.That(data.schemaVersion, Is.EqualTo(GameSaveData.CurrentSchemaVersion));
            Assert.That(data.metadata.savedAtUtc, Is.EqualTo("round-trip"));
            Assert.That(data.player, Is.Not.Null);
            Assert.That(data.progression, Is.Not.Null);
            Assert.That(data.equipment, Is.Not.Null);
            Assert.That(data.world, Is.Not.Null);
        }

        [Test]
        public void HasSave_IsFalseWhenSlotFilesDoNotExist()
        {
            Assert.That(service.HasSave(), Is.False);
        }

        [Test]
        public void FirstSave_WritesReadablePrimaryOnly()
        {
            SaveResult saveResult = service.Save(CreateData("first"));
            LoadResult loadResult = service.Load();

            Assert.That(saveResult.Succeeded, Is.True, saveResult.Message);
            Assert.That(File.Exists(service.PrimarySavePath), Is.True);
            Assert.That(File.Exists(service.BackupSavePath), Is.False);
            Assert.That(loadResult.Succeeded, Is.True, loadResult.Message);
            Assert.That(loadResult.Source, Is.EqualTo(SaveLoadSource.Primary));
            Assert.That(loadResult.Data.metadata.savedAtUtc, Is.EqualTo("first"));
        }

        [Test]
        public void SecondSave_ReplacesPrimaryAndPreservesPreviousPrimaryAsBackup()
        {
            Assert.That(service.Save(CreateData("first")).Succeeded, Is.True);
            SaveResult secondSave = service.Save(CreateData("second"));

            Assert.That(secondSave.Succeeded, Is.True, secondSave.Message);
            Assert.That(service.Load().Data.metadata.savedAtUtc, Is.EqualTo("second"));

            string backupJson = File.ReadAllText(service.BackupSavePath);
            GameSaveJsonSerializer serializer = new GameSaveJsonSerializer();
            Assert.That(serializer.TryDeserialize(backupJson, out GameSaveData backup, out _, out string message), Is.True, message);
            Assert.That(backup.metadata.savedAtUtc, Is.EqualTo("first"));
        }

        [Test]
        public void MalformedPrimary_DoesNotThrowAndBackupRecovers()
        {
            Assert.That(service.Save(CreateData("good-backup")).Succeeded, Is.True);
            Assert.That(service.Save(CreateData("new-primary")).Succeeded, Is.True);
            File.WriteAllText(service.PrimarySavePath, "{ truncated");

            LoadResult result = default;
            Assert.DoesNotThrow(() => result = service.Load());

            Assert.That(result.Succeeded, Is.True, result.Message);
            Assert.That(result.Source, Is.EqualTo(SaveLoadSource.Backup));
            Assert.That(result.Data.metadata.savedAtUtc, Is.EqualTo("good-backup"));
        }

        [Test]
        public void MalformedPrimaryWithoutBackup_ReturnsFailureWithoutThrowing()
        {
            Directory.CreateDirectory(testDirectory);
            File.WriteAllText(service.PrimarySavePath, "{ truncated");

            LoadResult result = default;
            Assert.DoesNotThrow(() => result = service.Load());

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(SaveError.MalformedJson));
        }

        [Test]
        public void FutureSchema_IsRejectedWithoutFallingBackToOlderBackup()
        {
            Assert.That(service.Save(CreateData("backup")).Succeeded, Is.True);
            Assert.That(service.Save(CreateData("primary")).Succeeded, Is.True);
            File.WriteAllText(service.PrimarySavePath, "{\"schemaVersion\":2}");

            LoadResult result = service.Load();

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Is.EqualTo(SaveError.UnsupportedFutureSchema));
            Assert.That(result.Path, Is.EqualTo(service.PrimarySavePath));
        }

        [Test]
        public void DeleteSave_RemovesOnlyFilesOwnedBySlot()
        {
            Assert.That(service.Save(CreateData("first")).Succeeded, Is.True);
            Assert.That(service.Save(CreateData("second")).Succeeded, Is.True);
            File.WriteAllText(service.TemporarySavePath, "stale temp");
            string unrelatedPath = Path.Combine(testDirectory, "keep-me.txt");
            File.WriteAllText(unrelatedPath, "unrelated");

            SaveResult result = service.DeleteSave();

            Assert.That(result.Succeeded, Is.True, result.Message);
            Assert.That(File.Exists(service.PrimarySavePath), Is.False);
            Assert.That(File.Exists(service.BackupSavePath), Is.False);
            Assert.That(File.Exists(service.TemporarySavePath), Is.False);
            Assert.That(File.Exists(unrelatedPath), Is.True);
        }

        [Test]
        public void InjectedSaveDirectory_DoesNotWriteInsideProjectAssets()
        {
            SaveResult result = service.Save(CreateData("outside-assets"));
            string assetsPath = Path.GetFullPath(UnityEngine.Application.dataPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            Assert.That(result.Succeeded, Is.True, result.Message);
            Assert.That(service.PrimarySavePath.StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase), Is.False);
        }

        private static GameSaveData CreateData(string marker)
        {
            GameSaveData data = new GameSaveData();
            data.metadata.savedAtUtc = marker;
            return data;
        }
    }
}
