using Atlas.Graph;
using Atlas.Graph.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeTests
{
    [TestMethod]
    public void Constructor_SetsTitle()
    {
        var node = new Node(
            new NodeTitle("Climate adaption"),
            NodeType.Question,
            DateTimeOffset.UtcNow);

        Assert.AreEqual(new NodeTitle("Climate adaption"), node.Title);
    }

    [TestMethod]
    public void Rename_WithValidTitle_ChangesTitle()
    {
        var node = new Node(
        new NodeTitle("Climate adaption"),
        NodeType.Question,
        DateTimeOffset.UtcNow);


        node.Rename(
            new NodeTitle("Updated title"),
            DateTimeOffset.UtcNow);

        Assert.AreEqual(new NodeTitle("Updated title"), node.Title);
    }

    [TestMethod]
    public void Rename_WithBlankTitle_ThrowsArgumentException()
    {
        var node = new Node(
            new NodeTitle("Climate adaption"),
            NodeType.Question,
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(
            () => node.Rename(
                new NodeTitle("    "),
                DateTimeOffset.UtcNow));

        Assert.AreEqual(new NodeTitle("Climate adaption"), node.Title);
    }
}