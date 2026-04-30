# Get Bricked — 80s Arcade Voice & Labeling Guide

This file defines the naming, tone, slang, and UI text conventions for **Get Bricked**, a synthwave/retro arcade brick-breaker. It is intended for a coding agent to use when naming power-ups, bricks, levels, achievements, menus, pickups, event callouts, and result screens.

## 1. Creative Target

**Game personality:** a lost 1984 arcade cabinet that speaks in neon, surf/skate slang, mall-era confidence, and coin-op challenge language.

**Core vibe:**

> Breakout mechanics + Arkanoid-style power-ups + neon grid / cassette-futurist arcade cabinet + playful 80s slang.

**Voice summary:**

- Fast, punchy, readable.
- Cheerfully cocky, never mean.
- Arcade-first, slang-second.
- Mechanical clarity always beats cleverness.
- Uses 80s slang as seasoning, not as a parody wall of “like, totally, gag me.”

## 2. Research Anchors

Use these as tone anchors, not as rigid historical reenactment.

- **Classic Breakout vocabulary was simple and readable:** brick wall, paddle, ball, score, serve, catch, timer, wall, points. Use this same clarity for mechanics.
- **Arkanoid-style brick-breakers introduced falling capsules/power-ups:** paddle size, multiball, sticky/catch ball, lasers, etc. Use short collectible names and instant callouts.
- **Arcade culture used compact cabinet phrases:** `INSERT COIN`, `PRESS START`, `GAME OVER`, `CONTINUE?`, `HIGH SCORE`, `ENTER INITIALS`.
- **High-score tables often used 3-character initials.** Support short initials in UI copy if the game has leaderboards.
- **80s slang draws from Southern California / Valley / surf / skate / early hip-hop overlap:** rad, gnarly, tubular, grody, bogus, stoked, fresh, wicked, choice, no way.
- **Synthwave visuals are a modern retro-futurist remix:** neon grids, vector landscapes, wireframes, chrome, VHS, CRT glow, cassette/future-that-never-was energy.

### Sources

- Atari Breakout rules/manual vocabulary: https://atariage.com/manual_html_page.php?SoftwareID=889
- Atari Breakout current description and original rules text: https://atari.com/pages/breakout
- Arkanoid mechanic summary: https://strategywiki.org/wiki/Arkanoid
- Arcade high-score initials and cabinet culture: https://arcadeblogger.com/2021/01/31/anatomy-of-arcade-high-score-tables/
- Continue / insert coin arcade phrase history: https://www.denofgeek.com/games/the-origins-of-the-video-game-continue-screen/
- Merriam-Webster on “grody” and 1980s Southern California / San Fernando Valley slang: https://www.merriam-webster.com/wordplay/words-we-dont-hear-anymore/grody
- Merriam-Webster on “dis/diss” entering wider use in the 1980s: https://www.merriam-webster.com/wordplay/dis-from-early-rap-to-academic-journals
- Frank Zappa / Moon Unit Zappa “Valley Girl” 1982 cultural reference: https://www.prnewswire.com/news-releases/frank-zappas-satirical-acid-tongued-valley-girl-like-totally-turns-40-301626072.html
- Retro grid / synthwave visual language: https://indieground.net/blog/80s-grid-history/

## 3. Naming Principles

### 3.1 Keep labels short

Most labels should be **1–3 words**.

Good:

- `Rad Rail`
- `Turbo Tape`
- `Laser Lock`
- `Bogus Brick`
- `Neon Drift`

Too long:

- `Totally Radical Mega Paddle Extension Power-Up`
- `The Gnarly Ball That Destroys Multiple Bricks`

### 3.2 Pair flavor with mechanics

A label can be stylized, but the tooltip should explain the mechanic plainly.

Example:

```yaml
id: powerup_rad_rail
displayName: Rad Rail
shortCallout: RAD RAIL!
description: Paddle widens for 12 seconds.
```

### 3.3 Slang density rules

Use this intensity scale:

