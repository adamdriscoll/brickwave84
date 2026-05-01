# Get Bricked - Power Down SFX

Negative pickup sounds for 1980s synthwave arcade modifiers.

## Files

- `power_down_cursed_pickup_vhs_drop.wav`
  - Main negative power-down pickup.
  - Use when the player collects a harmful modifier.
  - Tone: unstable downward synth, glitch ticks, dark VHS tail.

- `power_down_short_glitch_drop.wav`
  - Shorter alternate version.
  - Good when power-downs happen often or when the game is already sonically busy.

## Unity suggestions

Recommended AudioSource settings:

- Volume: 0.65 - 0.85
- Pitch randomization: 0.95 - 1.05
- Priority: Medium-high
- Spatial Blend: 0 for full 2D UI/gameplay SFX

## Mixing notes

Use this as the opposite of a positive pickup sound. The drop should feel wrong immediately:
downward pitch motion, slight distortion, and a short glitch texture.

For extra juice, briefly tint the collected power-down icon red/purple or shake the paddle
at the same time the sound plays.
