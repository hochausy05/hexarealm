using System;
using System.Collections.Generic;
using HexaRealm.Boss;
using HexaRealm.Equipment;
using HexaRealm.Loot;
using HexaRealm.Player;
using HexaRealm.Progression;
using HexaRealm.SaveIntegration;
using UnityEngine;

namespace HexaRealm.Save
{
    /// <summary>Coordinates explicit file operations with player gameplay-state capture and restore.</summary>
    [DisallowMultipleComponent]
    public sealed class GameSaveService : MonoBehaviour
    {
        private const string EquipmentCatalogResourcePath = "EquipmentCatalog";
        private static GameSaveService instance;

        private SaveFileService fileService;
        private PlayerSaveAdapter stateAdapter;
        private WorldSaveAdapter worldAdapter;
        private GameSaveData currentData;
        private bool isSaving;
        private bool isRestoring;
        private RegionBossProgressionReward pendingRegionAutosave;

        public static GameSaveService Instance => instance;
        public GameSaveData CurrentData => currentData;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStateIfNeeded();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (GetComponent<GameSaveDebugHotkeys>() == null) gameObject.AddComponent<GameSaveDebugHotkeys>();
#endif
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void OnEnable()
        {
            if (instance != this) return;
            SubscribeAutosaveEvents();
        }

        private void SubscribeAutosaveEvents()
        {
            LootChest.AnyChestOpened -= HandleChestOpened;
            LootChest.AnyChestOpened += HandleChestOpened;
            RegionBossProgressionReward.AnyProgressionCompleted -= HandleRegionProgressionCompleted;
            RegionBossProgressionReward.AnyProgressionCompleted += HandleRegionProgressionCompleted;
        }

        private void OnDisable()
        {
            UnsubscribeAutosaveEvents();
            pendingRegionAutosave = null;
        }

        private void LateUpdate()
        {
            FlushPendingRegionAutosave();
        }

        private void UnsubscribeAutosaveEvents()
        {
            LootChest.AnyChestOpened -= HandleChestOpened;
            RegionBossProgressionReward.AnyProgressionCompleted -= HandleRegionProgressionCompleted;
        }

        public SaveResult Save()
        {
            InitializeStateIfNeeded();
            if (isSaving || isRestoring)
            {
                return SaveResult.Failure(
                    SaveError.OperationInProgress,
                    fileService.PrimarySavePath,
                    "Save was skipped because another save/load operation is in progress.");
            }

            isSaving = true;
            try
            {
                return SaveInternal();
            }
            finally
            {
                isSaving = false;
            }
        }

        private SaveResult SaveInternal()
        {
            if (!TryFindPlayer(out GameObject playerRoot, out string playerError))
            {
                return LogSaveFailure(SaveError.PlayerNotFound, fileService.PrimarySavePath, playerError);
            }

            if (!TryGetStateAdapter(out string adapterError))
            {
                return LogSaveFailure(SaveError.GameplayStateUnavailable, fileService.PrimarySavePath, adapterError);
            }

            if (!stateAdapter.TryCapture(playerRoot, out GameSaveData captured, out string captureError))
            {
                return LogSaveFailure(SaveError.InvalidGameplayState, fileService.PrimarySavePath, captureError);
            }

            if (!worldAdapter.TryCapture(captured, out string worldCaptureError))
            {
                return LogSaveFailure(SaveError.InvalidGameplayState, fileService.PrimarySavePath, worldCaptureError);
            }

            captured.metadata.savedAtUtc = DateTime.UtcNow.ToString("O");
            SaveResult result = fileService.Save(captured);
            if (result.Succeeded)
            {
                currentData = captured;
            }
            else
            {
                Debug.LogError($"Save failed at '{result.Path}': {result.Message}", this);
            }

            return result;
        }

        public LoadResult Load()
        {
            InitializeStateIfNeeded();
            if (isSaving || isRestoring)
            {
                return LoadResult.Failure(
                    SaveError.OperationInProgress,
                    fileService.PrimarySavePath,
                    "Load was skipped because another save/load operation is in progress.");
            }

            isRestoring = true;
            try
            {
                return LoadInternal();
            }
            finally
            {
                isRestoring = false;
            }
        }

        private LoadResult LoadInternal()
        {
            LoadResult fileResult = fileService.Load();
            if (!fileResult.Succeeded)
            {
                if (fileResult.Error != SaveError.NoSaveFound)
                {
                    Debug.LogWarning($"Load failed at '{fileResult.Path}': {fileResult.Message}", this);
                }

                return fileResult;
            }

            if (!TryFindPlayer(out GameObject playerRoot, out string playerError))
            {
                return LogLoadFailure(SaveError.PlayerNotFound, fileResult.Path, playerError);
            }

            if (!TryGetStateAdapter(out string adapterError))
            {
                return LogLoadFailure(SaveError.GameplayStateUnavailable, fileResult.Path, adapterError);
            }

            List<string> warnings = new List<string>();
            if (!stateAdapter.TryPrepareRestore(
                    playerRoot,
                    fileResult.Data,
                    warnings,
                    out PlayerSaveAdapter.RestorePlan playerPlan,
                    out string restoreError))
            {
                return LogLoadFailure(SaveError.GameplayRestoreFailed, fileResult.Path, restoreError);
            }

            if (!worldAdapter.TryPrepareRestore(
                    fileResult.Data,
                    warnings,
                    out WorldSaveAdapter.RestorePlan worldPlan,
                    out string worldRestoreError))
            {
                return LogLoadFailure(SaveError.GameplayRestoreFailed, fileResult.Path, worldRestoreError);
            }

            stateAdapter.ApplyRestore(playerPlan);
            worldAdapter.ApplyRestore(worldPlan);

            currentData = fileResult.Data;
            for (int index = 0; index < warnings.Count; index++)
            {
                Debug.LogWarning($"Load warning at '{fileResult.Path}': {warnings[index]}", this);
            }

            if (fileResult.Source == SaveLoadSource.Backup)
            {
                Debug.LogWarning($"Primary save was unavailable or invalid. Loaded backup from '{fileResult.Path}'.", this);
            }

            return fileResult;
        }