| Context | Slang Density | Rule |
|---|---:|---|
| Power-up name | Medium | 1 slang/fashion word is fine. |
| Brick name | Low-medium | Keep the mechanic guessable. |
| Level name | Medium-high | Can be more poetic. |
| Tooltip | Low | Explain plainly. |
| One-shot callout | High | Big slang is okay. |
| Error/settings/save UI | Low | Clarity first. |

### 3.4 Avoid parody overload

Do **not** stack multiple slang terms in one label unless it is intentionally comedic.

Good: `Totally Toasted!`

Bad: `Like Totally Bodacious Gnarly Tubular Toasted, Dude!`

## 4. Approved Lexicon

### 4.1 Positive / reward words

Use for power-ups, perfect clears, bonus screens, achievements, and level clears.

| Term | Use | Notes |
|---|---|---|
| Rad | General positive | Best all-purpose term. |
| Radical | Bigger version of rad | Great for major rewards. |
| Gnarly | Extreme / difficult / awesome | Works for hazards or skill shots. |
| Tubular | Over-the-top good | Use sparingly; very 80s. |
| Bodacious | Big bonus / oversized power | Use sparingly. |
| Choice | Clean, stylish success | Good for perfect shots. |
| Fresh | New / stylish / upgraded | Good for unlocks. |
| Wicked | Strong / dangerous / cool | Works for hard levels. |
| Killer | Strong success | Use carefully for E-rated tone. |
| Boss | Excellent / high-status | Good for achievements. |
| Slammin’ | Big impact | Good for combo events. |
| Stoked | Excited / charged | Good for ready states. |
| Righteous | Heroic success | Good for perfect clears. |
| Mondo | Huge / extreme | Good prefix for bigger variants. |

### 4.2 Negative / failure words

Use for hazards, debuffs, misses, and bad pickups.

| Term | Use | Notes |
|---|---|---|
| Bogus | Bad / fake / cursed | Best all-purpose negative term. |
| Bummer | Mild failure | Good for ball loss. |
| Grody | Gross / unpleasant | Good for poison/corrosive bricks. |
| Harsh | Tough hit / penalty | Good for warnings. |
| Wiped Out | Lost ball / run ended | Surf/skate feel. |
| Toasted | Destroyed / burned | Good for brick destruction. |
| Burned | Temporary loss | Good for debuffs. |
| Static | Disruption | Good for visual/noise debuffs. |
| Meltdown | Critical failure | Good for game-over or boss hazard. |
| No Way | Hard denial | Good for indestructible bricks. |

### 4.3 Action verbs

Use for buttons, achievements, and event text.

- Blast
- Smash
- Shred
- Zap
- Cruise
- Drift
- Bounce
- Bank
- Break
- Boost
- Burn
- Charge
- Catch
- Drop
- Flip
- Jam
- Lock
- Pop
- Rip
- Slam
- Spark
- Split
- Sweep
- Warp
- Wipe

### 4.4 Arcade cabinet words

Use these throughout the UI to strengthen the coin-op personality.

- Insert Token
- Press Start
- Continue?
- Game Over
- High Score
- Enter Initials
- Bonus Round
- Extra Ball
- Player One
- Ready!
- Stage
- Wave
- Clear
- Combo
- Jackpot
- Attract Mode
- Cabinet
- Scoreboard
- Reset
- Tilt

### 4.5 Synthwave / retro-future words

Use for level names, visual modifiers, power-up variants, and cosmetic names.

- Neon
- Chrome
- Laser
- Vector
- Grid
- Wireframe
- VHS
- Cassette
- Mixtape
- CRT
- Scanline
- Static
- Glitch
- Prism
- Plasma
- Hologram
- Horizon
- Skyline
- Sunset
- Afterglow
- Nightdrive
- Circuit
- Arcade
- Token
- Cabinet
- Galleria
- Mall
- Coast
- Boardwalk

## 5. Words to Avoid or Use Carefully

Some 80s-era slang has aged badly, can be insensitive, or may not match the desired tone.

