# PLAN — HexaRealm Development Roadmap

> **Project:** HexaRealm  
> **Mục đích:** Roadmap phát triển tổng thể cho người phát triển và AI/Codex.  
> **Vai trò:** Giải thích thứ tự triển khai, dependency, checkpoint và phạm vi từng giai đoạn.  
> **Không thay thế:** `SYSTEM_AI.md`, `GAME_DESIGN_CORE.md`, `TASKS.md`, `PROJECT_CHANGELOG.md`.

---

# 1. Thứ tự tài liệu AI/Codex nên đọc

1. `SYSTEM_AI.md`
2. `GAME_DESIGN_CORE.md`
3. `PLAN.md`
4. `TASKS.md`
5. `PROJECT_CHANGELOG.md`
6. Chỉ các file trực tiếp liên quan đến task hiện tại

Vai trò:
- `SYSTEM_AI.md`: luật và giới hạn làm việc.
- `GAME_DESIGN_CORE.md`: source of truth về gameplay/game design.
- `PLAN.md`: roadmap và dependency tổng thể.
- `TASKS.md`: task cụ thể và trạng thái.
- `PROJECT_CHANGELOG.md`: lịch sử thay đổi cấp cao của project.

---

# 2. Logic phát triển tổng thể

Core loop:

```text
Di chuyển
→ Khám phá
→ Chiến đấu
→ Giết quái
→ Nhận Soul
→ Nâng chỉ số
→ Nhặt / trang bị đồ
→ Mạnh lên
→ Boss
→ Mở progression
```

Thứ tự kỹ thuật ưu tiên:

```text
Foundation
↓
Player
↓
Combat
↓
Enemy
↓
Soul / Upgrade
↓
Equipment / Loot
↓
Respawn / Farming
↓
World
↓
Boss
↓
Save
↓
Asset Pipeline
↓
HumanRealm Content
↓
Polish / Balance
```

Không triển khai hệ thống phụ thuộc trước khi dependency chính đã đủ ổn định.

---

# 3. Nguyên tắc triển khai

## 3.1. Một prompt = một thay đổi có thể kiểm chứng

Không gom quá nhiều hệ thống lớn vào một task.

Ví dụ:

```text
Task 03: Player Movement
Task 04: Camera + Y-Sorting
Task 05: Dash
```

Mục tiêu:
- dễ debug;
- giảm Codex tự mở rộng scope;
- giảm token sửa lại;
- dễ rollback;
- changelog rõ ràng.

## 3.2. Placeholder trước, asset đẹp sau

```text
Gameplay hoạt động
→ Architecture ổn
→ Playtest
→ Asset chuẩn
→ Polish
```

Không đầu tư mạnh vào map/art trước khi movement, combat, enemy loop và Soul progression hoạt động.

## 3.3. HumanRealm là vertical slice

Không phát triển 6 vùng song song.

HumanRealm phải chứng minh toàn bộ game loop bằng architecture có thể tái sử dụng cho 5 vùng còn lại.

---

# 4. Trạng thái hiện tại

## Task 01 — Project Folder Structure — DONE

- Tạo `Assets/_Game`.
- Phân chia Art, Animations, Audio, Data, Materials, Prefabs, Scenes, Scripts, Tilemaps, Tests.

## Task 02 — Unity 2D Technical Foundation — DONE

- Unity Input System.
- `HexaRealmInputActions`.
- Sorting Layers.
- Unity Layers.
- `TechnicalTest.unity`.
- Main Camera Orthographic.
- Grid + Tilemaps: Ground, GroundDetails, Collision.

Project hiện đủ nền để bắt đầu Player.

---

# 5. PHASE A — Player Foundation

## Task 03 — Player Foundation + Movement

Làm:
- Player prefab.
- Rigidbody2D.
- Collider2D.
- đọc `Move`.
- di chuyển 8 hướng.
- speed cấu hình được.
- freeze rotation.
- test trong `TechnicalTest`.

Chưa làm:
- camera follow;
- animation;
- dash;
- combat;
- health;
- stats;
- equipment.

## Task 04 — Camera Follow + Top-down Sorting

