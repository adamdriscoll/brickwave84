using System.Reflection;
using GetBricked.Gameplay;
using GetBricked.Gameplay.Data;
using NUnit.Framework;
using UnityEngine;

public sealed class BreakoutSplitBrickTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private BrickDefinition parentDefinition;
    private BrickDefinition fragmentDefinition;

    [TearDown]
    public void TearDown()
    {
        if (parentDefinition != null)
        {
            UnityEngine.Object.DestroyImmediate(parentDefinition);
        }

        if (fragmentDefinition != null)
        {
            UnityEngine.Object.DestroyImmediate(fragmentDefinition);
        }
    }

    [Test]
    public void SplitBrickDefinitionRequiresFragmentDefinition()
    {
        parentDefinition = CreateBrickDefinition("Split Brick", 2, 1f);

        SetPrivateField(parentDefinition, "splitsOnBreak", true);

        Assert.That(parentDefinition.SplitsOnBreak, Is.False);

        fragmentDefinition = CreateBrickDefinition("Tiny Brick", 1, 0.5f);
        SetPrivateField(parentDefinition, "splitBrickDefinition", fragmentDefinition);

        Assert.That(parentDefinition.SplitsOnBreak, Is.True);
        Assert.That(parentDefinition.SplitBrickDefinition, Is.SameAs(fragmentDefinition));
    }

    [Test]
    public void SplitBrickOffsetsCreateFourTinyCellsInsideParentFootprint()
    {
        fragmentDefinition = CreateBrickDefinition("Tiny Brick", 1, 0.5f);
        var brickServiceType = typeof(BreakoutGameController).Assembly.GetType("GetBricked.Gameplay.BreakoutBrickService", throwOnError: false);
        Assert.That(brickServiceType, Is.Not.Null);

        var method = brickServiceType.GetMethod("BuildSplitBrickOffsets", BindingFlags.Static | InstanceFlags);
        Assert.That(method, Is.Not.Null);

        var offsets = (Vector2[])method.Invoke(null, new object[] { new Vector2(1.6f, 0.7f), fragmentDefinition });

        Assert.That(offsets, Has.Length.EqualTo(4));
        AssertVectorNearlyEqual(new Vector2(-0.416f, 0.182f), offsets[0]);
        AssertVectorNearlyEqual(new Vector2(0.416f, 0.182f), offsets[1]);
        AssertVectorNearlyEqual(new Vector2(-0.416f, -0.182f), offsets[2]);
        AssertVectorNearlyEqual(new Vector2(0.416f, -0.182f), offsets[3]);
    }

    private static BrickDefinition CreateBrickDefinition(string displayName, int hitPoints, float sizeMultiplier)
    {
        var definition = ScriptableObject.CreateInstance<BrickDefinition>();
        SetPrivateField(definition, "displayName", displayName);
        SetPrivateField(definition, "hitPoints", hitPoints);
        SetPrivateField(definition, "sizeMultiplier", sizeMultiplier);
        return definition;
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, InstanceFlags);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }

    private static void AssertVectorNearlyEqual(Vector2 expected, Vector2 actual)
    {
        Assert.That(Vector2.Distance(expected, actual), Is.LessThan(0.0001f));
    }
}
