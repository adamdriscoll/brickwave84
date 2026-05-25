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
  - `36` pickup definitions
  - `8` permanent run upgrades
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
- Runs start with 3 Brick Missiles. Press `M` during play, or `Space` when no sticky/laser action consumes it first, to fire one from the paddle into stubborn bricks. Between-stage reward drafts can buy +1 missile for 5000 points.

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
  - token storm stages with higher capsule odds and mixed falling speeds
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
  - stronger explosive and split-brick effects

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
| `Reinforced Brick` | `2` | `175` | Tougher breakable mid-tier brick | `Wide Paddle`, `Slow Ball`, `Fast Ball`, `Multi-Ball`, `Narrow Paddle`, `Wavy Paddle`, `Reverse Controls`, `Split Paddle`, `Lag Spike`, `Tilt Rail` |
| `Fortified Brick` | `3` | `250` | High-durability breakable brick | `Fast Ball`, `Multi-Ball`, `Wide Paddle`, `Narrow Paddle`, `Slow Ball`, `Wavy Paddle`, `Phase Ball`, `Gravity Well`, `Fog of War`, `Brick Bloom` |
| `Tiny Brick` | `1` | `325` | Quarter-scale breakable precision brick that is rarer in procedural mixes and pays out extra score for the smaller hitbox | `Wide Paddle`, `Slow Ball`, `Multi-Ball`, `Narrow Paddle`, `Wavy Paddle`, `Sticky Paddle`, `Shield Wall`, `Brick Bloom` |
| `Spinner Brick` | `2` | `225` | Breakable rotor brick that starts spinning when hit, is rotation-anchored at its center, and kicks the ball into stranger ricochet angles while nearby bricks can physically limit its spin | `Wide Paddle`, `Slow Ball`, `Fast Ball`, `Reverse Controls`, `Split Paddle`, `Tilt Rail` |
| `Jelly Block` | `2` | `190` | Breakable squish brick that slows the ball briefly on contact and adds a wobble read to the impact | `Slow Ball`, `Sticky Paddle`, `Multi-Ball`, `Score Surge`, `Mega Ball` |
| `Split Brick` | `2` | `175` | Breakable brick that splits into `Tiny Brick` pieces when destroyed | `Wide Paddle`, `Slow Ball`, `Fast Ball`, `Multi-Ball`, `Mondo Multi`, `Bogus Multi`, `Mega Ball`, `Brick Bloom` |
| `Explosive Brick` | `1` | `250` | Breakable brick with an explosion burst that splits the impact ball into three smaller boosted balls | `Narrow Paddle`, `Slow Ball`, `Wavy Paddle`, `Chain Lightning`, `Laser Paddle`, `Brick Bloom` |
| `Steel Brick` | Indestructible | `0` | Obstacle brick that does not count toward completion | None |

### Unlock Progression

Permanent run upgrades are always available in the between-stage draft pool. Drop and glitch unlocks now advance on parallel 50-Heat tracks: clearing a Heat can award one drop signal and one glitch signal. Implemented live drops currently fill Heat `01` through Heat `46` plus Heat `48` through Heat `50`, with a planned drop placeholder at Heat `47`. Implemented live glitches currently fill Heat `01` through Heat `05`, with planned glitch placeholders filling Heat `06` through Heat `50`. Default drops and default glitches start unlocked for Neon Ladder runs. Rarity still tunes odds and draft weighting, but it no longer controls unlock Heat. Implementation status tracks whether the item has live runtime gameplay behavior or is currently only a progression-screen placeholder.

Current balance shape: drops already have a healthy spread of classic brick-breaker tools, hazards, and mixed-risk capsules, but the late ladder needs more capstone capsules that answer or amplify stage chaos. Glitches are the bigger content gap: the implemented set proves the format with portals, layout flips, pickup storms, gravity, and wall behavior, so the rest of the ladder should lean into readable stage-wide rule changes rather than simple stat debuffs. New draft upgrades should help players build around that mayhem without erasing it.

Late drop proposals should keep using mechanics the game already teaches: paddle routing, ball speed, visibility, catch/relaunch timing, rescue charges, missile pressure, split balls, score greed, and timed-effect stacking. Heat `38` through Heat `50` intentionally alternate relief, hazard, and mixed greed so the ladder keeps adding new decisions instead of becoming a pure punishment track.

