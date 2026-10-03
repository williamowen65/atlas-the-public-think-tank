using Atlas.Comments.Comments;

namespace Atlas.Comments.Tests;

[TestClass]
public sealed class ValidationTests
{
    [TestMethod]
    public void ReconstitutionRejectsUndefinedStatus()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentOutOfRangeException>(() => Comment.Reconstitute(CommentId.New(),
            CommentTarget.Node(Guid.NewGuid()), null, Guid.NewGuid(), "Body", (CommentStatus)999, now, now, null));
    }
}
