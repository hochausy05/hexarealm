# GAME DESIGN CORE — 2D Fantasy Top-Down RPG

> **Mục đích:** Tài liệu nguồn (source of truth) dùng để định hướng thiết kế game, triển khai Unity và làm ngữ cảnh cho AI/Codex trong quá trình phát triển.
>
> **Trạng thái:** Các nguyên tắc cốt lõi dưới đây đã được chốt. Các con số cân bằng cụ thể như damage, Soul cost, respawn time, số lượng quái... sẽ được tinh chỉnh sau khi prototype và playtest.

---

# 1. Tầm nhìn game

## 1.1. Thể loại

- 2D Pixel Art.
- Góc nhìn từ trên xuống (Top-down).
- Fantasy Action RPG.
- Single-player.
- Tập trung vào khám phá, chiến đấu cận chiến, loot, nâng cấp nhân vật và boss.

## 1.2. Cấu trúc thế giới

Thế giới gồm **6 vùng đất / 6 mặt thế giới**.

Mỗi vùng có:
- Môi trường riêng.
- Quái riêng.
- Boss thường / boss phụ.
- Boss chính của vùng.
- Vật phẩm và vũ khí đặc trưng.
- Một viên **Đá Dịch Chuyển**.

Khi thu thập đủ 6 Đá Dịch Chuyển:
- Mở khóa không gian trung tâm.
- Di chuyển đến khu vực cuối.
- Đánh Final Boss.
- Kết thúc game.

## 1.3. Vùng đầu tiên — Nhân Giới

Phong cách chính:
- Xanh tươi.
- Nhiều cây cỏ, đá, bụi cây, hoa.
- Có hồ / sông nhỏ nếu phù hợp.
- Một vài hang động nhỏ.
- Một khu dân làng.
- NPC đi lại để tạo cảm giác thế giới có sức sống.
- Nhiều khu hoang dã để khám phá và farm quái.
- Boss thường nằm rải rác.
- Boss chính của Nhân Giới nằm ở khu vực riêng.

---

# 2. Triết lý gameplay

Game **không phải game đi ải tuyến tính**.

Người chơi được tự do lựa chọn cách mạnh lên và đường đi trong từng vùng.

Không bắt buộc:
- Phải đánh quái theo thứ tự.
- Phải đánh boss thường.
- Phải đi theo một tuyến đường duy nhất.
- Phải đạt một Level nhất định.

Ví dụ:
- Người chơi có thể dành vài giờ farm Slime.
- Dùng Linh Hồn để nâng chỉ số.
- Sau đó đi đánh thẳng boss chính nếu muốn.
- Tuy nhiên nếu bỏ qua boss thường và rương hiếm thì sẽ thiếu trang bị tốt, khiến boss chính khó hơn đáng kể.

**Nguyên tắc:**
> Cho người chơi tự chọn con đường mạnh lên thay vì ép họ theo chuỗi nhiệm vụ cố định.

---

# 3. Core Gameplay Loop

```text
Khám phá
    ↓
Đánh quái
    ↓
Nhận Linh Hồn / mở rương
    ↓
Tìm trang bị
    ↓
Tìm Trụ Linh Hồn
    ↓
Nâng chỉ số
    ↓
Mạnh lên
    ↓
Khám phá khu vực nguy hiểm hơn
    ↓
Đánh boss thường (tùy chọn)
    ↓
Nhận trang bị đặc trưng
    ↓
Đánh boss chính của vùng
    ↓
Nhận Đá Dịch Chuyển
    ↓
Mở giới hạn sức mạnh + vùng tiếp theo
```

---

# 4. Không sử dụng hệ thống Level

Game **không có Character Level truyền thống**.

Không có:
- EXP bar.
- Level 1, 2, 3...
- Tự động cộng HP/ATK khi lên level.

Progression của nhân vật đến từ:
1. Linh Hồn.
2. Nâng chỉ số tại Trụ Linh Hồn.
3. Trang bị.
4. Vũ khí.
5. Phần thưởng từ boss.
6. Mở giới hạn sức mạnh sau khi đánh boss chính của vùng.

