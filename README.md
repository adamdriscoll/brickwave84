# Brickwave '84

`Brickwave '84` is a work-in-progress Unity 6 game: a synthwave arcade brick-breaker with run-based roguelite structure. The current build is playable end to end: you can launch from a main menu, tune a seeded run, clear procedural stages, catch helpful or harmful capsules, draft permanent upgrades between stages, and play until game over.

## At A Glance

- Engine: Unity `6000.3.6f1`
- Render path: URP with 2D renderer
- Enabled build scenes: `1` (`Assets/Scenes/SampleScene.unity`)
- Scene structure: the playable board is runtime-generated, not prefab-driven yet
- Current content counts:
  - `4` authored level profiles
  - `9` brick definitions
  - `28` pickup definitions
  - `6` permanent run upgrades
  - `3` themes
  - `3` authored Rogue paddle definitions (alternate types are currently disabled)
- Current UI state: runtime OnGUI menus, HUD, pause, upgrade draft, and end-state flow
- Current automation: starter Unity Edit Mode tests plus repo-local compile/test helper scripts

## What Is Implemented Right Now

- Runtime main menu with quick start, run setup access, and reset-to-defaults flow
- Run setup with seed entry, difficulty presets, score mode selection, modifier tuning, drop-pool filtering, Capsule Party, and theme selection
- Seeded multi-stage progression with deterministic procedural layouts
- Pause, restart, return-to-setup, and return-to-menu flows
- Default Rogue paddle tuning, one or more active balls, lives, serve/reset flow, and game-over handling
- Timed power-ups and power-downs from brick drops
- Tiny high-value bricks that read as precision targets instead of standard filler
- Falling capsules now spin as they drop for a little more arcade energy
- Spinning breakable bricks that wake up on impact, ricochet the ball at odd angles, and can have their rotation restricted by neighboring bricks
- Deterministic `pick 1 of 3` permanent upgrade drafts between cleared stages
- Data-driven brick, level, pickup, upgrade, and theme content through ScriptableObjects
- SVG-backed gameplay sprites plus rotating background art from `Resources/Backgrounds`, including dedicated pickup silhouettes for the active capsule set
- Bloom-backed glow on balls and pickups, with crisp unlit paddle and brick rendering
- Dynamic playfield scanlines that roll across the game area while the cabinet chrome and backdrop stay subtle
- A first layer of Unity Edit Mode coverage around gameplay modifier behavior

## Core Loop

1. Start from the runtime main menu.
2. Open `Run Setup` to pick a seed, difficulty, score mode, modifiers, and theme.
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
| Score Mode | `Classic`, `High Score` |
| Balls Per Serve | `1` to `4` before permanent upgrades |
| Paddle Width Bias | `-2` to `+2` setup steps |
| Ball Speed Bias | `-2` to `+2` setup steps |
| Brick Durability Bias | `-2` to `+2` setup steps |
| Drop Pool | `Mixed`, `Helpful Only`, `Harmful Only`, `Disabled` |
| Capsule Party | Forces every eligible brick to drop a capsule, and fresh setup resets now default it to `On` |
| Theme | `Classic`, `Neon Forge`, `Sunset Circuit` |

Rogue mode currently uses fixed default paddle tuning. Alternate Rogue paddle definitions remain in code for possible later reactivation, but they are not unlocked, selectable, or shown in the player-facing progression flow.

Difficulty presets already change more than labels:

- `Casual`: +1 starting life, wider paddle, slower ball, softer bricks, more drops
- `Standard`: baseline tuning
- `Brutal`: -1 starting life, narrower paddle, faster ball, tougher bricks, fewer drops, and the full brick/drop ecosystem is in play from stage 1

### Stage Generation And Progression

- The project has `4` authored level profiles that act as progression templates:
  - `Opening Volley`
  - `Crossfire`
  - `Fortress Gate`
  - `Pressure Test`