| Avoid / limit | Reason | Safer alternative |
|---|---|---|
| Spaz | Often ableist. | Glitch, jitter, wild, haywire. |
| Retarded | Offensive. | Bogus, busted, broken. |
| Gay as insult | Offensive. | Bogus, lame, weak, uncool. |
| Bitchin’ | Period-authentic but harsher. | Rad, boss, killer, choice. |
| Gag me with a spoon | Very specific parody phrase; gets old fast. | Grody, no way, bummer. |
| Drug references | May affect rating/tone. | Boost, charge, turbo, juice. |
| Direct movie/game references | Trademark/copyright risk. | Use generic retro-future wording. |

## 6. Core Game Object Naming

Use normal terms internally. Display names can be stylized.

| Internal Concept | Plain UI Term | Flavor Alternatives |
|---|---|---|
| Paddle | Paddle | Rail, Cruiser, Chrome Rail, Rad Rail |
| Ball | Ball | Orb, Shot, Spark, Comet, Neon Ball |
| Brick | Brick | Block, Tile, Wall, Cell |
| Level | Stage | Scene, Track, Wave, Zone |
| Run | Run | Mixtape, Tape, Session, Nightdrive |
| Seed | Seed | Tape ID, Cabinet Code, Grid Code |
| Power-up | Power-Up | Capsule, Token, Tape, Chip, Pickup |
| Debuff | Hazard | Bogus Pickup, Static, Curse, Jam |
| Boss | Boss Wall | Mega Wall, Bricklord, Wall Core |

Recommended default:

- Use **Stage** for individual levels.
- Use **Mixtape** for a generated run.
- Use **Tape ID** for seed.
- Use **Capsule** or **Token** for power-up pickups.

Example:

```yaml
runLabel: Mixtape
seedLabel: Tape ID
levelLabel: Stage
powerupPickupLabel: Capsule
leaderboardInitialsLabel: Enter Initials
```

## 7. Power-Up Naming Guide

Power-up names should be energetic, short, and readable during action. The `description` field should be mechanical and plain.

| Mechanic | Display Name | Callout | Description |
|---|---|---|---|
| Paddle grows wider | Rad Rail | RAD RAIL! | Paddle widens for a short time. |
| Paddle shrinks | Skinny Rail | SKINNY RAIL! | Paddle narrows for a short time. |
| Ball slows down | Chill Mode | CHILL MODE! | Ball speed decreases for a short time. |
| Ball speeds up | Turbo Tape | TURBO TAPE! | Ball speed increases for a short time. |
| Multiball | Party Split | PARTY SPLIT! | Adds extra balls to the playfield. |
| Sticky paddle | Catch Wave | CATCH WAVE! | Catch and release the ball from the paddle. |
| Laser paddle | Laser Fingers | LASER FINGERS! | Paddle fires lasers upward. |
| Piercing ball | Breakthru | BREAKTHRU! | Ball punches through bricks without bouncing. |
| Explosive ball | Boom Ball | BOOM BALL! | Ball explodes bricks near impact. |
| Fireball | Hot Shot | HOT SHOT! | Ball burns through weaker bricks. |
| Shield / bottom wall | Safety Net | SAFETY NET! | Saves one missed ball. |
| Extra ball/life | 1-Up Token | 1-UP! | Adds one extra ball/life. |
| Score multiplier | Mondo Points | MONDO POINTS! | Score multiplier is increased. |
| Combo boost | Slam Chain | SLAM CHAIN! | Combos are worth more for a short time. |
| Aim preview | Vector Sight | VECTOR SIGHT! | Shows the ball’s projected path. |
| Magnet paddle | Magno-Rail | MAGNO-RAIL! | Paddle lightly pulls the ball toward center. |
| Duplicate paddle | Twin Rail | TWIN RAIL! | Adds a second paddle or helper rail. |
| Ball steering | Steerable Streak | STEER IT! | Player can influence ball direction. |
| Slow motion | Freeze Frame | FREEZE FRAME! | Temporarily slows the whole stage. |
| Screen-clearing bomb | Neon Nuke | NEON NUKE! | Destroys a cluster of bricks. |
| Random power-up | Mystery Tape | MYSTERY TAPE! | Grants a random power-up. |
| Random hazard | Bogus Tape | BOGUS TAPE! | Triggers a random hazard. |
| Paddle moves faster | Cruise Control | CRUISE! | Paddle movement speed increases. |
| Paddle moves slower | Sticky Shoes | HARSH! | Paddle movement speed decreases. |
| Reverse controls | Wrong-Way Ray | NO WAY! | Paddle controls are reversed briefly. |

