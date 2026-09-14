# PLAN — HexaRealm Roadmap

> **AI usage:** Read only the current phase unless dependency/order questions require more.

## CURRENT PHASE

**Phase H — Asset Pipeline**

- Previous milestone: Phase G Persistence complete through Task 22.
- **Current transition: Phase H Asset Pipeline; Task 23 Art Bible is complete and Task 24 HumanRealm Tileset is next.**

Current goal: apply the approved Art Bible to the HumanRealm production tileset without reopening the completed Save architecture.

## Development order

`Foundation -> Player -> Combat -> Enemy/Soul -> Equipment/Loot/Farming -> World -> Boss/Region Progression -> Save -> Asset Pipeline -> Content -> Polish`

Principles:
- one verifiable task at a time;
- placeholders before final art;
- playtest at checkpoints;
- do not implement dependency-heavy systems early;
- finish HumanRealm as the vertical slice before Region 2.

## Phase roadmap

| Phase | Tasks | Outcome | Status |
|---|---:|---|---|
| A — Player | 03-05 | movement, camera/sorting, dash | Done |
| B — Stats/Combat | 06-08 | stats, health/damage, melee combat + mouse aim | Done |
| C — Enemy/Soul | 09-12 | enemy data/AI, Soul, Soul Pillar upgrades | Done |
| D — Equipment/Loot/Farming | 13-16.5 | weapon, armor, chest loot, respawn, cleanup | Done |
| E — HumanRealm World | 17-19 | graybox, cave transition, village NPC movement | Done |
| F — Boss/Region | 20-21 | optional boss, main boss, Teleport Stone | Prototype complete; verification pending |
| G — Persistence | 22 | save/load | Done |
| H — Asset Pipeline | 23-25 | art bible, tileset, animation pipeline | **Current — T23 done, T24 next** |
| I — HumanRealm Content | 26-38 | production content, audio/UI, balance | Planned |

## Phase E — current dependency notes

### Task 17 — HumanRealm Graybox
Depends on working:
- movement / dash / camera / Y-sorting;
- combat;
- Slime enemy + spawn zones;
- Soul Pillar;
- chest/loot;
- equipment.

Purpose:
- test map scale;
- travel time;
- zone spacing;
- farming density;
- village/cave/boss-route placement.

Use placeholders only. Do not commit to final art or final map size.

### Task 18 — Cave / Area Transition
Depends on Task 17 world layout. Prove HumanRealm <-> Cave transitions and correct return/spawn positions.

### Task 19 — Village NPC Movement
Depends on Task 17 village area. Add simple ambient NPC behavior: Idle -> Waypoint -> Walk -> Idle. No quest/dialogue framework.

## Future checkpoints

### Checkpoint E — HumanRealm Scale
After Task 19, verify:
- map is not too empty or too small;
- travel time feels reasonable;
- areas are readable/distinct;
- enemy density is usable;
- revise final HumanRealm size before production art.

### Checkpoint F — Technical Vertical Slice
After Task 21, the playable loop should include:

`Explore -> Combat -> Soul -> Upgrade -> Loot -> Equipment -> Optional Boss -> Main Boss -> Teleport Stone`

## Later roadmap

- T20 Optional Boss Framework
- T21 HumanRealm Main Boss + Teleport Stone
- T22 Save System
- T23 Art Bible
- T24 HumanRealm Tileset
- T25 Character/Enemy Animation Pipeline
- T26 Slime Production
- T27 Bat Production
- T28 Rank E Enemy #1
- T29 Rank E Enemy #2
- T30 Village Content
- T31 Cave Content
- T32 Optional Boss Content
- T33 Main Boss Content
- T34 Loot Balancing
- T35 HumanRealm Map Population
- T36 Audio
- T37 UI Polish
- T38 HumanRealm Playtest + Balance

Task 26+ details should be refined from the actual project state when reached rather than specified prematurely.