- Runtime generation remixes those templates into seeded stages using pattern families like bands, diamonds, steps, lattice, core, and columns.
- Generated boards can be mirrored or asymmetric, can row-shift horizontally, and can assign moving-brick behavior procedurally.
- Later stages ramp pressure by increasing density, introducing more movers, and leaning harder on the tougher brick/drop mixes already available in `Brutal` from stage 1.
- Some stages use `clear all required bricks`; later pressure stages can switch to `reach target score`.
- Clearing a stage advances the run and attempts to open a deterministic upgrade draft before the next stage loads.

### Balls, Lives, And Scoring

- Lives are active and are affected by setup difficulty plus permanent upgrade picks.
- The run only spends a life when the last active ball is lost. Multi-ball does not punish you until every ball is gone.
- `High Score` mode keeps score climbing from brick breaks, subtracts a fixed penalty every time a ball is lost, never ends the run on life depletion, and can go negative.
- `Balls Per Serve` applies on every fresh serve, not just the opening launch.
- `Shield Wall` can rescue a falling ball before it becomes a life loss.
- Ball speed is not only feel tuning; brick score payout scales with current ball speed.
- Combo score hooks now reward `Slam Chain` rapid breaks, `Bank Shot` ricochet finishes, and `Party Split` back-to-back multi-ball kills, with bonus point popups floating up from the broken brick.
- Manual speed tuning exists during gameplay through `Up/Down`, and the HUD includes a speed meter.

### Drop And Effect Rules

- Brick drops are data-driven per brick definition.
- The drop pool can be filtered to helpful-only, harmful-only, mixed, or fully disabled.
- Re-catching the same timed effect now increases that effect's stack count and adds another full base duration onto its timer, while the HUD/banner mark duplicate stacks with `xN`.
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
| `Tiny Brick` | `1` | `325` | Quarter-scale breakable precision brick that is rarer in procedural mixes and pays out extra score for the smaller hitbox | `Wide Paddle`, `Slow Ball`, `Multi-Ball`, `Narrow Paddle`, `Wavy Paddle`, `Sticky Paddle`, `Shield Wall` |
| `Spinner Brick` | `2` | `225` | Breakable rotor brick that starts spinning when hit, is rotation-anchored at its center, and kicks the ball into stranger ricochet angles while nearby bricks can physically limit its spin | `Wide Paddle`, `Slow Ball`, `Fast Ball`, `Reverse Controls`, `Split Paddle` |
| `Jelly Block` | `2` | `190` | Breakable squish brick that slows the ball briefly on contact and adds a wobble read to the impact | `Slow Ball`, `Sticky Paddle`, `Multi-Ball`, `Score Surge`, `Mega Ball` |
| `Split Brick` | `2` | `175` | Breakable brick that splits into `Tiny Brick` pieces when destroyed | `Wide Paddle`, `Slow Ball`, `Fast Ball`, `Multi-Ball`, `Mondo Multi`, `Bogus Multi`, `Mega Ball` |
| `Explosive Brick` | `1` | `250` | Breakable brick with an explosion burst and temporary speed boost on direct impact kills | `Narrow Paddle`, `Slow Ball`, `Wavy Paddle`, `Chain Lightning`, `Laser Paddle` |
| `Steel Brick` | Indestructible | `0` | Obstacle brick that does not count toward completion | None |

### Unlock Progression

Permanent run upgrades are always available in the between-stage draft pool. Drop unlocks enter the Neon Ladder ecosystem by default status or rarity gate: `Common` at Heat `01`, `Uncommon` at Heat `08`, `Rare` at Heat `18`, and `Epic` at Heat `32`. Default drops start unlocked for Neon Ladder runs. Implementation status tracks whether the item has live runtime gameplay behavior or is currently only a progression-screen placeholder.

