using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutBrickState
    {
        public BreakoutBrickState(
            BrickDefinition definition,
            Vector2 position,
            int row,
            int column,
            int hitPointsRemaining,
            BreakoutBrickMotionConfig motionConfig)
        {
            Definition = definition;
            Position = position;
            Row = Mathf.Max(0, row);
            Column = Mathf.Max(0, column);
            HitPointsRemaining = Mathf.Max(0, hitPointsRemaining);
            MotionConfig = motionConfig;
        }

        public BrickDefinition Definition { get; }

        public Vector2 Position { get; }

        public int Row { get; }

        public int Column { get; }

        public int HitPointsRemaining { get; }

        public BreakoutBrickMotionConfig MotionConfig { get; }
    }

    internal sealed class BreakoutBrickService
    {
        private const int BrickSortingOrder = 5;

        private readonly BreakoutGameController controller;
        private readonly IList<Brick> bricks;
        private readonly Transform bricksRoot;
        private readonly Vector2 brickSize;
        private readonly Vector2 brickSpacing;
        private readonly Sprite fallbackSprite;
        private readonly Material spriteMaterial;
        private readonly PhysicsMaterial2D physicsMaterial;
        private readonly Func<RunSettings> runSettingsResolver;
        private readonly Func<Rect> movementBoundsResolver;
        private readonly Func<BrickDefinition, ThemeVisualStyle> styleResolver;
        private readonly Action<GameObject> destroyRuntimeObject;
        private bool hasActiveFlickerBricks;
        private BreakoutFlickerBricksSpec activeFlickerBricksSpec;

        public BreakoutBrickService(
            BreakoutGameController controller,
            IList<Brick> bricks,
            Transform bricksRoot,
            Vector2 brickSize,
            Vector2 brickSpacing,
            Sprite fallbackSprite,
            Material spriteMaterial,
            PhysicsMaterial2D physicsMaterial,
            Func<RunSettings> runSettingsResolver,
            Func<Rect> movementBoundsResolver,
            Func<BrickDefinition, ThemeVisualStyle> styleResolver,
            Action<GameObject> destroyRuntimeObject)
        {
            this.controller = controller;
            this.bricks = bricks ?? throw new ArgumentNullException(nameof(bricks));
            this.bricksRoot = bricksRoot;
            this.brickSize = brickSize;
            this.brickSpacing = brickSpacing;
            this.fallbackSprite = fallbackSprite;
            this.spriteMaterial = spriteMaterial;
            this.physicsMaterial = physicsMaterial;
            this.runSettingsResolver = runSettingsResolver ?? throw new ArgumentNullException(nameof(runSettingsResolver));
            this.movementBoundsResolver = movementBoundsResolver ?? throw new ArgumentNullException(nameof(movementBoundsResolver));
            this.styleResolver = styleResolver ?? throw new ArgumentNullException(nameof(styleResolver));
            this.destroyRuntimeObject = destroyRuntimeObject ?? throw new ArgumentNullException(nameof(destroyRuntimeObject));
        }

        public int BuildBrickWall(BreakoutLevelLayoutPlan layoutPlan, float arenaTop)
        {
            if (layoutPlan == null || layoutPlan.BrickRows.Length == 0)
            {
                return 0;
            }

            var requiredBrickCount = 0;
            var layoutRows = layoutPlan.BrickRows;
            var startY = arenaTop - layoutPlan.TopInset;

            for (var row = 0; row < layoutRows.Length; row++)
            {
                var rowCells = layoutRows[row] ?? Array.Empty<BreakoutProceduralBrickCell>();
                var totalWidth = (rowCells.Length * brickSize.x) + (Mathf.Max(0, rowCells.Length - 1) * brickSpacing.x);
                var startX = (-totalWidth * 0.5f) + (brickSize.x * 0.5f);

                for (var column = 0; column < rowCells.Length; column++)
                {
                    var cell = rowCells[column];

                    if (cell == null || cell.Definition == null)
                    {
                        continue;
                    }

                    var position = new Vector2(
                        startX + (column * (brickSize.x + brickSpacing.x)),
                        startY - (row * (brickSize.y + brickSpacing.y)));
                    var brick = CreateBrick(position, cell.Definition, row, column, cell.MotionConfig);

                    if (brick != null && brick.CountsTowardLevelCompletion)
                    {
                        requiredBrickCount++;
                    }
                }
            }

            return requiredBrickCount;
        }

        public int BuildBrickWall(IReadOnlyList<BreakoutBrickState> brickStates)
        {
            if (brickStates == null || brickStates.Count == 0)
            {
                return 0;
            }

            var requiredBrickCount = 0;

            for (var index = 0; index < brickStates.Count; index++)
            {
                var state = brickStates[index];
                var definition = state.Definition;

                if (definition == null || (definition.IsBreakable && state.HitPointsRemaining <= 0))
                {
                    continue;
                }

                var brick = CreateBrick(state.Position, definition, state.Row, state.Column, state.MotionConfig);
                brick?.RestoreHitPoints(state.HitPointsRemaining);

                if (brick != null && brick.CountsTowardLevelCompletion)
                {
                    requiredBrickCount++;
                }
            }

            return requiredBrickCount;
        }

        public Brick CreateBrick(
            Vector2 position,
            BrickDefinition definition,
            int row,
            int column,
            BreakoutBrickMotionConfig motionConfig)
        {
            if (definition == null)
            {
                return null;
            }

            var brickObject = new GameObject($"{definition.DisplayName} {row + 1}-{column + 1}");
            brickObject.transform.SetParent(bricksRoot, false);
            brickObject.transform.position = position;
            brickObject.transform.localScale = new Vector3(
                brickSize.x * definition.SizeMultiplier,
                brickSize.y * definition.SizeMultiplier,
                1f);

            var visualObject = new GameObject("Visual");
            visualObject.transform.SetParent(brickObject.transform, false);

            var spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = fallbackSprite;
            spriteRenderer.sortingOrder = BrickSortingOrder;
            spriteRenderer.sharedMaterial = spriteMaterial;
            BreakoutSpriteRendererUtility.NormalizeScale(spriteRenderer);

            var collider = brickObject.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = physicsMaterial;

            var brick = brickObject.AddComponent<Brick>();
            brick.Initialize(
                controller,
                definition,
                GetEffectiveHitPoints(definition),
                styleResolver(definition),
                motionConfig.Speed,
                motionConfig.InitialDirection,
                row,
                column);
            brick.SetMovementBounds(movementBoundsResolver());
            ApplyActiveFlickerToBrick(brick, row, column, position);
            bricks.Add(brick);
            return brick;
        }

        public BreakoutBrickState[] CaptureBrickStates()
        {
            if (bricks.Count == 0)
            {
                return Array.Empty<BreakoutBrickState>();
            }

            var states = new List<BreakoutBrickState>(bricks.Count);

            for (var index = 0; index < bricks.Count; index++)
            {
                var brick = bricks[index];

                if (brick == null || brick.Definition == null)
                {
                    continue;
                }

                if (brick.Definition.IsBreakable && brick.HitPointsRemaining <= 0)
                {
                    continue;
                }

                states.Add(brick.CaptureState());
            }

            return states.ToArray();
        }

        public bool RemoveBrick(Brick brick)
        {
            return brick != null && bricks.Remove(brick);
        }

        public void DisableAndDestroyBrick(Brick brick)
        {
            if (brick == null)
            {
                return;
            }

            brick.gameObject.SetActive(false);
            destroyRuntimeObject(brick.gameObject);
        }

        public int SpawnSplitBricks(BrickDefinition parentDefinition, Vector2 splitCenter, float specialBrickEffectMultiplier = 1f)
        {
            var splitDefinition = parentDefinition != null ? parentDefinition.SplitBrickDefinition : null;

            if (splitDefinition == null)
            {
                return 0;
            }

            var requiredBrickCount = 0;
            var offsets = BuildSplitBrickOffsets(
                brickSize,
                splitDefinition,
                ResolveSplitBrickFragmentCount(specialBrickEffectMultiplier));

            for (var index = 0; index < offsets.Length; index++)
            {
                var brick = CreateBrick(splitCenter + offsets[index], splitDefinition, 0, index, default);

                if (brick != null && brick.CountsTowardLevelCompletion)
                {
                    requiredBrickCount++;
                }
            }

            return requiredBrickCount;
        }

        public int SpawnBonusBricks(BrickDefinition definition, Vector2 center, int count)
        {
            if (definition == null || count <= 0)
            {
                return 0;
            }

            var requiredBrickCount = 0;
            var offsets = BuildSplitBrickOffsets(brickSize, definition, count);

            for (var index = 0; index < offsets.Length; index++)
            {
                var brick = CreateBrick(ClampBrickPositionToMovementBounds(center + offsets[index], definition), definition, 0, index, default);

                if (brick != null && brick.CountsTowardLevelCompletion)
                {
                    requiredBrickCount++;
                }
            }

            return requiredBrickCount;
        }

        public int GetEffectiveHitPoints(BrickDefinition definition)
        {
            if (definition == null || !definition.IsBreakable)
            {
                return 0;
            }

            var durabilityMultiplier = runSettingsResolver()?.BrickDurabilityMultiplier ?? 1f;
            return Mathf.Max(1, Mathf.RoundToInt(definition.HitPoints * durabilityMultiplier));
        }

        public void ClearBricks()
        {
            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                if (bricks[index] == null)
                {
                    continue;
                }

                DisableAndDestroyBrick(bricks[index]);
            }

            bricks.Clear();
        }

        public int MirrorBrickGridHorizontally(float centerX)
        {
            var mirroredCount = 0;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                brick.MirrorHorizontally(centerX);
                mirroredCount++;
            }

            return mirroredCount;
        }

        public int ApplyRowDrift(BreakoutDriftRowsSpec spec)
        {
            var driftedCount = 0;
            var bounds = movementBoundsResolver();

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                if (brick.Definition == null || brick.IsPendingRemoval)
                {
                    continue;
                }

                var row = brick.CaptureState().Row;
                brick.SetMovementBounds(bounds);
                brick.SetGlitchMotion(spec.Speed, ResolveDriftDirectionForRow(row, spec.StartingDirectionSign));
                driftedCount++;
            }

            return driftedCount;
        }

        internal static Vector2 ResolveDriftDirectionForRow(int row, float startingDirectionSign)
        {
            var sign = Mathf.Sign(Mathf.Approximately(startingDirectionSign, 0f) ? 1f : startingDirectionSign);
            return new Vector2(((Mathf.Max(0, row) & 1) == 0 ? sign : -sign), 0f);
        }

        public int RewriteRow(int rowIndex, BreakoutProceduralBrickCell[] rowCells)
        {
            if (rowCells == null || rowCells.Length == 0)
            {
                return 0;
            }

            var removedRequiredCount = 0;
            var replacementY = 0f;
            var hasReplacementY = false;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                var state = brick.CaptureState();

                if (state.Row != rowIndex)
                {
                    continue;
                }

                if (!hasReplacementY)
                {
                    replacementY = state.Position.y;
                    hasReplacementY = true;
                }

                if (brick.CountsTowardLevelCompletion)
                {
                    removedRequiredCount++;
                }

                bricks.RemoveAt(index);
                DisableAndDestroyBrick(brick);
            }

            if (!hasReplacementY)
            {
                return -removedRequiredCount;
            }

            var addedRequiredCount = 0;
            var totalWidth = (rowCells.Length * brickSize.x) + (Mathf.Max(0, rowCells.Length - 1) * brickSpacing.x);
            var startX = (-totalWidth * 0.5f) + (brickSize.x * 0.5f);

            for (var column = 0; column < rowCells.Length; column++)
            {
                var cell = rowCells[column];

                if (cell == null || cell.Definition == null)
                {
                    continue;
                }

                var position = new Vector2(
                    startX + (column * (brickSize.x + brickSpacing.x)),
                    replacementY);
                var brick = CreateBrick(position, cell.Definition, rowIndex, column, cell.MotionConfig);

                if (brick != null && brick.CountsTowardLevelCompletion)
                {
                    addedRequiredCount++;
                }
            }

            return addedRequiredCount - removedRequiredCount;
        }

        public int DestroyBricksInExplosionRadius(
            Vector2 explosionCenter,
            float explosionRadius,
            Brick sourceBrick,
            BallController scoringBall)
        {
            if (explosionRadius <= 0.01f || bricks.Count == 0)
            {
                return 0;
            }

            var impactedBricks = new List<Brick>();
            var explosionRadiusSquared = explosionRadius * explosionRadius;

            for (var index = 0; index < bricks.Count; index++)
            {
                var candidate = bricks[index];

                if (candidate == null
                    || candidate == sourceBrick
                    || candidate.Definition == null
                    || !candidate.Definition.IsBreakable)
                {
                    continue;
                }

                var offset = (Vector2)candidate.transform.position - explosionCenter;

                if (offset.sqrMagnitude > explosionRadiusSquared)
                {
                    continue;
                }

                impactedBricks.Add(candidate);
            }

            for (var index = 0; index < impactedBricks.Count; index++)
            {
                impactedBricks[index]?.DestroyByExplosion(scoringBall);
            }

            return impactedBricks.Count;
        }

        public bool HasBreakableBricksRemaining()
        {
            for (var index = 0; index < bricks.Count; index++)
            {
                var brick = bricks[index];

                if (brick == null || brick.Definition == null)
                {
                    continue;
                }

                if (brick.Definition.IsBreakable)
                {
                    return true;
                }
            }

            return false;
        }

        public void ApplyVisibilityMultiplier(float visibilityMultiplier)
        {
            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                brick.SetVisibilityMultiplier(visibilityMultiplier);
            }
        }

        public int ApplyFlickerBricks(BreakoutFlickerBricksSpec spec)
        {
            hasActiveFlickerBricks = true;
            activeFlickerBricksSpec = spec;
            var flickeringCount = 0;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                var state = brick.CaptureState();
                if (ApplyFlickerToBrick(brick, state.Row, state.Column, state.Position, spec))
                {
                    flickeringCount++;
                }
            }

            return flickeringCount;
        }

        public void ClearFlickerBricks()
        {
            hasActiveFlickerBricks = false;
            activeFlickerBricksSpec = default;

            for (var index = bricks.Count - 1; index >= 0; index--)
            {
                var brick = bricks[index];

                if (brick == null)
                {
                    bricks.RemoveAt(index);
                    continue;
                }

                brick.ClearFlicker();
            }
        }

        internal static Vector2[] BuildSplitBrickOffsets(Vector2 baseBrickSize, BrickDefinition splitDefinition)
        {
            return BuildSplitBrickOffsets(baseBrickSize, splitDefinition, 4);
        }

        internal static Vector2[] BuildSplitBrickOffsets(Vector2 baseBrickSize, BrickDefinition splitDefinition, int fragmentCount)
        {
            if (splitDefinition == null)
            {
                return Array.Empty<Vector2>();
            }

            var resolvedFragmentCount = Mathf.Clamp(fragmentCount, 1, 8);

            var fragmentOffset = new Vector2(
                baseBrickSize.x * splitDefinition.SizeMultiplier * 0.52f,
                baseBrickSize.y * splitDefinition.SizeMultiplier * 0.52f);

            if (resolvedFragmentCount == 4)
            {
                return new[]
                {
                    new Vector2(-fragmentOffset.x, fragmentOffset.y),
                    new Vector2(fragmentOffset.x, fragmentOffset.y),
                    new Vector2(-fragmentOffset.x, -fragmentOffset.y),
                    new Vector2(fragmentOffset.x, -fragmentOffset.y),
                };
            }

            if (resolvedFragmentCount == 2)
            {
                return new[]
                {
                    new Vector2(-fragmentOffset.x, 0f),
                    new Vector2(fragmentOffset.x, 0f),
                };
            }

            var offsets = new Vector2[resolvedFragmentCount];
            var radius = Mathf.Max(fragmentOffset.x, fragmentOffset.y);
            var yScale = fragmentOffset.x > 0.001f
                ? Mathf.Clamp(fragmentOffset.y / fragmentOffset.x, 0.35f, 1f)
                : 1f;

            for (var index = 0; index < resolvedFragmentCount; index++)
            {
                var angle = (Mathf.PI * 2f * index / resolvedFragmentCount) + (Mathf.PI * 0.25f);
                offsets[index] = new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius * yScale);
            }

            return offsets;
        }

        private static int ResolveSplitBrickFragmentCount(float specialBrickEffectMultiplier)
        {
            return Mathf.Clamp(Mathf.RoundToInt(4f * Mathf.Max(1f, specialBrickEffectMultiplier)), 4, 8);
        }

        private void ApplyActiveFlickerToBrick(Brick brick, int row, int column, Vector2 position)
        {
            if (hasActiveFlickerBricks)
            {
                ApplyFlickerToBrick(brick, row, column, position, activeFlickerBricksSpec);
            }
        }

        private static bool ApplyFlickerToBrick(
            Brick brick,
            int row,
            int column,
            Vector2 position,
            BreakoutFlickerBricksSpec spec)
        {
            if (brick == null || brick.Definition == null || !brick.Definition.IsBreakable)
            {
                return false;
            }

            if (ResolveFlickerRoll(row, column, position, spec.PatternSeed) > spec.AffectedBrickChance)
            {
                brick.ClearFlicker();
                return false;
            }

            var cycleSeconds = spec.VisibleSeconds + spec.HiddenSeconds;
            var phaseSeconds = ResolveFlickerRoll(row + 17, column + 31, position, spec.PatternSeed ^ 0x2A3F) * cycleSeconds;
            brick.SetFlicker(spec.VisibleSeconds, spec.HiddenSeconds, spec.HiddenAlpha, phaseSeconds);
            return true;
        }

        private static float ResolveFlickerRoll(int row, int column, Vector2 position, int seed)
        {
            unchecked
            {
                var hash = seed == 0 ? 17 : seed;
                hash = (hash * 397) ^ Mathf.Max(0, row);
                hash = (hash * 397) ^ Mathf.Max(0, column);
                hash = (hash * 397) ^ Mathf.RoundToInt(position.x * 100f);
                hash = (hash * 397) ^ Mathf.RoundToInt(position.y * 100f);
                hash ^= hash >> 16;
                return (hash & 0x7fffffff) / (float)int.MaxValue;
            }
        }

        private Vector2 ClampBrickPositionToMovementBounds(Vector2 position, BrickDefinition definition)
        {
            var bounds = movementBoundsResolver();

            if (bounds.width <= 0.01f || bounds.height <= 0.01f)
            {
                return position;
            }

            var halfSize = new Vector2(
                brickSize.x * (definition?.SizeMultiplier ?? 1f) * 0.5f,
                brickSize.y * (definition?.SizeMultiplier ?? 1f) * 0.5f);

            return new Vector2(
                Mathf.Clamp(position.x, bounds.xMin + halfSize.x, bounds.xMax - halfSize.x),
                Mathf.Clamp(position.y, bounds.yMin + halfSize.y, bounds.yMax - halfSize.y));
        }
    }
}
