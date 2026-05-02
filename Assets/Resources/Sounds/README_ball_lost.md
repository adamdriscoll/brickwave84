# Brickwave '84 — Ball Lost SFX

Two synthwave arcade sounds for when the ball drops below the paddle and disappears into the void.

## Files

- `ball_lost_to_ether_vhs_drop.wav` — main version. A falling neon tone with a dark VHS-style tail. Best for normal ball loss.
- `ball_lost_to_ether_short_drop.wav` — shorter version. Better if multiple balls can be lost quickly during multiball.

## Unity Suggestions

- Play at `volume = 0.75–0.9`.
- For multiball, use the short version and lower volume to `0.45–0.65` so repeated losses do not get annoying.
- Randomize pitch slightly: `0.96–1.03`.
- Do not play this if the player still has another active ball unless you want each lost ball to feel dramatic. For multiball, consider playing the short version only when the active ball count decreases but the level continues.

## Design Intent

This should feel like the ball is not just missed — it is falling off the neon grid and being absorbed by the 1980s arcade ether.
