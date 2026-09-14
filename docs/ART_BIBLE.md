# HexaRealm — ART_BIBLE

> Source of truth for HexaRealm visual production.
> Scope: HumanRealm vertical slice.
> Purpose: guide AI/Codex and human artists when creating, reviewing, importing, or modifying 2D game assets.
> This document does NOT change gameplay, balance, or runtime architecture.

---

## 0. AI OPERATING RULES

### Priority order

Use source priority by domain instead of one global override chain.

**Gameplay / lore / progression intent**

1. Current local gameplay/code state
2. `GAME_DESIGN_CORE.md`
3. Task-specific instructions
4. This `ART_BIBLE.md`
5. AI inference

**Visual production**

1. This `ART_BIBLE.md`
2. Existing `APPROVED` production assets / approved visual references
3. Task-specific instructions that do not contradict locked project rules
4. `GAME_DESIGN_CORE.md` visual intent
5. AI inference

**Unity technical reality**

1. Current local Unity/project state
2. Current project documentation
3. Historical documentation / legacy assets

If two sources conflict, follow the higher-priority source for that domain and report the conflict.
AI MUST NOT use a broad lore/gameplay statement to override a more specific locked visual-production rule unless the task explicitly requires a design revision.

### Requirement keywords

- **MUST** = mandatory.
- **MUST NOT** = prohibited.
- **SHOULD** = recommended unless there is a documented reason not to.
- **MAY** = optional.
- **OPEN** = not finalized; do not invent a permanent standard.

### AI behavior

AI/Codex MUST:

- preserve existing gameplay and runtime architecture;
- preserve approved art direction;
- reuse existing conventions before inventing new ones;
- report assumptions before making permanent art-pipeline decisions;
- keep generated assets modular and reusable;
- validate assets at native game scale;
- distinguish production-ready assets from placeholders.

AI/Codex MUST NOT:

- silently change pixel density;
- silently change camera perspective;
- add new weapon classes;
- merge `Body`, `WeaponSprite`, and `SlashVFX` into one permanent player sprite;
- use anti-aliased or blurry scaling for pixel art;
- treat an attractive standalone asset as approved if it breaks project consistency;
- convert OPEN decisions into permanent rules without approval.

---

# 1. ART DIRECTION SUMMARY

```yaml
game: HexaRealm
genre: 2D top-down fantasy action RPG
visual_style: pixel art
camera: orthographic top-down
current_region: HumanRealm
current_scope: vertical slice
primary_goal:
  - gameplay readability
  - strong silhouettes
  - cohesive asset families
  - reusable production pipeline
human_realm_mood:
  - green
  - lively
  - warm
  - welcoming
  - fantasy
  - readable
```

HumanRealm is the starting region.

It SHOULD feel:
- bright and alive;
- warmer and friendlier than later regions;
- natural rather than neon;
- adventurous without excessive visual noise.

Dangerous areas, caves, and boss zones MAY increase contrast, saturation control, shadow depth, and visual tension while remaining recognizably part of HumanRealm.

---

# 2. NON-NEGOTIABLE VISUAL RULES

Every production asset MUST satisfy all applicable rules below.

## 2.1 Pixel consistency

- Pixel density MUST remain visually consistent across related assets.
- Pixel art MUST use hard pixel edges.
- Non-integer sprite scaling MUST NOT be used for production presentation.
- Anti-aliasing MUST NOT be baked into pixel-art sprites.
- Smooth gradients and airbrush shading MUST NOT be used.
- Random 1-pixel noise MUST NOT replace intentional pixel clusters.

## 2.2 Style consistency

Related assets MUST preserve:
- lighting direction;
- outline language;
- shading logic;
- detail density;
- material rendering logic;
- palette compatibility;
- body proportions;
- silhouette language.

## 2.3 Animation consistency

Frames in the same animation family MUST preserve:
- canvas size;
- body scale;
- baseline;
- center;
- pivot;
- facing convention;
- costume proportions;
- palette;
- lighting direction.

Character height and body proportions MUST NOT drift between frames.

## 2.4 Readability

At gameplay scale:
- player MUST be distinguishable from terrain;
- enemies MUST be distinguishable from each other;
- hazards MUST read clearly;
- interactive objects MUST read clearly;
- rewards SHOULD attract attention without overpowering the environment;
- important silhouettes MUST remain readable at approximately 50% preview scale.

