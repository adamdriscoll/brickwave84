# Get Bricked — Special Brick Synthwave SFX

Extra one-shot WAV effects for special brick behavior. Designed to match the existing 1980s retro arcade / synthwave sound set: short, bright, synthetic, and easy to layer under music.

## Files

| Brick Type | File | Intended Feel | Unity Notes |
|---|---|---|---|
| Explosive brick | `brick_explosive_neon_boom.wav` | Tiny warning chirp followed by a compressed neon blast | Use when the explosive brick detonates. Keep volume slightly lower if multiple bricks chain-react. |
| Split brick | `brick_split_shard_scatter.wav` | Digital crack plus little glassy shard tones | Use when one brick breaks into multiple pieces. Works well with small pitch randomization. |
| Indestructible brick | `brick_indestructible_vhs_clang.wav` | Rejected metallic synth thud / clang | Use when the ball hits but the brick does not break. Keep this darker and lower than normal brick hits. |
| Spinning brick | `brick_spinning_orbit_zap.wav` | Rotary doppler shimmer with quick ticks | Use on hit, state change, or when it deflects the ball in a special way. |

## Suggested Unity Settings

- Import as mono WAV.
- Disable spatial blend for classic arcade feel, or keep very low spatial blend if bricks are spread across the screen.
- Randomize pitch very slightly per play:
  - Explosive: `0.95–1.03`
  - Split: `0.90–1.12`
  - Indestructible: `0.94–1.02`
  - Spinning: `0.92–1.10`
- Avoid long tails. These are intentionally short so rapid brick hits do not muddy the mix.

## Layering Ideas

- Explosive brick can play this sound plus the regular `brick_break_crystal_burst.wav` at lower volume.
- Split brick can play this sound and then trigger tiny shard particle sounds if you add debris later.
- Indestructible brick should usually replace the regular brick hit sound, not layer with it.
- Spinning brick can use this sound both as a hit reaction and as a subtle UI-like feedback cue when its rotation state changes.
