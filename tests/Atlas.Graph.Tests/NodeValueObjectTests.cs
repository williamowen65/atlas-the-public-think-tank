using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

/// <summary>Verifies node value object behavior and boundary rules.</summary>
[TestClass]
public class NodeValueObjectTests
{
    /// <summary>Verifies that node title when blank throws.</summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void NodeTitle_WhenBlank_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => new NodeTitle(value));
    }

    /// <summary>Verifies that node title trims value.</summary>
    [TestMethod]
    public void NodeTitle_TrimsValue()
    {
        var title = new NodeTitle("  Climate adaptation  ");

        Assert.AreEqual("Climate adaptation", title.Value);
        Assert.AreEqual("Climate adaptation", title.ToString());
    }

    /// <summary>Verifies that node title at maximum length is accepted.</summary>
    [TestMethod]
    public void NodeTitle_AtMaximumLength_IsAccepted()
    {
        var value = new string('a', NodeTitle.MaximumLength);

        var title = new NodeTitle(value);

        Assert.AreEqual(value, title.Value);
    }

    /// <summary>Verifies that node title over maximum length throws.</summary>
    [TestMethod]
    public void NodeTitle_OverMaximumLength_Throws()
    {
        var value = new string('a', NodeTitle.MaximumLength + 1);

        Assert.Throws<ArgumentException>(() => new NodeTitle(value));
    }

    /// <summary>Verifies that node description id when empty throws.</summary>
    [TestMethod]
    public void NodeDescriptionId_WhenEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new NodeDescriptionId(Guid.Empty));
    }

    /// <summary>Verifies that node author id when empty throws.</summary>
    [TestMethod]
    public void NodeAuthorId_WhenEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new NodeAuthorId(Guid.Empty));
    }

    /// <summary>Verifies that requested sub node type when type id is empty throws.</summary>
    [TestMethod]
    public void RequestedSubNodeType_WhenTypeIdIsEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new RequestedSubNodeType(
                new NodeTypeId(Guid.Empty)));
    }
}