---

# 3. TECHNICAL STANDARD

## 3.1 Global import standard

| Property | Required value |
|---|---|
| Engine | Unity |
| Camera | Orthographic |
| Base tile size | `32x32 px` |
| Pixels Per Unit | `32` |
| Texture Filter | `Point / No Filter` |
| Compression | `None` |
| Sprite alpha | Transparent when applicable |
| Mipmaps | Off unless explicitly required |
| Sprite slicing | Fixed grid when sheet uses fixed cells |
| Preferred slicing mode | `Grid By Cell Size` |
| Character default pivot | `Bottom Center` |
| Scaling | Integer / nearest-neighbor |

## 3.2 Recommended canvas sizes

| Asset class | Recommended canvas |
|---|---:|
| Base terrain tile | `32x32` |
| Small enemy | `32x32` |
| Player frame | `48x48` or `64x64` |
| Medium enemy | `48x48` |
| Elite enemy | `48x48` or `64x64` |
| Boss | `96x96+` |
| Large environment prop | Multiple of base tile when practical |

Canvas size is an animation/import boundary, not permission to resize the subject inconsistently between frames.

## 3.3 Default pivot

Characters and enemies SHOULD use:

```text
Bottom Center
```

Reason:
- stable ground contact;
- predictable Y-sorting;
- consistent collider alignment;
- stable animation baseline.

A different pivot MAY be used only when the asset requires it and the reason is documented.

## 3.4 Production scale reference — HumanRealm v1

The base world unit is:

```text
1 tile = 32x32 px = 1 Unity world unit at PPU 32
```

`Player = 1.0x` is the primary visual-mass reference for characters and enemies.
This is a visual scale guide, NOT a collider or balance specification.

| Asset class | HumanRealm v1 visual scale guideline |
|---|---|
| Player | `1.0x` reference; body SHOULD occupy roughly `65–80%` of its frame height |
| Normal NPC | `0.90–1.10x` Player |
| Small Rank F enemy | `0.45–0.80x` Player visual mass |
| Medium / Rank E enemy | `0.80–1.20x` Player |
| Elite enemy | `1.10–1.60x` Player |
| Optional Boss | `1.50–2.25x` Player |
| Main Boss | `2.00–3.00x` Player |
| Common chest | roughly `0.75–1.25` tiles wide |
| Common rock | roughly `0.5–1.5` tile footprint |
| Common tree trunk/base | roughly `0.5–1.0` tile footprint |
| Common tree canopy | roughly `2–3` tiles wide when the design allows |

Scale rules:

- Canvas size and subject size are NOT the same thing.
- An asset MUST NOT fill its entire canvas merely because more pixels are available.
- Related animation frames MUST preserve the same subject scale.
- Armor MUST NOT change Player height/footprint materially.
- Doors, gates, bridges, paths, and interaction props MUST look usable at Player scale.
- Boss identity MUST come from silhouette/detail/animation as well as scale.
- Values outside these guidelines MAY be used only for a deliberate design reason and SHOULD be documented in the asset contract.
- When an `APPROVED` Player/reference sheet exists, match that reference before relying on numeric ranges.

---

# 4. CAMERA, ORIENTATION, AND SORTING

## 4.1 Camera

The game MUST remain:

```text
2D
Top-down
Orthographic
```

Do NOT introduce:
- side view;
- perspective horizon;
- isometric projection;
- faux-isometric proportions;

unless explicitly approved.

## 4.2 Production perspective lock — HumanRealm v1

Until an `APPROVED` visual reference sheet exists, the following textual perspective lock is binding for generated `CANDIDATE` assets:

```yaml
projection: Orthographic
horizon: None
vanishing_points: None
ground_plane: Flat top-down gameplay plane
isometric_diamond_grid: Forbidden
perspective_convergence: Forbidden
top_surfaces: Primary
front_vertical_surfaces: Secondary and controlled
side_surfaces: Minimal unless required for readability
```

Production interpretation:

- Terrain MUST read as a flat top-down plane.
- Characters MUST read as top-down game sprites, NOT platformer/side-view characters.
- Props/buildings MAY expose a controlled front vertical face to communicate height, but the visible-face ratio MUST stay consistent within the same asset family.
- Roofs, tree canopies, rocks, chests, and large props MUST use the same projection logic.
- No asset may introduce horizon perspective or converging lines.
- No asset may use a `45° isometric` diamond-axis construction.
- A standalone asset whose viewing angle does not match approved HumanRealm assets MUST be rejected even if attractive in isolation.

