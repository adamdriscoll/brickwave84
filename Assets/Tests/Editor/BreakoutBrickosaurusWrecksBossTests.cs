using System;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace GetBricked.Gameplay.Tests
{
    public sealed class BreakoutBrickosaurusWrecksBossTests
    {
        [Test]
        public void DestroyingCoreSegmentDropsDisconnectedTail()
        {
            var fixture = CreateFixture();
            var targetSegment = fixture.Parts.First(part => !part.IsHead && part.SegmentIndex == 4);

            targetSegment.RevealLayer(BreakoutBrickosaurusLayer.Core);
            fixture.Boss.TryHandlePartHit(targetSegment, fixture.Ball, null);

            Assert.That(fixture.Boss.ConnectedBodySegmentCount, Is.EqualTo(4));

            fixture.Destroy();
        }

        [Test]
        public void HeadBecomesVulnerableAfterBodyIsGoneAndDiesInThreeHits()
        {
            var fixture = CreateFixture();
            var firstSegment = fixture.Parts.First(part => !part.IsHead && part.SegmentIndex == 0);
            var head = fixture.Parts.First(part => part.IsHead);

            firstSegment.RevealLayer(BreakoutBrickosaurusLayer.Core);
            fixture.Boss.TryHandlePartHit(firstSegment, fixture.Ball, null);

            Assert.That(fixture.Boss.IsHeadVulnerable, Is.True);
            Assert.That(fixture.Boss.PhaseLabel, Is.EqualTo("Head Spin"));

            fixture.Boss.TryHandlePartHit(head, fixture.Ball, null);
            fixture.Boss.TryHandlePartHit(head, fixture.Ball, null);
            var finalHit = fixture.Boss.TryHandlePartHit(head, fixture.Ball, null);

            Assert.That(finalHit.Defeated, Is.True);
            Assert.That(fixture.Boss.IsDefeated, Is.True);

            fixture.Destroy();
        }

        [Test]
        public void PartColliderMatchesRenderedSpriteSize()
        {
            var fixture = CreateFixture();
            var part = fixture.Parts.First(candidate => !candidate.IsHead);
            var collider = part.GetComponent<CircleCollider2D>();
            Physics2D.SyncTransforms();

            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.bounds.size.x, Is.EqualTo(1.15f * 0.56f).Within(0.01f));
            Assert.That(collider.bounds.size.y, Is.EqualTo(1.15f * 0.56f).Within(0.01f));

            fixture.Destroy();
        }

        [Test]
        public void HeadUsesCircularCollider()
        {
            var fixture = CreateFixture();
            var head = fixture.Parts.First(part => part.IsHead);

            Assert.That(head.GetComponent<CircleCollider2D>(), Is.Not.Null);
            Assert.That(head.GetComponent<BoxCollider2D>(), Is.Null);

            fixture.Destroy();
        }

        [Test]
        public void ShieldAndScaleLayersBreakInOneHit()
        {
            var fixture = CreateFixture();
            var part = fixture.Parts.First(candidate => !candidate.IsHead);

            var shieldHit = fixture.Boss.TryHandlePartHit(part, fixture.Ball, null);

            Assert.That(shieldHit.LayerDestroyed, Is.True);
            Assert.That(part.Layer, Is.EqualTo(BreakoutBrickosaurusLayer.Scale));

            var scaleHit = fixture.Boss.TryHandlePartHit(part, fixture.Ball, null);

            Assert.That(scaleHit.LayerDestroyed, Is.True);
            Assert.That(part.Layer, Is.EqualTo(BreakoutBrickosaurusLayer.Core));

            fixture.Destroy();
        }

        [Test]
        public void BodyHitAddsBounceJitter()
        {
            var fixture = CreateFixture((minimum, maximum) => maximum);
            var part = fixture.Parts.First(candidate => !candidate.IsHead);
            fixture.Ball.SetWorldPosition((Vector2)part.transform.position + Vector2.down);

            var result = fixture.Boss.TryHandlePartHit(part, fixture.Ball, null);

            Assert.That(Mathf.Abs(result.BounceDirection.x), Is.GreaterThan(0.05f));

            fixture.Destroy();
        }

        [Test]
        public void BodyFollowsHeadDuringSwoop()
        {
            var fixture = CreateFixture((minimum, maximum) => minimum);
            var fixedUpdate = typeof(BreakoutBrickosaurusWrecksBoss).GetMethod(
                "FixedUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var head = fixture.Parts.First(part => part.IsHead);
            var firstBodySegment = fixture.Parts.First(part => !part.IsHead && part.SegmentIndex == 0);

            for (var step = 0; step < 220 && head.transform.position.y > 0.35f; step++)
            {
                fixedUpdate.Invoke(fixture.Boss, null);
            }

            Assert.That(fixture.Boss.IsSwooping, Is.True);
            Assert.That(head.transform.position.y, Is.LessThan(0.35f));
            Assert.That(firstBodySegment.transform.position.y, Is.LessThan(0.75f));
            Assert.That(Mathf.Abs(firstBodySegment.transform.position.y - head.transform.position.y), Is.LessThan(0.8f));
            Assert.That(head.transform.position.y, Is.GreaterThan(-1.25f));
            Assert.That(firstBodySegment.transform.position.y, Is.GreaterThan(-1.75f));

            fixture.Destroy();
        }

        [Test]
        public void ScaleSegmentShowsThreeTriangleSpikes()
        {
            var fixture = CreateFixture();
            var part = fixture.Parts.First(candidate => !candidate.IsHead);

            part.RevealLayer(BreakoutBrickosaurusLayer.Scale);

            var spikes = part.GetComponentsInChildren<SpriteRenderer>(includeInactive: true)
                .Where(renderer => renderer.gameObject.name.StartsWith("Scale Spike"))
                .OrderBy(renderer => renderer.transform.localPosition.x)
                .ToArray();

            Assert.That(spikes, Has.Length.EqualTo(3));
            Assert.That(spikes[0].transform.localPosition.x, Is.EqualTo(-0.32f).Within(0.01f));
            Assert.That(spikes[1].transform.localPosition.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(spikes[2].transform.localPosition.x, Is.EqualTo(0.32f).Within(0.01f));
            Assert.That(spikes.All(spike => spike.enabled), Is.True);

            fixture.Destroy();
        }

        [Test]
        public void PowerDownTimerStartsImmediately()
        {
            var fixture = CreateFixture();

            Assert.That(fixture.Boss.PowerDownTimer, Is.InRange(0.75f, 1.35f));

            fixture.Destroy();
        }

        [Test]
        public void BossStartsSwoopAfterOpeningInterval()
        {
            var fixture = CreateFixture();
            var fixedUpdate = typeof(BreakoutBrickosaurusWrecksBoss).GetMethod(
                "FixedUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic);

            for (var step = 0; step < 220 && !fixture.Boss.IsSwooping; step++)
            {
                fixedUpdate.Invoke(fixture.Boss, null);
            }

            Assert.That(fixture.Boss.IsSwooping, Is.True);

            fixture.Destroy();
        }

        private static BossFixture CreateFixture(Func<float, float, float> randomRange = null)
        {
            var sprite = CreateSprite();
            var bossObject = new GameObject("Brickosaurus Test Boss");
            var boss = bossObject.AddComponent<BreakoutBrickosaurusWrecksBoss>();
            boss.Configure(
                null,
                bossObject.transform,
                sprite,
                sprite,
                sprite,
                sprite,
                null,
                null,
                new PhysicsMaterial2D("Test Bounce"),
                Rect.MinMaxRect(-5f, -4f, 5f, 4f),
                new Vector2(1.15f, 0.58f),
                0,
                randomRange ?? ((minimum, maximum) => (minimum + maximum) * 0.5f));

            var ballObject = new GameObject("Brickosaurus Test Ball");
            ballObject.AddComponent<CircleCollider2D>();
            ballObject.AddComponent<Rigidbody2D>();
            var ball = ballObject.AddComponent<BallController>();
            ball.Configure(null, null, 7.5f, 0.35f, -5f, 0.5f, false);
            ball.Launch(Vector2.down);

            return new BossFixture(
                boss,
                ball,
                boss.GetComponentsInChildren<BreakoutBrickosaurusPart>(includeInactive: true),
                sprite,
                bossObject,
                ballObject);
        }

        private static Sprite CreateSprite()
        {
            var texture = Texture2D.whiteTexture;
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);
        }

        private sealed class BossFixture
        {
            public BossFixture(
                BreakoutBrickosaurusWrecksBoss boss,
                BallController ball,
                BreakoutBrickosaurusPart[] parts,
                Sprite sprite,
                GameObject bossObject,
                GameObject ballObject)
            {
                Boss = boss;
                Ball = ball;
                Parts = parts;
                Sprite = sprite;
                BossObject = bossObject;
                BallObject = ballObject;
            }

            public BreakoutBrickosaurusWrecksBoss Boss { get; }

            public BallController Ball { get; }

            public BreakoutBrickosaurusPart[] Parts { get; }

            private Sprite Sprite { get; }

            private GameObject BossObject { get; }

            private GameObject BallObject { get; }

            public void Destroy()
            {
                UnityEngine.Object.DestroyImmediate(BossObject);
                UnityEngine.Object.DestroyImmediate(BallObject);
                UnityEngine.Object.DestroyImmediate(Sprite);
            }
        }
    }
}
