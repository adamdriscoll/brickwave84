# Get Bricked — 1980s Synthwave Arcade SFX Pack

These are short mono WAV sound effects designed for a retro breakout-style game with a synthwave / neon arcade feel. They are intentionally simple so they cut through music without taking over the mix.

## Files

| File | Use | Character |
|---|---|---|
| `ball_hit_paddle_neon_boop.wav` | Ball hits player paddle | Warm analog-style boop with a little body. Good for the most common player-facing contact sound. |
| `ball_hit_wall_laser_ping.wav` | Ball hits wall or boundary | Short, bright laser ping. Higher and thinner than the paddle hit so it feels like hard glass/metal. |
| `ball_hit_brick_pixel_thunk.wav` | Ball hits a brick but does not destroy it | Mid-range pixel thunk with a tiny noise edge. Keeps repeated brick hits readable. |
| `brick_break_crystal_burst.wav` | Brick breaks / is destroyed | Crunchy descending zap with a small crystal burst tail. More satisfying than a normal hit. |
| `drop_pickup_powerup_rise.wav` | Player picks up a falling modifier/drop | Rising arpeggio coin/power-up sound. Should feel positive and fast. |
| `bonus_point_score_jingle.wav` | Bonus point / combo / special score | Bright mini-jingle with delay. Use sparingly for extra reward moments. |

## Unity Import Suggestions

- Import as **WAV**.
- Set **Load Type** to `Decompress On Load` for these tiny clips.
- Set **Compression Format** to `PCM` or `ADPCM`.
- Use **spatialBlend = 0** unless you want left/right movement.
- Slightly randomize pitch for repeated hits:
  - Paddle: `0.96–1.04`
  - Wall: `0.94–1.08`
  - Brick hit: `0.92–1.05`
- Keep `brick_break` and `bonus_point` a little louder than normal contact hits.
- Avoid playing the bonus jingle on every brick; reserve it for combo, special brick, multiplier, or streak events.

## Suggested Mixer Balance

| Sound | Relative Volume |
|---|---:|
| Paddle hit | 0.75 |
| Wall hit | 0.55 |
| Brick hit | 0.60 |
| Brick break | 0.85 |
| Drop pickup | 0.90 |
| Bonus point | 0.95 |

## Optional Variants to Generate Later

- `combo_2x`, `combo_3x`, `combo_max`
- `bad_drop_pickup`
- `level_start`
- `level_clear`
- `ball_lost`
- `game_over`
- `extra_life`
