# Get Bricked

`Get Bricked` is an early Unity 6 prototype for a synthwave arcade brick-breaker with run-based roguelite structure. The current build is already playable end to end: you can launch from a main menu, tune a seeded run, clear procedural stages, catch helpful or harmful capsules, draft permanent upgrades between stages, and play until game over.

## At A Glance

- Engine: Unity `6000.3.6f1`
- Render path: URP with 2D renderer
- Enabled build scenes: `1` (`Assets/Scenes/SampleScene.unity`)
- Scene structure: the playable board is runtime-generated, not prefab-driven yet
- Current content counts:
  - `4` authored level profiles
  - `6` brick definitions
  - `16` pickup definitions
  - `6` permanent run upgrades
  - `3` themes
- Current UI state: runtime OnGUI menus, HUD, pause, upgrade draft, and end-state flow
- Current automation: starter Unity Edit Mode tests plus repo-local compile/test helper scripts

## What Is Implemented Right Now

- Runtime main menu with quick start, run setup access, and reset-to-defaults flow
- Run setup with seed entry, difficulty presets, modifier tuning, drop-pool filtering, Capsule Party, and theme selection
- Seeded multi-stage progression with deterministic procedural layouts
- Pause, restart, return-to-setup, and return-to-menu flows
- One paddle, one or more active balls, lives, serve/reset flow, and game-over handling
- Timed power-ups and power-downs from brick drops
- Spinning breakable bricks that wake up on impact, ricochet the ball at odd angles, and can have their rotation restricted by neighboring bricks
- Deterministic `pick 1 of 3` permanent upgrade drafts between cleared stages
- Data-driven brick, level, pickup, upgrade, and theme content through ScriptableObjects
- SVG-backed gameplay sprites plus rotating background art from `Resources/Backgrounds`
- Bloom-backed glow on balls and pickups, with crisp unlit paddle and brick rendering
- A first layer of Unity Edit Mode coverage around gameplay modifier behavior

## Core Loop

1. Start from the runtime main menu.
2. Open `Run Setup` to pick a seed, difficulty, modifiers, and theme.
3. Launch into a seeded stage with one or more serve balls.
4. Break bricks, manage lives, and catch or dodge falling capsules.
5. Clear the stage objective to open a deterministic `1 of 3` upgrade draft.
6. Pick one permanent upgrade, advance to the next stage, and keep going until game over or the current stage set ends.

## Controls

- `A/D` or `Left/Right`: move the paddle during gameplay
- `Space`, `Enter`, or `Numpad Enter`: launch the ball, confirm menus, confirm upgrade picks, relaunch a sticky ball, or fire lasers while `Laser Paddle` is active
- `Esc` or `P`: pause and resume active gameplay
- `R`: abandon the current run and return to run setup
- `Up/Down`: navigate menu selections and adjust manual ball speed during gameplay
- Run setup only:
  - `0-9`: edit the seed
  - `Backspace`: delete a seed digit
  - `T`: randomize the seed
  - `N`: reset setup defaults

## Game Mechanics

### Run Setup And Build Shaping

The setup screen already supports these run-shaping inputs:

| Setting | Current implementation |
| --- | --- |
| Seed / Tape ID | Numeric seed. The same seed plus the same draft picks reproduces layout patterns, launch rolls, drop rolls, and draft offers. |
| Difficulty | `Casual`, `Standard`, `Brutal` |
| Balls Per Serve | `1` to `4` before permanent upgrades |
| Paddle Width Bias | `-2` to `+2` setup steps |
| Ball Speed Bias | `-2` to `+2` setup steps |
| Brick Durability Bias | `-2` to `+2` setup steps |
| Drop Pool | `Mixed`, `Helpful Only`, `Harmful Only`, `Disabled` |
| Capsule Party | Forces every eligible brick to drop a capsule |
| Theme | `Classic`, `Neon Forge`, `Sunset Circuit` |

Difficulty presets already change more than labels:

