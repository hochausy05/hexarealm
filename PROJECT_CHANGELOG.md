# PROJECT_CHANGELOG — HexaRealm

## 2026-09-10 — Task 16.5: Dọn TechnicalTest và đồng bộ visual runtime
- Đồng bộ preview Player với TrainingArmor + BasicSword nhưng vẫn giữ body mặc định để unequip an toàn.
- Xóa direct `Slime_F` dư khỏi `TechnicalTest`; `SlimeSpawnZone` tiếp tục là nguồn spawn runtime.
- Kiểm tra placeholder/material liên quan và không phát hiện lỗi shader/material bị hỏng.

## 2026-09-10 — Task 16: Enemy Spawn Zone + Respawn
- Thêm `EnemySpawnZone` với spawn points, initial/max alive, respawn delay/retry, khoảng cách an toàn với Player, clearance check và corpse cleanup.
- Respawn tạo enemy instance mới để có Health/AI/SoulReward state sạch; `BasicEnemyAI` nhận spawn point làm home position.
- `TechnicalTest` chuyển sang dùng `SlimeSpawnZone`.

## 2026-09-10 — Task 15: Chest + Loot
- Thêm `LootBundleData`, `LootChest`, `PlayerEquipmentInventory` và `PlayerLootReceiver`.
- Chest hỗ trợ reward cố định gồm Soul, Weapon, Armor; chống claim lặp và hỗ trợ auto-equip prototype qua `PlayerEquipment`.
- Thêm ExplorerSword, ExplorerArmor và LootTestChest trong `TechnicalTest`.

## 2026-09-10 — Task 14: Armor Equipment
- Thêm `ArmorData`, một armor slot và TrainingArmor prototype (+3 Defense).
- `PlayerEquipment` aggregate modifier từ Weapon + Armor trong một đường recalculation duy nhất.
- Armor đổi `Body` visual nhưng không ảnh hưởng `WeaponSprite`/`SlashVFX`; giữ logic Vitality không tự heal.

## 2026-09-10 — Task 13: Weapon Equipment
- Thêm melee-only `WeaponData`, `PlayerEquipment` và BasicSword prototype (+3 Attack).
- Weapon Damage đi vào Attack Equipment Modifier; `PlayerCombat` tiếp tục dùng Final Attack để tránh double-apply.
- Combat đọc attack-speed/crit/range và weapon visual/VFX override từ weapon hiện equip.

## 2026-09-10 — Task 12: Soul Pillar + Upgrade Progression
- Thêm `PlayerUpgradeProgression`, 5 upgrade count, global Upgrade Cap và transaction Soul atomic.
- Thêm cost prototype `1 + total upgrades`, `PlayerInteractor`, `IInteractable`, `SoulPillar` và panel nâng stat tối thiểu.
- Manual flow Soul -> Pillar -> Upgrade đã được xác minh.

## 2026-09-10 — Task 11: Soul Reward + Soul Wallet
- Thêm `PlayerSoulWallet` với Add/CanAfford/TrySpend và bảo vệ overflow.
- Thêm `EnemySoulReward`, trao đúng `EnemyData.SoulReward` một lần cho mỗi enemy life.
- Gắn wallet vào Player và reward component vào `Slime_F`.

## 2026-09-09 — Task 10: Basic Enemy AI + Slime Rank F
- Thêm `EnemyRuntime`, `EnemyHealth`, `BasicEnemyAI`, `EnemyMeleeAttack` và Slime_F prototype.
- AI hỗ trợ Idle/Chase/Attack/Return/Dead, home/leash và raw damage tới Player.
- Slime dùng EnemyData làm base-stat source of truth và Health/DamageCalculator hiện có.

## 2026-09-09 — Task 09: EnemyData + Power Budget
- Thêm `EnemyData` với Rank F-S, HP/ATK/DEF/Speed và SoulReward.
- Thêm weighted power-budget foundation/profile; SoulReward và declared Rank không làm thay đổi power score.
- Final rank thresholds/weights vẫn chưa chốt.

## 2026-09-09 — Task 08: Melee Combat + Mouse Aim
- Thêm `PlayerCombat`, raw damage receiver contract, directional melee query, crit/cooldown và deduplicate multi-collider hit.
- Tách melee aim khỏi movement: pointer quyết định attack direction, gồm cả hướng chéo.
- Hoàn thiện Player hierarchy với `WeaponSprite` + `SlashVFX` và CombatDummy test target.

## 2026-09-09 — Task 07: Health + Damage
- Thêm reusable `Health`, `PlayerHealth` và `DamageCalculator` prototype.
- Vitality đồng bộ Max Health mà không tự heal khi Max Health tăng; death/heal/damage được clamp an toàn.
- Thêm EditMode coverage cho health/damage và anti-exploit.

## 2026-09-09 — Task 06: Player Stats
- Thêm 5 core stats: Vitality, Attack, Defense, Agility, Rage.
- Tách Base / Upgrade / Equipment modifiers; Final Stat luôn được derive và không double-apply.

## 2026-09-09 — Task 05: Dash / Dodge
- Thêm `PlayerDash` theo Input System mới, có direction fallback, duration và cooldown.
- PlayerController nhường Rigidbody2D cho dash trong thời gian dash; Rigidbody2D dùng Continuous Collision Detection.

## 2026-09-09 — Task 04: Camera + Top-down Y-Sorting
- Thêm `CameraFollow2D`, `TopDownSorting` và `SortingGroup` cho Player.
- Thêm sorting probes trong `TechnicalTest` để kiểm tra Characters/AboveCharacters.

## 2026-09-09 — Task 03: Player Movement
- Thêm Player prefab và `PlayerController` dùng Rigidbody2D + Input System cho movement 8 hướng.
- Player có Rigidbody2D Dynamic, Gravity Scale 0, Freeze Rotation Z và placeholder Body.

## 2026-09-09 — Task 02: Unity 2D Technical Foundation
- Thêm `HexaRealmInputActions`, Gameplay actions, Sorting Layers và Unity Layers cần thiết.
- Tạo `TechnicalTest` với Orthographic camera, Grid và Tilemaps cơ bản.

## 2026-09-09 — Task 01: Project Folder Structure
- Tạo cấu trúc `Assets/_Game` cho Art, Animations, Audio, Data, Materials, Prefabs, Scenes, Scripts, Tilemaps và Tests.

## 2026-09-09 — Khởi tạo project
- Tạo Unity project HexaRealm bằng Universal 2D / URP 2D.
- Khởi tạo bộ tài liệu thiết kế và quy tắc làm việc cho AI/Codex.
