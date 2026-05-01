using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutLevelAssetTests
{
    [Test]
    public void AuthoredLevelsRequireClearingBricks()
    {
        var levels = Resources.LoadAll<LevelDefinition>("Levels");

        Assert.That(levels, Is.Not.Empty);

        for (var index = 0; index < levels.Length; index++)
        {
            Assert.That(levels[index].CompletionRule, Is.EqualTo(LevelCompletionRule.ClearRequiredBricks), levels[index].name);
            Assert.That(levels[index].TargetScore, Is.EqualTo(0), levels[index].name);
        }
    }
}
