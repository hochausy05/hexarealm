# PROJECT_CHANGELOG — Lịch sử thay đổi HexaRealm

> **Mục đích:** Tóm tắt các thay đổi quan trọng của project sau từng task/prompt để người dùng, AI và Codex có thể nhanh chóng hiểu project hiện đang có gì mà không cần quét lại toàn bộ repository.
>
> **Không dùng file này thay cho Git history.** Đây là bản tóm tắt cấp cao về tính năng, công nghệ và architecture.

---

# Cách sử dụng

Sau mỗi task có thay đổi đáng kể, AI/Codex phải thêm một entry mới ở đầu phần **Change History**.

Chỉ ghi:
- tính năng mới;
- hệ thống mới;
- package/công nghệ mới;
- architecture mới;
- Project Settings quan trọng;
- Scene/Prefab quan trọng;
- thay đổi gameplay đáng kể;
- bug fix đáng chú ý.

Không ghi:
- typo;
- formatting;
- comment;
- thay đổi rất nhỏ không ảnh hưởng project.

---

# Trạng thái hiện tại

## Project

```text
Name: HexaRealm
Engine: Unity 6
Template: Universal 2D
Genre: 2D Top-down Fantasy Action RPG
Art: Pixel Art
Current Region Focus: HumanRealm / Nhân Giới
```

## Source of truth

- `GAME_DESIGN_CORE.md`
- `SYSTEM_AI.md`
- `PROJECT_CHANGELOG.md`

## Gameplay nền tảng đã chốt

- Không có Character Level.
- Dùng Soul để nâng chỉ số tại Soul Pillar.
- 5 Player Stats:
  - Vitality / HP
  - Attack / ATK
  - Defense / DEF
  - Agility / AGI
  - Rage / RAGE
- Rage tăng Crit + Attack Speed.
- Upgrade Cap mở rộng sau Main Boss từng vùng.
- Người chơi được tự do farm.
- Enemy Rank: S → F.
- Rank quái dựa trên HP + ATK + DEF + Speed.
- Nhân Giới ban đầu chủ yếu dùng Rank F và E.
- Optional Boss không bắt buộc.
- Main Boss mở vùng tiếp theo và Upgrade Cap.
- Weapon scope hiện tại: Melee Slash only.
- Player visual:
  - Body
  - WeaponSprite
  - SlashVFX

---

# Công nghệ / kiến trúc hiện đang dùng

## Unity

- Unity 6
- Universal 2D / URP 2D
- Orthographic top-down direction
- Tilemap dự kiến cho world
- Rule Tile khi phù hợp
- Prefab
- ScriptableObject
- Component-based architecture
- Data-driven design

## Pixel Art Standard dự kiến

```text
Base Tile: 32x32 px
Pixels Per Unit: 32
Filter Mode: Point / No Filter
Compression: None
```

---

# Change History

<!--
MẪU ENTRY:

## YYYY-MM-DD — [Tên task ngắn]

### Added
- ...

### Changed
- ...

### Fixed
- ...

### Technology / Packages
- ...

### Main files
- `Assets/...`

### Notes
- ...

Chỉ giữ các heading thực sự có nội dung.
-->

## 2026-09-10 — TASK 14 Armor Equipment Foundation (awaiting Manual Play Mode verification)

### Added
- `ArmorData`, a single `EquippedArmor` slot, and `TrainingArmor` prototype (+3 Defense) with a point-filtered placeholder body sprite.
- EditMode coverage for armor aggregation, switching, default-body restoration, null-body fallback, and data immutability.

### Changed
- `PlayerEquipment` now rebuilds all Equipment Modifiers from the equipped weapon and armor together, preserving Base and Soul Upgrade layers.
- Armor swaps only the `Body` sprite; `WeaponSprite` and `SlashVFX` remain independent. Vitality continues through the existing PlayerHealth max-health sync without healing.

### Notes
- Prototype armor values and art are not final balance. Task remains in progress until Manual Play Mode verification.

## 2026-09-10 — TASK 13 Weapon Equipment Foundation (awaiting Manual Play Mode verification)

