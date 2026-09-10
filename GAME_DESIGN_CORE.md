# GAME_DESIGN_CORE — HexaRealm Design Source of Truth

> **AI usage:** Do not read this file end-to-end by default. Search the `GD-*` section referenced by the current task.
> **Scope:** stable design rules. Prototype balance numbers remain adjustable unless explicitly marked fixed.

## QUICK DESIGN CONTEXT

- 2D top-down fantasy pixel-art action RPG, single-player.
- World has 6 regions/faces; each region has a main boss and a Teleport Stone.
- Collect all 6 stones -> central/final area -> final boss.
- HumanRealm is the first region and current vertical-slice focus.
- No traditional Character Level or EXP progression.
- Progression: Soul -> Soul Pillar -> 5 stat upgrades + equipment.
- Stats: Vitality, Attack, Defense, Agility, Rage.
- Rage affects Crit Chance + Attack Speed.
- Farming is allowed; Upgrade Cap limits power progression by region.
- Enemy ranks: F, E, D, C, B, A, S. HumanRealm mainly uses F/E enemies.
- Enemy rank budget uses HP, ATK, DEF, Speed only.
- Current weapon class: melee slash only.
- Controls: movement and melee aim are independent; mouse pointer controls melee direction.
- Player visual layers: Body + WeaponSprite + SlashVFX.
- HumanRealm is semi-open, not a linear stage chain.
- Optional bosses reward exploration; only the main region boss gates progression.
- Base tile: 32x32 px, PPU 32, Point filtering, no compression.

---

## GD-01 — Game Vision and World

HexaRealm is a 2D top-down fantasy action RPG focused on exploration, melee combat, loot, stat growth, and bosses.

The world contains six distinct regions. Each region has its own environment, enemies, optional encounters/bosses, main boss, equipment, and one Teleport Stone.

Defeating each region's main boss grants its stone and unlocks progression. Six stones unlock the central/final region and final boss.

HumanRealm is the first and current development focus. Finish it as a reusable vertical slice before building the other five regions.

## GD-02 — Player Freedom and Core Loop

HumanRealm is semi-open. Do not force a single route or mandatory sequence of normal enemies/optional bosses.

The player may farm weak enemies for a long time and challenge the main boss early if desired. Skipping optional bosses/chests is allowed but should usually mean weaker equipment.

Core loop:

`Explore -> Fight -> Soul / Chest -> Equipment -> Soul Pillar -> Upgrade -> Stronger Areas -> Optional Boss -> Main Boss -> Teleport Stone`

The main region boss is the primary mandatory progression gate.

## GD-03 — Progression, Soul, and Upgrade Cap

There is no traditional Character Level, EXP bar, or automatic level-up stat growth.

Power comes from:
- Soul upgrades;
- weapons;
- armor;
- boss/chest rewards;
- higher Upgrade Caps unlocked by world progression.

Enemies grant Soul on death. Stronger enemies should generally reward more Soul, but exact values are balancing data.

Soul is spent at Soul Pillars to upgrade player stats. Soul Pillars may later also support healing/checkpoints/save/respawn, but those functions are separate features and must not be assumed unless implemented by a task.

Upgrade Cap applies to total Soul upgrades available in the current world progression state. The player remains free to distribute upgrades across any stats.

Do not reduce Soul rewards merely because the player farms for a long time.

## GD-04 — Player Stats

Exactly five core player stats:

| Stat | Meaning | Intended effect |
|---|---|---|
| Vitality | HP | Max Health |
| Attack | ATK | melee damage |
| Defense | DEF | damage mitigation |
| Agility | AGI | movement; may later affect mobility/dash |
| Rage | RAGE | Crit Chance + Attack Speed |

Stat architecture:

`Base + Upgrade Modifier + Equipment Modifier = Final`

Final values are derived. Upgrades and equipment must not mutate Base values.

Exact formulas/scaling remain balance decisions unless implemented as explicitly documented prototypes.

## GD-05 — Combat and Controls

Current combat scope:
- 8-direction movement;
- melee slash;
- directional hit detection;
- damage / health / death;
- dash;
- crit;
- attack speed;
- slash VFX;
- optional light knockback later if useful.

Current keyboard/mouse control intent:
- movement input controls movement;
- mouse pointer controls melee aim independently;
- attack slashes toward pointer;
- dash follows movement-direction logic, not pointer aim.

Current weapon class is **melee slash only**. Do not add bow, staff, spear, gun, or magic weapon classes during the current foundation unless a later task explicitly expands scope.

Crit and Attack Speed are influenced by Rage. Exact coefficients/caps are still balancing decisions.

## GD-06 — Enemy Rank and Power Budget

Normal enemy ranks:

`F < E < D < C < B < A < S`

HumanRealm primarily uses F and E enemies initially. Bosses are handled separately from normal rank balancing.

Normal enemy base power uses exactly:
- HP;
- ATK;
- DEF;
- Speed.

Behavior complexity, aggro range, attack pattern, collider size, Soul reward, flying, visuals, and loot do not directly determine rank.

If one base stat is unusually strong for a rank, other base stats should generally be lower so total power remains appropriate.

