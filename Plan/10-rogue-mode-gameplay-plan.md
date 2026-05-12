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

### Unlock Progression Contract

Rogue, player-facing as `Neon Ladder`, is the only mode that unlocks new drops and level glitches. `Neon Marathon` and multiplayer modes can use unlocked content, but they should not advance the unlock track themselves.

Default content:

- The current authored drop assets under `Assets/Resources/PowerUps/` are the default drop pool.
- The current implemented level glitches, `Warp Gates` and `Turbo Rail`, are default glitch archetypes once a mode enables glitches.
- Default content is always available in `Neon Ladder`, `Neon Marathon`, Custom Game, and multiplayer modes according to each mode's drop/glitch settings.

Unlock content:

- The 50 backlog drops in `Drop Backlog: 50 More Capsules` are new unlockable drops.
- The 50 backlog glitches in `Level Glitch Backlog: 50 More Stage Corruptions` are new unlockable glitch archetypes.
- Unlocks are account/profile progress, not per-run temporary rewards.
- Unlocks become eligible for future `Neon Ladder` runs, `Neon Marathon`, and multiplayer after they are earned in `Neon Ladder`.
- Custom Game can eventually expose a setting for `Default Only`, `Unlocked Pool`, or `All Content` for testing, but normal progression should treat locked content as unavailable.

Unlock rhythm:

- Completing Ladder stages, boss gates, and intensity milestones should reveal bundles of content rather than isolated items.
- A bundle should follow the `Content Growth Contract`: usually `2-3 drops`, `1-2 glitches`, and one related upgrade, brick behavior, boss variant, or synergy rule.
- Early Ladder unlocks should be mostly helpful or readable control tools.
- Harmful drops and disruptive glitches should arrive after the player has already seen counters, conversions, or rewards that make them interesting.
- Late intensity unlocks can combine families, but the Progression page must still explain the family and counterplay plainly.

### Progression Page

Add a top-level `Progression` surface that shows unlock progress across paddles, intensities, drops, and glitches. It should feel like a cabinet service screen or neon circuit board, not a shop.

Player questions it should answer:

- What have I unlocked?
- What is available in Marathon and multiplayer now?
- What is the next Ladder goal?
- Which content family am I building toward?
- Why is a locked item still hidden or teased?

Recommended layout:

| Region | Purpose |
| --- | --- |
| Ladder Header | Selected paddle, highest cleared intensity, next intensity, and run completion streak. |
| Unlock Meter | Total unlocked drops and glitches, grouped as `Default`, `Unlocked`, and `Locked`. |
| Family Tabs | Control, Precision, Damage, Split, Pickup, Layout, Speed, Paddle Disruption. |
| Content Grid | Cards for drops and glitches with icon, name, family, unlock state, and mode availability. |
| Next Signal | A short goal such as `Clear Intensity 08 with Classic Paddle` or `Beat Boss Gate 2`. |
| Mode Availability | Badges for `Ladder`, `Marathon`, and `Multiplayer` so players see where unlocked content now appears. |

Unlock states:

| State | UI Treatment | Meaning |
| --- | --- | --- |
| Default | Bright and labeled `Default` | Current content; always available when the mode allows it. |
| Unlocked | Neon-lit with family color and mode badges | Earned in Ladder and eligible for other modes. |
| Seen Locked | Dim silhouette with family and unlock hint | Player is close enough to know the content exists. |
| Hidden Locked | Unknown slot or static tile | Reserved for later bands, bosses, or surprise content. |

Do not show every locked item immediately. Reveal names by family and intensity band so the page motivates progress without turning the full future content list into homework.

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

### Content Growth Contract

Drops and glitches should grow as connected content families, not as isolated one-off mechanics.

Before any drop or glitch leaves backlog, it needs:

- a synergy family: what build, paddle, brick type, boss, or scoring rule wants this content
- an intensity band: when it can start appearing and when stronger variants can unlock
- a tradeoff: why the player might skip the reward, dodge the capsule, or choose a cleaner route
- a readable counter: how the player can respond through aim, timing, pickup routing, or draft choice
- at least one partner: another drop, glitch, brick behavior, upgrade, or boss phase that makes it more interesting

Ship new content in bundles, not singles. A good bundle is usually `2-3 drops`, `1-2 glitches`, and `1 upgrade, brick, or boss interaction` that all reinforce the same build story.

### Growth Families

Every item in the drop and glitch backlogs belongs to one of these families. The point is to make the game widen over time: early bands teach a rule, middle bands create draft tension, and late bands combine families into high-score or survival pressure.

