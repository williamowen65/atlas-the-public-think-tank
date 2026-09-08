using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

/// <summary>Verifies node type pluralization behavior and boundary rules.</summary>
[TestClass]
public class NodeTypePluralizationTests
{
    /// <summary>Verifies that create custom defaults auto pluralize to true.</summary>
    [TestMethod]
    public void CreateCustom_DefaultsAutoPluralizeToTrue()
    {
        var nodeType = NodeTypeDefinition.CreateCustom(
            "Comment",
            "A response.",
            "participant-id",
            DateTimeOffset.UtcNow);

        Assert.IsTrue(nodeType.AutoPluralize);
    }

    /// <summary>Verifies that create custom can disable auto pluralize.</summary>
    [TestMethod]
    public void CreateCustom_CanDisableAutoPluralize()
    {
        var nodeType = NodeTypeDefinition.CreateCustom(
            "Counter Evidence",
            "Evidence challenging a claim.",
            "participant-id",
            DateTimeOffset.UtcNow,
            autoPluralize: false);

        Assert.IsFalse(nodeType.AutoPluralize);
    }

    /// <summary>Verifies that change auto pluralize updates setting.</summary>
    [TestMethod]
    public void ChangeAutoPluralize_UpdatesSetting()
    {
        var nodeType = NodeTypeDefinition.CreateCustom(
            "Evidence",
            "Supporting information.",
            "participant-id",
            DateTimeOffset.UtcNow);

        nodeType.ChangeAutoPluralize(
            false,
            "participant-id",
            actorIsModerator: false,
            changedAt: DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.IsFalse(nodeType.AutoPluralize);
    }

    /// <summary>Verifies that change auto pluralize by different owner throws.</summary>
    [TestMethod]
    public void ChangeAutoPluralize_ByDifferentOwner_Throws()
    {
        var nodeType = NodeTypeDefinition.CreateCustom(
            "Comment",
            "A response.",
            "owner-id",
            DateTimeOffset.UtcNow);

        Assert.Throws<UnauthorizedAccessException>(
            () => nodeType.ChangeAutoPluralize(
                false,
                "different-participant",
                actorIsModerator: false,
                DateTimeOffset.UtcNow.AddMinutes(1)));
    }
}