---

# 5. Hệ thống Linh Hồn

## 5.1. Nhận Linh Hồn

Khi quái chết:
- Quái trao Linh Hồn cho người chơi.
- Quái càng mạnh thì càng cho nhiều Linh Hồn.

Boss có thể cho:
- Lượng Linh Hồn lớn.
- Trang bị đặc trưng.
- Phần thưởng riêng.

## 5.2. Trụ Linh Hồn

Linh Hồn được sử dụng tại **Trụ Linh Hồn** để nâng chỉ số.

Trụ có thể đảm nhiệm:
- Nâng chỉ số.
- Hồi phục.
- Điểm lưu / checkpoint.
- Điểm respawn.

Trụ được đặt rải rác, không quá dày. Trong Nhân Giới nên có trụ tại:
- Khu dân làng.
- Khu trung tâm map.
- Gần khu vực nguy hiểm.
- Gần đường dẫn tới boss chính.

---

# 6. Năm chỉ số chính của nhân vật

## 6.1. Sinh lực — HP

Tên nội bộ: `Vitality`

Tác dụng:
- Tăng Max HP.

Phong cách build:
- Tank.
- An toàn.
- Chịu được nhiều lỗi hơn khi chiến đấu.

## 6.2. Công kích — ATK

Tên nội bộ: `Attack`

Tác dụng:
- Tăng sát thương cận chiến.

Phong cách build:
- Sát thương cao.
- Đánh boss nhanh.
- Glass cannon nếu bỏ qua phòng thủ.

## 6.3. Giáp — DEF

Tên nội bộ: `Defense`

Tác dụng:
- Giảm sát thương nhận vào.

Phong cách build:
- Tank.
- Chịu đòn tốt.

## 6.4. Nhanh nhẹn — AGI

Tên nội bộ: `Agility`

Tác dụng:
- Tăng tốc độ di chuyển.
- Có thể ảnh hưởng nhẹ đến Dash / khả năng cơ động.

Phong cách build:
- Né đòn.
- Di chuyển nhanh.
- Hit-and-run.

## 6.5. Cuồng nộ — RAGE

Tên nội bộ: `Rage`

Tác dụng:
- Tăng Critical Chance.
- Tăng Attack Speed.

Phong cách build:
- Đánh nhanh.
- Nhiều crit.
- DPS cao.

---

# 7. Logic nâng chỉ số

Tại Trụ Linh Hồn:

```text
Chọn chỉ số
    ↓
Kiểm tra giới hạn hiện tại
    ↓
Kiểm tra đủ Linh Hồn
    ↓
Trừ Linh Hồn
    ↓
Tăng chỉ số
```

Chi phí nâng tăng dần theo số lần nâng.

Mục tiêu:
- Những điểm đầu dễ lấy.
- Những điểm sau cần farm nhiều hơn.
- Người chơi phải cân nhắc build.

Công thức cụ thể sẽ được cân bằng sau khi test.

---

# 8. Upgrade Cap và tự do farm

Mỗi vùng đất có một **Upgrade Cap**.

Trong Nhân Giới:
- Người chơi được farm Linh Hồn tùy ý.
- Có thể dành vài giờ đánh Slime nếu muốn.
- Không giảm thưởng vì farm lâu.
- Không ép rời khu vực.
- Chỉ không thể nâng vượt quá Upgrade Cap hiện tại.

Người chơi tự do phân phối điểm:
- Full ATK.
- Full HP + DEF.
- AGI + RAGE.
- Chia đều.
- Hoặc bất kỳ build nào khác.

Sau khi đánh boss chính của Nhân Giới:
- Upgrade Cap được mở rộng.
- Người chơi có thể tiếp tục mạnh hơn ở thế giới kế tiếp.

**Nguyên tắc:**
> Không chống farm bằng cách ép lối chơi; chỉ khóa trần sức mạnh theo tiến trình thế giới.