| Family | First Band | Drop Candidates | Glitch Candidates | Growth Purpose |
| --- | --- | --- | --- | --- |
| Control And Recovery | 1-10 | Chrome Rail, Clean Catch, Vector Sight, Chill Bubble, Rewind Tap, Soft Serve, Magno Nudge, Wall Hugger, Angle Lock, Second Chance, Life Line | Cold Corners, Soft Walls, Escape Hatch, Spiral Serve | Gives new players stability, then lets skilled players convert control into precise clears. |
| Precision And Score | 1-20 | Bank Bonus, Ricochet Charge, Weak Spot, Combo Saver, Score Tape, Jackpot Jolt, Score Leak | Prism Lanes, Hot Corners, Wide Open, Score Surge Stage, Bogus Bonus Stage, Final Input Lock | Makes angle mastery and risk scoring matter without turning every reward into raw damage. |
| Damage, Pierce, And Boss Tech | 11-30 | Solar Shot, Thunder Tag, Core Drill, Firecracker, Laser Burst, Chrome Comet, Sweep Beam | Lockout Tiles, Core Shield Pulse, Armor Surge, Boss Static Cameo | Creates answers to armor, cores, shielded bricks, and boss gates. |
| Split And Clone | 11-30 | Prism Pop, Twin Flicker, Spare Spark, Clone Chip | Jelly Grid, Split Orbit, Clone Tax | Supports multiball builds while adding readability checks and score tradeoffs. |
| Pickup Economy And Hazard Harvest | 11-40 | Capsule Magnet, Bonus Mint, Bogus Tape, Drop Tax, Sour Magnet, Glitch Debt, Hazard Bloom | Dead Air, Token Storm, Capsule Flood, Hazard Flood, Label Noise, Drop Delay | Turns capsules into a route-planning layer, including builds that profit from dodging or converting danger. |
| Layout Rewrite And Corruption | 21-40 | Blind Lane, Brick Rain, Noise Floor, Ball Static | Mirror Grid, Row Rewrite, Scanline Sweep, Rewind Row, Brick Shuffle, Ceiling Drop, Ghost Bricks, Flicker Field, Conveyor Wall, Crossfade Bricks, Seed Fracture | Makes stages feel corrupted without hiding the rules; best for Seed Breaker and late Rogue. |
| Speed, Momentum, And Gravity | 21-50 | Turbo Trouble, Wobble Ball, Bad Bounce, Black Ice | Gravity Pocket, VCR Skip, Fast Forward Floor, Pause Blink, Low Ceiling, Tight Squeeze, Magnetic North, Mondo Gravity, Hard Walls | Raises tempo and recovery pressure while rewarding players who drafted control tools earlier. |
| Paddle And Input Disruption | 31-50 | Skinny Signal, Wrong-Way Ray, Static Shoes, Sticky Miss, Dead Zone, Mirror Hands | Static Wall, Side Swap, Input Echo, Pulse Bumpers, Switchback Rails, Tilt Warning, Wall Phase | Late-band disruption for confident players; should be used sparingly and always telegraphed. |

### Intensity Growth Rules

| Band | New Content Shape | Synergy Requirement |
| --- | --- | --- |
| 1-10 | Control, recovery, simple score hooks, and low-pressure arena rules. | Each new drop should help a basic build survive or aim better. Glitches should teach one visible rule. |
| 11-20 | Pickup economy, precision scoring, first harmful draft choices, and boss-answer drops. | Each hazard should have at least one reward or upgrade that can exploit it. |
| 21-30 | Split builds, damage routing, moving layouts, and armor/core pressure. | Each damage or split drop should have a boss, brick, or glitch that makes it shine. |
| 31-40 | Corrupted layouts, hazard harvest, drop manipulation, and Seed Breaker pressure. | Each glitch should change draft priorities, not just make the board harder. |
| 41-50 | Combined families, elite disruption, and high-risk score premiums. | Every late-band modifier should pair one upside with one readable threat. |

### Drop Backlog: 50 More Capsules

The current pool has 30 authored default drops. Everything in this table is a new Ladder unlock candidate, not default content. Use these as expansion candidates, not all-at-once additions. Early bands should favor readable variants of existing systems; later bands can add new mechanics once pickup polarity, timers, and draft intent are clear.

