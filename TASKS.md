# TASKS — HexaRealm

> Checkbox tracker only. Detailed implementation instructions belong in the active Codex prompt, not here.

## CURRENT WORK — AI READ THIS FIRST

### [x] TASK 22 — Save System

- **T22.1 complete:** versioned JSON kernel, safe replacement, backup recovery, and runtime bootstrap.
- **T22.2 complete:** Soul, five upgrades, Upgrade Cap, equipment ownership/equipping, Regions, and Teleport Stones.
- **T22.3 complete:** stable LootChest IDs/opened state, conservative transaction-safe autosave, derived completed-region Main Boss suppression, and end-to-end hardening.
- **Next implementation milestone:** T24 HumanRealm Tileset; T23 Art Bible is already complete.

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
- [x] T19 — Village NPC Movement

## Boss / region progression

- [~] T20 — Optional Boss Framework  ← REBUILD VERIFICATION PENDING
- [~] T21 — HumanRealm Main Boss + Teleport Stone  ← VERIFICATION PENDING

## Persistence

- [x] T22 — Save System

## Asset pipeline

- [x] T23 — Art Bible
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