### Added
- Melee-only `WeaponData` and `PlayerEquipment`: idempotent equip, unequip, switch, and aggregate equipment-stat recalculation.
- Prototype `BasicSword` (+3 damage, 1.0 attack-speed multiplier, +0 crit, +0 range) with a 32 PPU point-filtered placeholder PNG.
- EditMode coverage for modifier isolation/idempotency, combat modifiers, data immutability, and prefab/data references.

### Changed
- Weapon damage contributes only to the Attack Equipment Modifier; `PlayerCombat` uses Final Attack, preventing double application.
- Combat now reads weapon attack-speed, crit, range, WeaponSprite, and optional SlashVFX override with no-weapon fallbacks.

### Notes
- Prototype values are not final balance. Task remains in progress until Manual Play Mode verification.

## 2026-09-10 — Soul Pillar and player upgrade progression prototype

### Added
- Thêm `PlayerUpgradeProgression`: năm upgrade count, global Upgrade Cap, aggregate modifier idempotent và transaction Soul atomic.
- Thêm formula Soul cost prototype tập trung (`1 + total upgrades`), cùng EditMode coverage cho cost, cap, transaction, event và re-sync.
- Thêm `PlayerInteractor`, `IInteractable`, `SoulPillar` placeholder và panel uGUI/TMP runtime tối thiểu để nâng năm stat tại pillar.
- Gắn progression/interactor vào Player và đặt SoulPillar trong `TechnicalTest`.

### Notes
- Cap 10, cost base/growth 1 và mỗi stat +1/point chỉ là prototype; final balancing chưa chốt.
- Manual Soul → Pillar → Upgrade đã được người dùng xác minh trong Play Mode; Task 12 được đánh dấu DONE.

## 2026-09-10 — Soul reward and player Soul wallet foundation

### Added
- Thêm `PlayerSoulWallet` runtime với API Add / CanAfford / TrySpend, event thay đổi Soul và bảo vệ overflow.
- Thêm `EnemySoulReward`, trao Soul trực tiếp khi `Health.Died` từ `EnemyData.SoulReward`, một lần cho mỗi enemy life.
- Gắn wallet vào `Player` và reward component vào `Slime_F`; thêm EditMode tests cho wallet, reward và prefab.

### Notes
- Chưa có Soul Pillar, Soul UI hoặc save/load.

## 2026-09-09 — Task 10 basic enemy AI and Slime Rank F prototype

### Added
- Thêm enemy runtime foundation: một `EnemyRuntime` sở hữu `EnemyData`; `EnemyHealth` tái sử dụng `Health` và áp Defense qua `DamageCalculator`.
- Thêm `BasicEnemyAI` với Idle, Chase, Attack, Return và Dead; có detection/attack hysteresis, home/leash và dừng hoàn toàn khi chết.
- Thêm `EnemyMeleeAttack` query một lần mỗi attack, cooldown và raw damage tới Player receiver.
- Thêm `Slime_F` Rank F placeholder prefab/data và một instance trong `TechnicalTest`, gồm Rigidbody2D, physical collider, EnemyHitbox và top-down sorting.
- Thêm EditMode tests cho enemy health initialization, defense, immutable data, death idempotency, attack data source và prefab foundation.

### Main files
- `Assets/_Game/Scripts/Enemy/EnemyRuntime.cs`
- `Assets/_Game/Scripts/Enemy/EnemyHealth.cs`
- `Assets/_Game/Scripts/Enemy/BasicEnemyAI.cs`
- `Assets/_Game/Scripts/Enemy/EnemyMeleeAttack.cs`
- `Assets/_Game/Data/Enemies/HumanRealm/Slime_F.asset`
- `Assets/_Game/Prefabs/Enemies/HumanRealm/Slime_F.prefab`

### Notes
- Slime values are prototype only; Task 10 remains IN PROGRESS until the Play Mode checklist is verified.

## 2026-09-09 — Task 09 enemy data and power-budget foundation