| # | Drop | Lane | Idea |
| ---: | --- | --- | --- |
| 1 | Chrome Rail | Control | Paddle widens slightly and sends cleaner bank angles for a short time. |
| 2 | Clean Catch | Control | Next paddle hit catches the ball, then releases with a stronger aimed launch. |
| 3 | Vector Sight | Control | Shows a short aim preview while the ball is near the paddle. |
| 4 | Chill Bubble | Control | Slows the nearest active ball without changing other ball speeds. |
| 5 | Rewind Tap | Recovery | Saves one bad rebound by snapping the ball back to its last safe lane. |
| 6 | Bank Bonus | Score | Wall bounces charge bonus points until the next brick hit. |
| 7 | Soft Serve | Recovery | Next serve starts slower and with a wider launch window. |
| 8 | Magno Nudge | Control | Paddle gently pulls the nearest ball toward its center. |
| 9 | Wall Hugger | Recovery | Side walls save one steep miss by bending the ball back inward. |
| 10 | Angle Lock | Precision | Paddle hits clamp to readable low, mid, or steep angles for a short time. |
| 11 | Solar Shot | Damage | Ball burns through the next weak brick it touches. |
| 12 | Prism Pop | Split | First brick hit splits a short-lived copy ball at a mirrored angle. |
| 13 | Thunder Tag | Damage | Marks one brick so the next hit zaps nearby bricks. |
| 14 | Core Drill | Boss | Ball deals extra damage to armor, core, and boss bricks. |
| 15 | Firecracker | Explosive | Next brick hit creates a small, readable blast. |
| 16 | Laser Burst | Damage | Fires one vertical laser volley from the paddle. |
| 17 | Ricochet Charge | Precision | Each clean wall bounce adds damage to the next brick hit. |
| 18 | Chrome Comet | Pierce | Ball pierces one brick, then returns to normal bouncing. |
| 19 | Sweep Beam | Damage | Clears a thin horizontal scanline through damaged bricks. |
| 20 | Weak Spot | Precision | Highlights one valuable target and boosts points for hitting it. |
| 21 | Twin Flicker | Split | Adds a faint helper paddle that blocks one miss, then burns out. |
| 22 | Capsule Magnet | Pickup | Nearby helpful capsules drift toward the paddle. |
| 23 | Bonus Mint | Pickup | The next few helpful drops last slightly longer. |
| 24 | Second Chance | Recovery | Arms a one-shot bottom shield if no shield is already active. |
| 25 | Spare Spark | Recovery | Adds one tiny backup ball with low damage and short lifetime. |
| 26 | Clone Chip | Synergy | Copies the strongest active helpful effect at half duration. |
| 27 | Combo Saver | Score | Prevents the current combo from dropping once. |
| 28 | Score Tape | Score | Stores bonus points and pays them out if the stage is cleared. |
| 29 | Jackpot Jolt | Score | Next special brick hit is worth extra points. |
| 30 | Life Line | Recovery | Grants a life only if the player clears the current stage without another miss. |
| 31 | Skinny Signal | Hazard | Paddle narrows for a short time. |
| 32 | Turbo Trouble | Hazard | Ball speed spikes briefly. |
| 33 | Wrong-Way Ray | Hazard | Paddle controls reverse briefly. |
| 34 | Static Shoes | Hazard | Paddle acceleration drops and stopping distance increases. |
| 35 | Bogus Tape | Hazard | Looks helpful until collected, then rolls a minor hazard. |
| 36 | Drop Tax | Hazard | Helpful capsules fall faster and expire sooner for a short time. |
| 37 | Score Leak | Hazard | Score multiplier slowly drains until the next brick clear. |
| 38 | Blind Lane | Hazard | A narrow screen band hides bricks until the ball enters it. |
| 39 | Sour Magnet | Hazard | Helpful capsules drift slightly away from the paddle. |
| 40 | Brick Rain | Hazard | Spawns a few low-health junk bricks near the top. |
| 41 | Sticky Miss | Hazard | Paddle catches the next ball but releases it at a fixed awkward angle. |
| 42 | Wobble Ball | Hazard | Ball angle jitters lightly after each wall bounce. |
| 43 | Bad Bounce | Hazard | Next paddle hit exaggerates the bounce angle. |
| 44 | Dead Zone | Hazard | A small center strip of the paddle gives weak rebounds. |
| 45 | Glitch Debt | Risk | Next helpful pickup is delayed, but pays a stronger effect if collected. |
| 46 | Noise Floor | Hazard | Active effect timers become less readable and tick down faster. |
| 47 | Hazard Bloom | Hazard | Harmful drops are weighted higher until one is collected or missed. |
| 48 | Ball Static | Hazard | Ball trail gets noisier, making fast rebounds harder to read. |
| 49 | Mirror Hands | Hazard | Horizontal input flickers between normal and reversed. |
| 50 | Black Ice | Hazard | Paddle slides with low friction for a short time. |

### Level Glitch Backlog: 50 More Stage Corruptions

The current implemented default archetypes are `Warp Gates` and `Turbo Rail`. Everything in this table is a new Ladder unlock candidate, not default content. Future glitches should behave like stage rules with score premiums, clear setup labels, and obvious visual language before the first serve.