- `Casual`: +1 starting life, wider paddle, slower ball, softer bricks, more drops
- `Standard`: baseline tuning
- `Brutal`: -1 starting life, narrower paddle, faster ball, tougher bricks, fewer drops

### Stage Generation And Progression

- The project has `4` authored level profiles that act as progression templates:
  - `Opening Volley`
  - `Crossfire`
  - `Fortress Gate`
  - `Pressure Test`
- Runtime generation remixes those templates into seeded stages using pattern families like bands, diamonds, steps, lattice, core, and columns.
- Generated boards can be mirrored or asymmetric, can row-shift horizontally, and can assign moving-brick behavior procedurally.
- Later stages ramp pressure by increasing density, introducing tougher brick types like spinner bricks, unlocking more drop types, and using more moving bricks.
- Some stages use `clear all required bricks`; later pressure stages can switch to `reach target score`.
- Clearing a stage advances the run and attempts to open a deterministic upgrade draft before the next stage loads.

### Balls, Lives, And Scoring

- Lives are active and are affected by setup difficulty plus permanent upgrade picks.
- The run only spends a life when the last active ball is lost. Multi-ball does not punish you until every ball is gone.
- `Balls Per Serve` applies on every fresh serve, not just the opening launch.
- `Shield Wall` can rescue a falling ball before it becomes a life loss.
- Ball speed is not only feel tuning; brick score payout scales with current ball speed.
- Manual speed tuning exists during gameplay through `Up/Down`, and the HUD includes a speed meter.

### Drop And Effect Rules

- Brick drops are data-driven per brick definition.
- The drop pool can be filtered to helpful-only, harmful-only, mixed, or fully disabled.
- Re-catching the same timed effect extends its timer instead of replacing it.
- Opposing timed modifiers can coexist and combine multiplicatively where it makes sense.
- Special-case effects already work in gameplay:
  - sticky catches and manual relaunch
  - laser volleys on demand
  - phase-through breakable bricks
  - chain-lightning follow-up hits
  - split-paddle center gap checks
  - gravity well ball bending
  - fog-based visibility reduction
  - lag-spike control disruption

### Permanent Builds

- After each cleared stage, the game generates a deterministic `1 of 3` upgrade draft from the current run seed, cleared-stage count, and prior picks.
- Picked upgrades apply immediately and persist for the rest of the run.
- The current permanent build space supports these long-term directions:
  - wider paddle coverage
  - faster permanent ball speed
  - higher drop frequency
  - extra lives
  - extra balls on every serve
  - a persistent mild wavy-paddle modifier

## Content Catalog

### Level Profiles

| Level profile | Base traits | Authored objective |
| --- | --- | --- |
| `Opening Volley` | Baseline speed and paddle tuning | Clear required bricks |
| `Crossfire` | Slightly faster ball, introduces steel bricks and authored brick motion | Clear required bricks |
| `Fortress Gate` | Faster ball, slightly faster paddle, more motion pressure | Clear required bricks |
| `Pressure Test` | Fastest authored template, includes explosive bricks | Reach target score |

### Brick Types

| Brick | HP | Score | Behavior | Current drop pool |
| --- | --- | --- | --- | --- |
| `Basic Brick` | `1` | `100` | Standard breakable starter brick | `Wide Paddle`, `Slow Ball`, `Multi-Ball`, `Narrow Paddle`, `Wavy Paddle`, `Sticky Paddle`, `Shield Wall` |
| `Reinforced Brick` | `2` | `175` | Tougher breakable mid-tier brick | `Wide Paddle`, `Slow Ball`, `Fast Ball`, `Multi-Ball`, `Narrow Paddle`, `Wavy Paddle`, `Reverse Controls`, `Split Paddle`, `Lag Spike` |
| `Fortified Brick` | `3` | `250` | High-durability breakable brick | `Fast Ball`, `Multi-Ball`, `Wide Paddle`, `Narrow Paddle`, `Slow Ball`, `Wavy Paddle`, `Phase Ball`, `Gravity Well`, `Fog of War` |
| `Spinner Brick` | `2` | `225` | Breakable rotor brick that starts spinning when hit, is rotation-anchored at its center, and kicks the ball into stranger ricochet angles while nearby bricks can physically limit its spin | `Wide Paddle`, `Slow Ball`, `Fast Ball`, `Reverse Controls`, `Split Paddle` |
| `Explosive Brick` | `1` | `250` | Breakable brick with an explosion burst and temporary speed boost on direct impact kills | `Narrow Paddle`, `Slow Ball`, `Wavy Paddle`, `Chain Lightning`, `Laser Paddle` |
| `Steel Brick` | Indestructible | `0` | Obstacle brick that does not count toward completion | None |

