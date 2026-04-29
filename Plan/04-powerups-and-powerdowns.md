# Chunk 04: Power-Ups And Power-Downs

## Goal

Introduce controlled chaos through drops that change the rules moment to moment.

## Systems In Scope

- Drop chance system
- Falling pickup behavior
- Paddle catch detection
- Timed and instant effects
- Effect stacking or conflict rules
- Basic feedback for active effects

## Initial Effect Candidates

- Larger platform
- Smaller platform
- Faster ball
- Slower ball
- Multi-ball
- Temporary paddle gun

## Deliverables

- Bricks can spawn drops on destruction using tunable probabilities
- Pickups fall through the playfield and can be caught or missed
- At least 4 effects implemented end to end
- Basic handling for effect duration, overlap, and cleanup

## Implementation Notes

- Effects should apply through a reusable modifier system instead of one-off hacks.
- Decide early whether negative effects can appear from the same drop table as positive ones.
- Multi-ball should integrate with life-loss rules before content scaling begins.

## Done When

- Drops feel exciting rather than random noise.
- Effects can be added through data plus a bounded amount of logic.

## Explicitly Deferred

- Large effect catalog
- Final VFX and audio polish