| # | Glitch | Lane | Idea |
| ---: | --- | --- | --- |
| 1 | Mirror Grid | Layout | The brick layout mirrors horizontally halfway through the stage. |
| 2 | Row Rewrite | Layout | One row rerolls into a new brick pattern after a timer. |
| 3 | Scanline Sweep | Hazard | A moving scanline briefly hides or reveals bricks as it passes. |
| 4 | Static Wall | Arena | One side wall flickers between normal bounce and weak bounce. |
| 5 | Gravity Pocket | Ball | A visible pocket bends nearby ball paths. |
| 6 | Prism Lanes | Ball | Marked lanes refract the ball into sharper angles. |
| 7 | VCR Skip | Timing | The ball stutters forward on a fixed beat, then resumes normal motion. |
| 8 | Rewind Row | Layout | A damaged row restores once unless fully cleared quickly. |
| 9 | Fast Forward Floor | Arena | Ball speed increases near the bottom of the playfield. |
| 10 | Pause Blink | Timing | Bricks blink invulnerable for a short readable pulse. |
| 11 | Dead Air | Arena | A small no-drop zone prevents capsules from spawning inside it. |
| 12 | Token Storm | Drops | More capsules spawn, but fall at mixed speeds. |
| 13 | Brick Shuffle | Layout | Surviving bricks shift one column after every few hits. |
| 14 | Ceiling Drop | Layout | A new row descends from the top after a timer. |
| 15 | Side Swap | Arena | Left and right wall effects swap positions mid-stage. |
| 16 | Input Echo | Paddle | Paddle repeats a faint delayed movement after sharp turns. |
| 17 | Ghost Bricks | Layout | Some bricks are visible but only become solid after the first hit nearby. |
| 18 | Flicker Field | Layout | A cluster alternates between hittable and pass-through states. |
| 19 | Jelly Grid | Ball | Brick impacts absorb speed, then rebound the ball harder. |
| 20 | Lockout Tiles | Layout | Certain bricks open only after their paired lock brick is hit. |
| 21 | Hot Corners | Arena | Corner rebounds add speed and bonus points. |
| 22 | Cold Corners | Arena | Corner rebounds slow the ball but reduce bonus scoring. |
| 23 | Pulse Bumpers | Arena | Temporary bumpers appear on a beat near the side walls. |
| 24 | Switchback Rails | Arena | Short angled rails redirect shots through the brick field. |
| 25 | Tilt Warning | Paddle | Repeated edge hits tilt the paddle response until a center hit resets it. |
| 26 | Low Ceiling | Arena | Top wall drops lower, shrinking recovery space. |
| 27 | Tight Squeeze | Arena | Side walls move inward for a denser, riskier stage. |
| 28 | Wide Open | Arena | Side walls move outward, rewarding precision over safety. |
| 29 | Conveyor Wall | Layout | Brick columns drift horizontally like a conveyor belt. |
| 30 | Crossfade Bricks | Layout | Two brick groups trade visibility and collision on a beat. |
| 31 | Score Surge Stage | Score | Stage score multiplier rises while the ball stays alive. |
| 32 | Bogus Bonus Stage | Score | Score premium is high, but misses remove the premium. |
| 33 | Capsule Flood | Drops | Helpful and harmful capsules both spawn more often. |
| 34 | Hazard Flood | Drops | Harmful capsules are common but worth bonus score when dodged. |
| 35 | Label Noise | Drops | Capsule labels flicker, so polarity color must carry readability. |
| 36 | Split Orbit | Ball | Extra balls orbit briefly before launching outward. |
| 37 | Magnetic North | Ball | The ball is pulled slightly toward the top-center wall. |
| 38 | Clone Tax | Split | Extra balls score less until only one remains. |
| 39 | Mondo Gravity | Ball | Ball paths bend downward more strongly near the paddle. |
| 40 | Soft Walls | Arena | Wall rebounds lose speed but grant safer angles. |
| 41 | Hard Walls | Arena | Wall rebounds gain speed and score value. |
| 42 | Wall Phase | Arena | One side wall becomes pass-through, with a visible return portal. |
| 43 | Escape Hatch | Recovery | A bottom hatch opens once as a save, then becomes a hazard slot. |
| 44 | Spiral Serve | Serve | Serves launch through a rotating aim cone. |
| 45 | Core Shield Pulse | Boss | Boss or core bricks shield and unshield on a readable beat. |
| 46 | Armor Surge | Bricks | A few bricks gain temporary armor after nearby bricks break. |
| 47 | Drop Delay | Drops | Capsules pause in midair, then resume falling together. |
| 48 | Boss Static Cameo | Boss | A boss pattern briefly interrupts a normal stage. |
| 49 | Final Input Lock | Paddle | The last brick requires a clean aimed hit while paddle assists are disabled. |
| 50 | Seed Fracture | Risk | The stage combines two minor glitches for a high score premium. |

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

Start releasing upgrades, drops, power-downs, brick behaviors, glitches, and boss variants into intensity bands as small synergy bundles. Favor content that creates build stories before adding raw count, and do not ship orphan drops or orphan glitches.

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