### Added
- Thêm `EnemyData` ScriptableObject với Rank F–S, HP, ATK, DEF, Speed và SoulReward là authoring data duy nhất cho base combat stats của enemy.
- Thêm power score weighted additive chỉ dùng HP/ATK/DEF/Speed, cùng `EnemyPowerBudgetProfile` có weight/range cấu hình và rank validation thuần, không tự mutate Rank.
- Thêm EditMode tests cho data validation, đóng góp từng combat stat, SoulReward/declared Rank không ảnh hưởng score, profile chưa cấu hình, invalid weights và rank budget validation idempotent.

### Notes
- SoulReward là dữ liệu riêng cho task sau; ngưỡng Rank F–S và weight cân bằng cuối vẫn chưa được chốt. Profile mặc định tắt validation.

### Main files
- `Assets/_Game/Scripts/Enemy/EnemyData.cs`
- `Assets/_Game/Scripts/Enemy/EnemyPowerBudget.cs`
- `Assets/_Game/Scripts/Enemy/EnemyPowerBudgetProfile.cs`
- `Assets/_Game/Tests/EditMode/EnemyDataTests.cs`

## 2026-09-09 — Task 08 mouse-aim correction

### Fixed
- Tách hướng melee attack khỏi movement: `Gameplay/Point` lấy vị trí pointer để slash liên tục, gồm cả hướng chéo.
- Directional `OverlapBox` và `SlashVFX` cùng follow hướng từ Player tới pointer; movement và dash giữ nguyên behavior cũ.

### Main files
- `Assets/_Game/Data/Input/HexaRealmInputActions.inputactions`
- `Assets/_Game/Scripts/Player/PlayerCombat.cs`

## 2026-09-09 — Melee slash combat foundation

### Added
- Thêm `PlayerCombat` dùng `Gameplay/Attack`, hướng di chuyển hiện tại/gần nhất, cooldown theo Rage, một crit roll mỗi swing và directional `OverlapBox`.
- Thêm raw damage receiver contract để combat gửi raw damage còn target tự áp Defense qua `DamageCalculator` và `Health`.
- Chống self-hit và deduplicate receiver để target nhiều collider chỉ nhận damage một lần mỗi swing.
- Hoàn thiện Player hierarchy với `WeaponSprite`, `SlashVFX` placeholder tự tắt và tạo `CombatDummy` hai collider trên layer `EnemyHitbox`.
- Thêm EditMode tests cho công thức combat, interval safety, raw damage contract, deduplication, Player prefab và TechnicalTest dummy.

### Main files
- `Assets/_Game/Scripts/Combat/IRawDamageReceiver.cs`
- `Assets/_Game/Scripts/Combat/CombatMath.cs`
- `Assets/_Game/Scripts/Combat/CombatDummyDamageReceiver.cs`
- `Assets/_Game/Scripts/Player/PlayerCombat.cs`
- `Assets/_Game/Prefabs/Player/Player.prefab`
- `Assets/_Game/Scenes/Test/TechnicalTest.unity`
- `Assets/_Game/Tests/EditMode/CombatMathTests.cs`

### Notes
- Các coefficient Attack/Rage/Crit là prototype chỉnh được, chưa phải balancing cuối; Task 08 giữ IN PROGRESS đến khi manual Play Mode checklist được xác minh.

## 2026-09-09 — Health và damage foundation

### Added
- Thêm `Health` tái sử dụng với initialization idempotent, damage/heal clamp, death một lần và quy tắc đổi Max Health không tự hồi máu.
- Thêm `PlayerHealth` đồng bộ Final Vitality 1:1 sang Max Health và đọc Final Defense hiện tại khi nhận raw damage.
- Thêm `DamageCalculator` chứa riêng công thức prototype `max(1, raw damage - defense)` với Defense không âm.
- Thêm EditMode test assembly và 20 test cases cho Health, damage calculation, Vitality integration, anti-exploit và Player prefab.

