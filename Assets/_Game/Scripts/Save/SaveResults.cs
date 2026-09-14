namespace HexaRealm.Save
{
    public enum SaveError
    {
        None,
        InvalidData,
        SerializationFailed,
        DirectoryAccessFailed,
        WriteFailed,
        NoSaveFound,
        ReadFailed,
        EmptyFile,
        MalformedJson,
        InvalidSchemaVersion,
        UnsupportedFutureSchema,
        MigrationUnavailable,
        DeleteFailed,
        PlayerNotFound,
        GameplayStateUnavailable,
        InvalidGameplayState,
        GameplayRestoreFailed,
        OperationInProgress
    }

    public enum SaveLoadSource
    {
        None,
        Primary,
        Backup
    }

    public readonly struct SaveResult
    {
        private SaveResult(bool succeeded, SaveError error, string path, string message)
        {
            Succeeded = succeeded;
            Error = error;
            Path = path;
            Message = message;
        }

        public bool Succeeded { get; }
        public SaveError Error { get; }
        public string Path { get; }
        public string Message { get; }

        public static SaveResult Success(string path) => new SaveResult(true, SaveError.None, path, string.Empty);

        public static SaveResult Failure(SaveError error, string path, string message) =>
            new SaveResult(false, error, path, message);
    }

    public readonly struct LoadResult
    {
        private LoadResult(bool succeeded, GameSaveData data, SaveError error, SaveLoadSource source, string path, string message)
        {
            Succeeded = succeeded;
            Data = data;
            Error = error;
            Source = source;
            Path = path;
            Message = message;
        }

        public bool Succeeded { get; }
        public GameSaveData Data { get; }
        public SaveError Error { get; }
        public SaveLoadSource Source { get; }
        public string Path { get; }
        public string Message { get; }

        public static LoadResult Success(GameSaveData data, SaveLoadSource source, string path) =>
            new LoadResult(true, data, SaveError.None, source, path, string.Empty);

        public static LoadResult Failure(SaveError error, string path, string message) =>
            new LoadResult(false, null, error, SaveLoadSource.None, path, message);
    }
}