Reference-sheet policy:

- `final_style_reference_sheet` remains an OPEN deliverable.
- Until it exists, these textual perspective rules are the source of truth.
- Once an `APPROVED` reference sheet exists, AI MUST match its visible-surface ratio and silhouette treatment while preserving the orthographic/no-isometric rules above.

## 4.3 Character facing

Player and character art SHOULD support the existing 8-direction gameplay where required.

All facings MUST preserve:
- body height;
- footprint;
- baseline;
- costume proportions;
- lighting logic.

## 4.4 Y-sorting

Assets MUST support top-down Y-based sorting.

Tall objects SHOULD be separable into visual layers when necessary.

Example:

```text
Tree
├── Trunk / Base
└── Canopy / AbovePlayer
```

The same logic MAY apply to:
- houses;
- arches;
- large rocks;
- signs;
- cave overhangs.

---

# 5. SHAPE LANGUAGE

## 5.1 General

Prefer:
- 1–3 major shape masses;
- one clear focal detail;
- readable silhouettes;
- controlled asymmetry;
- intentional clusters.

Avoid:
- uniform texture noise;
- excessive tiny details;
- silhouette ambiguity;
- detail that disappears at gameplay scale.

## 5.2 Friendly forms

Friendly or safe objects SHOULD use:
- softer curves;
- rounded shapes;
- stable symmetry;
- moderate contrast.

## 5.3 Dangerous forms

Dangerous enemies, boss forms, and hazards MAY use:
- sharper angles;
- spikes;
- asymmetry;
- stronger value contrast;
- more aggressive silhouettes.

Danger MUST NOT rely only on increasing sprite size.

---

# 6. HUMANREALM PALETTE

Status: **LOCKED FOR HUMANREALM VERTICAL SLICE v1**

This palette is the production baseline for HumanRealm from T24 onward until an explicit approved revision.
It is NOT a permanent palette for all six regions.
AI MUST NOT silently replace the anchor colors or redefine their semantic roles.
Small nearby variations MAY be introduced for material readability when they remain compatible with the anchor family.

The following colors define intended color roles for HumanRealm.

| Role | Color | Usage |
|---|---|---|
| Deep Outline | `#24333A` | deep contour, dark UI/icon detail |
| Cool Shadow | `#3E5360` | shadow, cave, occlusion |
| Grass Dark | `#39734A` | grass edge, bushes |
| Grass Mid | `#63A85B` | main grass |
| Grass Light | `#9BCB67` | natural highlight |
| Earth Dark | `#80533B` | dirt edge |
| Earth Mid | `#B8794A` | main dirt path |
| Earth Light | `#D7A463` | bright earth |
| Stone | `#7A8790` | rocks, stone structures |
| Stone Light | `#AAB3AE` | lit stone |
| Water Deep | `#28658A` | deep water/edge |
| Water Mid | `#3D94B5` | main water |
| Water Light | `#78C7C5` | water highlight |
| Wood | `#87533D` | wood, crates, village |
| Warm Accent | `#E5B84F` | loot, interaction accent |
| Danger | `#C94D4D` | damage, danger, boss warnings |
| Soul / Magic | `#8B70D1` | Soul, Soul Pillar, magic FX |
| UI Paper | `#F1E4C1` | light panel/text background |

## Palette rules

Environment colors SHOULD occupy most of the scene.

Accent colors MUST be used intentionally.

Avoid:
- uncontrolled neon colors;
- many near-duplicate colors;
- unnecessary color proliferation;
- highlights that compete with interaction or danger cues.

For the current HumanRealm vertical slice, the palette above is considered approved as **HumanRealm Palette v1**.
Future palette refinement requires an explicit revision rather than silent per-asset color drift.
A future global/cross-region palette system remains OPEN.

---

# 7. OUTLINE, LIGHTING, AND SHADING

## 7.1 Outline

Default:
- small sprites: usually `1 px`;
- large sprites/boss: up to `2 px` when justified.

Outline SHOULD use dark blue-gray rather than pure black.

Outline width and color MUST remain stable within the same asset family.

## 7.2 Lighting

Default key light:

```text
Direction: top-left
Temperature: slightly warm
Shadow direction: bottom-right
```

Outdoor HumanRealm:
- slightly warm ambient;
- fresh green environment;
- readable midtone contrast.

Caves:
- lower saturation;
- cooler shadows;
- stronger local contrast.

Boss scenes MAY use rim/accent lighting, but MUST NOT contradict the main lighting logic without explicit scene-specific approval.

## 7.3 Shading

Small sprites SHOULD use approximately:

```text
2–4 value bands
```

Use more bands only when needed for:
- bosses;
- large props;
- readable materials.

Material logic:

| Material | Rendering rule |
|---|---|
| Metal | sharper highlights, higher local contrast |
| Cloth | broader, softer clusters |
| Skin | controlled soft cluster transitions |
| Stone | irregular clustered planes |
| Foliage | grouped leaf masses, not random pixel noise |
| Slime | compact wet highlights |

Do NOT use:
- airbrush;
- smooth gradient shading;
- uncontrolled dithering;
- semi-transparent pseudo-pixel edges.

---

# 8. TERRAIN STANDARD

## 8.1 Terrain family

A terrain set SHOULD include applicable members:

```text
Base
Edges
InnerCorners
OuterCorners
Transitions
IsolatedPieces
Decorations
```

Terrain MUST tile cleanly.

Minimum QA:
- test repeating tile in `3x3`;
- inspect at native scale;
- inspect at gameplay camera scale;
- remove visible seams before production approval.

## 8.2 Tilemap separation

Preserve the current logical layer model:

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

Do NOT merge all visual/gameplay responsibilities into one Tilemap.

## 8.3 HumanRealm terrain

Default terrain:
- grass.

Supporting terrain:
- dirt paths;
- stone;
- water;
- cliffs;
- cave entrances;
- optional ruins.

Dirt paths SHOULD guide movement softly.

They MUST NOT force HumanRealm into a linear corridor structure.

---

# 9. ENVIRONMENT PROPS

## 9.1 Trees

Trees SHOULD support layered rendering when necessary:

```text
Tree
├── Base / Trunk
└── Canopy / AbovePlayer
```

Tree collision MUST remain visually understandable.

Canopy MUST NOT hide the collision footprint in a misleading way.

## 9.2 Rocks

Rocks SHOULD:
- have irregular silhouettes;
- use 2–3 readable value planes;
- preserve a clear footprint;
- avoid blocking important paths unless intentionally designed as obstacles.

## 9.3 Bushes, flowers, and grass decoration

Use:
- small controlled clusters;
- limited color counts;
- several reusable variants.

Do NOT distribute decoration as uniform random noise.

## 9.4 Secondary props

Examples:
- mushrooms;
- posts;
- barrels;
- fences;
- signs;
- crates.

These SHOULD contribute to:
- navigation;
- landmarks;
- storytelling;
- biome identity.

They MUST NOT create accidental gameplay collision.

---

# 10. HUMANREALM VILLAGE

Visual materials:
- wood;
- light stone;
- warm earth-colored roofs.

Village mood:
- safe;
- inhabited;
- readable;
- modest fantasy.

Buildings SHOULD use:
- simple readable facades;
- clear doors/windows;
- one primary focal detail;
- low-to-moderate texture density.

Tall roof/canopy elements MAY render above characters.

Walls and building bases MUST have readable collision footprints.

Village layout art MUST NOT visually imply that the region is strictly linear.

Gameplay text MUST NOT be baked into environment raster art.

---

# 11. PLAYER VISUAL CONTRACT

Player visual architecture is fixed:

```text
Player
├── Body
├── WeaponSprite
└── SlashVFX
```

## 11.1 Body

`Body` changes with armor.

Body art MUST preserve:
- anatomy;
- height;
- footprint;
- baseline;
- facing conventions;
- animation canvas.

## 11.2 WeaponSprite

`WeaponSprite` changes with equipped melee weapon.

Weapon art MUST:
- use a stable hand/origin relationship;
- remain separate from Body;
- remain reusable across compatible animations.

## 11.3 SlashVFX

Slash visual effects remain separate.

Slash VFX MUST NOT be permanently baked into:
- Body sprites;
- WeaponSprite textures.

## 11.4 Player readability

Player SHOULD contrast sufficiently with:
- grass;
- dirt;
- stone;
- water edges.

