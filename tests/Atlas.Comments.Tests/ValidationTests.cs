using Atlas.Comments.Comments;

namespace Atlas.Comments.Tests;

[TestClass]
public sealed class ValidationTests
{
    [TestMethod]
    public void DefaultIdsAndInconsistentRemovalStateAreRejected()
    {
        var now = DateTimeOffset.UtcNow;
        var target = new CommentTarget("Node", Guid.NewGuid());
        var actor = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => Comment.Reconstitute(default, target, null, actor, "Text", CommentStatus.Active, now, now, null));
        Assert.Throws<ArgumentException>(() => new Comment(target, default(CommentId), actor, "Text", now));
        Assert.Throws<ArgumentException>(() => Comment.Reconstitute(CommentId.New(), target, null, actor, "Text", CommentStatus.RemovedByAuthor, now, now, null));
        Assert.Throws<ArgumentException>(() => Comment.Reconstitute(CommentId.New(), target, null, actor, "Text", CommentStatus.Active, now, now, now));
        Assert.Throws<ArgumentException>(() => Comment.Reconstitute(CommentId.New(), target, null, actor, "Text", CommentStatus.RemovedByAuthor, now, now.AddMinutes(2), now.AddMinutes(1)));
    }

    [TestMethod]
    public void ReconstitutionRejectsUndefinedStatus()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentOutOfRangeException>(() => Comment.Reconstitute(CommentId.New(),
            CommentTarget.Node(Guid.NewGuid()), null, Guid.NewGuid(), "Body", (CommentStatus)999, now, now, null));
    }
}