---

# 9. Phân cấp quái

Quái thường được chia thành 7 cấp:

```text
S
A
B
C
D
E
F
```

- F thấp nhất.
- S cao nhất.

Trong Nhân Giới ban đầu:
- Chủ yếu Rank F.
- Một số Rank E.
- Boss tính riêng sau.

---

# 10. Chỉ số quái

Quái thường sử dụng 4 chỉ số chính:

1. `HP`
2. `ATK`
3. `DEF`
4. `Speed`

Các yếu tố hành vi như cách tấn công, aggro, tầm đánh... có thể khác nhau giữa quái nhưng **không dùng để tính Rank**.

---

# 11. Logic Rank quái — Power Budget

Rank được xác định dựa trên **tổng sức mạnh của 4 chỉ số chính**.

Thiết kế theo nguyên tắc:

```text
Rank
    ↓
Power Budget
    ↓
Phân bổ vào HP / ATK / DEF / Speed
```

Nếu một quái nổi trội ở một chỉ số thì các chỉ số còn lại phải thấp hơn để vẫn giữ đúng Rank.

Ví dụ:

### Bat Rank F
- Speed cao.
- HP rất thấp.
- DEF thấp.
- ATK thấp.
- Có thể chết sau 1–2 đòn.

### Slime Rank F
- Speed thấp.
- HP cao hơn Bat.
- ATK thấp/trung bình thấp.
- DEF thấp.

Cả hai vẫn là Rank F vì tổng sức mạnh tương đương.

**Nguyên tắc cho AI/Codex:**
> Một chỉ số tăng mạnh thì phải giảm một hoặc nhiều chỉ số khác nếu vẫn muốn giữ cùng Rank.

Ngưỡng Power Budget cụ thể của F → S sẽ được chốt sau khi có combat prototype để tránh cân bằng bằng số liệu lý thuyết quá sớm.

---

# 12. Linh Hồn theo Rank

Số Linh Hồn nhận được phụ thuộc vào Rank.

```text
Rank càng cao
    ↓
Power Budget càng cao
    ↓
Soul Reward càng lớn
```

Khoảng tương đối:

| Rank | Soul Reward |
|---|---|
| F | Rất thấp |
| E | Thấp |
| D | Trung bình thấp |
| C | Trung bình |
| B | Cao |
| A | Rất cao |
| S | Cực cao |

Con số cụ thể sẽ cân bằng sau.

---

# 13. Trang bị

Trang bị là nguồn sức mạnh quan trọng bên cạnh nâng chỉ số.

Nguồn trang bị:
- Rương.
- Boss thường.
- Boss chính.
- Khu vực bí mật.
- Có thể bổ sung shop/NPC sau.

Người chơi bỏ qua boss thường vẫn có thể đánh boss chính nếu đủ mạnh bằng cách farm Linh Hồn, nhưng sẽ thiếu trang bị tốt và khó hơn.

---

# 14. Vũ khí

## 14.1. Phạm vi hiện tại

Giai đoạn đầu chỉ sử dụng:

**Vũ khí cận chiến dạng chém.**

Chưa triển khai:
- Bow.
- Staff.
- Spear.
- Gun.
- Magic weapon.
- Nhiều weapon class.

Mục tiêu:
- Giảm animation.
- Giảm code.
- Dễ cân bằng.
- Hoàn thiện combat trước.

## 14.2. Vũ khí đặc trưng

Boss thường, rương hiếm và boss chính có thể cho vũ khí đặc trưng.

Vũ khí có thể khác nhau ở:
- Damage.
- Attack Speed.
- Crit bonus.
- Range nhẹ.
- Hiệu ứng đặc biệt trong tương lai.

---

# 15. Kiến trúc hình ảnh Player

Player được tách thành:

```text
Player
│
├── Body
├── WeaponSprite
└── SlashVFX
```

## 15.1. Body

