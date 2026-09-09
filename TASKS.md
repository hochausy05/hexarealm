# TASKS — HexaRealm Development Tasks

> **Project:** HexaRealm  
> **Mục đích:** Danh sách task thực thi cho AI/Codex.  
> **Quy tắc:** `PLAN.md` giải thích roadmap; file này theo dõi task cụ thể và trạng thái.

---

# 1. Status legend

```text
✅ DONE
🟡 IN PROGRESS
⬜ TODO
⛔ BLOCKED
🔁 NEEDS REVISION
```

---

# 2. Quy tắc xử lý task

Trước mỗi task, AI/Codex phải:

1. Đọc `SYSTEM_AI.md`.
2. Đọc phần liên quan trong `GAME_DESIGN_CORE.md`.
3. Đọc `PLAN.md`.
4. Đọc task hiện tại trong `TASKS.md`.
5. Đọc `PROJECT_CHANGELOG.md`.
6. Chỉ đọc code/file dependency trực tiếp.

Sau mỗi task:
- xác minh compile / Console nếu có thể;
- cập nhật `PROJECT_CHANGELOG.md` nếu có thay đổi đáng kể;
- dừng, không tự làm task kế tiếp.

---

# 3. Completed Foundation

## ✅ TASK 01 — Project Folder Structure

**Độ nặng:** Nhẹ

### Goal
Tạo cấu trúc `Assets/_Game` sạch cho HexaRealm.

### Completed
- Art
- Animations
- Audio
- Data
- Materials
- Prefabs
- Scenes
- Scripts
- Tilemaps
- Tests
- HumanRealm folders

### Không triển khai
- gameplay;
- script;
- prefab gameplay;
- package mới.

---

## ✅ TASK 02 — Unity 2D Technical Foundation

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Completed
- Unity Input System hiện có được sử dụng.
- `HexaRealmInputActions.inputactions`.
- Gameplay actions: Move, Attack, Dash, Interact.
- Sorting Layers: Ground, GroundDetails, Environment, Characters, AboveCharacters, VFX, UI.
- Unity Layers: Player, Enemy, NPC, World, Interactable, PlayerHitbox, EnemyHitbox.
- `TechnicalTest.unity`.
- Orthographic Main Camera.
- Grid + Tilemaps: Ground, GroundDetails, Collision.

---

# 4. Player Phase

## ✅ TASK 03 — Player Foundation + Movement

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Dependency
Task 02.

### Goal
Tạo Player prototype có movement 8 hướng ổn định.

### Do
- Player prefab.
- Rigidbody2D.
- Collider2D.
- Player layer.
- đọc `Gameplay/Move`.
- movement 8 hướng.
- normalized diagonal movement nếu cần.
- speed chỉnh được.
- freeze rotation.
- test trong `TechnicalTest`.

### Do not
- Camera follow.
- Animation.
- Dash.
- Attack.
- Health.
- Stats.
- Equipment.
- Soul.

### Acceptance
- WASD hoạt động.
- Arrow Keys hoạt động nếu binding có sẵn.
- Diagonal không nhanh bất thường.
- Không rotate do physics.
- Không compile error.
- Player test được trong scene.

---

## ✅ TASK 04 — Camera Follow + Top-down Y-Sorting

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Dependency
Task 03.

### Goal
Camera theo Player ổn định và sorting top-down đúng.

### Do
- Camera follow.
- smoothing nhẹ nếu cần.
- tránh jitter.
- Y-Sorting cơ bản.
- test Player trước/sau environment object.

### Do not
- Cinemachine nếu không cần.
- combat.
- animation system.

### Acceptance
- Camera follow không rung rõ rệt.
- Sorting đúng.
- Không phá movement.

---

## ✅ TASK 05 — Dash / Dodge

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Dependency
Task 03–04.

### Goal
Thêm dash cơ bản cho Player.

### Do
- đọc `Gameplay/Dash`.
- dash theo hướng hợp lý.
- cooldown.
- chống spam.
- tham số chỉnh được.

### Do not
- stamina.
- complex i-frame.
- final VFX.

### Acceptance
- Dash ổn định mọi hướng.
- Không spam vô hạn.
- Không gây lỗi physics nghiêm trọng.

---

# CHECKPOINT A — Player Feel

- [ ] Movement mượt.
- [ ] Camera không jitter.
- [ ] Y-Sorting đúng.
- [ ] Dash dễ điều khiển.
- [ ] Tốc độ phù hợp game top-down.

---

# 5. Stats + Combat Phase

## ✅ TASK 06 — Player Stats Foundation

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### Dependency
Checkpoint A.

### Stats
- Vitality
- Attack
- Defense
- Agility
- Rage

### Required design

```text
Base
+
Upgrade Modifier
+
Equipment Modifier
=
Final
```

### Acceptance
- Modifier không bị cộng rải rác trong nhiều script.
- Có thể test modifier độc lập.
- Không hard-code cho riêng HumanRealm.

---

## ✅ TASK 07 — Health + Damage Foundation

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Dependency
Task 06.

