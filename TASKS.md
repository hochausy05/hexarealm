# TASKS — HexaRealm

> Checkbox tracker only. Detailed implementation instructions belong in the active Codex prompt, not here.

## CURRENT WORK — AI READ THIS FIRST

### [ ] TASK 17 — HumanRealm Graybox  ← NEXT

- **Weight:** Heavy
- **Recommended model:** GPT-5.6 Sol
- **Thinking:** High
- **Depends on:** Tasks 01-16.5
- **Design refs:** `GD-02`, `GD-08`, `GD-09`, `GD-10`, `GD-11`
- **Goal:** create a playable placeholder HumanRealm world layout and test scale/travel/area spacing before final art.
- **Initial scale:** roughly 100-128 tiles per axis, 32x32 base tile; not final.
- **Must include:** readable village zone, wilderness/farm zones, roads, Soul Pillar/chest placements, cave entrance placeholder, main-boss route/area placeholder, enemy spawn-zone placements, world collision boundaries.
- **Out of scope:** final pixel art, final decoration, production NPC behavior, cave scene transition, boss implementation, Region 2.
- **Done when:** map can be explored end-to-end with current movement/camera/combat systems and gives useful travel/density feedback without breaking existing systems.

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

- [ ] T17 — HumanRealm Graybox
- [ ] T18 — Cave / Area Transition
- [ ] T19 — Village NPC Movement

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