| Name | Description | Default / unlock level | Rarity | Polarity | Type | Implementation |
| --- | --- | --- | --- | --- | --- | --- |
| `Multi-Ball` | +2 balls from an active ball. | Default | Common | Helpful | Multi-ball | Implemented |
| `Shield Wall` | Adds 1 bottom-edge rescue charge. | Default | Common | Helpful | Shield | Implemented |
| `Slow Ball` | Ball speed x0.78 for 10s. | Default | Common | Helpful | Ball speed | Implemented |
| `Wide Paddle` | Paddle width x1.45 for 12s. | Default | Common | Helpful | Paddle width | Implemented |
| `Fast Ball` | Ball speed x1.28 for 10s. | Heat 01 | Common | Hazard | Ball speed | Implemented |
| `Narrow Paddle` | Paddle width x0.72 for 10s. | Heat 01 | Common | Hazard | Paddle width | Implemented |
| `Wavy Paddle` | Adds paddle sway for 12s. | Heat 01 | Common | Hazard | Paddle drift | Implemented |
| `Bogus Tape` | Looks helpful, then rolls a random hazard. | Heat 08 | Uncommon | Hazard | Disguised hazard | Implemented |
| `Fog of War` | Reduces brick and pickup visibility for 10s. | Heat 08 | Uncommon | Hazard | Visibility | Implemented |
| `Lag Spike` | Intermittently stalls paddle response for 8s. | Heat 08 | Uncommon | Hazard | Input lag | Implemented |
| `Laser Paddle` | Enables paddle laser fire for 15s. | Heat 08 | Uncommon | Helpful | Laser paddle | Implemented |
| `Mondo Multi` | Multiplies active timed effects by x2.00. | Heat 08 | Uncommon | Helpful | Active-effect multiplier | Implemented |
| `Neon Shield` | Adds 1 bottom-edge rescue charge. | Heat 08 | Uncommon | Helpful | Shield | Implemented |
| `Reverse Controls` | Reverses paddle controls for 8s. | Heat 08 | Uncommon | Hazard | Reverse controls | Implemented |
| `Split Paddle` | Opens a center paddle gap for 12s. | Heat 08 | Uncommon | Hazard | Split paddle | Implemented |
| `Sticky Paddle` | Catches the next paddle ball until relaunch for 10s. | Heat 08 | Uncommon | Helpful | Sticky paddle | Implemented |
| `Blackout` | Blacks out brick visibility for 8s. | Heat 18 | Rare | Hazard | Visibility | Implemented |
| `Bogus Multi` | Cuts active timed effects to x0.50. | Heat 18 | Rare | Hazard | Active-effect multiplier | Implemented |
| `Brick Jammer` | Weakens brick readability/response for 7s. | Heat 18 | Rare | Hazard | Brick jam | Implemented |
| `Brick Magnet` | Pulls the ball toward nearby bricks for 10s. | Heat 18 | Rare | Helpful | Brick pull | Implemented |
| `Chain Lightning` | Broken bricks chain damage to nearby bricks for 14s. | Heat 18 | Rare | Helpful | Chain damage | Implemented |
| `Ghost Ball` | Lets balls phase through breakable bricks for 8s. | Heat 18 | Rare | Helpful | Phase ball | Implemented |
| `Gravity Well` | Pulls balls toward the arena midpoint for 12s. | Heat 18 | Rare | Hazard | Gravity well | Implemented |
| `Mega Ball` | Ball size x1.80 for 10s. | Heat 18 | Rare | Helpful | Ball size | Implemented |
| `Phase Ball` | Lets balls phase through breakable bricks for 12s. | Heat 18 | Rare | Helpful | Phase ball | Implemented |
| `Score Surge` | Score x2.00 for 10s. | Heat 18 | Rare | Helpful | Score multiplier | Implemented |
| `Signal Drift` | Intermittently stalls paddle response for 8s. | Heat 18 | Rare | Hazard | Input lag | Implemented |
| `Boom Ball` | Ball explosions damage nearby bricks for 10s. | Heat 32 | Epic | Helpful | Explosive ball | Implemented |
| `Hot Potato Ball` | Ball speed and score x1.28 for 9s. | Heat 32 | Epic | Helpful | Speed/score risk | Implemented |
| `Laser Grid` | Enables paddle laser fire for 10s. | Heat 32 | Epic | Helpful | Laser paddle | Implemented |
| `Paddle Clone` | Adds a clone rail for 10s. | Heat 32 | Epic | Helpful | Paddle clone | Implemented |
| `Warp Gates` | Linked portals reroute ball paths. | Default | Common | Glitch | Level glitch | Implemented |
| `Turbo Rail` | A hot wall rail accelerates rebounds. | Heat 18 | Rare | Glitch | Level glitch | Implemented |
| `Chrome Rail` | Paddle widens slightly and sends cleaner bank angles. | Heat 02 | N/A | Helpful | Control drop | Not implemented (UI placeholder) |
| `Clean Catch` | Next paddle hit catches, then releases with stronger aim. | Heat 04 | N/A | Helpful | Control drop | Not implemented (UI placeholder) |
| `Vector Sight` | Shows a short aim preview near the paddle. | Heat 06 | N/A | Helpful | Control drop | Not implemented (UI placeholder) |
| `Bank Bonus` | Wall bounces charge bonus points until the next brick hit. | Heat 08 | N/A | Helpful | Precision drop | Not implemented (UI placeholder) |
| `Solar Shot` | Ball burns through the next weak brick it touches. | Heat 12 | N/A | Helpful | Damage drop | Not implemented (UI placeholder) |
| `Prism Pop` | First brick hit splits a short-lived copy ball. | Heat 14 | N/A | Helpful | Split drop | Not implemented (UI placeholder) |
| `Capsule Magnet` | Nearby helpful capsules drift toward the paddle. | Heat 18 | N/A | Helpful | Pickup drop | Not implemented (UI placeholder) |
| `Mirror Grid` | Brick layout mirrors horizontally halfway through the stage. | Heat 08 | N/A | Glitch | Layout glitch | Not implemented (UI placeholder) |
| `Row Rewrite` | One row rerolls into a new brick pattern after a timer. | Heat 10 | N/A | Glitch | Layout glitch | Not implemented (UI placeholder) |
| `Prism Lanes` | Marked lanes refract the ball into sharper angles. | Heat 16 | N/A | Glitch | Precision glitch | Not implemented (UI placeholder) |
| `Token Storm` | More capsules spawn, but fall at mixed speeds. | Heat 20 | N/A | Glitch | Pickup glitch | Not implemented (UI placeholder) |
| `Gravity Pocket` | A visible pocket bends nearby ball paths. | Heat 26 | N/A | Glitch | Speed glitch | Not implemented (UI placeholder) |
| `Static Wall` | One side wall flickers between normal and weak bounce. | Heat 34 | N/A | Glitch | Paddle glitch | Not implemented (UI placeholder) |
| `Future Drop Slot` | Future drop signal pending. | Hidden | N/A | TBD | Hidden slot | Not implemented (reserved) |
| `Future Glitch Slot` | Future glitch signal pending. | Hidden | N/A | TBD | Hidden slot | Not implemented (reserved) |
| `Afterburn Coil` | The cabinet overclocks every serve, keeping the ball hotter for all remaining levels. | Draft pool | N/A | Build | Run upgrade | Implemented |
| `Flux Line` | The paddle develops a gentle permanent drift pattern that trades chaos for wider coverage arcs. | Draft pool | N/A | Build | Run upgrade | Implemented |
| `Lucky Circuit` | Pickup routing stays juiced, raising permanent drop odds for the rest of the cabinet run. | Draft pool | N/A | Build | Run upgrade | Implemented |
| `Repair Stock` | A stocked service bay grants an immediate extra life and keeps the cabinet run alive longer. | Draft pool | N/A | Build | Run upgrade | Implemented |
| `Split Serve` | Every new serve launches an extra ball, letting the run snowball faster between resets. | Draft pool | N/A | Build | Run upgrade | Implemented |
| `Wide Loader` | Your paddle chassis expands for the rest of the run, opening more forgiving save angles. | Draft pool | N/A | Build | Run upgrade | Implemented |

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