### Do
- Max HP.
- Current HP.
- TakeDamage.
- Defense application ban đầu.
- Death state.

### Acceptance
- Player nhận damage đúng.
- HP không xuống trạng thái lỗi.
- Death đúng.
- Có khả năng tái sử dụng cho Enemy mà không over-engineer.

---

## 🟡 TASK 08 — Melee Slash Combat

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### Dependency
Task 06–07.

### Do
- Attack input.
- WeaponPivot.
- WeaponSprite.
- SlashVFX placeholder.
- melee hitbox.
- attack cooldown.
- ATK.
- Crit.
- Rage → Attack Speed.
- Rage → Crit Chance.
- chống multi-hit ngoài ý muốn.

### Scope
Melee Slash only.

### Do not
- Bow.
- Spear.
- Staff.
- Magic weapon.
- combo tree lớn.

### Acceptance
- Attack ổn định.
- Damage đọc stat đúng.
- Crit hoạt động.
- Attack speed thay đổi theo Rage.
- Weapon visual tách khỏi Body.

---

# CHECKPOINT B — Combat Feel

- [ ] Player đánh được target.
- [ ] Hitbox dễ hiểu.
- [ ] Không double-hit lỗi.
- [ ] Crit đúng.
- [ ] Attack Speed đúng.
- [ ] Combat không phá movement/dash.

---

# 6. Enemy + Soul Phase

## ✅ TASK 09 — Enemy Data + Rank Foundation

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Fields
- Name
- Rank
- HP
- ATK
- DEF
- Speed
- SoulReward

### Rank
F, E, D, C, B, A, S.

### Rule
Rank dựa trên HP + ATK + DEF + Speed.
Behavior không tính Rank.

### Acceptance
- Tạo EnemyData mới mà không sửa core code.
- Không chốt cứng balancing F–S quá sớm.

---

## 🟡 TASK 10 — Basic Enemy AI

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### Prototype
Slime Rank F.

### Behavior

```text
Idle
→ Detect
→ Chase
→ Attack
→ Lose target
→ Return / Idle
```

### Acceptance
- Slime phát hiện Player.
- Chase hợp lý.
- Attack.
- Nhận damage.
- Chết.
- Không chase vô hạn toàn map nếu không chủ đích.

---

## ✅ TASK 11 — Soul Drop + Player Soul Wallet

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Goal
Enemy chết → Player nhận Soul.

### Acceptance
- SoulReward đọc từ EnemyData.
- Kill Slime → Soul tăng đúng.
- Không reward nhiều lần cho cùng death.

---

## ✅ TASK 12 — Soul Pillar + Upgrade System

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### Dependency
Task 06, 11.

### Flow

```text
Interact
→ chọn stat
→ check cost
→ check cap
→ spend Soul
→ upgrade
```

### Required
- cost scaling;
- Upgrade Cap;
- tự do build;
- không Character Level.

### Acceptance
- Upgrade làm Final Stat thay đổi thật.
- Không nâng vượt cap.
- Không trừ Soul khi upgrade thất bại.

---

# CHECKPOINT C — Core Progression

- [ ] Kill → Soul.
- [ ] Soul → Upgrade.
- [ ] Upgrade → Player mạnh hơn.
- [ ] Không Level.
- [ ] Build tự do.
- [ ] Cap hoạt động.

---

# 7. Equipment + Loot + Farming Phase

## ✅ TASK 13 — Weapon Equipment

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### WeaponData
- Name
- Damage
- AttackSpeed
- CritBonus
- Range
- WeaponSprite
- SlashVFX

### Acceptance
- Equip weapon thay sprite.
- Combat dùng stat weapon mới.
- Không cần body mới theo weapon.

---

## 🟡 TASK 14 — Armor Equipment

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Rule

```text
Body = Armor
WeaponSprite = Weapon
SlashVFX = Attack effect
```

### Acceptance
- Equip armor đổi modifier.
- Có thể đổi Body visual.
- Không tạo combination sprite armor+weapon.

---

## ⬜ TASK 15 — Chest + Loot

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### MVP reward
- Soul.
- Weapon.
- Armor.

### Acceptance
- Chest chỉ mở hợp lệ.
- Reward không nhận lặp ngoài thiết kế.
- Fixed reward hoạt động trước RNG.

---

## ⬜ TASK 16 — Enemy Spawn Zone + Respawn

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### Do
- spawn zone.
- enemy data/type.
- max alive.
- respawn delay.
- spawn positions.
- tránh respawn trước mặt Player.

### Acceptance
- Enemy quay lại sau điều kiện respawn.
- Không vượt max alive.
- Không duplicate vô hạn.
- Reusable cho nhiều vùng.

---

# CHECKPOINT D — Farming Loop

- [ ] Enemy spawn ổn.
- [ ] Kill / respawn ổn.
- [ ] Soul farm hoạt động.
- [ ] Không spawn khó chịu trước mặt Player.

---

# 8. World Phase