#### Drops

| Name | Description | Default / unlock Heat | Rarity | Polarity | Type | Implementation |
| --- | --- | --- | --- | --- | --- | --- |
| `Multi-Ball` | +2 balls from an active ball. | Default | Common | Helpful | Multi-ball | Implemented |
| `Shield Wall` | Adds 1 bottom-edge rescue charge. | Default | Common | Helpful | Shield | Implemented |
| `Slow Ball` | Ball speed x0.78 for 10s. | Default | Common | Helpful | Ball speed | Implemented |
| `Wide Paddle` | Paddle width x1.45 for 12s. | Default | Common | Helpful | Paddle width | Implemented |
| `Brick Missile` | Adds 1 available paddle-fired missile. | Default | Uncommon | Helpful | Missile stock | Implemented |
| `Fast Ball` | Ball speed x1.28 for 10s. | Heat 01 | Common | Hazard | Ball speed | Implemented |
| `Narrow Paddle` | Paddle width x0.72 for 10s. | Heat 02 | Common | Hazard | Paddle width | Implemented |
| `Wavy Paddle` | Adds paddle sway for 12s. | Heat 03 | Common | Hazard | Paddle drift | Implemented |
| `Vector Sight` | Shows a short paddle aim preview for 14s. | Heat 04 | Common | Helpful | Control drop | Implemented |
| `Bogus Tape` | Looks helpful, then rolls a random hazard. | Heat 05 | Uncommon | Hazard | Disguised hazard | Implemented |
| `Fog of War` | Reduces brick and pickup visibility for 10s. | Heat 06 | Uncommon | Hazard | Visibility | Implemented |
| `Lag Spike` | Intermittently stalls paddle response for 8s. | Heat 07 | Uncommon | Hazard | Input lag | Implemented |
| `Laser Paddle` | Enables paddle laser fire for 15s. | Heat 08 | Uncommon | Helpful | Laser paddle | Implemented |
| `Mondo Multi` | Multiplies active timed effects by x2.00. | Heat 09 | Uncommon | Helpful | Active-effect multiplier | Implemented |
| `Neon Shield` | Adds 1 bottom-edge rescue charge. | Heat 10 | Uncommon | Helpful | Shield | Implemented |
| `Reverse Controls` | Reverses paddle controls for 8s. | Heat 11 | Uncommon | Hazard | Reverse controls | Implemented |
| `Split Paddle` | Opens a center paddle gap for 12s. | Heat 12 | Uncommon | Hazard | Split paddle | Implemented |
| `Sticky Paddle` | Catches the next paddle ball until relaunch for 10s. | Heat 13 | Uncommon | Helpful | Sticky paddle | Implemented |
| `Mirror Image` | Adds an opposite-moving mirror paddle above your paddle for 12s. | Heat 14 | Uncommon | Helpful | Mirror paddle | Implemented |
| `Blackout` | Blacks out brick visibility for 8s. | Heat 15 | Rare | Hazard | Visibility | Implemented |
| `Bogus Multi` | Cuts active timed effects to x0.50. | Heat 16 | Rare | Hazard | Active-effect multiplier | Implemented |
| `Brick Jammer` | Weakens brick readability/response for 7s. | Heat 17 | Rare | Hazard | Brick jam | Implemented |
| `Brick Magnet` | Pulls the ball toward nearby bricks for 10s. | Heat 18 | Rare | Helpful | Brick pull | Implemented |
| `Capsule Magnet` | Nearby helpful capsules drift toward the paddle for 12s. | Heat 19 | Rare | Helpful | Pickup drop | Implemented |
| `Chain Lightning` | Broken bricks chain damage to nearby bricks for 14s. | Heat 20 | Rare | Helpful | Chain damage | Implemented |
| `Ghost Ball` | Lets balls phase through breakable bricks for 8s. | Heat 21 | Rare | Helpful | Phase ball | Implemented |
| `Gravity Well` | Pulls balls toward the arena midpoint for 12s. | Heat 22 | Rare | Hazard | Gravity well | Implemented |
| `Mega Ball` | Ball size x1.80 for 10s. | Heat 23 | Rare | Helpful | Ball size | Implemented |
| `Phase Ball` | Lets balls phase through breakable bricks for 12s. | Heat 24 | Rare | Helpful | Phase ball | Implemented |
| `Score Surge` | Score x2.00 for 10s. | Heat 25 | Rare | Helpful | Score multiplier | Implemented |
| `Signal Drift` | Intermittently stalls paddle response for 8s. | Heat 26 | Rare | Hazard | Input lag | Implemented |
| `Boom Ball` | Ball explosions damage nearby bricks for 10s. | Heat 27 | Epic | Helpful | Explosive ball | Implemented |
| `Hot Potato Ball` | Ball speed and score x1.28 for 9s. | Heat 28 | Epic | Helpful | Speed/score risk | Implemented |
| `Laser Grid` | Enables paddle laser fire for 10s. | Heat 29 | Epic | Helpful | Laser paddle | Implemented |
| `Paddle Clone` | Adds a clone rail for 10s. | Heat 30 | Epic | Helpful | Paddle clone | Implemented |
| `Clean Catch` | Next paddle hit catches, then releases with stronger aim. | Heat 31 | Epic | Helpful | Control drop | Implemented |
| `Bank Bonus` | Wall bounces bank +50 points until the next brick hit for 12s. | Heat 32 | Epic | Helpful | Precision drop | Implemented |
| `Prism Pop` | First brick hit within 10s splits a short-lived copy ball. | Heat 33 | Epic | Helpful | Split drop | Implemented |
| `Mystery Tape` | Rolls one random unlocked helpful drop and one random unlocked hazard from a mystery capsule. | Heat 34 | Epic | Mixed | Mystery drop | Implemented |
| `Tilt Rail` | Each paddle hit tilts the rail 9 degrees for 12s. | Heat 35 | Epic | Hazard | Paddle tilt | Implemented |
| `Solar Shot` | Ball burns through the next weak brick it touches. | Heat 36 | Epic | Helpful | Damage drop | Implemented |
| `Wrap Rail` | Paddle wraps from one side wall to the other for 10s. | Heat 37 | Epic | Helpful | Paddle control | Implemented |
| `Static Shoes` | Paddle movement x0.60 for 8s. | Heat 38 | Epic | Hazard | Paddle speed | Implemented |
| `Jackpot Jam` | Score x3.00, but ball speed x1.35 for 8s. | Heat 39 | Epic | Mixed | Score/speed risk | Implemented |
| `Rewind Catch` | Next missed ball rewinds to its last paddle hit instead of costing a life. | Heat 40 | Epic | Helpful | Rescue drop | Implemented |
| `Micro Spark` | Ball size x0.55 and score x1.75 for 10s. | Heat 41 | Epic | Mixed | Precision score | Implemented |
| `Brick Bloom` | Next broken brick spawns two tiny bonus bricks that pay score and can drop capsules. | Heat 42 | Epic | Mixed | Brick spawn | Implemented |
| `Magnet Flip` | Balls are pushed away from nearby bricks for 8s. | Heat 43 | Epic | Hazard | Ball repulsion | Implemented |
| `Double Tap` | Next paddle hit launches two angled copy balls and shrinks the rail briefly. | Heat 44 | Epic | Mixed | Split/control | Implemented |
| `Fuse Burst` | Clears one damaged brick, or one weak brick if none are damaged, and triggers a short blackout. | Heat 45 | Epic | Mixed | Damage/visibility | Implemented |
| `Overdrive Tape` | Ball, paddle, and capsules all move x1.25 for 9s. | Heat 46 | Epic | Mixed | Speed chaos | Implemented |
| `Chrome Catch` | Every paddle hit catches for 4s, then relaunches hotter. | Heat 47 | Epic | Mixed | Sticky speed | Not implemented (proposal) |
| `Bogus Bounce` | Next three wall bounces leave at wild angles. | Heat 48 | Epic | Hazard | Ricochet | Implemented |
| `Cabinet Jackpot` | Refreshes every active timed effect, helpful or harmful. | Heat 49 | Epic | Mixed | Effect refresh | Implemented |
| `Final Breakthru` | Ball pierces weak bricks, explodes on hit, and scores x2.00 for 6s. | Heat 50 | Epic | Helpful | Capstone drop | Implemented |

