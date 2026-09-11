# HexaRealm

> **AI entry point and project overview.** Keep this file short. Read it before opening larger project documents.

## Current snapshot

- Engine: Unity 6, Universal 2D / URP 2D
- Genre: 2D top-down fantasy action RPG
- Current region: HumanRealm
- Current milestone: core player/combat/progression/equipment/loot/farming foundations are implemented
- Latest completed task: Task 20 — Optional Boss Framework
- **Current task: Task 21 — HumanRealm Main Boss + Teleport Stone**

## Core loop

`Explore -> Combat -> Soul -> Soul Pillar -> Upgrade -> Loot -> Equipment -> Stronger -> Boss -> Region progression`

No traditional character level system is used.

## AI reading router

Use progressive disclosure. **Do not read every document in full by default.**

| Need | Read |
|---|---|
| Start any task | `README.md` + `SYSTEM_AI.md` Quick Rules |
| Know current/next task | `TASKS.md` -> `CURRENT WORK` only |
| Need a gameplay/design rule | Search `GAME_DESIGN_CORE.md` for the referenced `GD-*` section only |
| Need roadmap/dependency context | `PLAN.md` -> current phase only |
| Need recent implementation history | `PROJECT_CHANGELOG.md` -> latest 1-3 relevant entries only |
| Need implementation details | Only the directly related scripts/assets/prefabs/scenes |

Never read the full changelog, full design document, or entire `Assets/` tree unless the task explicitly requires it.

## Document roles

- `SYSTEM_AI.md` — operating rules for AI/Codex.
- `GAME_DESIGN_CORE.md` — design source of truth.
- `PLAN.md` — compact development roadmap and phase dependencies.
- `TASKS.md` — checkbox task tracker; `CURRENT WORK` is authoritative for the next task.
- `PROJECT_CHANGELOG.md` — Vietnamese implementation log only.

## Project paths

```text
HexaRealm/
├── Assets/
│   ├── _Game/          # HexaRealm-owned game content
│   ├── Scenes/         # Unity template content; may be removable if unused
│   └── Settings/       # URP/Universal 2D renderer settings; keep unless deliberately migrated
├── Packages/
├── ProjectSettings/
├── README.md
├── SYSTEM_AI.md
├── GAME_DESIGN_CORE.md
├── PLAN.md
├── TASKS.md
└── PROJECT_CHANGELOG.md
```

All new game-owned content should live under `Assets/_Game/` unless Unity itself requires another location.

## Current controls

- WASD / movement bindings: move
- Mouse pointer: melee aim
- Attack action / left mouse: slash toward pointer
- Dash action / Space: dash using movement direction logic
- Interact action: interact with Soul Pillars, chests, and future interactables

## Current architectural anchors

```text
PlayerStats: Base + Upgrade Modifier + Equipment Modifier = Final
Player visual: Body + WeaponSprite + SlashVFX
Enemy base data: EnemyData ScriptableObject
Enemy rank: F, E, D, C, B, A, S
Enemy rank budget inputs: HP + ATK + DEF + Speed only
Soul progression: Enemy death -> Soul wallet -> Soul Pillar -> stat upgrade
Equipment: one melee weapon slot + one armor slot
Farming: EnemySpawnZone -> new enemy instance on respawn
```

## Development principle

Finish HumanRealm as the vertical slice before expanding to the other five regions. Prefer small verified tasks, placeholders before final art, and reusable systems without speculative frameworks.
