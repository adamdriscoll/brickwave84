# Chunk 10: Rogue Mode Gameplay Plan

## Goal

Define the concrete `Rogue` mode contract: a 10-stage arcade roguelite run with lives, between-stage build choices, escalating drops and brick pressure, boss gates, intensity progression, and unlockable paddle types.

This is a gameplay plan, not an implementation spec. The details should change as playtesting teaches us what is actually fun.

## Mode Promise

`Rogue` is the primary progression mode for `Brickwave '84`.

The player starts a run with one paddle type, 3 lives, a generated seed, and a small active content pool. They clear a 10-stage mixtape, draft a reward after each cleared stage, survive three boss gates, and either wipe out or complete the run to push that paddle to a higher intensity.

The ideal player question is:

`Can this paddle and this build survive one more hotter mixtape?`

## Core Run Contract

- A Rogue run is built around `10` stages.
- The player starts with `3` lives. Player-facing UI can keep using the clearer arcade term `BALLS` where appropriate.
- Standard stages start easy and add pressure through brick types, drop pools, layout density, hazards, and base ball speed.
- Clearing a standard stage grants a deterministic `pick 1 of 3` reward for the rest of the run.
- Rewards can include permanent run upgrades and, eventually, new drop unlocks that enter the in-stage pickup pool for the rest of that run.
- Boss gates happen after stage `3`, stage `6`, and stage `10`.
- Defeating the final boss completes the run and unlocks the next intensity for the selected paddle.

### Encounter Rhythm

| Beat | Role | Design Intent |
| --- | --- | --- |
| Stages 1-3 | Opening | Teach the run seed, let the first build choices form, keep speed readable. |
| Boss 1 | Rival check | Test angles and controlled rallies without overwhelming the player. |
| Stages 4-6 | Pressure | Add special bricks, sharper layouts, and less forgiving drop mixes. |
| Boss 2 | Chaos check | Test target priority, damage routing, and recovery under clutter. |
| Stages 7-10 | Late run | Let the build sing while the board gets unstable and faster. |
| Boss 3 | Mastery check | Test the whole build against rule changes, fake rewards, and arena disruption. |

Open tuning question: decide whether boss gates are extra encounters between numbered stages or replace stages `4`, `7`, and `10` in the moment-to-moment menu flow. The progression promise should still read as a 10-stage run either way.

## First Build Chunk: Simple Rogue Run

Build this before intensity, paddle unlocks, or the full boss roster.

### Scope

- Add `Rogue` as the first progression-oriented Singleplayer mode.
- Keep the run to 10 standard stages with 3 lives.
- Increase base ball speed gradually across stages.
- Grant a deterministic reward draft after each cleared stage.
- Start with existing upgrades and a small number of hand-picked new rewards if needed.
- Introduce a run-local drop pool concept, even if the first version only uses existing drops.
- Save a basic run result: completed, failed, stage reached, selected paddle, seed, and score.

### Stage Curve

| Stage | Pressure Target | Content Direction |
| --- | --- | --- |
| 1 | Warm start | Basic bricks, friendly layout, low base speed. |
| 2 | First nudge | Slightly denser layout, one simple special brick or drop possibility. |
| 3 | First test | One meaningful special brick family and a modest speed bump. |
| 4 | Build response | Reward choices should start mattering against the layout. |
| 5 | Mid-run mix | More special bricks, more drop variance, faster recovery demands. |
| 6 | Pre-boss heat | A deliberate challenge spike without visual overload. |
| 7 | Late-run identity | Layouts assume the player has several run upgrades. |
| 8 | Hazard blend | Add power-downs or disruptive brick behavior if available. |
| 9 | Stress test | Fast, dense, but still readable. |
| 10 | Finale setup | A final standard clear or the entrance to the final boss. |

### Reward Types

Start with two reward lanes:

- `Run Upgrade`: a permanent modifier for the rest of the run.
- `Drop Unlock`: adds one power-up, power-down, or special pickup to the drop pool for the rest of the run.

Drop unlocks are important because they let the player choose not only what they are now, but what kind of chaos they are willing to invite later.

### Done When

- A player can start Rogue, clear or fail a 10-stage run, and see a result.
- The run gets gradually faster and more complex without needing meta progression.
- Between-stage rewards persist and are visible enough to understand.
- The same seed and choices can reproduce the same run structure.

## Boss Plan

Bosses should feel like corrupted arcade machines and synthwave entities. They should use Breakout language first: paddles, balls, bricks, cores, shields, bumpers, capsules, and the playfield itself.

### Boss 1: The Paddle Punk

`The Paddle Punk` is a rival neon paddle at the top of the screen. It turns the board into a vertical duel and teaches that bosses are not just oversized brick walls.

| Phase | Behavior |
| --- | --- |
| Warm-Up, Nerd | Simple top-paddle movement and readable returns. |
| Turbo Mode | Faster tracking and sharper angled deflections. |
| No Mercy Rally | Side bumpers enter the arena and make trajectories harder to predict. |

Core mechanics:

- The boss paddle blocks ordinary shots unless the player finds sharp angles.
- Successful rallies expose weak-point bricks behind the boss paddle.
- The boss occasionally spawns small brick shields.
- Deflections get faster after repeated boss hits, but should still respect readability.

Reward direction:

- `Bank Shot Bonus`: angled hits deal extra damage to special bricks or exposed boss weak points.

Primary skill tested:

- Angles, rally control, and deliberate bank shots.

### Boss 2: The Brickasaurus Wrecks

`The Brickasaurus Wrecks` is a giant dinosaur-shaped boss made of connected brick segments, neon bones, and glitchy polygon plates. The player breaks pieces off instead of draining a plain health bar.

| Phase | Behavior |
| --- | --- |
| Fossilized Funk | Armor plates protect weak points. |
| Extinction Event | Explosive bricks create chain-reaction opportunities and risks. |
| Skeleton Mode | Glowing core bricks remain and move around. |

Core mechanics:

- Body segments use different brick types: explosive, split, spinning, armor, and indestructible plates.
- Breaking certain parts changes the attack pattern.
- The tail sweeps across the lower brick field and redirects the ball.
- The mouth fires power-down meteors toward the paddle.

Reward direction:

- `Chain Reaction`: explosive bricks have a small chance to trigger nearby special bricks.

Primary skill tested:

- Target priority, chaos management, and build-specific damage routing.

### Boss 3: The Mainframe Maniac

`The Mainframe Maniac` is the corrupted arcade AI controlling the whole grid. The arena becomes the boss.

| Phase | Behavior |
| --- | --- |
| Boot Sequence | Basic brick waves and modifier drops. |
| System Override | Rows and sections change shape mid-rally. |
| Blue Screen of Doom | The screen glitches, controls may briefly invert, and the core appears. |
| Final Input | One exposed neon core brick must be hit several times while the arena collapses. |

Core mechanics:

- Rewrites rows of bricks during the fight.
- Applies temporary modifiers, both helpful and harmful.
- Spawns fake power-ups that become power-downs if collected.
- Rotates sections of the brick field like a glitching circuit board.
- Periodically captures the ball and relaunches it at an odd angle.

Reward direction:

- `Seed Breaker Mode`: future runs can include corrupted levels with higher rewards.

Primary skill tested:

- Full build mastery, adaptation, and surviving rule disruption.

## Intensity Progression

Intensity is the long-term Rogue climb after the player completes a full run.

### Rules

- Rogue has `50` intensity levels.
- Completing a run at the current available intensity unlocks the next intensity for the selected paddle type.
- Each paddle tracks its own highest reached intensity.
- Failing a run records the best stage reached for that paddle and intensity, but does not advance intensity.
- Each intensity slightly increases base ball speed.
- New power-ups, power-downs, upgrades, brick behaviors, boss variants, and reward rules enter the content pool across intensity bands.

### Tuning Direction

Ball speed should rise enough to be felt across the long climb, but not so much that high intensity becomes pure reaction speed. Start with a small per-intensity increase, then clamp per-stage and per-boss speeds during playtesting.

Intensity should add variety before punishment. A higher intensity should mean:

- more interesting drop pools
- more special brick combinations
- more boss modifiers
- more demanding reward choices
- slightly faster baseline play

It should not mean:

- every board is simply dense
- every ball is unreadably fast
- early paddles become obsolete
- permanent stat grinding becomes mandatory

### Suggested Intensity Bands

| Band | Theme | Unlock Direction |
| --- | --- | --- |
| 1-10 | Cabinet Warm-Up | Core Rogue run, basic upgrades, simple hazards, first boss versions. |
| 11-20 | Static Pressure | More power-downs, fake drops, moving bricks, stronger reward synergies. |
| 21-30 | Turbo Circuit | Faster layouts, speed-risk upgrades, boss phase variants. |
| 31-40 | Glitch Grid | Corrupted levels, row rewrites, drop-pool manipulation, harder hazard mixes. |
| 41-50 | Mainframe Burn | Full content pool, high-risk rewards, elite boss patterns, score-chase pressure. |

## Paddle Progression

Paddle types are progression unlocks that change playstyle rather than becoming straight upgrades.

### Rules

- `Classic Paddle` is unlocked by default.
- Completing a full Rogue run with the current paddle unlocks the next paddle type.
- Each paddle stores its own highest completed intensity.
- Paddle selection happens before a Rogue run starts.
- Upgrades and drops can support paddle-specific synergies, but no paddle should require one exact build to function.

### Recommended Starting Set

| Paddle | Identity | Strength | Drawback |
| --- | --- | --- | --- |
| Classic Paddle | Balanced starter | Stable, readable, flexible. | No extreme specialty. |
| Comet Paddle | Small and fast | High acceleration and precision control. | Less surface area, punishes sloppy recovery. |
| Cruiser Paddle | Wide and slow | Safer catches and survival builds. | Lower mobility and fewer extreme angles. |
| Orb Paddle | Circular angle chaos | Curved contact sends wild, pinball-like rebounds. | Harder to aim consistently. |
| Block Paddle | Rotating cube | Sides and corners create intentional ricochet choices. | Requires timing and rotation awareness. |
| Magnet Paddle | Catch and control | Brief catch, pull, or bend behavior creates recovery windows. | Slower or cooldown-limited. |
| Twin Paddle | Split coverage | Two small paddles cover more total width. | Center gap creates a meaningful danger zone. |
| Prism Paddle | Refracted angles | Extreme angle control and possible split/refract synergies. | Can create risky trajectories and fast chaos. |

