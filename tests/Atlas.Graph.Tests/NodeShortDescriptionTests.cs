using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tests;

[TestClass]
public sealed class NodeShortDescriptionTests
{
    [TestMethod]
    public void AuthoredIntroductionIsBoundedAndDoesNotChangeContentReference()
    {
        var node = NodeTestFactory.Create();
        var document = node.DescriptionId;
        var at = node.UpdatedAt.AddMinutes(1);
        node.ChangeShortDescription("  A concise introduction.  ", node.AuthorId.Value, at);
        Assert.AreEqual("A concise introduction.", node.ShortDescription);
        Assert.AreEqual(document, node.DescriptionId);
        Assert.AreEqual(at, node.UpdatedAt);
        Assert.ThrowsExactly<ArgumentException>(() => node.ChangeShortDescription(new string('x', 281), node.AuthorId.Value, at));
        Assert.AreEqual("A concise introduction.", node.ShortDescription);
        node.ChangeShortDescription(new string('x', 280), node.AuthorId.Value, at);
        Assert.AreEqual(280, node.ShortDescription.Length);
    }

    [TestMethod]
    public void OnlyAuthorCanChangeIntroductionAndTimeCannotGoBackwards()
    {
        var node = NodeTestFactory.Create();
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => node.ChangeShortDescription("Changed", Guid.NewGuid(), node.UpdatedAt));
        Assert.ThrowsExactly<ArgumentException>(() => node.ChangeShortDescription("Changed", node.AuthorId.Value, node.UpdatedAt.AddSeconds(-1)));
        Assert.AreEqual("", node.ShortDescription);
    }

    [TestMethod]
    public void ReconstitutionPreservesIntroductionAndRejectsInvalidStoredText()
    {
        var node = NodeTestFactory.Create();
        Node Restore(string text) => Node.Reconstitute(node.Id, node.Title, node.DescriptionId,
            node.TypeId, node.AuthorId, node.Status, node.CreatedAt, node.UpdatedAt, text);
        var restored = Restore("Saved introduction");
        Assert.AreEqual("Saved introduction", restored.ShortDescription);
        Assert.IsEmpty(restored.DomainEvents);
        Assert.ThrowsExactly<ArgumentException>(() => Restore(new string('x', 281)));
        Assert.ThrowsExactly<ArgumentNullException>(() => Restore(null!));
    }
}