### Main files
- `Assets/_Game/Scripts/Combat/Health.cs`
- `Assets/_Game/Scripts/Combat/DamageCalculator.cs`
- `Assets/_Game/Scripts/Player/PlayerHealth.cs`
- `Assets/_Game/Scripts/Player/PlayerStats.cs`
- `Assets/_Game/Prefabs/Player/Player.prefab`
- `Assets/_Game/Tests/EditMode/HealthDamageTests.cs`

### Notes
- Player prefab giữ nguyên movement, dash, physics, sorting và visual hierarchy; chưa thêm UI, respawn hay combat của Task 08.

## 2026-09-09 — Player stats foundation

### Added
- Thêm `PlayerStats` với 5 core stats: Vitality, Attack, Defense, Agility và Rage.
- Tách riêng Base, Upgrade và Equipment modifier; Final Stat luôn là giá trị derived có minimum validation.
- Gắn `PlayerStats` vào Player prefab với các base value prototype chưa phải balancing cuối.

### Main files
- `Assets/_Game/Scripts/Player/PlayerStats.cs`
- `Assets/_Game/Prefabs/Player/Player.prefab`

### Notes
- API modifier dùng set aggregate value để tránh double-apply; các case calculation, repeated set, reset và minimum clamp đã pass.
- Chưa tích hợp stat vào Health, combat, movement hoặc dash.

## 2026-09-09 — Player dash / dodge prototype

### Added
- Thêm `PlayerDash`, đọc `Gameplay/Dash` và dash theo hướng input hiện tại hoặc hướng di chuyển hợp lệ gần nhất.
- Thêm dash duration và cooldown chống spam với các thông số prototype chỉnh được trong Inspector.

### Changed
- `PlayerController` expose hướng di chuyển dạng read-only và tạm ngừng ghi velocity trong lúc `PlayerDash` giữ quyền điều khiển Rigidbody2D.
- Player Rigidbody2D dùng Continuous Collision Detection để giảm nguy cơ xuyên collider khi dash.

### Main files
- `Assets/_Game/Scripts/Player/PlayerDash.cs`
- `Assets/_Game/Scripts/Player/PlayerController.cs`
- `Assets/_Game/Prefabs/Player/Player.prefab`

### Notes
- Task 04 đã được người dùng xác minh trong Play Mode. Task 05 chờ manual playtest trước khi đánh dấu DONE.

## 2026-09-09 — Camera follow và top-down Y-sorting foundation

### Added
- Thêm `CameraFollow2D` theo target trong `LateUpdate`, hỗ trợ offset và smoothing nhẹ trong khi giữ nguyên camera Z.
- Thêm `TopDownSorting` tái sử dụng, tính `SortingGroup.sortingOrder` từ world Y của root hoặc `SortPoint` riêng.
- Cấu hình Player dùng `SortingGroup` trên layer `Characters` và thêm các sorting probe placeholder trong `TechnicalTest`, gồm phần top trên layer `AboveCharacters`.

### Main files
- `Assets/_Game/Scripts/Core/CameraFollow2D.cs`
- `Assets/_Game/Scripts/Core/TopDownSorting.cs`
- `Assets/_Game/Prefabs/Player/Player.prefab`
- `Assets/_Game/Scenes/Test/TechnicalTest.unity`

### Notes
- Task 04 giữ trạng thái IN PROGRESS cho đến khi camera và thứ tự render được người dùng xác minh trong Play Mode.

## 2026-09-09 — Player foundation và movement

### Added
- Thêm `PlayerController` đọc `Gameplay/Move` bằng Input System mới và di chuyển `Rigidbody2D` theo 8 hướng với tốc độ chéo được chuẩn hóa.
- Tạo prefab `Player` gồm root Player, child `Body`, placeholder sprite, `Rigidbody2D`, `CapsuleCollider2D` và movement speed chỉnh được trong Inspector.
- Đặt Player prefab gần origin trong scene `TechnicalTest` để kiểm thử kỹ thuật.

### Main files
- `Assets/_Game/Scripts/Player/PlayerController.cs`
- `Assets/_Game/Prefabs/Player/Player.prefab`
- `Assets/_Game/Scenes/Test/TechnicalTest.unity`

