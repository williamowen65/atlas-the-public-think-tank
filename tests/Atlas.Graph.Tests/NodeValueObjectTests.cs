using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeValueObjectTests
{
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void NodeTitle_WhenBlank_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => new NodeTitle(value));
    }

    [TestMethod]
    public void NodeTitle_TrimsValue()
    {
        var title = new NodeTitle("  Climate adaptation  ");

        Assert.AreEqual("Climate adaptation", title.Value);
        Assert.AreEqual("Climate adaptation", title.ToString());
    }

    [TestMethod]
    public void NodeTitle_AtMaximumLength_IsAccepted()
    {
        var value = new string('a', NodeTitle.MaximumLength);

        var title = new NodeTitle(value);

        Assert.AreEqual(value, title.Value);
    }

    [TestMethod]
    public void NodeTitle_OverMaximumLength_Throws()
    {
        var value = new string('a', NodeTitle.MaximumLength + 1);

        Assert.Throws<ArgumentException>(() => new NodeTitle(value));
    }

    [TestMethod]
    public void NodeDescriptionId_WhenEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new NodeDescriptionId(Guid.Empty));
    }

    [TestMethod]
    public void NodeAuthorId_WhenEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new NodeAuthorId(Guid.Empty));
    }

    [TestMethod]
    public void RequestedSubNodeType_WhenTypeIdIsEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new RequestedSubNodeType(
                new NodeTypeId(Guid.Empty)));
    }
}