### Power-up implementation schema

Use this shape when creating data assets:

```yaml
id: powerup_rad_rail
category: powerup
displayName: Rad Rail
shortCallout: RAD RAIL!
description: Paddle widens for 12 seconds.
mechanicKeywords:
  - paddle
  - width
  - temporary
slangIntensity: medium
isPositive: true
```

## 8. Brick Naming Guide

Brick names should preserve mechanical readability. If the player cannot infer the behavior from the name, use a plain tooltip.

| Mechanic | Display Name | Short Tooltip | Hit / Break Text |
|---|---|---|---|
| Normal one-hit brick | Basic Brick | Breaks in one hit. | POP! |
| Two-hit brick | Tough Stuff | Takes two hits. | TOUGH! |
| Multi-hit armored brick | Chrome Block | Takes multiple hits. | CLANG! |
| Indestructible brick | No-Way Wall | Cannot be destroyed. | NO WAY! |
| Explosive brick | Boom Box | Explodes nearby bricks. | BOOM! |
| Moving brick | Cruise Brick | Moves along a path. | CRUISE! |
| Invisible brick | Sneak Brick | Appears when hit or scanned. | SURPRISE! |
| Flickering brick | Static Tile | Flickers in and out. | STATIC! |
| Score bonus brick | Jackpot Brick | Gives bonus points. | JACKPOT! |
| Combo brick | Chain Brick | Extends combo timer. | CHAIN! |
| Speed-up brick | Turbo Tile | Speeds up the ball. | TURBO! |
| Slow-down brick | Chill Brick | Slows down the ball. | CHILL! |
| Laser-trigger brick | Laser Lock | Opens when shot. | UNLOCKED! |
| Switch brick | Flip Switch | Toggles stage elements. | FLIPPED! |
| Portal brick | Warp Tile | Teleports the ball. | WARP! |
| Splitter brick | Split Brick | Splits the ball. | SPLIT! |
| Hazard brick | Bogus Brick | Triggers a penalty. | BOGUS! |
| Corrupt brick | Grody Block | Spreads or corrupts nearby bricks. | GRODY! |
| Shield brick | Forcefield Brick | Protects nearby bricks. | SHIELD! |
| Prism brick | Prism Popper | Changes ball angle/color. | PRISM! |
| Magnet brick | Magnetron | Pulls the ball slightly. | MAGNO! |
| Boss brick | Wall Core | Destroy to clear the boss wall. | CORE HIT! |

### Brick implementation schema

```yaml
id: brick_boom_box
category: brick
displayName: Boom Box
shortTooltip: Explodes nearby bricks.
onHitCallout: BOOM!
mechanicKeywords:
  - explosive
  - areaDamage
slangIntensity: medium
isHazard: false
```

## 9. Hazard / Debuff Naming Guide

Hazards should feel mischievous rather than punishingly cruel.

| Mechanic | Display Name | Callout | Description |
|---|---|---|---|
| Reverse controls | Wrong-Way Ray | WRONG WAY! | Paddle controls reverse briefly. |
| Paddle shrink | Skinny Rail | SKINNY RAIL! | Paddle narrows temporarily. |
| Ball speed spike | Turbo Trouble | TURBO TROUBLE! | Ball speeds up temporarily. |
| Visual noise | Static Jam | STATIC JAM! | Screen distortion briefly increases. |
| Hidden bricks | Blackout | LIGHTS OUT! | Some bricks become hidden. |
| Random angle change | Ricochet Riot | RIOT! | Ball bounces at a wild angle. |
| Score penalty | Bogus Bonus | BOGUS! | Score bonus is reduced or stolen. |
| Paddle drag | Sticky Shoes | HARSH! | Paddle moves slower briefly. |
| Extra brick spawn | Brickstorm | BRICKSTORM! | New bricks appear. |
| Power-up fakeout | Fake Tape | FAKE OUT! | Pickup reveals a hazard. |

