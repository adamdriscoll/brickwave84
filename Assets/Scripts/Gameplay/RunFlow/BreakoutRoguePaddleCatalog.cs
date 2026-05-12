using System;
using System.Collections.Generic;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutRoguePaddleDefinition
    {
        public BreakoutRoguePaddleDefinition(
            string id,
            string displayName,
            string identity,
            string strength,
            string drawback,
            float widthMultiplier,
            float speedMultiplier)
        {
            Id = string.IsNullOrWhiteSpace(id) ? "classic_paddle" : id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? BreakoutRogueRunResultStore.DefaultPaddleLabel
                : displayName.Trim();
            Identity = identity ?? string.Empty;
            Strength = strength ?? string.Empty;
            Drawback = drawback ?? string.Empty;
            WidthMultiplier = Mathf.Clamp(widthMultiplier, 0.6f, 1.8f);
            SpeedMultiplier = Mathf.Clamp(speedMultiplier, 0.5f, 1.6f);
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Identity { get; }

        public string Strength { get; }

        public string Drawback { get; }

        public float WidthMultiplier { get; }

        public float SpeedMultiplier { get; }
    }

    internal static class BreakoutRoguePaddleCatalog
    {
        private static readonly BreakoutRoguePaddleDefinition[] Paddles =
        {
            new BreakoutRoguePaddleDefinition(
                "classic_paddle",
                "Classic Paddle",
                "Balanced starter",
                "Stable, readable, flexible.",
                "No extreme specialty.",
                1f,
                1f),
            new BreakoutRoguePaddleDefinition(
                "comet_paddle",
                "Comet Paddle",
                "Small and fast",
                "High acceleration and precision control.",
                "Less surface area.",
                0.82f,
                1.22f),
            new BreakoutRoguePaddleDefinition(
                "cruiser_paddle",
                "Cruiser Paddle",
                "Wide and slow",
                "Safer catches and survival builds.",
                "Lower mobility.",
                1.22f,
                0.82f),
        };

        public static IReadOnlyList<BreakoutRoguePaddleDefinition> AllPaddles => Paddles;

        public static BreakoutRoguePaddleDefinition DefaultPaddle => Paddles[0];

        public static BreakoutRoguePaddleDefinition Resolve(string selectedPaddleLabel)
        {
            if (!string.IsNullOrWhiteSpace(selectedPaddleLabel))
            {
                for (var index = 0; index < Paddles.Length; index++)
                {
                    var paddle = Paddles[index];

                    if (string.Equals(paddle.Id, selectedPaddleLabel.Trim(), StringComparison.OrdinalIgnoreCase)
                        || string.Equals(paddle.DisplayName, selectedPaddleLabel.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return paddle;
                    }
                }
            }

            return DefaultPaddle;
        }

        public static BreakoutRoguePaddleDefinition[] BuildUnlockedPaddles()
        {
            return new[] { DefaultPaddle };
        }

        public static BreakoutRoguePaddleDefinition ResolveUnlockedByIndex(int index)
        {
            var unlocked = BuildUnlockedPaddles();
            return unlocked[Mathf.Clamp(index, 0, Mathf.Max(0, unlocked.Length - 1))];
        }

        public static int GetUnlockedIndex(string selectedPaddleLabel)
        {
            var unlocked = BuildUnlockedPaddles();
            var resolved = Resolve(selectedPaddleLabel);

            for (var index = 0; index < unlocked.Length; index++)
            {
                if (string.Equals(unlocked[index].DisplayName, resolved.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return 0;
        }

        public static BreakoutRoguePaddleDefinition GetNextLockedPaddle()
        {
            return default;
        }

        public static BreakoutRoguePaddleDefinition GetUnlockRequirement(BreakoutRoguePaddleDefinition paddle)
        {
            return default;
        }
    }
}