### Helpful Capsules

| Capsule | Duration | What it does |
| --- | --- | --- |
| `Wide Paddle` | `12s` | Multiplies paddle width by `1.45` |
| `Slow Ball` | `10s` | Multiplies ball speed by `0.78` |
| `Multi-Ball` | Instant | Spawns `2` extra balls from an active ball |
| `Sticky Paddle` | `15s` | The first ball caught on the paddle sticks until you relaunch it |
| `Laser Paddle` | `15s` | `Space` fires two upward brick hits from the paddle |
| `Shield Wall` | Instant | Adds a bottom-edge rescue charge |
| `Phase Ball` | `12s` | Lets balls travel through breakable bricks while still damaging them |
| `Chain Lightning` | `14s` | Destroyed bricks arc follow-up damage into nearby breakable bricks |

### Harmful Capsules

| Capsule | Duration | What it does |
| --- | --- | --- |
| `Narrow Paddle` | `10s` | Multiplies paddle width by `0.72` |
| `Fast Ball` | `10s` | Multiplies ball speed by `1.28` |
| `Wavy Paddle` | `12s` | Adds a disruptive paddle sway/drift effect |
| `Reverse Controls` | `8s` | Inverts paddle movement input |
| `Split Paddle` | `12s` | Opens a center gap that balls can slip through |
| `Gravity Well` | `12s` | Pulls balls toward the arena midpoint |
| `Fog of War` | `10s` | Reduces brick and pickup visibility |
| `Lag Spike` | `8s` | Intermittently stalls paddle response |

### Upgrade Draft Pool

| Upgrade | Stack cap | Permanent effect |
| --- | --- | --- |
| `Wide Loader` | `3` | Paddle width x`1.15` |
| `Afterburn Coil` | `4` | Ball speed x`1.08` |
| `Lucky Circuit` | `3` | Drop chance x`1.20` |
| `Repair Stock` | `2` | `+1` life immediately |
| `Split Serve` | `2` | `+1` extra ball on every serve |
| `Flux Line` | `1` | Paddle width x`1.08` plus permanent mild wave strength |

### Themes

- `Classic`
- `Neon Forge`
- `Sunset Circuit`

Theme selection already changes the runtime palette across background, walls, paddle, bricks, pickups, ball, and UI chrome.

## Project Structure Notes

- The only enabled build scene is [`Assets/Scenes/SampleScene.unity`](Assets/Scenes/SampleScene.unity), but gameplay objects are spawned at runtime by the bootstrap/controller path.
- Most durable game data currently lives under `Assets/Resources/` as ScriptableObject content.
- There are still no prefabs and no `.asmdef` files yet.
- The best source files to inspect for gameplay behavior are:
  - `Assets/Scripts/Gameplay/BreakoutGameController.cs`
  - `Assets/Scripts/Gameplay/BreakoutLevelPlanner.cs`
  - `Assets/Scripts/Gameplay/BreakoutPowerUpService.cs`
  - `Assets/Scripts/Gameplay/BreakoutRunState.cs`
  - `Assets/Scripts/Gameplay/BreakoutUpgradeDraftService.cs`

## Keeping This README Useful

This file should stay aligned with the real playable state of the repo. If gameplay systems, controls, content pools, setup options, stage profiles, pickups, upgrades, themes, or presentation layers change, update this README in the same pass so someone can understand the current game without reading every script first.