`Body`:
- Sprite nhân vật.
- Thay đổi theo bộ giáp đang mặc.
- Không gắn cố định vũ khí vào sprite body.

## 15.2. WeaponSprite

`WeaponSprite`:
- Sprite vũ khí tách riêng.
- Thay đổi khi đổi vũ khí.
- Gắn vào điểm neo / pivot trên nhân vật.

Nhờ đó:
- Không cần vẽ lại toàn bộ nhân vật cho từng thanh kiếm.
- Một animation chém có thể tái sử dụng cho nhiều vũ khí cận chiến.

## 15.3. SlashVFX

`SlashVFX`:
- Hiệu ứng chém.
- Ban đầu dùng một VFX mặc định.
- Sau này có thể thay đổi theo vũ khí.

---

# 16. Giáp

Giáp ảnh hưởng đến:
1. Chỉ số.
2. Hình ảnh `Body`.

Khi đổi giáp:
- Có thể thay sprite / animation body tương ứng.
- WeaponSprite vẫn là lớp riêng.
- SlashVFX vẫn là lớp riêng.

Không tạo tổ hợp sprite riêng cho từng giáp + từng kiếm.

```text
Body theo giáp
+
WeaponSprite theo vũ khí
+
SlashVFX
```

---

# 17. Combat cơ bản

MVP combat cần:
- Di chuyển 4/8 hướng.
- Đánh cận chiến.
- Hitbox.
- Damage.
- Nhận damage.
- Death.
- Dash / né.
- Crit.
- Attack Speed.
- Slash VFX.
- Knockback nhẹ nếu phù hợp.

Không mở rộng combat quá sớm.

---

# 18. Boss

## 18.1. Boss thường / boss phụ

- Không bắt buộc.
- Nằm ở các khu vực khám phá.
- Khó hơn quái thường.
- Cho phần thưởng tốt.
- Có thể cho vũ khí đặc trưng hoặc trang bị mạnh.

Vai trò:
- Khuyến khích khám phá.
- Giúp người chơi mạnh hơn.
- Không dùng để ép tuyến truyện.

## 18.2. Boss chính của vùng

Mỗi vùng có một boss chính.

Khi đánh bại:
- Nhận Đá Dịch Chuyển.
- Mở vùng tiếp theo.
- Mở Upgrade Cap mới.
- Có thể nhận vũ khí / trang bị đặc trưng.

Boss chính là cột mốc progression bắt buộc duy nhất để sang vùng tiếp theo.

---

# 19. Respawn quái

Quái thường có thể respawn để:
- Farm Linh Hồn.
- Giữ map luôn có hoạt động.
- Cho phép người chơi grind tùy ý.

Nguyên tắc:
- Không respawn ngay trước mặt player.
- Có thời gian chờ hoặc yêu cầu player rời khu vực đủ lâu.
- Có thể respawn khi reload/quay lại khu vực.
- Nên dùng spawn zone thay vì hard-code từng vị trí nếu phù hợp.

Boss chính:
- Không tự động respawn trong progression thông thường.

Boss thường:
- Quyết định respawn hay không sau khi playtest.

---

# 20. Triết lý thiết kế Nhân Giới

Nhân Giới là một **semi-open region**, không phải chuỗi level.

Người chơi được:
- Đi nhiều hướng.
- Farm quái.
- Tìm hang.
- Tìm rương.
- Tìm boss thường.
- Ghé làng.
- Tìm Trụ Linh Hồn.
- Tự quyết định khi nào đủ mạnh để đánh boss chính.

---

# 21. Nội dung Nhân Giới dự kiến

## Môi trường
- Đồng cỏ.
- Rừng.
- Cây.
- Đá.
- Bụi cây.
- Hoa.
- Đường đất.
- Hồ / sông nhỏ.
- Vách đá.
- Hang nhỏ.
- Tàn tích nếu phù hợp.
- Khu dân làng.
- Khu boss chính.

