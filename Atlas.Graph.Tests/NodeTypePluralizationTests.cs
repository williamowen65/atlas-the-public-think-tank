using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeTypePluralizationTests
{
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