#### Glitches

| Name | Description | Default / unlock Heat | Rarity | Type | Implementation |
| --- | --- | --- | --- | --- | --- |
| `Warp Gates` | Linked portals reroute ball paths. | Default | Common | Level glitch | Implemented |
| `Turbo Rail` | A hot wall rail accelerates rebounds. | Heat 01 | Rare | Level glitch | Implemented |
| `Mirror Grid` | Brick layout mirrors horizontally halfway through the stage. | Heat 02 | Rare | Layout glitch | Implemented |
| `Token Storm` | More capsules spawn, but fall at mixed speeds. | Heat 03 | Epic | Pickup glitch | Implemented |
| `Gravity Pocket` | A slow drifting pocket bends nearby ball paths. | Heat 04 | Epic | Speed glitch | Implemented |
| `Static Wall` | One side wall flickers between normal and weak bounce. | Heat 05 | Epic | Paddle glitch | Implemented |
| `Row Rewrite` | One row rerolls into a new brick pattern after a timer. | Heat 06 | Rare | Layout glitch | Implemented |
| `Prism Lanes` | Marked lanes refract the ball into sharper angles. | Heat 07 | Rare | Precision glitch | Not implemented (UI placeholder) |
| `Switchback Rails` | Side rails swap rebound angles every few seconds. | Heat 08 | Rare | Wall glitch | Not implemented (proposal) |
| `Capsule Roulette` | Falling capsules rotate polarity until caught or missed. | Heat 09 | Rare | Pickup glitch | Not implemented (proposal) |
| `Drift Rows` | Brick rows slide slowly in opposite directions. | Heat 10 | Rare | Layout glitch | Not implemented (proposal) |
| `Hot Corners` | Corner bumpers kick balls back toward center at higher speed. | Heat 11 | Rare | Wall glitch | Not implemented (proposal) |
| `Flicker Bricks` | Some bricks only collide while visible. | Heat 12 | Rare | Visibility glitch | Not implemented (proposal) |
| `Cassette Skip` | Every few paddle hits, the ball skips forward along its current path. | Heat 13 | Rare | Ball glitch | Not implemented (proposal) |
| `Ghost Row` | One row phases out after hits, then snaps back later. | Heat 14 | Rare | Layout glitch | Not implemented (proposal) |
| `Split Horizon` | Crossing the arena midpoint bends the ball angle slightly. | Heat 15 | Rare | Trajectory glitch | Not implemented (proposal) |
| `Tilt Alarm` | Paddle hits tilt the whole rebound field until the next brick break. | Heat 16 | Epic | Paddle glitch | Not implemented (proposal) |
| `Brick Conveyor` | Brick bands crawl sideways while gaps stay dangerous. | Heat 17 | Epic | Layout glitch | Not implemented (proposal) |
| `Rogue Gate` | A single moving portal relocates after each use. | Heat 18 | Epic | Warp glitch | Not implemented (proposal) |
| `Pickup Pinball` | Capsules bounce off walls and bricks before falling again. | Heat 19 | Epic | Pickup glitch | Not implemented (proposal) |
| `Magnet Storm` | Pull pockets drift across the board and tug balls plus capsules. | Heat 20 | Epic | Gravity glitch | Not implemented (proposal) |
| `Blacklight Bricks` | Brick health and special types hide until first contact. | Heat 21 | Epic | Visibility glitch | Not implemented (proposal) |
| `Rewind Wall` | A broken non-objective row can rebuild once mid-stage. | Heat 22 | Epic | Layout glitch | Not implemented (proposal) |
| `Score Leak` | Score trickles down until the next brick break. | Heat 23 | Epic | Score glitch | Not implemented (proposal) |
| `Laser Rain` | Warning lanes fire brief vertical beams that can crack bricks or bounce balls. | Heat 24 | Epic | Hazard glitch | Not implemented (proposal) |
| `Thin Air` | One side wall opens and closes on a timer. | Heat 25 | Epic | Wall glitch | Not implemented (proposal) |
| `Prism Shuffle` | Rebounds from marked bricks rotate into sharper angles. | Heat 26 | Epic | Precision glitch | Not implemented (proposal) |
| `Clone Static` | A ghost paddle copies your last movement with a delay. | Heat 27 | Epic | Paddle glitch | Not implemented (proposal) |
| `Drop Tide` | Capsules fall in waves instead of one at a time. | Heat 28 | Epic | Pickup glitch | Not implemented (proposal) |
| `Brick Lock` | A random brick cluster shields itself until another cluster breaks. | Heat 29 | Epic | Objective glitch | Not implemented (proposal) |
| `Speed Steps` | Ball speed climbs with each brick hit and resets on paddle contact. | Heat 30 | Epic | Speed glitch | Not implemented (proposal) |
| `Mirror Serve` | Fresh serves launch a mirror ball that vanishes after one brick hit. | Heat 31 | Epic | Serve glitch | Not implemented (proposal) |
| `Static Jackpot` | Bonus score zones appear, but missing them speeds the ball. | Heat 32 | Epic | Score glitch | Not implemented (proposal) |
| `Jammed Rails` | Paddle width pulses between wide and narrow during the stage. | Heat 33 | Epic | Paddle glitch | Not implemented (proposal) |
| `Gravity Swap` | The gravity pocket flips pull direction after each wall bounce. | Heat 34 | Epic | Gravity glitch | Not implemented (proposal) |
| `VHS Tear` | A horizontal tear line deflects balls crossing it. | Heat 35 | Epic | Trajectory glitch | Not implemented (proposal) |
| `Capsule Blackout` | Catching any capsule briefly hides the next wave of drops. | Heat 36 | Epic | Pickup glitch | Not implemented (proposal) |
| `Brickquake` | Brick clusters nudge out of alignment after heavy hits. | Heat 37 | Epic | Layout glitch | Not implemented (proposal) |
| `Turbo Tax` | High-speed brick breaks pay more, but slow hits add hazard drops. | Heat 38 | Epic | Score glitch | Not implemented (proposal) |
| `Warp Jam` | Gates sometimes spit the ball out of the wrong linked exit. | Heat 39 | Epic | Warp glitch | Not implemented (proposal) |
| `Neon Flood` | Helpful and harmful capsules spawn together after combo spikes. | Heat 40 | Epic | Pickup glitch | Not implemented (proposal) |
| `Lockstep Rows` | Rows move only when the paddle moves, punishing over-correction. | Heat 41 | Epic | Layout glitch | Not implemented (proposal) |
| `Static Serve` | Each serve starts with a different rail rule until first brick break. | Heat 42 | Epic | Serve glitch | Not implemented (proposal) |
| `Blind Bank` | Wall-bounce aim previews vanish, but bank shots score extra. | Heat 43 | Epic | Precision glitch | Not implemented (proposal) |
| `Meltdown Core` | One glowing core brick overclocks every remaining brick until destroyed. | Heat 44 | Epic | Objective glitch | Not implemented (proposal) |
| `Phase Storm` | Balls and select bricks phase on alternating beats. | Heat 45 | Epic | Phase glitch | Not implemented (proposal) |
| `Score Switch` | Score target and clear-all objective swap after a warning timer. | Heat 46 | Epic | Objective glitch | Not implemented (proposal) |
| `Token Overload` | Every drop splits into a helpful and harmful capsule with different fall speeds. | Heat 47 | Epic | Pickup glitch | Not implemented (proposal) |
| `Rail Riot` | Paddle hits can spawn short temporary side bumpers. | Heat 48 | Epic | Paddle glitch | Not implemented (proposal) |
| `Cabinet Tilt` | The whole arena rebound bias drifts left and right. | Heat 49 | Epic | Trajectory glitch | Not implemented (proposal) |
| `Final Static` | Multiple unlocked glitches stack with boosted score payout. | Heat 50 | Epic | Glitch stack | Not implemented (proposal) |

