# SYSTEM_AI — AI/Codex Operating Rules

> **Purpose:** keep AI work focused, safe, and context-efficient.

## QUICK RULES — READ FIRST

1. Read `README.md` first.
2. Read this `QUICK RULES` section and only other rules relevant to the task.
3. Read `TASKS.md` -> `CURRENT WORK` and the target task only.
4. Search `GAME_DESIGN_CORE.md` by `GD-*` heading; do **not** read it end-to-end unless explicitly required.
5. Read `PLAN.md` only when task order/dependency matters, and only the current phase.
6. Read only the latest 1-3 relevant `PROJECT_CHANGELOG.md` entries when recent history is needed.
7. Read only directly related code/assets/prefabs/scenes. Never scan the whole project by default.
8. Make the smallest correct patch. Do not implement the next task.
9. `GAME_DESIGN_CORE.md` is the gameplay/design source of truth.
10. Stop after verification and a short report.

## 1. Context and token policy

Use **progressive disclosure** instead of loading all context up front.

### Default document behavior

| File | Default behavior |
|---|---|
| `README.md` | Read fully; intentionally short |
| `SYSTEM_AI.md` | Read Quick Rules + relevant section |
| `TASKS.md` | Read `CURRENT WORK` + target task |
| `GAME_DESIGN_CORE.md` | Search heading/keyword and read only matching `GD-*` section |
| `PLAN.md` | Read current phase only when needed |
| `PROJECT_CHANGELOG.md` | Read latest 1-3 relevant entries only |

Do not use full-file reads (`cat`, equivalent broad reads, or whole-repository scans) on large docs unless the task requires them.

Do not repeatedly re-read unchanged context within one task.

## 2. Source of truth

- Gameplay/design decisions: `GAME_DESIGN_CORE.md`.
- Current next task/status: `TASKS.md` -> `CURRENT WORK`.
- Roadmap/dependency intent: `PLAN.md`.
- Recent implementation history: `PROJECT_CHANGELOG.md`.
- Actual implementation behavior: current project files.

If code conflicts with the design source of truth, report the conflict before making a large design change.

## 3. Scope control

Implement only the requested task/fix.

Do not silently:
- start the next task;
- add unrelated features;
- refactor unrelated files;
- install packages;
- change project-wide settings;
- create speculative systems;
- redesign gameplay.

Useful ideas outside scope belong in the final note only.

## 4. Minimal engineering

Prefer:
- small components;
- explicit dependencies;
- Unity component-based design;
- ScriptableObjects for reusable authoring data;
- small deterministic helpers for formulas;
- local C# events when actually needed.

Avoid unless required:
- global Event Bus;
- Service Locator;
- dependency injection frameworks;
- generic RPG frameworks;
- complex state-machine frameworks;
- managers with project-wide responsibility;
- abstractions with no current value.

Patch instead of rewrite whenever practical.

## 5. Unity safety

Never manually edit generated folders:
- `Library/`
- `Temp/`
- `Logs/`
- `UserSettings/`

Do not manually break `.meta`/GUID references.

Do not change Render Pipeline, Input System, Sorting Layers, Physics Settings, packages, or Build Settings unless the active task requires it.

For Unity authoring that can be completed manually in a few minutes, prefer explicit manual user steps over brittle Editor automation, hierarchy hard-coding, or scene/prefab builder scripts.

Use Unity/MCP/Editor APIs for scene, prefab, hierarchy, inspector, console, and asset-import operations when useful. Use direct text/file access for scripts and docs.

If MCP cannot control Play Mode, do not build a complicated workaround just to simulate a click. Report the manual verification needed.

## 6. Project file rules

Game-owned content belongs under:

`Assets/_Game/`

Code/internal naming:
- English
- C# file/class: PascalCase
- namespace prefix: `HexaRealm`

In-game display text may be Vietnamese.

Do not create case-only duplicate folders.

## 7. Gameplay guardrails

Never add a traditional Character Level/EXP system.

Current player stats:
- Vitality
- Attack
- Defense
- Agility
- Rage

Current progression:

`Soul -> Soul Pillar -> stat upgrades`

Rage affects Crit Chance and Attack Speed.

Enemy ranks:

`F, E, D, C, B, A, S`

Rank power inputs are only:
- HP
- ATK
- DEF
- Speed

Current weapon scope: **melee slash only**.

Current player visual structure:

```text
Player
├── Body
├── WeaponSprite
└── SlashVFX
```

Do not create combined body sprites for every armor + weapon pair.

HumanRealm is semi-open. Optional bosses are not progression gates; the region main boss is.

## 8. Data rules

Authoring data should not be mutated at runtime.

Examples of suitable ScriptableObjects:
- EnemyData
- WeaponData
- ArmorData
- LootBundleData

Do not duplicate a stat as separate serialized sources of truth in multiple runtime components.

Derived values should be recalculated from their sources, not incrementally accumulated when that can cause double application.

## 9. Testing rules

After a meaningful implementation change:
1. compile;
2. inspect Console if available;
3. run only relevant existing tests;
4. perform focused runtime verification when possible;
5. report anything that still requires manual Play Mode validation.

Do not run expensive unrelated checks for a small patch.

Never claim runtime verification that did not actually happen.

## 10. Changelog writing rules

`PROJECT_CHANGELOG.md` is **log-only**. Do not put instructions, project overview, templates, source-of-truth notes, or AI rules inside it.

Write changelog entries in **Vietnamese**.

For a significant task/fix, prepend one compact entry:

```text
## YYYY-MM-DD — Task XX: Tên ngắn
- Thêm/thay đổi/sửa điều quan trọng nhất.
- Nêu hệ thống hoặc file chính khi hữu ích.
- Ghi một giới hạn đáng chú ý chỉ khi cần.
```

Keep entries to roughly 1-4 bullets. Do not paste prompts, diffs, long acceptance lists, or repetitive status text.

Update the changelog for:
- new systems/features;
- meaningful architecture changes;
- important project settings/packages;
- important scenes/prefabs;
- meaningful gameplay changes;
- significant bug fixes.

Skip changelog entries for typo/format/comment-only changes.

## 11. Task tracker rules

`TASKS.md` uses checkboxes.

Only change task status when evidence supports it:
- `[x]` verified/accepted;
- `[ ]` not done;
- `[~]` implemented but awaiting verification;
- `[!]` blocked/revision required.

Keep `CURRENT WORK` at the top accurate.

Do not copy full task prompts into `TASKS.md`.

## 12. Final response format

Keep the final task report compact:

```text
Completed:
- ...

Changed:
- ...

Verification:
- Compile: OK / issue
- Tests: OK / not run / issue
- Runtime: verified / manual check needed

Docs:
- TASKS.md: updated / unchanged
- PROJECT_CHANGELOG.md: updated / unchanged
```

Do not write a tutorial unless asked.

## 13. Priority order

1. Correct requirement.
2. Do not break the project.
3. Preserve design source of truth.
4. Keep implementation simple and maintainable.
5. Reuse existing systems where appropriate.
6. Minimize unnecessary context/token use.
7. Optimize only after the above are satisfied.