## 10. Level Naming Guide

### 10.1 Level-name formula

Prefer one of these patterns:

```text
[80s modifier] + [place/object]
[neon/synth word] + [motion word]
[mall/arcade word] + [danger word]
[slang adjective] + [brick/wall/grid word]
```

Examples:

- `Neon Boardwalk`
- `Chrome Galleria`
- `Turbo Grid`
- `Bogus Boulevard`
- `Radical Ramp`

### 10.2 Level sets / worlds

#### World 1: The Galleria

Beginner-friendly mall / arcade / pop-culture names.

1. Neon Food Court
2. Token Fountain
3. Escalator Drift
4. Galleria Grid
5. Prize Counter
6. Arcade Atrium
7. Mallrat Maze
8. Cassette Kiosk
9. Chrome Carousel
10. After-Hours Arcade

#### World 2: Sunset Circuit

Synthwave highway and horizon names.

1. Sunset Sprint
2. Vector Vista
3. Horizon Heat
4. Nightdrive Neon
5. Turbo Overpass
6. Wireframe Canyon
7. Laser Lane
8. Chrome Causeway
9. Prism Parkway
10. Afterglow Run

#### World 3: Boardwalk Breakout

Surf/skate/coastal energy.

1. Rad Pier
2. Gnarly Break
3. Tubular Tide
4. Boardwalk Blitz
5. Wipeout Wharf
6. Coastline Combo
7. Mondo Marina
8. Neon Lagoon
9. Sunset Halfpipe
10. Breaker Bay

#### World 4: VHS Void

Glitch, CRT, cassette, and static names.

1. Static Alley
2. Scanline Sector
3. Tape Hiss Tunnel
4. Glitch Grotto
5. CRT Cathedral
6. Rewind Ridge
7. Tracking Trouble
8. Pause Screen Panic
9. VCR Vortex
10. The Lost Tape

#### World 5: Chrome Core

Harder, more mechanical, more boss-like.

1. Chrome Lockdown
2. Circuit Breaker
3. Laser Lock
4. Wall Core Alpha
5. Neon Reactor
6. Prism Protocol
7. Grid Meltdown
8. Boss Blockade
9. Cabinet Overdrive
10. Final Breakthru

### 10.3 Procedural level name pools

Use these arrays to generate names.

```json
{
  "adjectives": [
    "Rad", "Radical", "Gnarly", "Tubular", "Bogus", "Mondo", "Chrome", "Neon", "Laser", "Vector", "Static", "Turbo", "Prism", "Wicked", "Choice", "Fresh"
  ],
  "places": [
    "Grid", "Galleria", "Boardwalk", "Boulevard", "Causeway", "Circuit", "Canyon", "Lagoon", "Pier", "Arcade", "Atrium", "Skyline", "Horizon", "Overpass", "Vortex", "Reactor", "Kiosk", "Food Court"
  ],
  "motions": [
    "Drift", "Dash", "Blitz", "Run", "Cruise", "Sprint", "Sweep", "Break", "Jam", "Rampage", "Bounce", "Loop", "Rush"
  ]
}
```

Generation patterns:

```text
{adjective} {place}
{place} {motion}
{adjective} {motion}
{adjective} {place} {motion}   // rare; use for boss or late-game stages
```

## 11. UI Copy Pack

### 11.1 Main menu

| UI Element | Recommended Label | Alternatives |
|---|---|---|
| Start game | Press Start | Get Bricked, Start Run, Drop Token |
| Daily run | Daily Tape | Today’s Tape, Daily Mixtape |
| Seeded run | Tape ID | Cabinet Code, Grid Code |
| Level select | Stage Select | Pick a Stage, Choose Track |
| Leaderboard | High Scores | Scoreboard, Hall of Rad |
| Settings | Tune Cabinet | Options, Cabinet Setup |
| Credits | Crew | Cabinet Crew, Backglass Credits |
| Quit | Power Down | Exit Cabinet |

### 11.2 In-game HUD

