# PROJECT_CHANGELOG — HexaRealm

## 2026-09-14 — Task 23: Hoàn tất Art Bible
- Hoàn thiện Art Bible cho vertical slice HumanRealm: khóa chuẩn visual/pixel/perspective/scale/palette cùng contract cho Player, enemy/boss và environment.
- Định nghĩa contract tạo asset bằng AI và pipeline review/phê duyệt; Task 23 hoàn tất, Task 24 có thể bắt đầu.

## 2026-09-14 — Task 22.3: Lưu trạng thái thế giới
- Lưu trạng thái đã mở của ba LootChest HumanRealm bằng ID ổn định, kiểm tra ID rỗng/trùng và phục hồi visual/interact mà không phát lại loot.
- Thêm autosave bảo thủ sau chest thành công và sau giao dịch Main Boss/Region hoàn tất, có chặn reentrancy; trạng thái Main Boss sau load được dẫn xuất từ Region completion thay vì lưu cờ trùng.
- Giữ schema phiên bản 1 tương thích save T22.2, bổ sung test world/end-to-end và hoàn tất Task 22; CurrentHealth, vị trí, Boss HP và trạng thái combat vẫn không được lưu.

## 2026-09-14 — Task 22.2: Lưu tiến trình Player
- Tích hợp capture/restore trực tiếp cho Soul, năm chỉ số nâng cấp, Upgrade Cap, Region/Teleport Stone và equipment sở hữu/đang trang bị; load lặp không cộng dồn hoặc phát lại reward.
- Thêm ID ổn định cho bốn equipment hiện có và `EquipmentCatalog` runtime trong Resources, có kiểm tra ID rỗng/trùng và cảnh báo ID save không còn tồn tại.
- Tách `HexaRealm.SaveIntegration` khỏi kernel, phục hồi theo thứ tự xác định rồi đưa Player về đầy HP theo MaxHealth mới; thêm phím debug F5/F9 chỉ cho Editor/Development Build và test tích hợp tập trung.

## 2026-09-14 — Task 22.1: Nền tảng Save/Load
- Thêm schema JSON phiên bản 1, dịch vụ file một slot tại `persistentDataPath` và runtime bootstrap duy nhất không phụ thuộc scene/prefab.
- Ghi qua file tạm cùng thư mục, thay thế an toàn kèm backup; load có thể phục hồi backup khi file chính hỏng và từ chối schema mới hơn.
- Thêm EditMode tests cho serialize/version, ghi lần đầu/lần hai, corruption recovery, delete đúng phạm vi và đường dẫn test tách khỏi `Assets`; tích hợp gameplay dành cho T22.2.

## 2026-09-13 — Task 20: Hoàn thiện HP UI và reward Boss
- Tách chẩn đoán reference thiếu/đã bị hủy cho `BossHealthBarUI`, đồng bộ ngay trạng thái hiển thị và bảo vệ host UI khỏi cấu hình `BarRoot` không an toàn.
- Bảo vệ lifecycle/subscription và giao reward khỏi reference Unity đã bị hủy hoặc gọi lặp; chỉ đặt `Granted` sau khi `PlayerLootReceiver` nhận thành công, kèm test EditMode tập trung.

## 2026-09-13 — Task 23: Art Bible
- Thêm `docs/ART_BIBLE.md` định nghĩa hệ visual 2D top-down fantasy pixel art cho HumanRealm và các chuẩn palette, scale, outline, ánh sáng, shading, terrain, nhân vật, item, UI, animation, sprite-sheet, naming, folder và QA.
- Giữ nguyên chuẩn kỹ thuật đã chốt: tile 32×32, PPU 32, Point/No Filter, Compression None và pivot Bottom Center ưu tiên.
- Ghi rõ palette sản xuất, style target, camera framing, roster và pipeline generation vẫn cần phê duyệt; không tạo asset thật hoặc thay đổi gameplay/architecture.

## 2026-09-12 — Dọn công cụ Editor authoring cũ
- Chuyển các builder Task 17/19/20/21 và utility tạo Boss vào `Editor/Legacy`, gỡ toàn bộ menu thực thi để tránh ghi đè scene, prefab hoặc dữ liệu đã hoàn thiện.
- Xóa hai thư mục script trùng tên số nhiều `Bosses`/`Enemies` vì rỗng; giữ nguyên các thư mục runtime `Boss`/`Enemy`.