## Làng
- Một số nhà.
- NPC.
- NPC đi lại bằng waypoint đơn giản.
- Trụ Linh Hồn.
- Có thể có shop / thợ rèn sau.

Mục tiêu ban đầu của NPC:
- Làm thế giới có cảm giác sống.
- Không cần quest/dialogue phức tạp ngay.

---

# 22. Kích thước map

Không chốt tuyệt đối trước khi test.

Mục tiêu:
- Đủ rộng để có cảm giác khám phá.
- Không lớn đến mức trống.
- Có đủ không gian để farm và đi vòng.
- Thời gian chơi một map không quá ngắn.

Giá trị khởi điểm đề xuất:

```text
Outdoor map: khoảng 100–128 tiles mỗi chiều
Tile size: 32x32
```

Hang động:
- Có thể là scene hoặc sub-map riêng.
- Nhỏ hơn map ngoài trời.

Trước khi xây full map:
- Test tileset ở map nhỏ.
- Test mật độ quái.
- Test camera.
- Test tốc độ di chuyển.
- Sau đó mới chốt kích thước cuối.

---

# 23. Style Asset — Art Direction

## 23.1. Phong cách tổng thể

**2D Top-down Fantasy Pixel Art**

Cảm giác:
- Fantasy.
- Sạch.
- Dễ đọc gameplay.
- Màu sắc tươi nhưng không quá chói.
- Không quá realistic.
- Không quá chibi nếu làm giảm cảm giác phiêu lưu.
- Ưu tiên sự đồng nhất hơn độ chi tiết.

## 23.2. Nhân Giới

Palette chủ đạo:
- Xanh lá.
- Nâu đất.
- Xám đá.
- Xanh nước.
- Màu gỗ.
- Một số màu hoa / điểm nhấn.

Không khí:
- Tươi.
- Sống động.
- Là vùng khởi đầu nên thân thiện hơn các vùng sau.
- Có chất fantasy nhưng vẫn mang cảm giác “thế giới con người”.

---

# 24. Chuẩn kỹ thuật asset

Mặc định ban đầu:

```text
Tile Size: 32x32 px
Pixels Per Unit: 32
Texture Filter: Point / No Filter
Compression: None
Camera: Orthographic
Style: Pixel Art
```

## Tile
- Tile cơ bản: `32x32 px`.
- Object có thể lớn hơn một tile.

Ví dụ:
```text
Tree: 64x64 / 64x96
Rock lớn: 64x64
House: nhiều tile
Boss: lớn hơn nhân vật rõ rệt
```

## Player
Player không bắt buộc nằm trong đúng 32x32.

Khuyến nghị:
```text
Animation cell: 48x48 hoặc 64x64
```

Lý do:
- Có chỗ cho chuyển động.
- Có chỗ cho tay/giáp.
- Dễ xử lý animation chém.

## Enemy
- Quái nhỏ: 32x32.
- Quái vừa: 48x48.
- Elite: 48x48 hoặc 64x64.
- Boss: 96x96 trở lên tùy thiết kế.

---

# 25. Quy tắc consistency của Pixel Art

Mọi asset trong cùng game phải giữ:
- Pixel density tương đồng.
- Tỷ lệ nhân vật tương đồng.
- Cùng hướng ánh sáng.
- Cùng độ dày outline.
- Cùng cách đổ bóng.
- Palette tương thích.
- Cùng mức độ chi tiết.
- Không scale sprite pixel bằng tỷ lệ lẻ gây blur.

---

# 26. Animation Standard

Mọi frame của cùng animation phải:
- Cùng kích thước canvas.
- Cùng baseline.
- Cùng vị trí tâm.
- Cùng tỷ lệ cơ thể.
- Cùng pivot.
- Không thay đổi kích thước nhân vật giữa các frame.

Ưu tiên slice:

```text
Grid By Cell Size
```

Không dùng `Automatic Slice` nếu sprite sheet có grid cố định.

---

# 27. Pivot

Đối với nhân vật và quái top-down, ưu tiên:

**Bottom Center**