## ⬜ TASK 17 — HumanRealm Graybox

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### Initial test scale
- khoảng 100–128 tiles mỗi chiều;
- tile 32x32.

### Test
- travel time.
- camera.
- area spacing.
- enemy density.
- village position.
- cave position.
- boss route.

### Do not
- final art.
- full decoration.
- lock kích thước map trước playtest.

---

## ⬜ TASK 18 — Cave / Area Transition

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Acceptance
- vào đúng cave.
- ra đúng return point.
- không spawn sai vị trí.

---

## ⬜ TASK 19 — Village NPC Movement

**Độ nặng:** Vừa  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium

### Behavior
- Idle.
- Waypoint.
- Walk.
- Idle.

### Do not
- quest system.
- relationship.
- schedule lớn.
- dialogue tree phức tạp.

---

# CHECKPOINT E — HumanRealm Scale

- [ ] Map không quá trống.
- [ ] Map không quá nhỏ.
- [ ] Travel time hợp lý.
- [ ] Khu vực dễ nhận biết.
- [ ] Chốt lại kích thước map sau playtest.

---

# 9. Boss + Region Progression Phase

## ⬜ TASK 20 — Optional Boss Framework

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### Do
- boss health.
- boss state.
- attacks.
- arena.
- HP bar.
- reward.

### Rule
Optional Boss không bắt buộc progression.

---

## ⬜ TASK 21 — HumanRealm Main Boss + Teleport Stone

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### On defeat
- reward;
- Teleport Stone;
- HumanRealm completed;
- Upgrade Cap tăng;
- unlock khả năng sang region tiếp theo.

### Do not
- xây Region 2 ngay.

---

# CHECKPOINT F — Technical Vertical Slice

- [ ] Explore
- [ ] Combat
- [ ] Soul
- [ ] Upgrade
- [ ] Loot
- [ ] Equipment
- [ ] Optional Boss
- [ ] Main Boss
- [ ] Teleport Stone

---

# 10. Persistence Phase

## ⬜ TASK 22 — Save System

**Độ nặng:** Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** High

### Save
- Player stats.
- Soul.
- upgrade progress.
- equipment.
- boss state.
- important chest state.
- region progression.
- checkpoint.

---

# 11. Asset Pipeline Phase

## ⬜ TASK 23 — Art Bible

**Độ nặng:** Vừa/Nặng  
**Model:** GPT-5.6 Sol  
**Thinking:** Medium hoặc High

### Chốt
- tile size;
- PPU;
- palette;
- outline;
- lighting;
- pixel density;
- player frame canvas;
- enemy canvas;
- baseline;
- pivot;
- sprite sheet layout;
- animation consistency.

---

## ⬜ TASK 24 — HumanRealm Tileset

**Độ nặng:** Nặng về asset production

### Modules
- Grass.
- Dirt.
- Water.
- Cliff.
- Tree.
- Rock.
- Bush.
- Flower.
- Bridge.
- Fence.
- Props.

### Rule
Không dùng một ảnh map lớn làm gameplay map.

### Acceptance
- tile nối đúng.
- corner/edge đúng.
- Rule Tile test được.
- không seam lỗi rõ rệt.

---

## ⬜ TASK 25 — Character / Enemy Animation Pipeline

**Độ nặng:** Nặng về asset consistency

### Required
- fixed frame size;
- fixed canvas;
- fixed baseline;
- fixed center;
- fixed pivot;
- consistent body proportion;
- Grid By Cell Size;
- tránh Automatic Slice.

### Goal
Loại bỏ jitter và frame lệch do AI asset.

---

# 12. HumanRealm Content Production

## ⬜ TASK 26 — Slime Production
## ⬜ TASK 27 — Bat Production
## ⬜ TASK 28 — Rank E Enemy #1
## ⬜ TASK 29 — Rank E Enemy #2
## ⬜ TASK 30 — Village Content
## ⬜ TASK 31 — Cave Content
## ⬜ TASK 32 — Optional Boss Content
## ⬜ TASK 33 — Main Boss Content
## ⬜ TASK 34 — Loot Balancing
## ⬜ TASK 35 — HumanRealm Map Population
## ⬜ TASK 36 — Audio
## ⬜ TASK 37 — UI Polish
## ⬜ TASK 38 — HumanRealm Playtest + Balance

Chi tiết Task 26+ sẽ được viết lại khi tới giai đoạn đó dựa trên project thực tế.

---

# 13. Quy tắc tạo prompt cho từng task

Mỗi prompt Codex phải nêu ở đầu:

```text
TASK XX — Name
Độ nặng: Nhẹ / Vừa / Nặng
Model: ...
Thinking: ...
Dependency: ...
```

Prompt phải có:
- Goal.
- Scope.
- Do.
- Do not.
- Safety.
- Acceptance criteria.
- Changelog rule.
- Stop condition.

Không yêu cầu Codex tự làm task tiếp theo.

---

# 14. Current Next Task

```text
NEXT TASK:
TASK 13 — Weapon Equipment
```
