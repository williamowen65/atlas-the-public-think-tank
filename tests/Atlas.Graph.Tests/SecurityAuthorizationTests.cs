using Atlas.Graph.Nodes;

namespace Atlas.Graph.Tests;

/// <summary>Security regression tests for the Graph authorization boundary.</summary>
[TestClass]
public class SecurityAuthorizationTests
{
    [TestMethod]
    public void SEC002_MissingActor_CannotRenameOrChangeNodeState()
    {
        var node = NodeTestFactory.Create();
        var originalTitle = node.Title;
        var originalTime = node.UpdatedAt;
        var originalEventCount = node.DomainEvents.Count;

        Assert.ThrowsExactly<ArgumentException>(() =>
            node.Rename(new NodeTitle("Changed"), Guid.Empty, originalTime.AddMinutes(1)));
        Assert.ThrowsExactly<ArgumentException>(() =>
            node.Archive(Guid.Empty, originalTime.AddMinutes(1)));

        Assert.AreEqual(originalTitle, node.Title);
        Assert.AreEqual(originalTime, node.UpdatedAt);
        Assert.AreEqual(originalEventCount, node.DomainEvents.Count);
    }

    [TestMethod]
    public void SEC002_ForeignActor_CannotPassExplicitAuthorCheck()
    {
        var node = NodeTestFactory.Create();
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => node.EnsureAuthoredBy(Guid.NewGuid()));
        node.EnsureAuthoredBy(node.AuthorId.Value);
    }
}
