# Brickwave '84 Style Guide

## Core Direction

`Brickwave '84` should look like a brick-breaker being played inside an oversized neon arcade cabinet. The target style is `1980s synthwave retro-futurism`: dark violet skies, electric cyan and hot magenta highlights, chrome-like UI accents, perspective grids, and controlled CRT-style glow.

The short version is:

- `TRON` energy
- synthwave color and glow
- arcade cabinet framing
- gameplay readability first

This is not:

- soft pastel vaporwave
- grimy cyberpunk
- flat minimalist mobile UI
- random rainbow neon with no hierarchy

## Style Pillars

1. `Arcade Spectacle`
   The game should feel theatrical and oversized, like a machine meant to pull people across the room with light and motion.
2. `Dark Stage, Hot Highlights`
   Most of the screen should be a dark base that lets the important objects burn bright.
3. `Readable Action First`
   The ball, paddle, brick strength, and pickup polarity must stay legible even when the style gets flashy.
4. `Retro-Future Tech`
   Shapes should feel like a future imagined in the 1980s: grids, stripes, chrome bars, hard edges, smoked glass, and luminous trim.
5. `Controlled Nostalgia`
   Use CRT and cabinet cues as seasoning, not noise. The goal is evocative, not muddy.

## Primary References

These references are directional, not assets to copy:

