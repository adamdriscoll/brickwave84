# Slot Guide

## Default Runtime Fallback Paths

| Role | Resource Path | Theme Slot | Material |
| --- | --- | --- | --- |
| Background image | `Assets/Resources/Backgrounds/synthwave.*` | `Background` | Unlit |
| Ball | `Assets/Resources/Sprites/ball.svg` | `Ball` | Additive |
| Brick | `Assets/Resources/Sprites/brick.svg` | `BrickPrimary`, `BrickSecondary`, `BrickTertiary`, `BrickObstacle` | Unlit |
| Paddle | `Assets/Resources/Sprites/paddle.svg` | `Paddle` | Unlit |
| Power-up | `Assets/Resources/Sprites/powerup.svg` | `PickupBeneficial`, `PickupHarmful`, `PickupBurst` | Additive |

## Style Targets By Role

### Ball

- Brightest moving object or close to it
- Gold or white energy core with a simple outer ring
- Avoid overly detailed interiors because the sprite stays small

### Paddle

- Strong horizontal light bar silhouette
- Cyan-forward with chrome or white highlights
- Keep the collision edge easy to read

### Brick

- Clear rectangular read with durability still conveyed mostly by color
- Add bevels, stripe cuts, or inset panels, but keep the shape stable
- Obstacle bricks should feel heavier and less emissive

### Power-up

- Use crisp glowing linework for authored upgrade and special-mechanic icons when it reads better than a badge
- Reduce the icon to the core mechanic mark; for example, a split mechanic can be just a Y path plus three ball rings
- Use a bright center, clean stroked rings/paths, and restrained echo rings so bloom stays controlled
- Color meaning still comes from the theme slot

## Theme Hookups

- `ThemeDefinition` entries can override the sprite in any slot.
- If a theme entry leaves `sprite` empty, runtime falls back to the default `Resources/Sprites/*` asset for that role.
- Keep color logic inside the theme assets whenever the sprite can stay shared.

## Practical Rules

- Prefer one shared SVG per gameplay role unless the request specifically wants theme-specific silhouettes.
- Keep SVG markup hand-editable.
- Prefer white-base art for gameplay sprites because the current runtime theme system tints the imported SVG through `SpriteRenderer.color`.
- Avoid blur filters and other SVG effects that rely on browser-style filter support; build glow through shape language and let bloom handle the halo.
- Exception: when matching existing authored upgrade icons such as `phase-ball.svg`, a restrained `softGlow` filter on white strokes/fills is acceptable if the mark remains legible without the glow.
- Preview icon sprites on a dark project background at both `256px` and `64px`; the small preview should still read as one clean symbol.
- Name new files predictably and keep them in `Assets/Resources/Sprites/` unless the request calls for a new content path.
- If you change the default file names, update the resource loader in `BreakoutGameController`.