| Concept | Label |
|---|---|
| Score | SCORE |
| High score | HIGH SCORE |
| Lives | BALLS |
| Current level | STAGE |
| Combo | COMBO |
| Multiplier | MULTI |
| Seed | TAPE ID |
| Time | TIMER |
| Boss health | WALL CORE |

### 11.3 Event callouts

Keep these big, short, and readable.

#### Start / ready

- READY!
- GET BRICKED!
- PLAYER ONE, GO!
- DROP IN!
- LOCK THE GRID!

#### Ball loss

- BUMMER!
- WIPED OUT!
- MISSED IT!
- HARSH!
- TRY AGAIN!

#### Game over

- GAME OVER
- TOTALLY TOASTED
- WIPED OUT
- CABINET CLOSED
- NO MORE BALLS

#### Level clear

- STAGE CLEAR!
- RAD CLEAR!
- WALL WIPED!
- BRICKED IT!
- RIGHTEOUS!

#### Perfect / no miss

- PERFECT RUN!
- CHOICE CLEAR!
- NO-MISS MAGIC!
- RADICAL!
- BOSS MOVES!

#### New high score

- NEW HIGH SCORE!
- DROP YOUR INITIALS!
- HALL OF RAD!
- SCOREBOARD HERO!
- PLAYER ONE RULES!

#### Continue

- CONTINUE?
- INSERT TOKEN?
- ONE MORE RUN?
- DON’T BAIL!
- KEEP IT RAD?

### 11.4 Settings text

Settings should use light flavor but remain clear.

| Setting | Label | Description |
|---|---|---|
| Music volume | Synth Volume | Adjust music volume. |
| SFX volume | Cabinet SFX | Adjust sound effects. |
| Screen shake | Rumble | Adjust impact shake. |
| Visual effects | Neon Glow | Adjust bloom and glow intensity. |
| CRT effect | Scanlines | Toggle CRT-style scanlines. |
| Colorblind mode | Clear Colors | Improves color readability. |
| Input remap | Tune Controls | Change keyboard/controller bindings. |
| Difficulty | Heat Level | Adjust run difficulty. |

## 12. Achievements

Use achievement names that sound like arcade cabinet bragging rights.

| Achievement Trigger | Name | Description |
|---|---|---|
| Clear first stage | First Brick | Clear your first stage. |
| Clear without losing a ball | No-Miss Magic | Clear a stage without losing a ball. |
| Trigger multiball | Party Started | Launch multiball. |
| Maintain long combo | Combo Commander | Reach a long combo chain. |
| Destroy many bricks with one explosion | Boom Box Hero | Destroy multiple bricks with one blast. |
| Hit only edge shots | Edge Lord | Clear bricks with precise bank shots. |
| Use laser power-up | Laser Fingers | Fire the laser paddle. |
| Beat a boss wall | Wall Core Wrecked | Destroy a boss wall core. |
| Clear a seeded run | Tape Master | Complete a seeded mixtape. |
| Set high score | Hall of Rad | Enter the high score table. |
| Clear daily run | Daily Drop | Complete today’s tape. |
| Finish on one ball | One-Ball Wonder | Complete a stage with one ball remaining. |
| Collect many power-ups | Capsule Collector | Collect many capsules in one run. |
| Hit max multiplier | Mondo Multi | Reach maximum multiplier. |
| Finish hardest mode | Totally Bricked | Complete the hardest heat level. |

## 13. Difficulty / Mode Naming

| Difficulty | Display Name | Notes |
|---|---|---|
| Easy | Chill | Friendly speed and forgiving pickups. |
| Normal | Rad | Intended default. |
| Hard | Gnarly | Faster, more hazards. |
| Very hard | Mondo | Heavy brick pressure. |
| Nightmare | Bogus | Optional chaos mode. |

Alternative label: `Heat Level` instead of `Difficulty`.

```yaml
heatLevels:
  - id: chill
    displayName: Chill
  - id: rad
    displayName: Rad
  - id: gnarly
    displayName: Gnarly
  - id: mondo
    displayName: Mondo
  - id: bogus
    displayName: Bogus
```

## 14. Run / Seed Naming

