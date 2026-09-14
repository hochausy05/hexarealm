# Save System

Task 22 is complete across three focused stages:

- T22.1: versioned JSON and safe one-slot file kernel — complete.
- T22.2: Player, progression, and equipment integration — complete.
- T22.3: chest/world persistence and end-to-end hardening — complete.

## Public runtime flow

`GameSaveService.Instance` is bootstrapped automatically and exposes `Save()`, `Load()`, `HasSave()`, and `DeleteSave()`. No scene object or prefab is required.

F5 saves and F9 loads in the Unity Editor and Development Builds only. These controls are excluded from normal release builds.

## Persistent state

- exact Soul balance;
- Vitality, Attack, Defense, Agility, and Rage upgrade counts;
- absolute Upgrade Cap;
- owned Weapon and Armor IDs;
- equipped Weapon and Armor IDs;
- completed Regions;
- acquired Teleport Stone Regions;
- opened LootChest IDs.

The save intentionally does not contain current health, death state, exact Player position, Boss HP/combat state, Optional Boss defeat, UI state, arena engagement, Rigidbody velocity, attacks, cooldowns, VFX, or temporary enemy/spawn state. Player placement continues to use the normal scene entry/spawn flow.

## File format and recovery

The current schema remains version 1. T22.3 adds an optional `world.openedChestIds` list, so valid T22.2 files remain compatible and default all current chests to unopened. No migration or schema bump is required.

The one-slot files are `save_slot_0.json` and `save_slot_0.backup.json` under `Application.persistentDataPath`; a same-directory temporary file is flushed before replacement. A malformed/unreadable primary falls back to a valid backup, while a future schema is rejected rather than overwritten.

## Equipment IDs and runtime resolution

`WeaponData` and `ArmorData` contain an author-authored `persistentId`. IDs are independent of display names, asset ordering, hierarchy, and runtime instance IDs. The Resources-backed `EquipmentCatalog` resolves those IDs in player builds without `AssetDatabase`.

Empty or duplicate catalog IDs make capture/restore fail before gameplay state changes. Unknown IDs found in an otherwise valid save are logged and skipped; an unresolved equipped item leaves that slot empty rather than selecting a fallback.

## World and chest IDs

Each persisted `LootChest` has an author-authored serialized `persistentId`. The ID is runtime-readable and independent of hierarchy, name, position, and Unity instance ID. Save and Load perform one-time scene collection, reject empty or duplicate current IDs, and store only IDs whose authoritative state is opened.

Unknown removed chest IDs are warned and skipped. Chests added after a save remain unopened. `RestoreOpenedState` changes state and visuals without delivering loot or raising gameplay/autosave events. HumanRealm completion also derives Main Boss unavailability on load from `PlayerRegionProgression`; no separate boss-dead field is saved.

## Restore transaction

Before mutation, the integration validates the save payload, required Player components, equipment catalog, all saved equipment IDs, and Region values. It then applies replacement state in this order:

1. deserialize and validate schema, Player dependencies, equipment IDs, Region values, and all current/saved chest IDs;
2. five upgrade counts and absolute Upgrade Cap;
3. owned equipment;
4. equipped slots and recalculated equipment modifiers;
5. exact Soul balance;
6. completed Regions and Teleport Stones;
7. health reset to the final restored MaxHealth;
8. chest opened/closed replacement state;
9. derived completed-region Main Boss suppression.

Restore APIs do not spend Soul, replay loot or boss rewards, raise upgrade-purchase events, or raise region-progression reward events. Collections use set-like replacement, and stat modifiers are rebuilt from authoritative inputs, making repeated loads idempotent.

## Autosave boundaries

Explicit `Save()` remains authoritative. A chest autosaves once only after reward delivery succeeds and opened state is committed. Region/Main Boss progression queues one save for the next frame, after all death listeners finish; if that boss has a `BossReward`, autosave proceeds only when its loot transaction reports success. Load/restore events never autosave, and save/load reentrancy guards prevent recursive operations. Damage, movement, attacks, ordinary Soul changes, and per-frame state do not autosave.

## Health policy

Task 22 does not save combat health. After persistent stats and equipment are restored, the Player is revived and set to full health at the resulting MaxHealth so loading cannot resume in an unusable dead state.