        public bool HasSave()
        {
            InitializeStateIfNeeded();
            return fileService.HasSave();
        }

        public SaveResult DeleteSave()
        {
            InitializeStateIfNeeded();
            SaveResult result = fileService.DeleteSave();
            if (!result.Succeeded)
            {
                Debug.LogError($"Delete save failed at '{result.Path}': {result.Message}", this);
            }

            return result;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureRuntimeService()
        {
            GameSaveService existing = FindAnyObjectByType<GameSaveService>(FindObjectsInactive.Include);
            if (existing != null)
            {
                instance = existing;
                DontDestroyOnLoad(existing.gameObject);
                existing.InitializeStateIfNeeded();
                return;
            }

            GameObject root = new GameObject("[GameSaveService]");
            root.AddComponent<GameSaveService>();
        }

        private void InitializeStateIfNeeded()
        {
            if (fileService == null) fileService = SaveFileService.CreateDefault();
            if (worldAdapter == null) worldAdapter = new WorldSaveAdapter();
            if (currentData == null) currentData = new GameSaveData();
        }

        private bool TryGetStateAdapter(out string errorMessage)
        {
            if (stateAdapter != null)
            {
                errorMessage = string.Empty;
                return true;
            }

            EquipmentCatalog catalog = Resources.Load<EquipmentCatalog>(EquipmentCatalogResourcePath);
            if (catalog == null)
            {
                errorMessage = $"EquipmentCatalog was not found at Resources/{EquipmentCatalogResourcePath}.";
                return false;
            }

            stateAdapter = new PlayerSaveAdapter(catalog);
            errorMessage = string.Empty;
            return true;
        }

        private void HandleChestOpened(LootChest chest)
        {
            RequestAutosave(chest != null ? $"LootChest '{chest.PersistentId}' completion" : "LootChest completion");
        }

        private void HandleRegionProgressionCompleted(RegionBossProgressionReward reward)
        {
            if (isSaving || isRestoring || reward == null) return;
            pendingRegionAutosave = reward;
        }

        private void FlushPendingRegionAutosave()
        {
            if (isSaving || isRestoring) return;
            RegionBossProgressionReward reward = pendingRegionAutosave;
            pendingRegionAutosave = null;
            if (reward == null) return;

            BossReward bossLootReward = reward.GetComponent<BossReward>();
            if (bossLootReward != null && !bossLootReward.Granted)
            {
                Debug.LogWarning(
                    $"Region '{reward.CompletedRegion}' autosave was skipped because its BossReward transaction did not complete.",
                    reward);
                return;
            }

            RequestAutosave($"Region '{reward.CompletedRegion}' progression completion");
        }

        private void RequestAutosave(string boundary)
        {
            if (isSaving || isRestoring) return;

            try
            {
                SaveResult result = Save();
                if (!result.Succeeded)
                {
                    Debug.LogWarning($"Autosave after {boundary} did not complete: {result.Message}", this);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Autosave after {boundary} failed unexpectedly: {exception.Message}", this);
            }
        }

#if UNITY_EDITOR
        public void ConfigureForTests(
            SaveFileService testFileService,
            PlayerSaveAdapter testPlayerAdapter,
            WorldSaveAdapter testWorldAdapter)
        {
            if (!ReferenceEquals(instance, null) && instance != this) instance.UnsubscribeAutosaveEvents();
            instance = this;
            fileService = testFileService ?? throw new ArgumentNullException(nameof(testFileService));
            stateAdapter = testPlayerAdapter ?? throw new ArgumentNullException(nameof(testPlayerAdapter));
            worldAdapter = testWorldAdapter ?? throw new ArgumentNullException(nameof(testWorldAdapter));
            currentData = new GameSaveData();
            SubscribeAutosaveEvents();
        }

        public void FlushPendingRegionAutosaveForTests()
        {
            FlushPendingRegionAutosave();
        }
#endif

        private static bool TryFindPlayer(out GameObject playerRoot, out string errorMessage)
        {
            PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats == null)
            {
                playerRoot = null;
                errorMessage = "No active PlayerStats component was found; save/load requires an active Player.";
                return false;
            }

            playerRoot = playerStats.gameObject;
            errorMessage = string.Empty;
            return true;
        }

        private SaveResult LogSaveFailure(SaveError error, string path, string message)
        {
            Debug.LogError($"Save failed at '{path}': {message}", this);
            return SaveResult.Failure(error, path, message);
        }

        private LoadResult LogLoadFailure(SaveError error, string path, string message)
        {
            Debug.LogWarning($"Load failed at '{path}': {message}", this);
            return LoadResult.Failure(error, path, message);
        }
    }
}
