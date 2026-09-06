using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeTests
{
    [TestMethod]
    public void Constructor_SetsTitle()
    {
        var node = CreateNode("Climate adaptation");

        Assert.AreEqual(
            new NodeTitle("Climate adaptation"),
            node.Title);
    }

    [TestMethod]
    public void Constructor_SetsDescriptionReference()
    {
        var descriptionId = new NodeDescriptionId(Guid.NewGuid());

        var node = new Node(
            new NodeTitle("Climate adaptation"),
            descriptionId,
            NodeTypeId.New(),
            DateTimeOffset.UtcNow);

        Assert.AreEqual(descriptionId, node.DescriptionId);
    }

    [TestMethod]
    public void Rename_WithValidTitle_ChangesTitle()
    {
        var node = CreateNode("Climate adaptation");

        node.Rename(
            new NodeTitle("Updated title"),
            DateTimeOffset.UtcNow);

        Assert.AreEqual(
            new NodeTitle("Updated title"),
            node.Title);
    }

    [TestMethod]
    public void Rename_WithBlankTitle_ThrowsArgumentException()
    {
        var node = CreateNode("Climate adaptation");

        Assert.Throws<ArgumentException>(
            () => node.Rename(
                new NodeTitle("    "),
                DateTimeOffset.UtcNow));

        Assert.AreEqual(
            new NodeTitle("Climate adaptation"),
            node.Title);
    }

    [TestMethod]
    public void ReplaceDescriptionReference_ChangesReference()
    {
        var node = CreateNode("Climate adaptation");
        var replacementId =
            new NodeDescriptionId(Guid.NewGuid());

        node.ReplaceDescriptionReference(
            replacementId,
            DateTimeOffset.UtcNow);

        Assert.AreEqual(replacementId, node.DescriptionId);
    }

    private static Node CreateNode(string title)
    {
        return new Node(
            new NodeTitle(title),
            new NodeDescriptionId(Guid.NewGuid()),
            NodeTypeId.New(),
            DateTimeOffset.UtcNow);
    }
}
