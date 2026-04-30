---
name: breakout-svg-art
description: Create, refine, and wire SVG gameplay art for Get Bricked. Use when Codex needs to make or update vector sprites, background art hookups, or theme-slot sprite assignments that must follow `Plan/STYLE.md`, live under `Assets/Resources/`, and work with the runtime SVG plus bloom pipeline.
---

# Breakout SVG Art

## Overview

Create gameplay-facing SVG art for `Get Bricked` in a way that stays readable at play speed, matches the synthwave arcade style guide, and plugs directly into the runtime theme system.

Read `Plan/STYLE.md` before changing art direction. Treat the SVGs as crisp shape language first and glow carriers second.

## Workflow

1. Inspect the current role and hookup path.
   Use [references/slot-guide.md](references/slot-guide.md) for the default resource paths, theme slots, and material behavior.
2. Prefer editing or replacing repo-native SVGs under `Assets/Resources/Sprites/`.
   Keep file names stable when possible so the runtime fallback loader keeps working without extra code changes.
3. Generate a starter SVG when it saves time.
   Run `python .codex/skills/breakout-svg-art/scripts/new_svg_template.py --kind <ball|brick|paddle|powerup> --out <path>`.
4. Keep the art readable.
   Use a sharp silhouette, a bright core, and at most one secondary accent. Avoid fuzzy vector filters, noisy gradients, or detail that disappears when the sprite is scaled down.
5. Encode glow with shape design, not blur spam.
   Use inner fills, thin edge strokes, cut lines, and controlled outer rings. Let URP bloom do the halo work for additive sprites.
6. Hook the sprite into the runtime theme pipeline.
   The controller loads default fallback art from `Resources/Sprites/ball`, `brick`, `paddle`, and `powerup`.
   Theme-specific overrides still belong in `ThemeDefinition` slot entries when a theme needs a different sprite.
7. Validate after art or hookup changes.
   Run `python .codex/skills/unity-compile/scripts/run_unity_compile.py`.
   If Unity has not generated `.meta` files for new assets yet, let Unity import them before finalizing git state.

## SVG Rules

- Use `viewBox` and geometric shapes that scale cleanly.
- Keep the canvas tight to the sprite silhouette with a little breathing room for additive edge light.
- Prefer flat fills, light gradients, and crisp strokes over raster effects.
- For the current runtime tint pipeline, prefer white or near-white source art with alpha variation so `SpriteRenderer.color` can do the palette work cleanly.
- Keep backgrounds transparent for gameplay sprites.
- Do not embed bitmap images inside SVGs.
- Avoid SVG filters such as `feGaussianBlur` and avoid depending on complex gradients for core readability; Unity import plus runtime tinting can make those resolve poorly for this project.
- For bricks and paddle, prioritize silhouette clarity over glow.
- For ball and powerups, design for additive bloom by keeping the center bright and the outer shapes lighter-weight.

## Repo Notes

- The runtime controller now enables URP bloom and uses:
  - additive sprite material for `ball` and `powerup`
  - unlit sprite material for `paddle`, `bricks`, `walls`, and the backdrop
- The fallback background image loads from `Assets/Resources/Backgrounds/synthwave.*`.
- If you add a brand-new sprite role, update the runtime loader and document it in `AGENTS.md`.

## Resources

### scripts/

- `new_svg_template.py`: generate a clean starter SVG for core gameplay roles

### references/

- `slot-guide.md`: slot-to-file mapping, color direction, and hookup reminders