Lý do:
- Điểm chân ổn định.
- Dễ căn sorting.
- Dễ đặt collider.
- Dễ đặt nhân vật trên terrain.

Nếu một nhóm asset dùng pivot khác thì phải có lý do rõ ràng và phải nhất quán trong nhóm đó.

---

# 28. Layer hình ảnh của map

Không đặt mọi thứ vào một Tilemap duy nhất.

Cấu trúc đề xuất:

```text
Grid
│
├── Ground
├── GroundDetails
├── Water
├── Collision
├── Decorations_Back
├── Decorations_Front
└── AbovePlayer
```

Mục tiêu:
- Dễ chỉnh map.
- Dễ xử lý collision.
- Dễ sorting.
- Player có thể đi sau tán cây/object cao.
- Dễ thay asset về sau.

---

# 29. Sorting

Game top-down phải hỗ trợ Y-Sorting hoặc hệ thống sorting tương đương.

Mục tiêu:
- Player đứng phía trước object khi ở thấp hơn.
- Player đứng phía sau object khi ở cao hơn.
- Tree/house có thể tách phần chân và phần trên nếu cần.

---

# 30. Nguyên tắc xây map

Map phải được xây từ asset module / tileset.

Không dùng một ảnh map lớn làm gameplay trực tiếp.

Map được ghép bằng:
- Tilemap.
- Rule Tile khi phù hợp.
- Prefab decoration.
- Prefab object.
- Spawn point.
- Enemy spawn zone.

Lý do:
- Dễ chỉnh sửa.
- Dễ mở rộng.
- Dễ collision.
- Dễ thay asset.
- Dễ tái sử dụng.
- Dễ cho AI/Codex thao tác trong Unity.

---

# 31. Architecture định hướng cho Unity

Không hard-code nội dung riêng cho từng quái/vũ khí nếu có thể dùng data.

Ưu tiên:
- Prefab.
- ScriptableObject.
- Component-based architecture.
- Data-driven design.

## Enemy

```text
Enemy
├── EnemyController
├── EnemyStats
├── Health
├── Attack
├── Movement
├── Drop/SoulReward
└── Collider
```

## Player

```text
Player
├── PlayerController
├── PlayerStats
├── PlayerCombat
├── PlayerHealth
├── PlayerEquipment
├── PlayerAnimation
├── BodyRenderer
├── WeaponPivot
├── WeaponRenderer
└── SlashVFX
```

---

# 32. Data đề xuất

## EnemyData

```text
Name
Rank
HP
ATK
DEF
Speed
SoulReward
Prefab
Drops
```

## WeaponData

```text
Name
Damage
AttackSpeed
CritBonus
Range
WeaponSprite
SlashVFX
```

## ArmorData

```text
Name
DefenseBonus
OtherStats
BodySprite / AnimationSet
```

## PlayerUpgradeData

```text
UpgradeType
CurrentValue
UpgradeCost
UpgradeCap
```

---

# 33. Quy tắc dành cho AI / Codex

Khi AI/Codex làm việc trên project:

1. Đọc tài liệu này trước khi đưa ra thay đổi gameplay lớn.
2. Không tự ý thêm Character Level.
3. Không tự ý biến game thành linear stage progression.
4. Không ép người chơi đánh boss thường.
5. Không tự ý thêm nhiều weapon class trong giai đoạn đầu.
6. Giữ weapon ban đầu là melee slash.
7. Giữ đúng 5 Player Stat:
   - HP
   - ATK
   - DEF
   - AGI
   - RAGE
8. Quái dùng Rank S–F.
9. Rank quái dựa trên Power Budget của:
   - HP
   - ATK
   - DEF
   - Speed
10. Giữ kiến trúc visual:
    - Body
    - WeaponSprite
    - SlashVFX