#### Draft Pool

| Name | Description | Type | Implementation |
| --- | --- | --- | --- |
| `Afterburn Coil` | The cabinet overclocks every serve, keeping the ball hotter for all remaining levels. | Run upgrade | Implemented |
| `Flux Line` | The paddle develops a gentle permanent drift pattern that trades chaos for wider coverage arcs. | Run upgrade | Implemented |
| `Lucky Circuit` | Pickup routing stays juiced, raising permanent drop odds for the rest of the cabinet run. | Run upgrade | Implemented |
| `Repair Stock` | A stocked service bay grants an immediate extra life and keeps the cabinet run alive longer. | Run upgrade | Implemented |
| `Split Serve` | Every new serve launches an extra ball, letting the run snowball faster between resets. | Run upgrade | Implemented |
| `Wide Loader` | Your paddle chassis expands for the rest of the run, opening more forgiving save angles. | Run upgrade | Implemented |
| `Brick Magnet` | Balls tug slightly toward uncleared brick clusters, helping dead runs find action faster. | Run upgrade | Implemented |
| `Spare Fuse` | The first time you would lose your last life, the cabinet pops a fuse and saves the run once. | Run upgrade | Proposed |
| `Combo Cassette` | Consecutive brick breaks build a louder score streak before cooling off between slow moments. | Run upgrade | Proposed |
| `Punch Card` | Every few bricks broken stamps the card and triggers a small bonus payout. | Run upgrade | Proposed |
| `Hot Shrapnel` | Explosive and split brick effects hit a little harder for the rest of the run. | Run upgrade | Implemented |
| `Neon Insurance` | Dropped pickups linger longer before fading, giving you more time to route for them. | Run upgrade | Implemented |
| `Static Filter` | The next glitch each level is softened, delayed, or reduced in strength. | Run upgrade | Proposed |
| `Tilt Warning` | Once per level, a near-miss below the paddle bumps the ball slightly upward instead of losing it. | Run upgrade | Implemented |
| `Arcade Grease` | Paddle movement gets smoother and slightly more responsive for the rest of the run. | Run upgrade | Proposed |
| `Prize Hopper` | The first pickup collected each level has a chance to duplicate itself. | Run upgrade | Proposed |
| `Glitch Dividend` | Glitched stages pay a higher clear bonus and slightly favor relief drops in the next draft. | Run upgrade | Proposed |
| `Drop Decoder` | Mystery and Bogus capsules reveal their polarity one beat before collection. | Run upgrade | Proposed |
| `Warp Handle` | The first portal or wall-glitch exit each level trims ball speed back toward baseline. | Run upgrade | Proposed |
| `Missile Rack` | Between-stage missile purchases add +2 stock instead of +1 once per draft. | Run upgrade | Proposed |
| `Prism Warranty` | First sharp-angle brick hit after a glitch grants a short aim preview. | Run upgrade | Proposed |
| `Crowd Control` | New hazards trim the oldest active hazard when too many hazard stacks are running. | Run upgrade | Proposed |
| `Heat Sink` | Breaking bricks while the ball is over baseline speed cools active hazards slightly faster. | Run upgrade | Proposed |
| `Score Buffer` | The first life-loss penalty each level is reduced by the score earned since the last serve. | Run upgrade | Proposed |
| `Safety Glass` | Shield saves also crack the nearest damaged brick when they trigger. | Run upgrade | Proposed |
| `Capsule Reader` | Helpful capsules fall slightly slower, while harmful capsules keep their normal speed. | Run upgrade | Proposed |
| `Static Refund` | Catching a hazard grants a small score kick and brief resistance to the same hazard. | Run upgrade | Proposed |
| `Rail Wrap Kit` | Once per level, a near-miss lets the paddle wrap across one side wall for a short burst. | Run upgrade | Proposed |
| `Free Token` | The first missile fired each level does not spend missile stock. | Run upgrade | Proposed |
| `Prism Ledger` | Phase, split, explosive, and sharp-angle breaks earn a small bonus payout. | Run upgrade | Proposed |
| `Blackout Map` | Visibility hazards leave faint outlines on required bricks. | Run upgrade | Proposed |
| `Overclock Brake` | High ball speed pays extra score while nudging paddle width slightly wider. | Run upgrade | Proposed |
| `Drop Sifter` | The first harmful capsule rolled each level has a chance to become a mixed capsule. | Run upgrade | Proposed |
| `Sticky Servo` | Sticky and clean catches show a stronger aim preview before relaunch. | Run upgrade | Proposed |
| `Brick Scanner` | Hitting a fortified, spinner, split, or explosive brick briefly marks nearby special bricks. | Run upgrade | Proposed |
| `Risk Rebate` | Catching a mixed or hazard capsule extends the next helpful timed effect. | Run upgrade | Proposed |
| `Last Call` | Final required bricks in a stage pay bonus score and have higher capsule odds. | Run upgrade | Proposed |

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