- `SynthWave '84` for palette logic and selective glow:
  [README](https://github.com/robb0wen/synthwave-vscode),
  [theme json](https://raw.githubusercontent.com/robb0wen/synthwave-vscode/master/themes/synthwave-color-theme.json),
  [glow css](https://raw.githubusercontent.com/robb0wen/synthwave-vscode/master/synthwave84.css),
  [theme template](https://raw.githubusercontent.com/robb0wen/synthwave-vscode/master/src/js/theme_template.js),
  [editor chrome](https://raw.githubusercontent.com/robb0wen/synthwave-vscode/master/src/css/editor_chrome.css)
- `TRON`, `Blade Runner`, and `Back to the Future` as core retro-futurist mood anchors
- Arcade cabinet design cues such as the backlit marquee, bezel framing, and illuminated control area:
  [Arcade cabinet overview](https://en.wikipedia.org/wiki/Arcade_cabinet)
- CRT display behavior such as phosphor glow, scanlines, and softened pixel edges:
  [CRT filter overview](https://www.retrotechlab.com/best-crt-filters-and-shaders-for-authentic-retro-gameplay/)
- A concise breakdown of synthwave motifs like neon pink/cyan, grid lines, striped suns, and chrome type:
  [Aesthetics Exploration: Synthwave](https://www.aesdes.org/2024/01/24/aesthetics-exploration-synthwave/)

Key takeaways from the `SynthWave '84` reference set:

- dark violet and indigo bases carry most surfaces
- hot magenta and electric cyan handle the identity accents
- warm coral and gold provide secondary heat
- the strongest glow is reserved for selected or high-energy elements
- magenta-to-cyan edge stripes and lit chrome details sell the machine-like presentation

## Visual Summary

If someone glances at a screenshot, it should read as:

- a dark machine interior
- a glowing playfield floating in front of a horizon
- bright geometric gameplay objects
- menu states that resemble an arcade attract screen
- a subtle sense that the whole image is being viewed through a lit display surface

## Palette System

Use a narrow, intentional palette. The game should not try to make every element equally loud.

| Role | Color | Suggested Hex |
| --- | --- | --- |
| Void black | near-black plum | `#120914` |
| Night indigo | primary deep background | `#241B2F` |
| Deep violet | secondary background / panels | `#2A2139` |
| Electric cyan | hero accent / tech light | `#03EDF9` |
| Hot magenta | hero accent / selection / neon trim | `#FF7EDB` |
| Laser pink | saturated glow accent | `#FC28A8` |
| Sunset coral | warm impact color | `#F97E72` |
| Danger red | fail states / harmful pickups | `#FE4450` |
| Bonus mint | beneficial pickups / success cues | `#72F1B8` |
| Arcade gold | ball energy / burst / reward | `#FEDE5D` |
| Chrome white | brightest text / specular pop | `#FDFDFD` |

### Palette Rules

- Backgrounds should stay in the black, indigo, and violet range.
- `Cyan` and `magenta` are the main identity colors. They should do most of the branding work.
- `Gold`, `coral`, and `red` are heat colors. Use them to signal momentum, danger, and scoring spikes.
- `Mint` is reserved for beneficial pickup language so good effects read instantly.
- Unbreakable or inert elements should lean cooler and more muted than active gameplay objects.
- Prefer one dominant accent and one supporting accent per screen or panel instead of using the full palette at once.

## Materials And Shape Language

### Materials

- glossy black plastic
- smoked glass
- brushed gunmetal
- chrome trim
- emissive neon tubing
- luminous acrylic panels

### Shapes

- bold rectangles
- trapezoids and angle cuts
- horizon grids
- segmented suns
- stripes and light bars
- thin frame lines around important surfaces

Avoid:

- rustic texture
- painterly organic forms
- realistic dirt and corrosion
- cute rounded toy-like silhouettes unless a specific pickup calls for it

## Lighting And Glow

Glow is part of the identity, but it has to be disciplined.

### Glow Priorities

- `Highest`: ball, selected menu item, burst pickups, level-complete highlights
- `Medium`: paddle edge, marquee framing, active buttons, wall trim
- `Low`: most bricks, passive HUD panels, background scenery

### Glow Rules

- Let bright objects bloom outward from a crisp core.
- Keep the center of interactive objects sharp enough to read collision and position.
- Use bloom to create halos, not fog.
- Large background glows should be broad and soft.
- Small gameplay glows should be tight and punchy.
- Never let post effects hide the ball against the background.

## Background And Environment Direction

The playfield should feel suspended in a retro-futurist stage set.

Recommended layering:

1. A very dark sky or interior backdrop
2. A horizon glow or dusk gradient
3. Optional grid, wireframe, or faint geometric landscape
4. The gameplay board framed like a cabinet screen or lit bezel
5. Subtle overlay treatment such as scanlines, vignette, or screen bloom

Background motion should be slow and atmospheric. Gameplay motion should remain the main event.

## CRT And Cabinet Cues

Use these sparingly to sell the fantasy that the game lives inside a giant machine:

- backlit marquee treatment for titles and top-level menu headers
- bezel-like framing around the play area
- faint scanline or screen-door texture
- light curvature, bloom, or vignette if it stays subtle
- illuminated control-panel language for buttons, prompts, and focus states

Do not overdo:

- VHS distortion
- heavy chromatic aberration
- extreme blur
- constant flicker
- effects that make aiming harder

## Gameplay Readability Rules

Style only works if the game remains instantly parseable.

- The `ball` should usually be the brightest or second-brightest moving object on screen.
- The `paddle` needs a stable silhouette that does not vanish into background glow.
- Brick durability tiers must differ in both hue and value, not hue alone.
- Damaged bricks should visibly shift toward their secondary color so hits are readable at speed.
- Harmful and beneficial pickups must be color-coded consistently across HUD and world space.
- Obstacles should read as heavier, duller, and less emissive than breakable bricks.
- UI overlays must not hide the center of play during active control unless the game is paused or in a state transition.

## UI Direction

UI should feel like illuminated cabinet hardware, not standard grey debug boxes.

### Headings

- large
- uppercase or small-caps friendly
- wide tracking
- chrome, white, cyan, or magenta emphasis

### Panels

- dark violet or smoked-black bases
- lit outline or underglow
- occasional magenta-to-cyan accent stripe
- strong silhouette separation from the background

### Buttons And Focus

- focus states should pulse or brighten
- selected actions can use a magenta-to-cyan edge, stripe, or glow
- confirm actions should feel like big arcade buttons, not flat hyperlinks

### HUD

- score and lives should feel like machine readouts
- pickup timers should read like energized status modules
- use minimal clutter; the playfield should stay dominant

## Motion And VFX

Movement should feel musical and electrically charged.

- Use short pulses, sweeps, and flashes instead of long lingering particles.
- Impacts should create light bursts, shard pops, or energy ripples.
- Ball movement can support a restrained trail if it improves speed readability.
- Menu transitions should feel like an attract mode waking up.
- Power-up catches should spike color and light briefly, then settle back fast.

Avoid effects that leave the screen visually busy after the important moment has passed.

## Audio-Visual Mood

Even before the final audio pass, presentation should suggest:

- synth arpeggios
- cabinet speaker thumps
- energized button clicks
- triumphant but slightly melancholic retro-future atmosphere

The tone should be exciting and stylish, not horror-dark or parody-cheesy.

## Current Repo Implementation Notes

This repo already has a color-and-sprite-driven theme pipeline. Use that before introducing hard-coded presentation logic.

- `Assets/Scripts/Gameplay/Data/ThemeDefinition.cs` defines semantic slots for the background, walls, paddle, ball, brick tiers, obstacle bricks, and pickup types.
- `Assets/Scripts/Gameplay/BreakoutThemeService.cs` applies those slots to the current runtime objects.
- The current system is best suited to `palette-first` theme work now, with future room for sprite, material, shader, and overlay upgrades later.

### Slot Guidance For A Future Synthwave Theme

| Theme Slot | Direction |
| --- | --- |
| `Background` | deep indigo or violet base, ideally with a future gradient or backdrop sprite |
| `Wall` | dark steel or violet with cyan edge emphasis |
| `Paddle` | cyan-white light bar or chrome-lit slab |
| `Ball` | gold-white energy core with the highest readability contrast |
| `BrickPrimary` | hot magenta with a darker magenta damage state |
| `BrickSecondary` | coral or gold with a warmer damaged state |
| `BrickTertiary` | electric cyan with a darker blue-cyan damage state |
| `BrickObstacle` | muted steel, slate, or low-emissive violet |
| `PickupBeneficial` | mint or cyan-green |
| `PickupHarmful` | danger red or hot magenta-red |
| `PickupBurst` | arcade gold or bright yellow-white |

## Implementation Priorities

When visual polish work starts, tackle it in this order:

1. Establish a strong synthwave palette through `ThemeDefinition` assets.
2. Improve UI panel, title, and focus-state styling so menus match the target identity.
3. Add restrained glow and bloom support.
4. Add a background treatment such as a gradient, grid, or skyline layer.
5. Add subtle CRT and cabinet framing details.

This order keeps the game readable and cohesive even before advanced shaders or authored art exist.

## Non-Negotiables

- The game must read clearly at gameplay speed.
- The style should feel premium and intentional, not like stock neon wallpaper.
- Glow is an accent system, not a replacement for contrast.
- The synthwave direction should stay consistent across gameplay, UI, and future marketing art.
