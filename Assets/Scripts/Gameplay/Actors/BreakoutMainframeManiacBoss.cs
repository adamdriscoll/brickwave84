using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutMainframeManiacLayer
    {
        Firewall = 0,
        DataBank = 1,
        Core = 2,
    }

    internal readonly struct BreakoutMainframeManiacHitResult
    {
        public BreakoutMainframeManiacHitResult(
            bool handled,
            bool damaged,
            bool layerDestroyed,
            bool defeated,
            Vector2 bounceDirection,
            float speedBurstMultiplier,
            float speedBurstDuration,
            int scorePoints,
            string callout,
            Color calloutColor)
        {
            Handled = handled;
            Damaged = damaged;
            LayerDestroyed = layerDestroyed;
            Defeated = defeated;
            BounceDirection = bounceDirection;
            SpeedBurstMultiplier = Mathf.Max(1f, speedBurstMultiplier);
            SpeedBurstDuration = Mathf.Max(0f, speedBurstDuration);
            ScorePoints = Mathf.Max(0, scorePoints);
            Callout = callout ?? string.Empty;
            CalloutColor = calloutColor;
        }

        public bool Handled { get; }

        public bool Damaged { get; }

        public bool LayerDestroyed { get; }

        public bool Defeated { get; }

        public Vector2 BounceDirection { get; }

        public float SpeedBurstMultiplier { get; }

        public float SpeedBurstDuration { get; }

        public int ScorePoints { get; }

        public string Callout { get; }

        public Color CalloutColor { get; }
    }

    internal sealed class BreakoutMainframeManiacBoss : MonoBehaviour
    {
        private const float DataBankOrbitWidth = 3.2f;
        private const float DataBankOrbitHeight = 1.54f;
        private const float PacketInitialDelayMinimumSeconds = 0.65f;
        private const float PacketInitialDelayMaximumSeconds = 1.15f;
        private const float PacketMinimumSeconds = 1.25f;
        private const float PacketMaximumSeconds = 2.25f;
        private const float CorePulseDegreesPerSecond = 220f;
        private const float DataBankSpeedBurstMultiplier = 1.18f;
        private const float CoreSpeedBurstMultiplier = 1.28f;
        private const float SpeedBurstSeconds = 1.35f;

        private readonly List<BreakoutMainframeManiacNode> dataBanks = new List<BreakoutMainframeManiacNode>();

        private BreakoutGameController controller;
        private Transform projectileRoot;
        private Sprite dataBankSprite;
        private Sprite coreSprite;
        private Sprite packetSprite;
        private Material nodeMaterial;
        private Material packetMaterial;
        private PhysicsMaterial2D physicsMaterial;
        private Rect arenaBounds;
        private Vector2 dataBankSize;
        private Vector2 coreSize;
        private Func<float, float, float> randomRangeResolver;
        private BreakoutMainframeManiacNode coreNode;
        private Vector2 bossCenter;
        private float orbitPhase;
        private float packetTimer;
        private int gateIndex;
        private bool isDefeated;

        public int DataBanksRemaining => CountActiveDataBanks();

        public int CoreHitsRemaining => coreNode != null ? coreNode.HitsRemaining : 0;

        public bool IsCoreVulnerable => !isDefeated && DataBanksRemaining == 0;

        public bool IsDefeated => isDefeated;

        public string PhaseLabel
        {
            get
            {
                if (isDefeated)
                {
                    return "Fatal Error";
                }

                if (IsCoreVulnerable)
                {
                    return "Core Crash";
                }

                return DataBanksRemaining <= Mathf.Max(1, dataBanks.Count / 2) ? "Overheat" : "Firewall";
            }
        }

        public void Configure(
            BreakoutGameController gameController,
            Transform powerDownRoot,
            Sprite dataSprite,
            Sprite coreVisualSprite,
            Sprite powerDownSprite,
            Material spriteMaterial,
            Material powerDownMaterial,
            PhysicsMaterial2D sharedPhysicsMaterial,
            Rect playfieldBounds,
            Vector2 baseBrickSize,
            int bossGateIndex,
            Func<float, float, float> randomRange)
        {
            controller = gameController;
            projectileRoot = powerDownRoot != null ? powerDownRoot : transform.parent;
            dataBankSprite = dataSprite;
            coreSprite = coreVisualSprite != null ? coreVisualSprite : dataSprite;
            packetSprite = powerDownSprite;
            nodeMaterial = spriteMaterial;
            packetMaterial = powerDownMaterial;
            physicsMaterial = sharedPhysicsMaterial;
            arenaBounds = playfieldBounds;
            gateIndex = Mathf.Max(0, bossGateIndex);
            randomRangeResolver = randomRange;
            dataBankSize = new Vector2(baseBrickSize.x * 0.72f, baseBrickSize.y * 0.88f);
            coreSize = new Vector2(baseBrickSize.x * 1.3f, baseBrickSize.x * 1.3f);
            bossCenter = new Vector2(0f, Mathf.Lerp(arenaBounds.yMin + 2.1f, arenaBounds.yMax - 1.35f, 0.78f));
            BuildBossParts();
            packetTimer = BuildNextPacketSeconds(initialDelay: true);
        }

        public BreakoutMainframeManiacHitResult TryHandleNodeHit(
            BreakoutMainframeManiacNode node,
            BallController ball,
            Collision2D collision)
        {
            if (isDefeated || node == null || ball == null || node.IsDestroyed)
            {
                return default;
            }

            var bounceDirection = BuildBounceDirection(node, ball, collision);

            if (node.IsCore && !IsCoreVulnerable)
            {
                return new BreakoutMainframeManiacHitResult(
                    true,
                    false,
                    false,
                    false,
                    bounceDirection,
                    1f,
                    0f,
                    0,
                    "LOCKED!",
                    new Color(0.1f, 0.9f, 1f, 1f));
            }

            var layer = node.Layer;
            var destroyedLayer = node.ApplyHit();
            var defeated = false;
            var scorePoints = destroyedLayer ? ResolveLayerScore(layer) : 0;
            var callout = string.Empty;
            var calloutColor = ResolveLayerCalloutColor(layer);

            if (destroyedLayer)
            {
                if (node.IsCore)
                {
                    node.MarkDestroyed();
                    isDefeated = true;
                    defeated = true;
                    callout = "FATAL ERROR!";
                }
                else if (layer == BreakoutMainframeManiacLayer.Firewall)
                {
                    node.RevealLayer(BreakoutMainframeManiacLayer.DataBank);
                    callout = "FIREWALL!";
                }
                else
                {
                    node.MarkDestroyed();
                    callout = "DATA WIPE!";
                }
            }
            else if (node.IsCore)
            {
                callout = "CORE HIT!";
            }

            return new BreakoutMainframeManiacHitResult(
                true,
                true,
                destroyedLayer,
                defeated,
                node.IsCore ? ApplyCoreBounceTilt(bounceDirection) : ApplyDataBankBounceTilt(bounceDirection),
                node.IsCore ? CoreSpeedBurstMultiplier : DataBankSpeedBurstMultiplier,
                SpeedBurstSeconds,
                scorePoints,
                callout,
                calloutColor);
        }

        private void FixedUpdate()
        {
            if (isDefeated || coreNode == null)
            {
                return;
            }

            orbitPhase += Time.fixedDeltaTime * (0.82f + (gateIndex * 0.08f));
            SyncNodePoses();
            UpdatePacketTimer();
        }

        private void BuildBossParts()
        {
            dataBanks.Clear();
            coreNode = CreateNode("Mainframe Core", -1, true, BreakoutMainframeManiacLayer.Core, coreSize);

            var dataBankCount = Mathf.Clamp(6 + gateIndex, 6, 8);

            for (var index = 0; index < dataBankCount; index++)
            {
                dataBanks.Add(CreateNode(
                    $"Mainframe Data Bank {index + 1:00}",
                    index,
                    false,
                    BreakoutMainframeManiacLayer.Firewall,
                    dataBankSize));
            }

            SyncNodePoses();
        }

        private BreakoutMainframeManiacNode CreateNode(
            string objectName,
            int nodeIndex,
            bool isCore,
            BreakoutMainframeManiacLayer layer,
            Vector2 worldSize)
        {
            var nodeObject = new GameObject(objectName);
            nodeObject.transform.SetParent(transform, false);
            nodeObject.AddComponent<BreakoutGlowRenderer>();

            var node = nodeObject.AddComponent<BreakoutMainframeManiacNode>();
            node.Configure(
                nodeIndex,
                isCore,
                layer,
                dataBankSprite,
                coreSprite,
                nodeMaterial,
                physicsMaterial,
                worldSize);
            return node;
        }

        private void SyncNodePoses()
        {
            coreNode.SetWorldPose(
                bossCenter + new Vector2(0f, Mathf.Sin(orbitPhase * 1.7f) * 0.06f),
                Mathf.Repeat(orbitPhase * CorePulseDegreesPerSecond, 360f));

            for (var index = 0; index < dataBanks.Count; index++)
            {
                var node = dataBanks[index];

                if (node == null || node.IsDestroyed)
                {
                    continue;
                }

                var ratio = dataBanks.Count <= 0 ? 0f : index / (float)dataBanks.Count;
                var angle = (ratio * Mathf.PI * 2f) + orbitPhase;
                var jitter = Mathf.Sin((orbitPhase * 2.9f) + (index * 0.77f)) * 0.08f;
                var position = bossCenter + new Vector2(
                    Mathf.Cos(angle) * (DataBankOrbitWidth + jitter),
                    Mathf.Sin(angle) * DataBankOrbitHeight);
                var rotation = Mathf.Sin((orbitPhase * 1.8f) + index) * 9f;
                node.SetWorldPose(position, rotation);
            }
        }

        private void UpdatePacketTimer()
        {
            packetTimer = Mathf.Max(0f, packetTimer - Time.fixedDeltaTime);

            if (packetTimer > 0f)
            {
                return;
            }

            SpawnGlitchPacket();
            packetTimer = BuildNextPacketSeconds(initialDelay: false);
        }

        private void SpawnGlitchPacket()
        {
            if (controller == null || projectileRoot == null || packetSprite == null)
            {
                return;
            }

            var sourceNode = ResolvePacketSourceNode();

            if (sourceNode == null)
            {
                return;
            }

            var projectileObject = new GameObject("Mainframe Glitch Packet");
            projectileObject.transform.SetParent(projectileRoot, false);
            projectileObject.transform.position = sourceNode.transform.position;

            var direction = new Vector2(RandomRange(-0.34f, 0.34f), -1f).normalized;
            var projectile = projectileObject.AddComponent<BreakoutBrickosaurusPowerDownProjectile>();
            projectile.Configure(
                controller,
                packetSprite,
                packetMaterial,
                physicsMaterial,
                direction,
                3.25f + (gateIndex * 0.18f),
                arenaBounds.yMin - 0.8f);
        }

        private BreakoutMainframeManiacNode ResolvePacketSourceNode()
        {
            for (var attempt = 0; attempt < dataBanks.Count; attempt++)
            {
                var index = Mathf.Clamp(Mathf.FloorToInt(RandomRange(0f, dataBanks.Count)), 0, dataBanks.Count - 1);
                var node = dataBanks[index];

                if (node != null && !node.IsDestroyed)
                {
                    return node;
                }
            }

            return coreNode != null && !coreNode.IsDestroyed ? coreNode : null;
        }

        private int CountActiveDataBanks()
        {
            var count = 0;

            for (var index = 0; index < dataBanks.Count; index++)
            {
                if (dataBanks[index] != null && !dataBanks[index].IsDestroyed)
                {
                    count++;
                }
            }

            return count;
        }

        private Vector2 BuildBounceDirection(BreakoutMainframeManiacNode node, BallController ball, Collision2D collision)
        {
            var contactPoint = collision != null && collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)ball.transform.position;
            var direction = contactPoint - (Vector2)node.transform.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = -ball.CurrentVelocity;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.down;
            }

            return direction.normalized;
        }

        private Vector2 ApplyDataBankBounceTilt(Vector2 direction)
        {
            return RotateDirection(direction, RandomRange(-10f, 10f));
        }

        private Vector2 ApplyCoreBounceTilt(Vector2 direction)
        {
            return RotateDirection(direction, RandomRange(-18f, 18f));
        }

        private static Vector2 RotateDirection(Vector2 direction, float angleDegrees)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.down;
            }

            var angleRadians = angleDegrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(angleRadians);
            var cos = Mathf.Cos(angleRadians);
            var normalized = direction.normalized;
            return new Vector2(
                (normalized.x * cos) - (normalized.y * sin),
                (normalized.x * sin) + (normalized.y * cos)).normalized;
        }

        private float BuildNextPacketSeconds(bool initialDelay)
        {
            var minimum = initialDelay ? PacketInitialDelayMinimumSeconds : PacketMinimumSeconds;
            var maximum = initialDelay ? PacketInitialDelayMaximumSeconds : PacketMaximumSeconds;

            if (IsCoreVulnerable)
            {
                minimum -= 0.28f;
                maximum -= 0.42f;
            }

            return RandomRange(minimum, maximum);
        }

        private float RandomRange(float minimumInclusive, float maximumInclusive)
        {
            return randomRangeResolver != null
                ? randomRangeResolver(minimumInclusive, maximumInclusive)
                : UnityEngine.Random.Range(minimumInclusive, maximumInclusive);
        }

        private static int ResolveLayerScore(BreakoutMainframeManiacLayer layer)
        {
            return layer switch
            {
                BreakoutMainframeManiacLayer.Firewall => 140,
                BreakoutMainframeManiacLayer.DataBank => 320,
                BreakoutMainframeManiacLayer.Core => 2200,
                _ => 100,
            };
        }

        private static Color ResolveLayerCalloutColor(BreakoutMainframeManiacLayer layer)
        {
            return layer switch
            {
                BreakoutMainframeManiacLayer.Firewall => new Color(0.1f, 0.9f, 1f, 1f),
                BreakoutMainframeManiacLayer.DataBank => new Color(1f, 0.47f, 0.86f, 1f),
                BreakoutMainframeManiacLayer.Core => new Color(1f, 0.86f, 0.32f, 1f),
                _ => Color.white,
            };
        }
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    internal sealed class BreakoutMainframeManiacNode : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private BreakoutGlowRenderer glowRenderer;
        private Rigidbody2D nodeBody;
        private Collider2D nodeCollider;
        private Sprite dataBankSprite;
        private Sprite coreSprite;
        private Vector2 targetWorldSize;
        private Color baseColor;
        private Color damagedColor;
        private int maxHits;
        private int hitsRemaining;
        private bool isDestroyed;

        public int NodeIndex { get; private set; }

        public bool IsCore { get; private set; }

        public BreakoutMainframeManiacLayer Layer { get; private set; }

        public int HitsRemaining => hitsRemaining;

        public bool IsDestroyed => isDestroyed;

        public void Configure(
            int nodeIndex,
            bool isCoreNode,
            BreakoutMainframeManiacLayer layer,
            Sprite dataSprite,
            Sprite coreVisualSprite,
            Material material,
            PhysicsMaterial2D physicsMaterial,
            Vector2 worldSize)
        {
            NodeIndex = nodeIndex;
            IsCore = isCoreNode;
            dataBankSprite = dataSprite;
            coreSprite = coreVisualSprite != null ? coreVisualSprite : dataSprite;
            targetWorldSize = worldSize;
            spriteRenderer = GetComponent<SpriteRenderer>();
            glowRenderer = GetComponent<BreakoutGlowRenderer>();
            nodeBody = GetComponent<Rigidbody2D>();

            spriteRenderer.sharedMaterial = material;
            spriteRenderer.sortingOrder = IsCore ? 14 : 10;

            nodeCollider = IsCore
                ? gameObject.AddComponent<CircleCollider2D>()
                : gameObject.AddComponent<BoxCollider2D>();
            nodeCollider.sharedMaterial = physicsMaterial;

            nodeBody.bodyType = RigidbodyType2D.Kinematic;
            nodeBody.gravityScale = 0f;
            nodeBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            nodeBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            nodeBody.useFullKinematicContacts = true;

            RevealLayer(layer);
        }

        public void RevealLayer(BreakoutMainframeManiacLayer layer)
        {
            Layer = IsCore ? BreakoutMainframeManiacLayer.Core : layer;
            maxHits = ResolveLayerHitPoints(Layer);
            hitsRemaining = maxHits;
            isDestroyed = false;
            gameObject.SetActive(true);
            ResolveLayerColors(Layer, out baseColor, out damagedColor);
            RefreshVisual();
        }

        public bool ApplyHit()
        {
            if (isDestroyed || hitsRemaining <= 0)
            {
                return false;
            }

            hitsRemaining = Mathf.Max(0, hitsRemaining - 1);
            RefreshVisual();
            return hitsRemaining <= 0;
        }

        public void MarkDestroyed()
        {
            isDestroyed = true;
            hitsRemaining = 0;
            gameObject.SetActive(false);
        }

        public void SetWorldPose(Vector2 worldPosition, float rotationDegrees)
        {
            transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(0f, 0f, rotationDegrees));

            if (nodeBody == null)
            {
                return;
            }

            nodeBody.MovePosition(worldPosition);
            nodeBody.MoveRotation(rotationDegrees);
        }

        private void Update()
        {
            if (isDestroyed || spriteRenderer == null)
            {
                return;
            }

            var pulse = 0.5f + (Mathf.Sin((Time.time * (IsCore ? 9.5f : 6.4f)) + (NodeIndex * 0.61f)) * 0.5f);
            var pulseColor = Color.Lerp(BuildCurrentColor(), Color.white, IsCore ? 0.08f + (pulse * 0.18f) : pulse * 0.12f);
            spriteRenderer.color = pulseColor;
            glowRenderer?.ApplyColor(pulseColor);
        }

        private void RefreshVisual()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = IsCore ? coreSprite : dataBankSprite;
            spriteRenderer.color = BuildCurrentColor();
            transform.localScale = BuildSpriteScale(spriteRenderer.sprite, targetWorldSize);
            SyncColliderToSpriteBounds();
            glowRenderer?.Configure(
                IsCore ? 1.34f : 1.2f,
                IsCore ? 0.24f : 0.17f,
                IsCore ? 1.62f : 1.38f,
                IsCore ? 0.1f : 0.06f);
            glowRenderer?.ApplyColor(spriteRenderer.color);
        }

        private void SyncColliderToSpriteBounds()
        {
            if (nodeCollider == null)
            {
                return;
            }

            var spriteSize = spriteRenderer != null && spriteRenderer.sprite != null
                ? (Vector2)spriteRenderer.sprite.bounds.size
                : Vector2.one;

            if (nodeCollider is BoxCollider2D boxCollider)
            {
                boxCollider.size = spriteSize;
                boxCollider.offset = Vector2.zero;
            }
            else if (nodeCollider is CircleCollider2D circleCollider)
            {
                circleCollider.radius = Mathf.Min(spriteSize.x, spriteSize.y) * 0.5f;
                circleCollider.offset = Vector2.zero;
            }
        }

        private Color BuildCurrentColor()
        {
            if (maxHits <= 1)
            {
                return baseColor;
            }

            var damageProgress = 1f - Mathf.Clamp01((hitsRemaining - 1f) / (maxHits - 1f));
            return Color.Lerp(baseColor, damagedColor, damageProgress * 0.86f);
        }

        private static int ResolveLayerHitPoints(BreakoutMainframeManiacLayer layer)
        {
            return layer switch
            {
                BreakoutMainframeManiacLayer.DataBank => 2,
                BreakoutMainframeManiacLayer.Core => 5,
                _ => 1,
            };
        }

        private static void ResolveLayerColors(BreakoutMainframeManiacLayer layer, out Color primary, out Color damaged)
        {
            switch (layer)
            {
                case BreakoutMainframeManiacLayer.Firewall:
                    primary = new Color(0.08f, 0.88f, 1f, 0.95f);
                    damaged = new Color(1f, 0.24f, 0.72f, 0.95f);
                    break;
                case BreakoutMainframeManiacLayer.DataBank:
                    primary = new Color(0.98f, 0.18f, 0.82f, 1f);
                    damaged = new Color(1f, 0.75f, 0.2f, 1f);
                    break;
                default:
                    primary = new Color(1f, 0.84f, 0.3f, 1f);
                    damaged = new Color(1f, 0.18f, 0.22f, 1f);
                    break;
            }
        }

        private static Vector3 BuildSpriteScale(Sprite sprite, Vector2 worldSize)
        {
            if (sprite == null || sprite.bounds.size.x <= 0.0001f || sprite.bounds.size.y <= 0.0001f)
            {
                return new Vector3(worldSize.x, worldSize.y, 1f);
            }

            return new Vector3(worldSize.x / sprite.bounds.size.x, worldSize.y / sprite.bounds.size.y, 1f);
        }
    }
}
