using System;
using System.Collections.Generic;

namespace HexaRealm.Save
{
    /// <summary>Versioned persistent-data root. Gameplay fields are added by focused integration tasks.</summary>
    [Serializable]
    public sealed class GameSaveData
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public SaveMetadataData metadata = new SaveMetadataData();
        public PlayerSaveData player = new PlayerSaveData();
        public ProgressionSaveData progression = new ProgressionSaveData();
        public EquipmentSaveData equipment = new EquipmentSaveData();
        public WorldSaveData world = new WorldSaveData();

        public void EnsureSections()
        {
            if (metadata == null) metadata = new SaveMetadataData();
            if (player == null) player = new PlayerSaveData();
            if (progression == null) progression = new ProgressionSaveData();
            if (equipment == null) equipment = new EquipmentSaveData();
            if (world == null) world = new WorldSaveData();
            if (progression.completedRegions == null) progression.completedRegions = new List<int>();
            if (progression.teleportStoneRegions == null) progression.teleportStoneRegions = new List<int>();
            if (equipment.ownedWeaponIds == null) equipment.ownedWeaponIds = new List<string>();
            if (equipment.ownedArmorIds == null) equipment.ownedArmorIds = new List<string>();
            if (equipment.equippedWeaponId == null) equipment.equippedWeaponId = string.Empty;
            if (equipment.equippedArmorId == null) equipment.equippedArmorId = string.Empty;
            if (world.openedChestIds == null) world.openedChestIds = new List<string>();
        }
    }

    [Serializable]
    public sealed class SaveMetadataData
    {
        public string savedAtUtc = string.Empty;
        public bool hasPlayerProgressionEquipmentState;
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public int souls;
    }

    [Serializable]
    public sealed class ProgressionSaveData
    {
        public int vitalityUpgradeCount;
        public int attackUpgradeCount;
        public int defenseUpgradeCount;
        public int agilityUpgradeCount;
        public int rageUpgradeCount;
        public int upgradeCap = 10;
        public List<int> completedRegions = new List<int>();
        public List<int> teleportStoneRegions = new List<int>();
    }

    [Serializable]
    public sealed class EquipmentSaveData
    {
        public List<string> ownedWeaponIds = new List<string>();
        public List<string> ownedArmorIds = new List<string>();
        public string equippedWeaponId = string.Empty;
        public string equippedArmorId = string.Empty;
    }

    [Serializable]
    public sealed class WorldSaveData
    {
        public List<string> openedChestIds = new List<string>();
    }
}