### Notes
- Rigidbody2D dùng Dynamic body, Gravity Scale 0, Interpolate và Freeze Rotation Z; chưa thêm camera follow, animation, dash, combat hay các gameplay system ngoài phạm vi.

## 2026-09-09 — Thiết lập nền kỹ thuật Unity 2D

### Added
- Tạo `HexaRealmInputActions` với action map `Gameplay` cho Move, Attack, Dash và Interact bằng keyboard, mouse và gamepad.
- Thêm Sorting Layers: Ground, GroundDetails, Environment, Characters, AboveCharacters, VFX và UI.
- Thêm Unity Layers: Player, Enemy, NPC, World, Interactable, PlayerHitbox và EnemyHitbox.
- Tạo scene `TechnicalTest` với Main Camera Orthographic, World, Grid và các Tilemap Ground, GroundDetails, Collision.

### Technology / Packages
- Sử dụng Unity Input System `1.20.0` đã có sẵn; không cài package mới.

### Main files
- `Assets/_Game/Data/Input/HexaRealmInputActions.inputactions`
- `Assets/_Game/Scenes/Test/TechnicalTest.unity`
- `ProjectSettings/TagManager.asset`

### Notes
- Active Input Handling đã dùng Input System mới; không thay Render Pipeline.

---

## 2026-09-09 — Thiết lập cấu trúc thư mục ban đầu

### Added
- Tạo cây thư mục `Assets/_Game` cho nội dung do HexaRealm tự phát triển.
- Phân tách các khu vực cho Art, Animations, Audio, Data, Materials, Prefabs, Scenes, Scripts, Tilemaps và Tests.
- Chuẩn bị các thư mục riêng cho vùng `HumanRealm` và các nhóm asset/gameplay mở rộng sau này.

### Main files
- `Assets/_Game/`

### Notes
- Chỉ thay đổi cấu trúc folder; chưa tạo gameplay, script, prefab, scene, ScriptableObject hay package mới.

---

## 2026-09-09 — Khởi tạo project

### Added
- Tạo Unity project `HexaRealm`.
- Sử dụng template Universal 2D.
- Thêm `GAME_DESIGN_CORE.md` làm tài liệu thiết kế nguồn.
- Thêm `SYSTEM_AI.md` để quy định cách AI/Codex làm việc.
- Thêm `PROJECT_CHANGELOG.md` để theo dõi thay đổi cấp cao của project.

### Technology / Packages
- Unity 6.
- Universal 2D / URP 2D mặc định từ template.

### Notes
- Project đang ở giai đoạn thiết lập ban đầu.
- Chưa triển khai gameplay.
- Ưu tiên hiện tại là hoàn thiện Nhân Giới trước khi mở rộng sang các vùng khác.

---

# Mẫu cho task tiếp theo

Khi hoàn thành một task, thêm entry mới **phía trên entry cũ**:

```text
## YYYY-MM-DD — Tên task

### Added
- Hệ thống/tính năng mới.

### Changed
- Hành vi hoặc architecture đã thay đổi.

### Fixed
- Bug quan trọng đã sửa.

### Technology / Packages
- Package/công nghệ mới nếu có.

### Main files
- Các file chính liên quan.

### Notes
- Lưu ý ngắn nếu cần.
```

---

# Quy tắc dành cho AI/Codex

1. Đọc file này để biết project hiện có gì trước khi tạo một hệ thống đã tồn tại.
2. Không dùng changelog làm source of truth cho gameplay; dùng `GAME_DESIGN_CORE.md`.
3. Không xóa lịch sử cũ.
4. Không rewrite toàn bộ file sau mỗi task.
5. Chỉ thêm entry mới.
6. Entry phải ngắn và mang tính tổng quan.
7. Không paste diff code vào đây.
8. Không paste nguyên prompt vào đây.
9. Nếu một task không có thay đổi đáng kể, không cần tạo entry.
10. Khi báo cáo cuối task, nói rõ có cập nhật changelog hay không.
