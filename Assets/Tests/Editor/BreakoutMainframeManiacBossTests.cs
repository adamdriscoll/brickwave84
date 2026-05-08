using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace GetBricked.Gameplay.Tests
{
    public sealed class BreakoutMainframeManiacBossTests
    {
        [Test]
        public void CoreIsLockedUntilAllDataBanksAreDestroyed()
        {
            var fixture = CreateFixture();
            var core = fixture.Nodes.First(node => node.IsCore);

            var lockedHit = fixture.Boss.TryHandleNodeHit(core, fixture.Ball, null);

            Assert.That(lockedHit.Handled, Is.True);
            Assert.That(lockedHit.Damaged, Is.False);
            Assert.That(lockedHit.Callout, Is.EqualTo("LOCKED!"));
            Assert.That(fixture.Boss.IsCoreVulnerable, Is.False);
            Assert.That(fixture.Boss.CoreHitsRemaining, Is.EqualTo(5));

            fixture.Destroy();
        }

        [Test]
        public void DataBanksExposeThenDestroyBeforeCoreCanCrash()
        {
            var fixture = CreateFixture();
            var dataBanks = fixture.Nodes.Where(node => !node.IsCore).ToArray();

            Assert.That(dataBanks, Has.Length.EqualTo(8));
            Assert.That(fixture.Boss.DataBanksRemaining, Is.EqualTo(8));

            foreach (var dataBank in dataBanks)
            {
                var firewallHit = fixture.Boss.TryHandleNodeHit(dataBank, fixture.Ball, null);

                Assert.That(firewallHit.LayerDestroyed, Is.True);
                Assert.That(dataBank.Layer, Is.EqualTo(BreakoutMainframeManiacLayer.DataBank));

                fixture.Boss.TryHandleNodeHit(dataBank, fixture.Ball, null);
                var dataWipe = fixture.Boss.TryHandleNodeHit(dataBank, fixture.Ball, null);

                Assert.That(dataWipe.LayerDestroyed, Is.True);
                Assert.That(dataBank.IsDestroyed, Is.True);
            }

            Assert.That(fixture.Boss.DataBanksRemaining, Is.Zero);
            Assert.That(fixture.Boss.IsCoreVulnerable, Is.True);
            Assert.That(fixture.Boss.PhaseLabel, Is.EqualTo("Core Crash"));

            fixture.Destroy();
        }

        [Test]
        public void CoreDefeatTakesFiveVulnerableHits()
        {
            var fixture = CreateFixture();
            var core = fixture.Nodes.First(node => node.IsCore);

            foreach (var dataBank in fixture.Nodes.Where(node => !node.IsCore))
            {
                dataBank.MarkDestroyed();
            }

            BreakoutMainframeManiacHitResult hit = default;

            for (var index = 0; index < 5; index++)
            {
                hit = fixture.Boss.TryHandleNodeHit(core, fixture.Ball, null);
            }

            Assert.That(hit.Defeated, Is.True);
            Assert.That(hit.Callout, Is.EqualTo("FATAL ERROR!"));
            Assert.That(fixture.Boss.IsDefeated, Is.True);
            Assert.That(fixture.Boss.PhaseLabel, Is.EqualTo("Fatal Error"));

            fixture.Destroy();
        }

        [Test]
        public void CoreUsesCircularColliderAndDataBanksUseBoxes()
        {
            var fixture = CreateFixture();
            var core = fixture.Nodes.First(node => node.IsCore);
            var dataBank = fixture.Nodes.First(node => !node.IsCore);

            Assert.That(core.GetComponent<CircleCollider2D>(), Is.Not.Null);
            Assert.That(core.GetComponent<BoxCollider2D>(), Is.Null);
            Assert.That(dataBank.GetComponent<BoxCollider2D>(), Is.Not.Null);
            Assert.That(dataBank.GetComponent<CircleCollider2D>(), Is.Null);

            fixture.Destroy();
        }

        private static BossFixture CreateFixture(Func<float, float, float> randomRange = null)
        {
            var sprite = CreateSprite();
            var bossObject = new GameObject("Mainframe Maniac Test Boss");
            var boss = bossObject.AddComponent<BreakoutMainframeManiacBoss>();
            boss.Configure(
                null,
                bossObject.transform,
                sprite,
                sprite,
                sprite,
                null,
                null,
                new PhysicsMaterial2D("Test Bounce"),
                Rect.MinMaxRect(-5f, -4f, 5f, 4f),
                new Vector2(1.15f, 0.58f),
                2,
                randomRange ?? ((minimum, maximum) => (minimum + maximum) * 0.5f));

            var ballObject = new GameObject("Mainframe Maniac Test Ball");
            ballObject.AddComponent<CircleCollider2D>();
            ballObject.AddComponent<Rigidbody2D>();
            var ball = ballObject.AddComponent<BallController>();
            ball.Configure(null, null, 7.5f, 0.35f, -5f, 0.5f, false);
            ball.Launch(Vector2.down);

            return new BossFixture(
                boss,
                ball,
                boss.GetComponentsInChildren<BreakoutMainframeManiacNode>(includeInactive: true),
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
                BreakoutMainframeManiacBoss boss,
                BallController ball,
                BreakoutMainframeManiacNode[] nodes,
                Sprite sprite,
                GameObject bossObject,
                GameObject ballObject)
            {
                Boss = boss;
                Ball = ball;
                Nodes = nodes;
                Sprite = sprite;
                BossObject = bossObject;
                BallObject = ballObject;
            }

            public BreakoutMainframeManiacBoss Boss { get; }

            public BallController Ball { get; }

            public BreakoutMainframeManiacNode[] Nodes { get; }

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
