using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
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
                motionConfig.InitialDirection);
            brick.SetMovementBounds(movementBoundsResolver());
            bricks.Add(brick);
            return brick;
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

        public int SpawnSplitBricks(BrickDefinition parentDefinition, Vector2 splitCenter)
        {
            var splitDefinition = parentDefinition != null ? parentDefinition.SplitBrickDefinition : null;

            if (splitDefinition == null)
            {
                return 0;
            }

            var requiredBrickCount = 0;
            var offsets = BuildSplitBrickOffsets(brickSize, splitDefinition);

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

        public void DestroyBricksInExplosionRadius(
            Vector2 explosionCenter,
            float explosionRadius,
            Brick sourceBrick,
            BallController scoringBall)
        {
            if (explosionRadius <= 0.01f || bricks.Count == 0)
            {
                return;
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

        internal static Vector2[] BuildSplitBrickOffsets(Vector2 baseBrickSize, BrickDefinition splitDefinition)
        {
            if (splitDefinition == null)
            {
                return Array.Empty<Vector2>();
            }

            var fragmentOffset = new Vector2(
                baseBrickSize.x * splitDefinition.SizeMultiplier * 0.52f,
                baseBrickSize.y * splitDefinition.SizeMultiplier * 0.52f);

            return new[]
            {
                new Vector2(-fragmentOffset.x, fragmentOffset.y),
                new Vector2(fragmentOffset.x, fragmentOffset.y),
                new Vector2(-fragmentOffset.x, -fragmentOffset.y),
                new Vector2(fragmentOffset.x, -fragmentOffset.y),
            };
        }
    }
}