### Later Paddle Backlog

- `Blade Paddle`: thin, medium speed, high-speed rebounds.
- `Gel Paddle`: absorbs speed and supports control builds.
- `Pulse Paddle`: periodic horizontal pulse can bump balls or damage low bricks.
- `Rail Paddle`: moves in stepped grid increments for strange precision play.
- `Boomerang Paddle`: curved shape tends to redirect balls toward center.

## Upgrade, Drop, And Power-Down Expansion

Rogue mode needs more content, but content should arrive as connected families.

### Content Categories

- `Run Upgrades`: permanent choices that define the current run.
- `Drop Unlocks`: reward choices that add new pickups or hazards to the run drop pool.
- `Power-Ups`: temporary or instant helpful pickups.
- `Power-Downs`: temporary or instant harmful pickups.
- `Boss Relics`: special rewards from boss gates, either for the current run or future intensity unlocks.
- `Paddle Synergies`: upgrades that work differently with specific paddle shapes or playstyles.

### Synergy Tags

Use tags to keep new content coherent:

- `Control`
- `Speed`
- `Precision`
- `Split`
- `Explosive`
- `Pierce`
- `Pickup`
- `Hazard`
- `Shield`
- `Score`
- `Boss`
- `Paddle`
- `Risk`
- `Recovery`

Every new upgrade or drop should answer:

- What build does this support?
- What builds does it conflict with?
- What boss or brick behavior does it help solve?
- What intensity band should unlock it?
- What visual/readability risk does it create?

### Example Synergy Directions

| Build Direction | Likes | Risk |
| --- | --- | --- |
| Bank Shot Build | Bank Shot Bonus, Prism Paddle, angle rewards, shielded bricks. | Can become too aim-dependent for high-speed stages. |
| Chain Reaction Build | Boom Box bricks, Chain Reaction, explosive drops, Brickasaurus parts. | Can destroy target priority if explosions become too automatic. |
| Control Build | Magnet Paddle, Gel-style effects, Chill Ball, Vector Sight. | Can trivialize fast stages if control uptime is too high. |
| Split Build | Party Split, Twin Paddle, refract effects, pickup magnetism. | Can become unreadable if too many balls and particles stack. |
| Risk-Speed Build | Comet Paddle, Turbo Ball, score multipliers, precision rewards. | Can punish average players too hard if speed stacks uncapped. |
| Hazard Harvest Build | Fake Tape manipulation, power-down conversion, corrupted drops. | Needs clear pickup polarity so choices feel fair. |

## Recommended Build Sequence

### 1. Rogue Run MVP

Build the 10-stage run with 3 lives, stage speed ramp, post-stage rewards, and result tracking. Boss slots can be placeholders.

### 2. Reward Draft And Drop Unlocks

Deepen the reward system so choices can either grant a direct run upgrade or add a pickup to the run drop pool.

### 3. Boss Gate Framework

Add the encounter framework for boss gates after stages 3, 6, and 10. Implement `The Paddle Punk` first because it tests boss language with the smallest art and systems footprint.

### 4. Intensity V1

Track 50 intensity levels, per-paddle highest intensity, completion unlocks, and a small ball-speed increase per intensity.

### 5. Paddle Progression V1

Add paddle selection and unlock the first alternate paddle, probably `Comet Paddle` or `Cruiser Paddle`, before building the stranger shapes.

### 6. Content Bands

Start releasing upgrades, drops, power-downs, brick behaviors, and boss variants into intensity bands. Favor content that creates synergy stories before adding raw count.

### 7. Boss Roster Completion

Implement `The Brickasaurus Wrecks` and `The Mainframe Maniac` once the run, reward, and intensity systems are stable enough to make those fights matter.

## Near-Term Non-Goals

- Do not implement all 50 intensity bands at once.
- Do not add every paddle type before the run loop is fun.
- Do not flood the drop pool before pickup polarity and reward intent are readable.
- Do not make intensity a permanent stat grind.
- Do not build cinematic boss presentation before boss mechanics are fun.

## Open Questions

- Should boss gates count as extra encounters or occupy stages within the 10-stage contract? Extra
- Should boss rewards always be current-run relics, meta unlocks, or a mix? Mix
- Should drop unlock rewards be purely beneficial at low intensity, with power-down unlocks arriving later? Power-downs later
- How much should paddle-specific upgrades appear in general drafts versus paddle-focused pools? We want to support paddle synergies without forcing them, so probably a mix with clear tagging.
- Should `Seed Breaker Mode` be a post-run modifier, a high-intensity rule, or a separate challenge lane? High-intensity rule seems best for now, as it creates a clear risk-reward decision for players climbing intensity.
- Which intensity bands should remain score-valid for daily or leaderboard comparison? Let's not have score-chasing at all for rogue. That's just in the custom\classic game.