Làm:
- camera follow;
- smoothing nhẹ nếu cần;
- Y-Sorting Player/Environment;
- test trước/sau object.

## Task 05 — Dash / Dodge

Làm:
- Dash input;
- dash direction;
- duration/distance;
- cooldown;
- chống spam.

### Checkpoint A

Sau Task 05 phải kiểm tra:
- movement mượt;
- camera không jitter;
- sorting đúng;
- dash dễ điều khiển;
- tốc độ phù hợp top-down.

---

# 6. PHASE B — Stats + Combat

## Task 06 — Player Stats Foundation

5 stat:
- Vitality / HP
- Attack / ATK
- Defense / DEF
- Agility / AGI
- Rage / RAGE

Architecture:

```text
Base Value
+
Upgrade Modifier
+
Equipment Modifier
=
Final Value
```

## Task 07 — Health + Damage Foundation

Làm:
- Max HP;
- Current HP;
- TakeDamage;
- DEF calculation ban đầu;
- Death;
- tái sử dụng hợp lý cho Player/Enemy.

## Task 08 — Melee Slash Combat

Làm:
- Attack input;
- WeaponPivot;
- WeaponSprite;
- melee hitbox;
- cooldown;
- damage;
- crit;
- Rage → Attack Speed;
- SlashVFX placeholder.

Scope weapon hiện tại: **Melee Slash only**.

### Checkpoint B

- đánh được target;
- hitbox ổn;
- crit đúng;
- attack speed đúng;
- không double-hit lỗi.

---

# 7. PHASE C — Enemy + Soul Progression

## Task 09 — Enemy Data + Rank Foundation

EnemyData:
- Name
- Rank
- HP
- ATK
- DEF
- Speed
- SoulReward

Rank:

```text
F E D C B A S
```

Rank dựa trên Power Budget của:

```text
HP + ATK + DEF + Speed
```

## Task 10 — Basic Enemy AI

Chỉ tạo một prototype, ưu tiên Slime Rank F.

Behavior:

```text
Idle
→ Detect
→ Chase
→ Attack
→ Lose target
→ Return / Idle
```

## Task 11 — Soul Drop + Soul Wallet

```text
Enemy dies
→ EnemyData.SoulReward
→ Player Soul
```

## Task 12 — Soul Pillar + Upgrade System

```text
Interact
→ chọn HP / ATK / DEF / AGI / RAGE
→ check cost
→ check Upgrade Cap
→ spend Soul
→ upgrade
```

Hỗ trợ:
- cost tăng dần;
- Upgrade Cap;
- build tự do;
- không Level.

### Checkpoint C

Core progression phải chứng minh được:

```text
Kill
→ Soul
→ Upgrade
→ Stronger
```

---

# 8. PHASE D — Equipment + Loot + Farming

## Task 13 — Weapon Equipment

WeaponData dự kiến:
- Name
- Damage
- AttackSpeed
- CritBonus
- Range
- WeaponSprite
- SlashVFX

Chỉ melee slash.

## Task 14 — Armor Equipment

Architecture:

```text
Body theo Armor
+
WeaponSprite theo Weapon
+
SlashVFX
```

## Task 15 — Chest + Loot

Reward MVP:
- Soul;
- Weapon;
- Armor.

## Task 16 — Enemy Spawn Zone + Respawn

Làm:
- spawn zone;
- max alive;
- spawn positions;
- respawn delay;
- không respawn trước mặt Player;
- reusable.

### Checkpoint D

```text
Explore
→ Kill
→ Leave area
→ Return
→ Enemy respawn
→ Farm Soul
```

---

# 9. PHASE E — HumanRealm World Foundation

## Task 17 — HumanRealm Graybox

Dùng placeholder, chưa dùng final art.

Test:
- kích thước map;
- travel time;
- camera;
- area spacing;
- enemy density;
- vị trí làng/hang/boss.

Prototype ban đầu:

```text
100–128 tiles mỗi chiều
Base Tile: 32x32
```

Không chốt kích thước cuối trước playtest.

## Task 18 — Cave / Area Transition

```text
HumanRealm
→ Cave
→ HumanRealm
```