Since the game is procedural and seed-based, lean into cassette / mixtape language.

| System Concept | Display Label | Example Copy |
|---|---|---|
| Seed | Tape ID | Enter a Tape ID to replay the same mixtape. |
| Generated run | Mixtape | This mixtape is locked and ready. |
| Random seed | Shuffle Tape | Generate a fresh Tape ID. |
| Daily seed | Daily Tape | Everyone gets the same Tape ID today. |
| Share seed | Share Tape | Send this Tape ID to a friend. |
| Save run | Save Tape | Save this generated mixtape. |
| Replay seed | Rewind Tape | Replay this Tape ID. |

Example copy:

```yaml
seedInputLabel: Tape ID
seedPlaceholder: ENTER-TAPE-ID
randomSeedButton: Shuffle Tape
copySeedButton: Share Tape
replaySeedButton: Rewind Tape
seedLockedMessage: Mixtape locked. Same bricks, same chaos.
```

## 15. Modifier Naming

Use these for run modifiers, paddle modifiers, ball modifiers, and stage modifiers.

### 15.1 Paddle modifiers

| Mechanic | Name | Description |
|---|---|---|
| Wider paddle | Rad Rail | Wider paddle. |
| Narrow paddle | Skinny Rail | Narrower paddle. |
| Faster paddle | Cruise Control | Faster movement. |
| Slower paddle | Sticky Shoes | Slower movement. |
| Sticky paddle | Catch Wave | Catch and release ball. |
| Laser paddle | Laser Fingers | Fire upward. |
| Shield paddle | Safety Net | Saves one miss. |
| Split paddle | Twin Rail | Adds a helper rail. |
| Paddle wraps screen | Wrap Rail | Exit one side, enter the other. |
| Paddle leaves trail | Afterglow Rail | Trail can deflect once. |

### 15.2 Ball modifiers

| Mechanic | Name | Description |
|---|---|---|
| Faster ball | Turbo Ball | Faster movement. |
| Slower ball | Chill Ball | Slower movement. |
| Piercing ball | Breakthru Ball | Punches through bricks. |
| Explosive ball | Boom Ball | Explodes on hit. |
| Heavy ball | Mondo Ball | Stronger impacts, slower turns. |
| Tiny ball | Micro Spark | Smaller ball. |
| Split ball | Party Split | Adds extra balls. |
| Fire ball | Hot Shot | Burns weak bricks. |
| Ghost ball | Phantom Bounce | Passes through some bricks. |
| Steering ball | Steerable Streak | Player can influence direction. |

### 15.3 Stage modifiers

| Mechanic | Name | Description |
|---|---|---|
| Dark stage | Blackout | Bricks only show on hit. |
| Timed stage | Beat the Clock | Clear before time runs out. |
| Moving wall | Cruise Wall | Brick wall shifts. |
| Brick respawn | Brickstorm | Bricks can reappear. |
| Extra hazards | Bogus Mode | More hazard pickups. |
| Extra powerups | Capsule Party | More power-up drops. |
| Mirror layout | Mirror Grid | Stage is mirrored. |
| Small arena | Tight Squeeze | Less room to recover. |
| Boss stage | Wall Core | Destroy the core. |
| High-speed stage | Turbo Grid | Everything moves faster. |

## 16. Flavor Text Patterns

Use these sentence patterns to generate consistent copy.

### 16.1 Power-up description pattern

```text
[Plain mechanic] for [duration/condition].
```

Examples:

- `Paddle widens for 12 seconds.`
- `Ball explodes nearby bricks on impact.`
- `Adds two extra balls to the playfield.`

### 16.2 Level intro pattern

```text
Stage {number}: {levelName}
{short hype line}
```

Hype line examples:

- `Keep it clean. Keep it rad.`
- `The grid is hot tonight.`
- `No bogus bounces.`
- `Drop in and break out.`
- `The wall thinks it can stop you.`

### 16.3 Results screen pattern

```text
{resultCallout}
Score: {score}
Best Combo: {combo}
Balls Lost: {ballsLost}
Tape ID: {seed}
```

Result callout examples:

