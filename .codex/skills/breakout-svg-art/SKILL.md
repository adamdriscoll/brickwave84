---
name: breakout-svg-art
description: Create, refine, and wire SVG gameplay art for Get Bricked. Use when Codex needs to make or update vector sprites, crisp glowing upgrade or power-up icons, background art hookups, or theme-slot sprite assignments that must follow `Plan/STYLE.md`, live under `Assets/Resources/`, and work with the runtime SVG plus bloom pipeline.
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
5. For upgrade and power-up icons, prefer crisp glowing linework.
   Use a `256` viewBox, reduce the symbol to the fewest readable shapes, and favor white stroked lines/rings with a small shared glow over filled badges, dark cutout borders, or decorative frames.
6. Encode glow with shape design, not blur spam.
   Use inner fills, thin edge strokes, cut lines, and controlled outer rings. Let URP bloom do the halo work for additive sprites. When matching existing upgrade icons such as `phase-ball.svg`, reuse its restrained `softGlow` filter pattern on bright strokes/fills only.
7. Hook the sprite into the runtime theme pipeline.
   The controller loads default fallback art from `Resources/Sprites/ball`, `brick`, `paddle`, and `powerup`.
   Theme-specific overrides still belong in `ThemeDefinition` slot entries when a theme needs a different sprite.
8. Validate after art or hookup changes.
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
- Exception: for authored upgrade/power-up icons that need to visually match `phase-ball.svg`, a small reusable `softGlow` filter is acceptable when the base symbol remains clear without it.
- For bricks and paddle, prioritize silhouette clarity over glow.
- For ball and powerups, design for additive bloom by keeping the center bright and the outer shapes lighter-weight.

## Crisp Glowing Icon Pattern

Use this pattern when polishing upgrade, power-up, or special-mechanic sprites that need the same feel as `phase-ball.svg`.

- Start with a `viewBox="0 0 256 256"` so curves and line weights have enough room to breathe.
- Build the icon as a symbol, not a tiny scene. Keep only the shapes that explain the mechanic.
- Prefer `fill="none"` white strokes with round caps/joins for path lines, rings, arcs, or beams.
- Use faint echo rings or secondary strokes for energy. Avoid dark bordering shapes, heavy filled frames, decorative platforms, and small spark clutter unless the mechanic requires them.
- Apply a single restrained `softGlow` filter to the bright group, then keep linework crisp through strong stroke widths and simple geometry.
- Preview on `#120914` or another dark project background at both `256px` and `64px`; the `64px` preview should still read as one clear symbol.

Minimal filter/group scaffold:

```svg
<defs>
  <filter id="softGlow" x="-60%" y="-60%" width="220%" height="220%">
    <feGaussianBlur stdDeviation="2.25" result="blur"/>
    <feColorMatrix in="blur" type="matrix" values="1 0 0 0 0  0 1 0 0 0  0 0 1 0 0  0 0 0 .8 0" result="glow"/>
    <feMerge>
      <feMergeNode in="glow"/>
      <feMergeNode in="SourceGraphic"/>
    </feMerge>
  </filter>
  <style><![CDATA[
    .icon { fill: none; stroke: #fff; stroke-linecap: round; stroke-linejoin: round; filter: url(#softGlow); }
    .main { stroke-width: 12; opacity: .9; }
    .ring { stroke-width: 9; opacity: .96; }
    .echo { stroke-width: 4; opacity: .32; }
  ]]></style>
</defs>
<g class="icon">
  <!-- Draw the simplest readable mechanic mark here. -->
</g>
```

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