Soul-purple SHOULD NOT dominate the default player palette because it is reserved primarily for Soul/magic identity.

---

# 12. ENEMY VISUAL CONTRACT

HumanRealm currently prioritizes Rank:

```text
F
E
```

Enemy rank remains gameplay data.

Art MAY communicate danger through:
- silhouette;
- animation;
- contrast;
- focal marks;
- size within reason.

Art MUST NOT silently redefine rank balance.

## 12.1 Rank readability

Rank F SHOULD generally use:
- simpler shapes;
- fewer details;
- restrained contrast.

Elite/boss enemies MAY increase:
- silhouette complexity;
- focal marks;
- value contrast;
- visual mass.

Do NOT create elite/boss identity by merely scaling a normal enemy sprite.

## 12.2 Existing examples

### Slime

Visual language:
- low rounded body;
- readable squish;
- wet highlight;
- simple silhouette.

### Bat

Visual language:
- strong wing silhouette;
- visually light body;
- readable against terrain.

New enemies SHOULD be distinguishable at thumbnail scale.

---

# 13. BOSS VISUAL CONTRACT

Recommended boss canvas:

```text
96x96+
```

Bosses MUST have:
- a strong silhouette;
- readable combat footprint;
- at least one persistent identity feature.

Examples of identity features:
- mask;
- horns;
- signature weapon;
- colored core;
- unique head shape.

The identity feature MUST remain recognizable across:
- idle;
- movement;
- attack;
- hurt;
- death.

Telegraph FX MAY use:
- Danger red;
- Soul/magic purple;
- warm accent where appropriate.

FX MUST NOT obscure the boss silhouette for extended periods.

---

# 14. WEAPON STANDARD

Current production scope:

```text
Melee Slash Only
```

AI MUST NOT introduce new weapon classes unless explicitly approved.

Do NOT add:
- bow;
- staff;
- gun;
- magic weapon class;
- spear as a new weapon class;

during the current scope.

Weapon readability comes from:
- blade silhouette;
- handle;
- length;
- proportion;
- slash behavior.

WeaponSprite MUST retain stable pivot/origin alignment.

Slash FX MUST remain separate.

---

# 15. ARMOR STANDARD

Armor modifies:

```text
Player Body visual
+
Stats handled by game data
```

Armor MUST preserve:
- player height;
- player anatomy;
- animation footprint;
- baseline;
- canvas standard.

Armor SHOULD differentiate through:
- material;
- silhouette additions;
- controlled accent color;
- focal detail.

Armor MUST NOT require unique body+weapon combinations for every weapon.

---

# 16. ITEM STANDARD

World items and UI icons SHOULD share:
- recognizable silhouette;
- role color;
- family identity.

Important items MAY use:
- warm accent;
- Soul accent;
- stronger outline contrast.

Small icons SHOULD remain readable below approximately `24x24 px`.

Avoid micro-details that vanish at final display size.

---

# 17. CHEST STANDARD

Chest MUST clearly communicate:
- reward;
- interactability;
- closed/open state.

Closed and opened versions MUST preserve:
- canvas;
- footprint;
- pivot;
- scale.

A glow MAY be used but MUST NOT overwhelm the chest shape.

---

# 18. SOUL PILLAR STANDARD

Soul Pillar is a major progression landmark.

Core visual identity:

```text
Stone structure
+
Purple Soul core
+
Controlled pixel glow
```

It MUST remain recognizable in:
- grassland;
- village;
- cave.

States MAY include:
- inactive;
- active;
- used.

State changes SHOULD primarily use:
- accent;
- FX;
- controlled emissive treatment.

State changes MUST NOT silently alter gameplay footprint/collision.

---

# 19. UI ICON FAMILY

All primary game icons SHOULD belong to one visual family.

Shared properties:
- clear silhouette;
- dark outline;
- 2–3 value bands;
- consistent canvas;
- consistent padding;
- role-based color.

Required icon categories include:

```text
Vitality
Attack
Defense
Agility
Rage
Soul
Weapon
Armor
Chest
TeleportStone
```

Possible UI states:

```text
Normal
Selected
Disabled
Warning
```

Text, numbers, tooltips, and accessibility labels MUST remain code/UI-native rather than rasterized into icon artwork.

---

# 20. ANIMATION STANDARD

## 20.1 Locked properties

Within one animation family, lock:

```yaml
canvas_size: fixed
baseline: fixed
body_scale: fixed
center: fixed
pivot: fixed
facing: fixed per sequence
palette: fixed
costume_proportions: fixed
key_light: fixed
```

## 20.2 Quality rules

MUST NOT contain:
- frame drift;
- body scale drift;
- foot sliding;
- floating ground contact;
- inconsistent lighting;
- accidental palette shifts.

Animation MAY use controlled:
- squash;
- stretch;
- anticipation;
- recoil.

These effects MUST NOT break:
- contact point;
- gameplay readability;
- collision expectation.

## 20.3 Validation

Before approval:
1. inspect contact sheet;
2. preview with nearest-neighbor scaling;
3. preview in Unity;
4. test at actual camera scale.

---

# 21. SPRITE-SHEET STANDARD

A sprite sheet SHOULD contain:

```text
One asset family
One cell size
One slicing rule
One documented frame order
```

Frame order defaults to:

```text
left → right
top → bottom
```

unless a manifest explicitly says otherwise.

## Import rules

```yaml
TextureType: Sprite (2D and UI)
SpriteMode: Single or Multiple as required
PixelsPerUnit: 32
FilterMode: Point
Compression: None
MipMaps: Off by default
ReadWrite: Off unless runtime pixel access is required
```

Fixed-grid sprite sheets MUST use fixed-grid slicing.

Do NOT use Automatic Slice when a known grid is available.

Every frame MUST use:
- identical canvas;
- identical alpha policy;
- documented pivot;
- consistent baseline.

---

# 22. NAMING STANDARD

Use English names.

Avoid ambiguous names.

Do NOT use:

```text
final
final2
new
new2
test
thing
sprite1
```

unless clearly marked as temporary.

## Recommended pattern

```text
<Region>_<Category>_<Subject>_<Variant>_<Action>[_<Facing>]
```

Examples:

```text
HumanRealm_Environment_Grass_Base.png
HumanRealm_Environment_Dirt_Transition_North.png
Player_Armor_Training_Body_Idle_S.png
Player_Weapon_BasicSword_Slash_E.png
Enemy_Slime_F_Move_S.png
Boss_HumanRealm_MainBoss_Idle_S.png
UI_Icon_Stat_Vitality.png
```

Related:
- ScriptableObject;
- prefab;
- animation;
- source art;

SHOULD share a recognizable stem.

Example:

```text
Slime_F
TrainingArmor
BasicSword
```

Temporary content MUST contain:

```text
Prototype
```

or

```text
Placeholder
```

when appropriate.

---

# 23. FOLDER STANDARD

Game-owned assets belong under:

```text
Assets/_Game/
```

Recommended structure:

```text
Assets/_Game/
├── Art/
│   ├── Characters/
│   │   ├── Player/
│   │   ├── NPCs/
│   │   ├── Enemies/
│   │   └── Bosses/
│   ├── Environment/
│   │   ├── Shared/
│   │   └── HumanRealm/
│   │       ├── Tilesets/
│   │       ├── Nature/
│   │       ├── Props/
│   │       └── Buildings/
│   ├── Items/
│   │   ├── Weapons/
│   │   ├── Armor/
│   │   └── Icons/
│   ├── UI/
│   ├── VFX/
│   └── Placeholders/
├── Animations/
│   ├── Player/
│   ├── NPCs/
│   ├── Enemies/
│   ├── Bosses/
│   └── Environment/
├── Tilemaps/
│   ├── HumanRealm/
│   ├── Palettes/
│   ├── Tiles/
│   └── RuleTiles/
├── Data/
│   ├── Enemies/
│   ├── Weapons/
│   ├── Armor/
│   ├── Bosses/
│   ├── Loot/
│   └── Progression/
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── NPCs/
│   ├── Bosses/
│   ├── Items/
│   ├── World/
│   ├── UI/
│   └── Progression/
└── Scenes/
```

Production and placeholder art MUST remain separated.

Do NOT modify:

```text
Library/
Temp/
Logs/
UserSettings/
```

as part of normal asset production.

---

# 24. ASSET GENERATION CONTRACT

Before generating a new asset, define:

```yaml
asset_id:
region:
category:
subject:
purpose:
production_status: Placeholder | Prototype | Candidate | Approved
pixel_style: true
canvas_size:
frame_size:
frame_count:
facing_count:
pivot:
ppu: 32
filter: Point
compression: None
lighting_direction: top-left
outline_rule:
palette_family:
animation_actions:
unity_destination:
special_constraints:
```