11. Không tạo một sprite body mới cho mọi tổ hợp giáp + vũ khí.
12. Không hard-code dữ liệu nếu có thể đưa vào ScriptableObject.
13. Ưu tiên hệ thống tái sử dụng cho 5 thế giới còn lại.
14. Hoàn thiện Nhân Giới trước khi mở rộng thế giới khác.
15. Mọi thay đổi lớn về architecture/gameplay phải đối chiếu với tài liệu nguồn này.

---

# 34. Phạm vi phát triển hiện tại

Ưu tiên số 1:

**Hoàn thiện Nhân Giới trước.**

Không triển khai 6 vùng song song.

Nhân Giới phải chứng minh được toàn bộ gameplay loop:

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

Khi Nhân Giới hoàn chỉnh:
- Dùng lại architecture hiện có.
- Tạo vùng thứ hai.
- Chủ yếu thay Map, Asset, Quái, Boss, Trang bị và cân bằng.

---

# 35. Những thứ chưa chốt

Các mục sau **chưa phải quyết định cuối cùng**:
- Công thức damage.
- Công thức DEF.
- Crit multiplier.
- Attack Speed cap.
- AGI scaling.
- RAGE scaling.
- Giá Linh Hồn mỗi lần nâng.
- Upgrade Cap cụ thể của từng vùng.
- Soul Reward chính xác theo Rank.
- Power Budget/ngưỡng điểm cụ thể của S–F.
- Số boss thường trong Nhân Giới.
- Boss chính của Nhân Giới.
- Danh sách quái cụ thể.
- Danh sách vũ khí cụ thể.
- Shop.
- Crafting.
- Quest.
- NPC dialogue.
- Save system chi tiết.
- Pipeline dùng AI để tạo asset.
- Palette màu chính xác.
- Bộ tileset cuối cùng.

Các mục này sẽ được thiết kế sau dựa trên prototype thực tế.

---

# 36. Tóm tắt quyết định đã chốt

```text
GAME
2D top-down fantasy pixel-art action RPG

PROGRESSION
Không Level
Linh Hồn → Trụ Linh Hồn → nâng chỉ số

PLAYER STATS
HP
ATK
DEF
AGI
RAGE (Crit + Attack Speed)

ANTI-OVERFARM
Không cấm farm
Chỉ giới hạn Upgrade Cap theo từng thế giới
Boss chính mở cap tiếp theo

ENEMY RANK
S / A / B / C / D / E / F

NHÂN GIỚI
Chủ yếu F và E

ENEMY POWER
HP + ATK + DEF + Speed
Phân bổ theo Power Budget của Rank

GAME FLOW
Semi-open
Không đi ải tuyến tính
Không bắt buộc boss thường

OPTIONAL BOSSES
Cho trang bị/vũ khí mạnh
Không bắt buộc

MAIN BOSS
Bắt buộc để sang thế giới tiếp theo
Cho Đá Dịch Chuyển
Mở Upgrade Cap

WEAPON
Hiện tại chỉ melee slash

PLAYER VISUAL
Body = thay theo giáp
WeaponSprite = thay theo vũ khí
SlashVFX = mặc định, có thể mở rộng sau

ART
2D top-down fantasy pixel art

TILE
32x32

UNITY
Tilemap
Rule Tile khi phù hợp
ScriptableObject
Prefab
Component-based
Data-driven

DEVELOPMENT
Hoàn thiện Nhân Giới trước
Sau đó mới mở rộng 5 vùng còn lại
```

---

# 37. Nguyên tắc cao nhất của project

> **Ưu tiên hoàn thiện gameplay loop, khả năng tái sử dụng hệ thống và sự đồng nhất của asset hơn việc thêm nhiều tính năng.**

> **Người chơi được tự do quyết định cách mạnh lên và cách khám phá; boss chính chỉ đóng vai trò mở khóa giới hạn sức mạnh và thế giới kế tiếp.**

> **Nhân Giới là vertical slice hoàn chỉnh của toàn bộ game. Nếu Nhân Giới hoạt động tốt, 5 vùng sau phải có thể xây dựng dựa trên cùng một nền tảng kỹ thuật.**
