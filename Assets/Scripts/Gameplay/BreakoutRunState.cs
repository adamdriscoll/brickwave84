using System;
using System.Collections.Generic;
using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal readonly struct BreakoutRunUpgradeModifiers
    {
        public BreakoutRunUpgradeModifiers(
            float paddleWidthMultiplier,
            float ballSpeedMultiplier,
            float dropChanceMultiplier,
            float wavyPaddleStrength,
            int extraBallsPerServe)
        {
            PaddleWidthMultiplier = paddleWidthMultiplier;
            BallSpeedMultiplier = ballSpeedMultiplier;
            DropChanceMultiplier = dropChanceMultiplier;
            WavyPaddleStrength = wavyPaddleStrength;
            ExtraBallsPerServe = extraBallsPerServe;
        }

        public float PaddleWidthMultiplier { get; }

        public float BallSpeedMultiplier { get; }

        public float DropChanceMultiplier { get; }

        public float WavyPaddleStrength { get; }

        public int ExtraBallsPerServe { get; }
    }

    internal sealed class BreakoutRunState
    {
        private readonly List<RunUpgradeDefinition> chosenUpgrades = new List<RunUpgradeDefinition>();
        private readonly List<RunUpgradeDefinition> pendingDraftOffers = new List<RunUpgradeDefinition>();
        private readonly Dictionary<string, int> upgradeStacks = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<RunUpgradeDefinition> ChosenUpgrades => chosenUpgrades;

        public IReadOnlyList<RunUpgradeDefinition> PendingDraftOffers => pendingDraftOffers;

        public int ClearedLevelCount { get; private set; }

        public bool HasActiveBuild => chosenUpgrades.Count > 0;

        public void Reset()
        {
            chosenUpgrades.Clear();
            pendingDraftOffers.Clear();
            upgradeStacks.Clear();
            ClearedLevelCount = 0;
        }

        public void RegisterLevelClear()
        {
            ClearedLevelCount++;
        }

        public void SetPendingDraftOffers(IList<RunUpgradeDefinition> offers)
        {
            pendingDraftOffers.Clear();

            if (offers == null)
            {
                return;
            }

            for (var index = 0; index < offers.Count; index++)
            {
                if (offers[index] != null)
                {
                    pendingDraftOffers.Add(offers[index]);
                }
            }
        }

        public void ClearPendingDraftOffers()
        {
            pendingDraftOffers.Clear();
        }

        public int GetStackCount(RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }

            return GetStackCount(definition.UpgradeId);
        }

        public int GetStackCount(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId))
            {
                return 0;
            }

            return upgradeStacks.TryGetValue(upgradeId.Trim(), out var count) ? count : 0;
        }

        public bool HasConflict(RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            var candidateId = definition.UpgradeId;

            for (var index = 0; index < chosenUpgrades.Count; index++)
            {
                var chosen = chosenUpgrades[index];

                if (chosen == null)
                {
                    continue;
                }

                if (UpgradeListsConflict(candidateId, definition.ExcludedUpgradeIds, chosen.UpgradeId, chosen.ExcludedUpgradeIds))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanOffer(RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            return GetStackCount(definition) < definition.MaxStacks && !HasConflict(definition);
        }

        public bool TryApplyPendingDraftOffer(int offerIndex, out RunUpgradeDefinition appliedUpgrade)
        {
            appliedUpgrade = null;

            if (offerIndex < 0 || offerIndex >= pendingDraftOffers.Count)
            {
                return false;
            }

            var selectedUpgrade = pendingDraftOffers[offerIndex];

            if (!CanOffer(selectedUpgrade))
            {
                return false;
            }

            ApplyUpgrade(selectedUpgrade);
            appliedUpgrade = selectedUpgrade;
            pendingDraftOffers.Clear();
            return true;
        }

        public BreakoutRunUpgradeModifiers CalculateModifiers()
        {
            var paddleWidthMultiplier = 1f;
            var ballSpeedMultiplier = 1f;
            var dropChanceMultiplier = 1f;
            var wavyPaddleStrength = 0f;
            var extraBallsPerServe = 0;

            for (var index = 0; index < chosenUpgrades.Count; index++)
            {
                var upgrade = chosenUpgrades[index];

                if (upgrade == null)
                {
                    continue;
                }

                paddleWidthMultiplier *= upgrade.PaddleWidthMultiplier;
                ballSpeedMultiplier *= upgrade.BallSpeedMultiplier;
                dropChanceMultiplier *= upgrade.DropChanceMultiplier;
                wavyPaddleStrength = Mathf.Max(wavyPaddleStrength, upgrade.WavyPaddleStrength);
                extraBallsPerServe += upgrade.ExtraBallsPerServe;
            }

            return new BreakoutRunUpgradeModifiers(
                paddleWidthMultiplier,
                ballSpeedMultiplier,
                dropChanceMultiplier,
                wavyPaddleStrength,
                extraBallsPerServe);
        }

        private void ApplyUpgrade(RunUpgradeDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            chosenUpgrades.Add(definition);
            var upgradeId = definition.UpgradeId;
            upgradeStacks[upgradeId] = GetStackCount(upgradeId) + 1;
        }

        private static bool UpgradeListsConflict(
            string candidateId,
            string[] candidateExclusions,
            string chosenId,
            string[] chosenExclusions)
        {
            if (string.IsNullOrWhiteSpace(candidateId) || string.IsNullOrWhiteSpace(chosenId))
            {
                return false;
            }

            return ArrayContains(candidateExclusions, chosenId) || ArrayContains(chosenExclusions, candidateId);
        }

        private static bool ArrayContains(string[] values, string candidate)
        {
            if (values == null || string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            for (var index = 0; index < values.Length; index++)
            {
                if (string.Equals(values[index], candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