## 2026-09-12 — Sửa reward, tiến trình và HP UI của Boss
- Chỉ đánh dấu Boss loot đã trao sau khi `PlayerLootReceiver` nhận thành công, tránh mất hoặc nhân đôi reward khi cấu hình lỗi.
- Main Boss tự tìm tiến trình trên Player đang hoạt động, lấy Upgrade Cap từ cùng Player và vẫn giữ các reference đã gán hợp lệ.
- Ngăn HP UI tự tắt GameObject chứa component; đồng thời từ chối mọi giá trị vô hạn/NaN trong `BossData`.

## 2026-09-11 — Sửa nền tảng runtime Boss
- Loại bỏ vòng lặp `RequireComponent`, ngăn component Boss gốc bị trùng và tăng kiểm tra cấu hình/BossData trước khi chạy.
- Hỗ trợ danh sách từ một đòn đánh trở lên theo thứ tự xác định, hủy đòn đúng vòng đời và dừng charge an toàn.
- Theo dõi từng collider của một Player trong BossArena, chỉ disengage/reset khi Player đã rời hoàn toàn.

## 2026-09-11 — Task 21: Main Boss và tiến trình HumanRealm
- Thêm trạng thái tiến trình vùng theo session, Teleport Stone HumanRealm và mở khóa Region2 theo trạng thái dẫn xuất.
- Thêm reward adapter cho boss để mở Upgrade Cap tuyệt đối, không đổi Soul hay các điểm nâng cấp hiện có.
- Thêm tool authoring hẹp để tạo MainBoss_Prototype, dữ liệu reward riêng và đặt encounter tại MainBossArea phía bắc.

## 2026-09-11 — Task 20: Optional Boss Framework
- Thêm nền tảng boss độc lập gồm BossData/Runtime/Health, arena, điều khiển combat, melee sweep và charge có telegraph.
- Thêm prefab OptionalBoss_Prototype, reward Soul cố định, boss HP bar và authoring tool hẹp cho HumanRealm side area.
- Boss reset về home khi rời arena, hồi HP nếu còn sống; boss chết chỉ thưởng một lần trong session.

## 2026-09-10 — Task 19: Village NPC Movement
- Thêm `NPCPatrolPath` và `VillageNPCMovement` cho vòng Idle → Walk → Idle bằng Rigidbody2D, không có pathfinding hay tương tác.
- Thêm prefab `Villager_Prototype` và tool authoring hẹp để đặt 3 dân làng cùng 3 patrol path trong HumanRealm.

## 2026-09-10 — Task 18: Cave / Area Transition
- Thêm `AreaTransitionPortal` dùng lại `IInteractable` và `PlayerAreaTransition` để dịch chuyển cùng Player trong một scene, xóa velocity Rigidbody2D và reset dash tối thiểu.
- Mở rộng builder để tạo Cave01 graybox 28x20 tile tại offset X=160, có collision biên, CaveEntrance/CaveExit và các điểm `CaveEntryPoint`/`CaveReturnPoint`.
- Giữ T18 ở trạng thái chờ kiểm tra Play Mode; không tải scene và không thêm nội dung Cave sản xuất.

## 2026-09-10 — Task 17.2: Chẩn đoán collision runtime HumanRealm
- Xác định `CompositeCollider2D` không tạo path khi scene vừa vào Play Mode (`pathCount=0`) dù tile và component hợp lệ; toggle collider mới ép tạo geometry.
- Loại bỏ Composite khỏi Collision Tilemap, giữ `TilemapCollider2D` + `Rigidbody2D` tĩnh; runtime tạo 1888 shape ngay khi load và builder đã đồng bộ.

## 2026-09-10 — Task 17.1: Sửa collision biên HumanRealm
- Bổ sung vòng ô collision vật lý một ô bên ngoài chu vi 112x112, giữ nguyên vùng chơi và toàn bộ tuyến đường.
- Đồng bộ `Task17HumanRealmBuilder` để tái tạo đúng perimeter với TilemapCollider2D + CompositeCollider2D + Rigidbody2D tĩnh.

## 2026-09-10 — Task 17: Graybox HumanRealm
- Thêm scene `HumanRealm` 112x112 bằng Tilemap mô-đun, gồm làng khởi đầu, ngã tư trung tâm, rừng phía tây, đồng ruộng phía đông, lối hang và khu boss placeholder.
- Tái sử dụng Player, CameraFollow2D, 3 Soul Pillar, 3 Loot Chest và 3 EnemySpawnZone Slime_F với tối đa 9 Slime hoạt động.
- Thêm bộ tile PNG placeholder 32x32 cùng collision biên/vật cản; giữ Task 17 ở trạng thái chờ kiểm tra Play Mode và quy mô bản đồ.

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
