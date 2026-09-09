# SYSTEM_AI — Quy tắc làm việc cho AI/Codex

> **Project:** HexaRealm  
> **Vai trò:** Luật vận hành bắt buộc cho AI/Codex khi đọc, phân tích hoặc chỉnh sửa project.  
> **Mục tiêu:** Giảm sai sót, tránh over-engineering, hạn chế thao tác thừa và tiết kiệm token/context.

---

# 1. Thứ tự tài liệu ưu tiên

Trước khi thực hiện task, AI phải ưu tiên đọc tài liệu theo thứ tự:

1. `SYSTEM_AI.md`
2. `GAME_DESIGN_CORE.md`
3. `PROJECT_CHANGELOG.md`
4. Các file liên quan trực tiếp đến task hiện tại

Không đọc toàn bộ project nếu task chỉ liên quan đến một khu vực nhỏ.

---

# 2. Nguyên tắc source of truth

`GAME_DESIGN_CORE.md` là source of truth về gameplay, progression, player stats, enemy rank, world structure, art direction và architecture định hướng.

Không tự ý thay đổi các nguyên tắc cốt lõi nếu prompt không yêu cầu.

Nếu code hiện tại mâu thuẫn với `GAME_DESIGN_CORE.md`:
- báo rõ mâu thuẫn;
- không tự quyết thay đổi thiết kế lớn;
- ưu tiên hỏi hoặc đề xuất phương án ngắn gọn.

---

# 3. Quy tắc tiết kiệm token

## 3.1. Chỉ đọc file cần thiết

Không:
- đọc toàn bộ `Assets/`;
- đọc toàn bộ project;
- quét mọi script;
- mở các file không liên quan.

Chỉ đọc:
- file được prompt chỉ định;
- dependency trực tiếp;
- file cần thiết để hiểu hoặc sửa đúng task.

## 3.2. Không lặp lại tài liệu dài

Không copy lại toàn bộ:
- `GAME_DESIGN_CORE.md`;
- `SYSTEM_AI.md`;
- code dài đã tồn tại;
- nội dung prompt.

Chỉ tóm tắt phần liên quan.

## 3.3. Không giải thích dài dòng sau khi hoàn thành

Báo cáo cuối task phải ngắn:
- đã làm gì;
- file nào thay đổi;
- có lỗi hay không;
- lưu ý quan trọng nếu có.

Không viết tutorial dài nếu người dùng không yêu cầu.

## 3.4. Không tạo kế hoạch nhiều tầng cho task nhỏ

Task đơn giản:
- kiểm tra;
- thực hiện;
- xác minh;
- báo cáo.

Không tạo roadmap, architecture proposal hoặc tài liệu phụ nếu không được yêu cầu.

---

# 4. Không over-engineer

Không tự ý thêm:
- service locator;
- dependency injection framework;
- event bus toàn project;
- generic framework;
- state machine phức tạp;
- manager toàn cục;
- abstraction nhiều lớp;
- interface chỉ có một implementation;
- pattern chỉ để “chuẩn kiến trúc”.

Ưu tiên:
- code dễ đọc;
- component nhỏ;
- dependency rõ ràng;
- ScriptableObject khi thực sự phù hợp;
- Prefab;
- Unity component-based design.

Chỉ tăng độ phức tạp khi task thực tế cần.

---

# 5. Giới hạn phạm vi task

AI chỉ thực hiện đúng task được giao.

Không tự ý:
- làm bước tiếp theo;
- thêm tính năng “tiện thể”;
- refactor file không liên quan;
- đổi tên hàng loạt;
- thay architecture;
- thêm package;
- sửa Project Settings;
- tạo asset ngoài phạm vi.

Nếu thấy một cải tiến hữu ích:
- ghi thành đề xuất cuối task;
- không tự thực hiện.

---

# 6. Quy tắc sửa code

Trước khi sửa:
1. Đọc file hiện tại.
2. Xác định dependency trực tiếp.
3. Giữ coding style hiện có nếu hợp lý.
4. Sửa ít nhất có thể để đạt mục tiêu.

Ưu tiên patch nhỏ hơn rewrite.

Không rewrite toàn bộ file nếu chỉ cần sửa một phần.

Không xóa code đang hoạt động nếu chưa xác định rõ lý do.

---

# 7. Quy tắc Unity

Không chỉnh thủ công:
- `Library/`
- `Temp/`
- `Logs/`
- `UserSettings/`

Không sửa `.meta` bằng tay trừ khi thật sự bắt buộc.

Không làm mất GUID asset.

Không tự ý thay:
- Render Pipeline;
- Input System;
- Physics Settings;
- Sorting Layer;
- Package;
- Build Settings;

trừ khi task yêu cầu.

Sau thay đổi quan trọng:
- để Unity import asset bình thường;
- kiểm tra Console nếu MCP cho phép;
- ưu tiên sửa compile error trước khi kết thúc task.

---

# 8. Quy tắc file và folder

Nội dung tự phát triển của game nằm trong:

`Assets/_Game/`

Tên nội bộ:
- dùng tiếng Anh;
- PascalCase cho class/file C#;
- namespace bắt đầu bằng `HexaRealm`.

Tên hiển thị trong game có thể dùng tiếng Việt.

Không tạo duplicate folder chỉ vì khác chữ hoa/chữ thường.

Không di chuyển tài liệu root nếu prompt không yêu cầu.

---

# 9. Quy tắc gameplay bắt buộc

Không tự ý thêm hệ thống Level.

Player progression hiện tại:

`Soul -> Soul Pillar -> nâng chỉ số`

5 Player Stats:
- HP / Vitality
- ATK / Attack
- DEF / Defense
- AGI / Agility
- RAGE / Rage

RAGE ảnh hưởng:
- Critical Chance
- Attack Speed

Enemy Rank:
- S
- A
- B
- C
- D
- E
- F

Enemy Rank dựa trên Power Budget của:
- HP
- ATK
- DEF
- Speed

Không tự ý biến game thành tuyến tính.

Boss thường:
- tùy chọn.

Boss chính:
- mở vùng tiếp theo;
- cho Đá Dịch Chuyển;
- mở Upgrade Cap tiếp theo.

---

# 10. Quy tắc combat hiện tại

Phạm vi weapon hiện tại:

**Melee Slash only**

Không tự ý thêm:
- bow;
- spear;
- staff;
- gun;
- magic weapon class.

Player visual architecture:

```text
Player
├── Body
├── WeaponSprite
└── SlashVFX
```

- `Body` thay theo giáp.
- `WeaponSprite` thay theo vũ khí.
- `SlashVFX` ban đầu dùng mặc định.

Không tạo sprite body riêng cho từng tổ hợp giáp + vũ khí.

---

# 11. Quy tắc data

Ưu tiên data-driven khi dữ liệu cần thay đổi thường xuyên.

Ví dụ phù hợp với ScriptableObject:
- EnemyData
- WeaponData
- ArmorData
- LootData
- ProgressionData

Không hard-code hàng loạt thông số gameplay vào nhiều script khác nhau.

Nhưng cũng không tạo ScriptableObject chỉ để chứa một giá trị không cần tái sử dụng.

---

# 12. Quy tắc kiểm thử

Sau khi thay đổi code, nếu có thể:
1. Kiểm tra compile.
2. Kiểm tra Unity Console.
3. Chạy test liên quan nếu project đã có test.
4. Chỉ test phạm vi liên quan.

Không chạy các bước nặng không cần thiết cho một thay đổi nhỏ.

---

# 13. Quy tắc dùng MCP / Unity

Nếu đang kết nối Unity qua MCP:

Ưu tiên dùng MCP khi cần:
- kiểm tra Scene;
- Hierarchy;
- Inspector;
- Console;
- component;
- prefab;
- object trong Unity Editor.

Không dùng MCP chỉ để đọc file text nếu có thể đọc trực tiếp nhanh hơn.

Không thực hiện nhiều thao tác Editor lặp lại nếu có cách chỉnh dữ liệu/code an toàn hơn.

---

# 14. Quy tắc phản hồi cuối task

Báo cáo theo mẫu ngắn:

```text
Hoàn thành:
- ...

File thay đổi:
- ...

Kiểm tra:
- Compile: OK / lỗi
- Console: OK / cảnh báo

PROJECT_CHANGELOG.md:
- Đã cập nhật / Không cần cập nhật

Lưu ý:
- ...
```

Không lặp lại toàn bộ code đã viết.

---

# 15. Khi nào phải cập nhật PROJECT_CHANGELOG.md

Phải cập nhật nếu task:
- thêm tính năng;
- thêm hệ thống;
- thay architecture;
- thêm package/công nghệ;
- thay Project Settings quan trọng;
- thêm Scene chính;
- thêm Prefab/hệ thống gameplay;
- thay đổi hành vi gameplay đáng kể;
- sửa bug có ảnh hưởng đáng kể.

Không cần cập nhật với:
- đổi comment;
- sửa typo;
- formatting;
- thay đổi không ảnh hưởng chức năng;
- tạo folder rỗng ban đầu nếu chưa có hệ thống.

---

# 16. Quy tắc cập nhật changelog

Khi cập nhật `PROJECT_CHANGELOG.md`:
- thêm entry mới;
- không rewrite lịch sử cũ;
- không xóa entry cũ;
- mô tả ngắn gọn;
- ghi rõ file/hệ thống chính liên quan;
- không copy toàn bộ prompt;
- không copy diff code.

---

# 17. Ưu tiên cao nhất

Theo thứ tự:

1. Đúng yêu cầu.
2. Không phá project.
3. Giữ đúng source of truth.
4. Code đơn giản và dễ bảo trì.
5. Tái sử dụng hợp lý.
6. Tiết kiệm token/context.
7. Chỉ sau đó mới tối ưu thêm.

> **Không cố làm nhiều nhất. Hãy làm đúng phần cần thiết nhất.**
