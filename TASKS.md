# TASKS — HexaRealm

> Checkbox tracker only. Detailed implementation instructions belong in the active Codex prompt, not here.

## CURRENT WORK — AI READ THIS FIRST

### [~] TASK 19 — Village NPC Movement

- **Weight:** Medium
- **Depends on:** Task 17 village area and Task 18 transition foundation
- **Goal:** reusable ambient villagers looping Idle → Waypoint → Walk → Idle with authored village patrol paths.
- **Out of scope:** dialogue, interactions, schedules, combat, pathfinding, Task 20.
- **Done when:** manual Play Mode confirms stable physics-safe patrol movement and no HumanRealm regression.

## Status legend

- `[x]` verified/accepted
- `[ ]` not started / TODO
- `[~]` implemented, verification pending
- `[!]` blocked / needs revision

## Completed foundations

- [x] T01 — Project Folder Structure
- [x] T02 — Unity 2D Technical Foundation
- [x] T03 — Player Foundation + Movement
- [x] T04 — Camera Follow + Top-down Y-Sorting
- [x] T05 — Dash / Dodge
- [x] T06 — Player Stats Foundation
- [x] T07 — Health + Damage Foundation
- [x] T08 — Melee Slash Combat + Mouse Aim Correction
- [x] T09 — EnemyData + Rank / Power-Budget Foundation
- [x] T10 — Basic Enemy AI + Slime Rank F Prototype
- [x] T11 — Soul Reward + Player Soul Wallet
- [x] T12 — Soul Pillar + Player Upgrade Progression
- [x] T13 — Weapon Equipment Foundation
- [x] T14 — Armor Equipment Foundation
- [x] T15 — Chest + Loot Foundation
- [x] T16 — Enemy Spawn Zone + Respawn Foundation
- [x] T16.5 — TechnicalTest Visual/Runtime Cleanup

## World phase

- [x] T17 — HumanRealm Graybox
- [x] T18 — Cave / Area Transition
- [~] T19 — Village NPC Movement  ← VERIFICATION PENDING

## Boss / region progression

- [ ] T20 — Optional Boss Framework
- [ ] T21 — HumanRealm Main Boss + Teleport Stone

## Persistence

- [ ] T22 — Save System

## Asset pipeline

- [ ] T23 — Art Bible
- [ ] T24 — HumanRealm Tileset
- [ ] T25 — Character / Enemy Animation Pipeline

## HumanRealm production and polish

- [ ] T26 — Slime Production
- [ ] T27 — Bat Production
- [ ] T28 — Rank E Enemy #1
- [ ] T29 — Rank E Enemy #2
- [ ] T30 — Village Content
- [ ] T31 — Cave Content
- [ ] T32 — Optional Boss Content
- [ ] T33 — Main Boss Content
- [ ] T34 — Loot Balancing
- [ ] T35 — HumanRealm Map Population
- [ ] T36 — Audio
- [ ] T37 — UI Polish
- [ ] T38 — HumanRealm Playtest + Balance

## Checkpoints

- [x] A — Player Feel
- [x] B — Combat Foundation
- [x] C — Soul Progression Loop
- [x] D — Equipment / Loot / Farming Foundation
- [ ] E — HumanRealm Scale
- [ ] F — HumanRealm Technical Vertical Slice

## Tracker rules

- Keep this file compact.
- Do not paste full prompts or changelog entries here.
- After finishing a task, update only its checkbox and `CURRENT WORK`.
- Add task detail here only for the current/next task; future task details stay one-line until needed.