AI SHOULD NOT begin batch generation until required fields are known.

For uncertain fields, write:

```text
OPEN
```

instead of silently inventing a permanent project standard.

## 24.1 AI production pipeline — HumanRealm v1

The default production pipeline is now LOCKED for the HumanRealm vertical slice:

```text
ART_BIBLE
→ Task-specific Asset Contract
→ Reuse APPROVED visual references when available
→ Generate ONE base CANDIDATE
→ Perspective / silhouette / scale / palette review
→ Pixel cleanup and transparency cleanup
→ Native-scale review
→ Unity import + slicing/pivot/import validation
→ Real HumanRealm context test
→ CANDIDATE approval decision
→ Mark APPROVED only after checklist passes
→ Expand into variants / directions / animation family
```

### Candidate-first rule

AI MUST NOT begin large batch production before a base candidate is approved.

For character/enemy animation families:

```text
1. Create one canonical idle/base frame.
2. Validate scale, anatomy, silhouette, palette, lighting, and pivot.
3. Approve the base frame.
4. Create required facing reference frames.
5. Validate facing consistency.
6. Only then generate full animation sequences.
```

Do NOT generate an entire `8-direction × many-frame` sheet as the first production attempt unless an approved family reference already exists.

For terrain families:

```text
1. Create base tile.
2. Create one representative edge.
3. Create one representative corner/transition.
4. Run 3x3 tiling and seam test.
5. Approve the visual language.
6. Expand to the full tile family.
```

For props/environment families:

```text
1. Approve one family anchor asset.
2. Reuse its perspective, outline, palette, material rendering, and detail density.
3. Generate variants only after the anchor is accepted.
```

### Reference policy

- Only `APPROVED` assets are authoritative style references.
- `CANDIDATE` assets MAY be compared, but MUST NOT silently become new style anchors.
- If no approved reference exists, generated output remains `CANDIDATE` and MUST follow the textual rules in this document.
- AI SHOULD state which approved references were used.

## 24.2 AI image-generation hard constraints

For standalone sprites unless a task explicitly says otherwise:

- background MUST be transparent;
- no baked terrain patch or floor circle;
- no text, watermark, logo, labels, UI frame, or presentation border;
- no soft anti-aliased halo around alpha edges;
- no drop shadow baked outside the intended sprite unless the asset contract requires it;
- no perspective background or scenery;
- subject MUST fit fully inside the requested canvas with deliberate padding;
- one asset per canvas unless the task explicitly requests a sprite sheet/contact sheet;
- do not upscale/downscale with smooth interpolation;
- do not invent extra accessories that change gameplay identity;
- do not clip weapon tips, ears, wings, horns, canopy edges, or other silhouette-critical features;
- keep palette, outline, key light, and pixel cluster logic compatible with HumanRealm v1.

For animation/sprite-sheet generation:

- every cell MUST use identical dimensions;
- subject baseline and scale MUST remain fixed;
- background MUST remain transparent;
- no labels may be baked into production cells;
- frame order MUST match the declared manifest/contract;
- spacing/padding MUST be documented when present.

For tilesets:

- tile edges MUST be designed for exact integer-grid reuse;
- decorative elements MUST NOT accidentally cross tile boundaries unless intentionally part of the tile system;
- standalone preview backgrounds MUST NOT be baked into production tile textures.

---

# 25. ASSET APPROVAL CHECKLIST

An asset is NOT production-ready until all applicable checks pass.

## 25.1 Art direction

- [ ] Correct top-down view.
- [ ] Correct HumanRealm role/style.
- [ ] Silhouette readable at native scale.
- [ ] Silhouette readable at thumbnail scale.
- [ ] Palette compatible.
- [ ] Outline consistent.
- [ ] Shading consistent.
- [ ] Detail density consistent.
- [ ] Top-left lighting respected.

## 25.2 Scale

- [ ] Correct canvas/cell size.
- [ ] PPU = 32.
- [ ] No non-integer production scaling.
- [ ] Baseline stable.
- [ ] Pivot correct.
- [ ] Footprint stable.

## 25.3 Animation