- `RAD CLEAR!`
- `CHOICE RUN!`
- `TOTALLY TOASTED!`
- `HALL OF RAD!`

### 16.4 Achievement pattern

```text
{name}
{plain description}
```

Do not make achievement descriptions too slangy. The name carries the flavor.

## 17. Random Label Generation Rules

When the agent generates a display name:

1. Use at most **one** overt slang word per label.
2. Favor alliteration when possible: `Turbo Tape`, `Bogus Brick`, `Prism Popper`.
3. Avoid direct references to real films, bands, brands, or existing game names.
4. Keep power-up callouts uppercase and under 16 characters when possible.
5. Use plain mechanic descriptions.
6. Do not rename core concepts so aggressively that the player cannot understand them.

### Good generated names

- `Rad Rail`
- `Turbo Tape`
- `Static Jam`
- `Chrome Block`
- `Neon Drift`
- `Bogus Bonus`
- `Prism Popper`
- `Vector Sight`

### Bad generated names

- `Totally Gnarly Radical Tubular Rail` — too much slang.
- `The Pink VHS Death Paddle` — too long and unclear.
- `Tron Wall` — direct IP reference.
- `Spaz Ball` — avoid this term.
- `Bitchin’ Brick` — period-authentic but not ideal for broad rating.

## 18. Sample Data Pack

Use this as starter content.

```yaml
powerups:
  - id: powerup_rad_rail
    displayName: Rad Rail
    shortCallout: RAD RAIL!
    description: Paddle widens for 12 seconds.

  - id: powerup_party_split
    displayName: Party Split
    shortCallout: PARTY SPLIT!
    description: Adds two extra balls to the playfield.

  - id: powerup_laser_fingers
    displayName: Laser Fingers
    shortCallout: LASER FINGERS!
    description: Paddle fires lasers upward for 10 seconds.

  - id: powerup_breakthru
    displayName: Breakthru
    shortCallout: BREAKTHRU!
    description: Ball punches through bricks without bouncing for 8 seconds.

  - id: powerup_vector_sight
    displayName: Vector Sight
    shortCallout: VECTOR SIGHT!
    description: Shows the ball’s projected path for 10 seconds.

bricks:
  - id: brick_basic
    displayName: Basic Brick
    shortTooltip: Breaks in one hit.
    onHitCallout: POP!

  - id: brick_chrome_block
    displayName: Chrome Block
    shortTooltip: Takes multiple hits.
    onHitCallout: CLANG!

  - id: brick_boom_box
    displayName: Boom Box
    shortTooltip: Explodes nearby bricks.
    onHitCallout: BOOM!

  - id: brick_no_way_wall
    displayName: No-Way Wall
    shortTooltip: Cannot be destroyed.
    onHitCallout: NO WAY!

  - id: brick_static_tile
    displayName: Static Tile
    shortTooltip: Flickers in and out.
    onHitCallout: STATIC!

levels:
  - id: level_neon_food_court
    displayName: Neon Food Court
    introLine: Keep it clean. Keep it rad.

  - id: level_galleria_grid
    displayName: Galleria Grid
    introLine: The cabinet is warmed up.

  - id: level_turbo_overpass
    displayName: Turbo Overpass
    introLine: The grid is hot tonight.

  - id: level_vhs_void
    displayName: VHS Void
    introLine: Watch the tracking.

  - id: level_final_breakthru
    displayName: Final Breakthru
    introLine: One wall left. No bogus bounces.
```

## 19. Final Agent Checklist

Before adding or generating any label, verify:

- Is it short enough to read during arcade action?
- Does the tooltip explain the mechanic plainly?
- Does it use at most one strong slang word?
- Does it avoid insensitive/outdated slang?
- Does it avoid direct references to copyrighted IP?
- Does it fit neon / arcade / brick-breaker language?
- Would it look good in all caps on a CRT-style HUD?

Default answer when uncertain:

```text
Use plain arcade wording with one neon or 80s modifier.
```

Examples:

- `Chrome Brick`
- `Turbo Ball`
- `Neon Grid`
- `Rad Rail`
- `Bogus Pickup`