## Task 19 — Village NPC Movement

```text
Idle
→ Waypoint
→ Walk
→ Idle
```

Chưa làm quest/dialogue tree phức tạp.

### Checkpoint E

Chốt lại quy mô HumanRealm sau playtest.

---

# 10. PHASE F — Boss + Region Progression

## Task 20 — Optional Boss Framework

Làm một optional boss prototype:
- Health;
- Boss state;
- attack patterns;
- arena;
- HP bar;
- reward.

Boss thường không bắt buộc progression.

## Task 21 — HumanRealm Main Boss + Teleport Stone

```text
Main Boss defeated
→ Teleport Stone
→ HumanRealm completed
→ Upgrade Cap tăng
→ Region tiếp theo được phép mở
```

Không xây Region 2 ngay.

### Checkpoint F — Technical Vertical Slice

Core loop phải đầy đủ:

```text
Explore
→ Combat
→ Soul
→ Upgrade
→ Loot
→ Equipment
→ Optional Boss
→ Main Boss
→ Teleport Stone
```

---

# 11. PHASE G — Persistence

## Task 22 — Save System

Dự kiến lưu:
- Player stats;
- Soul;
- upgrade progress;
- equipment;
- boss state;
- important chest state;
- region progression;
- checkpoint.

Làm sau khi runtime data tương đối ổn.

---

# 12. PHASE H — Asset Pipeline

## Task 23 — Art Bible

Chốt:
- tile size;
- PPU;
- palette;
- outline;
- lighting;
- pixel density;
- player/enemy canvas;
- pivot;
- baseline;
- sprite sheet layout;
- animation rules.

## Task 24 — HumanRealm Tileset

Tạo theo module:
- Grass
- Dirt
- Water
- Cliff
- Tree
- Rock
- Bush
- Flower
- Bridge
- Fence
- Props

Không dùng một ảnh map lớn làm gameplay map.

## Task 25 — Character / Enemy Animation Pipeline

Chốt:
- fixed canvas;
- fixed frame size;
- fixed baseline;
- fixed pivot;
- fixed body proportion;
- Grid By Cell Size;
- tránh Automatic Slice.

---

# 13. PHASE I — HumanRealm Content Production

Dự kiến:
- Task 26 — Slime production
- Task 27 — Bat production
- Task 28 — Rank E Enemy #1
- Task 29 — Rank E Enemy #2
- Task 30 — Village Content
- Task 31 — Cave Content
- Task 32 — Optional Boss Content
- Task 33 — Main Boss Content
- Task 34 — Loot Balancing
- Task 35 — HumanRealm Map Population
- Task 36 — Audio
- Task 37 — UI Polish
- Task 38 — HumanRealm Playtest + Balance

Chi tiết Task 26+ sẽ được chốt lại khi tới giai đoạn đó.

---

# 14. Model / Thinking guideline

Mỗi prompt Codex phải ghi:
- độ nặng;
- model;
- thinking level.

Khuyến nghị:

| Loại task | Model | Thinking |
|---|---|---|
| Nhẹ: folder, rename, data nhỏ | GPT-5.6 Sol hoặc model nhanh phù hợp | Low/Medium |
| Vừa: movement, camera, component đơn | GPT-5.6 Sol | Medium |
| Nặng: stats, combat, AI, spawn | GPT-5.6 Sol | High |
| Rất nặng: refactor nhiều hệ thống | GPT-5.6 Sol | Extra High nếu có |

Không dùng High cho mọi task.

---

# 15. Nguyên tắc checkpoint

Sau mỗi checkpoint:
1. Playtest.
2. Kiểm tra Console.
3. Kiểm tra architecture.
4. Xác nhận gameplay feel.
5. Chỉ sau đó mở task tiếp.

Nếu task trước chưa ổn thì sửa task đó, không xây hệ thống mới lên nền lỗi.

---

# 16. Nguyên tắc cao nhất

> **Xây đúng dependency, test từng lớp, giữ task nhỏ và hoàn thiện HumanRealm trước.**

> **Ưu tiên vertical slice chơi được và dễ mở rộng hơn số lượng feature.**