- [ ] No frame drift.
- [ ] No body-scale drift.
- [ ] No foot sliding.
- [ ] No accidental palette drift.
- [ ] No lighting-direction drift.

## 25.4 Terrain

- [ ] 3x3 repetition test passed.
- [ ] No visible seams.
- [ ] Transition/corner family complete when required.
- [ ] Collision meaning is visually clear.

## 25.5 Context

- [ ] Tested against real HumanRealm background.
- [ ] Sorting behavior is correct.
- [ ] Does not hide critical gameplay information.
- [ ] Decoration does not create false collision.

## 25.6 Unity import

- [ ] Texture Type correct.
- [ ] Sprite Mode correct.
- [ ] Grid slicing correct.
- [ ] Point filtering enabled.
- [ ] Compression disabled.
- [ ] Alpha clean.
- [ ] Mipmaps disabled unless justified.

## 25.7 File hygiene

- [ ] Naming convention followed.
- [ ] Folder convention followed.
- [ ] Placeholder/production status clear.
- [ ] Source/generation provenance recorded when required.

---

# 26. APPROVAL STATES

Use one of these states:

```text
PLACEHOLDER
PROTOTYPE
CANDIDATE
APPROVED
REJECTED
```

Meaning:

| State | Meaning |
|---|---|
| PLACEHOLDER | temporary functional art |
| PROTOTYPE | used for experimentation |
| CANDIDATE | visually plausible, awaiting QA |
| APPROVED | production-ready |
| REJECTED | failed project standards |

Only `APPROVED` assets SHOULD be treated as visual references for future production assets.

---

# 27. OPEN DECISIONS

The following are NOT final.

AI MUST NOT convert them into permanent rules without approval.

```yaml
humanrealm_palette_v1: LOCKED_FOR_VERTICAL_SLICE
humanrealm_production_generation_pipeline_v1: LOCKED
global_cross_region_palette: OPEN
final_style_reference_sheet: OPEN
final_native_viewport: OPEN
final_camera_framing: OPEN
final_environment_detail_density: OPEN
full_humanrealm_enemy_roster: OPEN
humanrealm_optional_boss_visuals: OPEN
humanrealm_main_boss_visuals: OPEN
exact_animation_frame_counts: OPEN
asset_manifest_format: OPEN
external_asset_license_policy: OPEN
```

---

# 28. QUICK AI REFERENCE

For routine asset work, AI may use this section as the fast path.

```yaml
PROJECT: HexaRealm
REGION: HumanRealm
STYLE: 2D top-down fantasy pixel art
CAMERA: Orthographic
PERSPECTIVE_LOCK: Top-down, no horizon, no vanishing points, no isometric diamond grid
BASE_TILE: 32x32
PPU: 32
FILTER: Point
COMPRESSION: None
CHARACTER_PIVOT: Bottom Center
KEY_LIGHT: Top-left
SCALE_ANCHOR: Player = 1.0x visual mass
PLAYER_FRAME: 48x48 or 64x64
SMALL_ENEMY: 32x32
MEDIUM_ENEMY: 48x48
ELITE: 48x48 or 64x64
BOSS: 96x96+
PLAYER_VISUAL:
  - Body
  - WeaponSprite
  - SlashVFX
CURRENT_WEAPON_SCOPE: Melee Slash Only
HUMANREALM_ENEMY_PRIORITY:
  - F
  - E
TERRAIN_DEFAULT: Grass
HUMANREALM_PALETTE_STATUS: Locked v1 for current vertical slice
AI_PIPELINE: Candidate-first, approve anchor before batch/family generation
PRIMARY_ACCENTS:
  interaction_reward: Warm Gold
  danger: Red
  soul_magic: Purple
PRODUCTION_ROOT: Assets/_Game/
```

Before creating or modifying any production asset:

```text
1. Read ART_BIBLE.md for visual production rules.
2. Read only relevant GAME_DESIGN_CORE.md sections for gameplay/lore intent.
3. Define the task-specific Asset Generation Contract.
4. Reuse APPROVED references when available.
5. Create ONE base CANDIDATE before batch generation.
6. Validate perspective, scale, silhouette, palette, and pixel quality.
7. Clean alpha/background and validate at native scale.
8. Import/test in real Unity/HumanRealm context.
9. Run the approval checklist.
10. Mark approval state and report OPEN decisions/conflicts.
11. Expand to variants/animation family only after the anchor candidate is approved.
```