Example intent:
- F Bat: fast, fragile, low damage/defense.
- F Slime: slower, more HP, low-to-moderate low-rank damage.

Final power-budget weights and F-S thresholds are intentionally undecided until enough combat playtesting exists.

## GD-07 — Equipment and Loot

Equipment is a major progression source alongside Soul upgrades.

Sources may include:
- chests;
- optional bosses;
- main bosses;
- secret areas;
- shops/NPCs later if added.

Current equipment slots:
- one melee weapon;
- one armor set/slot.

Weapon data may influence:
- Damage;
- Attack Speed;
- Crit bonus;
- small Range bonus;
- WeaponSprite;
- optional SlashVFX override.

Armor may influence the five player stats and changes the Body visual.

Player visual structure:

```text
Player
├── Body          # armor/body visual
├── WeaponSprite  # weapon visual
└── SlashVFX      # attack effect
```

Do not create a unique combined character sprite for every armor + weapon pairing.

Loot/chest rewards should initially be fixed and deterministic before adding random loot systems.

## GD-08 — Enemy Respawn and Farming

Normal enemies may respawn so the player can farm Soul and keep areas active.

Rules:
- do not respawn directly on/next to the player;
- allow a delay and/or spatial safety condition;
- prefer reusable spawn zones over hard-coded individual respawn logic;
- a respawned enemy is a new life and may grant Soul again;
- the same corpse/death must not grant repeated Soul.

Main bosses should not automatically respawn during normal progression.

Optional-boss respawn policy is a later playtest decision.

## GD-09 — HumanRealm World

HumanRealm mood: bright, green, human/fantasy, friendly as the starting region but with dangerous wilderness pockets.

Expected world ingredients:
- grasslands;
- forests;
- dirt roads;
- rocks/bushes/flowers;
- small water features where useful;
- cliffs/terrain boundaries;
- caves;
- a village;
- wandering NPC ambience;
- wilderness farming areas;
- optional encounters/bosses;
- main boss area;
- Soul Pillars and chests.

The graybox should test exploration scale before final art.

Starting outdoor test scale:
- roughly 100-128 tiles per axis;
- base tile 32x32 px.

Do not lock final map size until travel time, density, camera, area spacing, and player movement have been playtested.

## GD-10 — Pixel Art and Asset Standards

Style: 2D top-down fantasy pixel art. Prioritize readability and consistency over detail.

Default technical standard:

```text
Base Tile: 32x32 px
Pixels Per Unit: 32
Filter Mode: Point / No Filter
Compression: None
Camera: Orthographic
```

Player animation cells may be 48x48 or 64x64 when extra motion space is needed. Enemy canvas size may vary by enemy scale.

Consistency rules:
- consistent pixel density;
- consistent body scale;
- consistent lighting direction;
- compatible palette;
- consistent outline/shading language;
- no arbitrary non-integer scaling that blurs pixel art.

Animation frames in one animation must keep fixed canvas, baseline, body scale, center, and pivot.

Prefer `Grid By Cell Size` for fixed-grid sprite sheets. Avoid Automatic Slice when a known grid exists.

Preferred character/enemy pivot: Bottom Center unless a specific asset family has a documented reason otherwise.

## GD-11 — Map Construction and Sorting

Do not use one giant rendered map image as the gameplay map.

Build world content from:
- Tilemaps;
- Rule Tiles when useful;
- environment prefabs;
- interactable prefabs;
- spawn points/zones.

Suggested tilemap separation:

```text
Grid
├── Ground
├── GroundDetails
├── Water
├── Collision
├── Decorations_Back
├── Decorations_Front
└── AbovePlayer
```

Top-down rendering must support Y-based sorting. Objects that sort dynamically against characters must share an appropriate dynamic Sorting Layer; always-above pieces such as canopies/roofs may use `AboveCharacters`.

## GD-12 — Architecture Principles

Prefer:
- Prefabs;
- ScriptableObjects for authoring data;
- small reusable MonoBehaviours;
- component-based systems;
- explicit data ownership;
- reusable foundations for later regions.

Do not hard-code content-specific values in multiple runtime scripts when one authoring data asset can be the source of truth.

HumanRealm must prove the reusable architecture before building the other five regions.

## GD-13 — Boss and Region Progression

Optional bosses:
- optional;
- stronger than normal enemies;
- reward exploration with useful equipment/rewards;
- must not gate the main path by default.

Main region boss:
- required to progress to the next region;
- grants the region's Teleport Stone;
- marks region progression completion;
- unlocks a higher Upgrade Cap;
- may grant signature equipment/rewards.

Do not build Region 2 before HumanRealm proves the vertical slice.

## GD-14 — Intentionally Undecided

Do not treat these as final unless a later approved task explicitly locks them:
- final damage formula;
- final DEF formula;
- crit multiplier;
- Rage coefficients/caps;
- Agility scaling;
- final Soul upgrade cost curve;
- exact Upgrade Caps by region;
- exact Soul rewards by rank;
- enemy-rank power weights/thresholds;
- exact HumanRealm enemy/boss roster;
- final HumanRealm size/layout;
- shop/crafting/quest/dialogue systems;
- final save format;
- final AI asset-generation pipeline;
- exact palette and production tileset.
