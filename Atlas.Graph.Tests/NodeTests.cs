using Microsoft.VisualStudio.TestTools.UnitTesting;
using Atlas.Graph;

namespace Atlas.Graph.Tests;

[TestClass]
public class NodeTests
{
    [TestMethod]
    public void Constructor_SetsTitle()
    {
        var node = new Node("Climate adaptation");

        Assert.AreEqual("Climate adaptation", node.Title);
    }
}