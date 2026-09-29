using Atlas.Comments.Comments;

namespace Atlas.Comments.Tests;

[TestClass]
public sealed class CommentTests
{
    [TestMethod]
    public void AddTopLevel_CreatesRootComment()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(repo, new AvailableTargets());
        var target = CommentTarget.Node(Guid.NewGuid());

        var comment = service.AddTopLevel(target, Guid.NewGuid(), "First", DateTimeOffset.UtcNow);

        Assert.IsNull(comment.ParentCommentId);
        Assert.AreEqual(target, comment.Target);
    }

    [TestMethod]
    public void Reply_UsesExactlyOneParentAndInheritsTarget()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(repo, new AvailableTargets());
        var target = CommentTarget.Node(Guid.NewGuid());
        var root = service.AddTopLevel(target, Guid.NewGuid(), "Root", DateTimeOffset.UtcNow);

        var reply = service.Reply(root.Id, Guid.NewGuid(), "Reply", root.CreatedAt.AddMinutes(1));

        Assert.AreEqual(root.Id, reply.ParentCommentId);
        Assert.AreEqual(root.Target, reply.Target);
    }

    [TestMethod]
    public void Reply_WithMissingParent_IsRejected()
    {
        var service = new CommentService(new MemoryRepository(), new AvailableTargets());
        Assert.Throws<InvalidOperationException>(() =>
            service.Reply(CommentId.New(), Guid.NewGuid(), "Reply", DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void GetThread_ReturnsNestedParentChildOrder()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(repo, new AvailableTargets());
        var target = CommentTarget.Node(Guid.NewGuid());
        var t = DateTimeOffset.UtcNow;
        var first = service.AddTopLevel(target, Guid.NewGuid(), "First", t);
        var reply = service.Reply(first.Id, Guid.NewGuid(), "Reply", t.AddMinutes(1));
        var nested = service.Reply(reply.Id, Guid.NewGuid(), "Nested", t.AddMinutes(2));
        var second = service.AddTopLevel(target, Guid.NewGuid(), "Second", t.AddMinutes(3));

        var thread = service.GetThread(target);

        CollectionAssert.AreEqual(
            new[] { first.Id, reply.Id, nested.Id, second.Id },
            thread.Select(x => x.Comment.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 0 }, thread.Select(x => x.Depth).ToArray());
    }

    [TestMethod]
    public void Edit_ByNonAuthor_IsRejected()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(repo, new AvailableTargets());
        var comment = service.AddTopLevel(CommentTarget.Node(Guid.NewGuid()), Guid.NewGuid(), "Original", DateTimeOffset.UtcNow);

        Assert.Throws<UnauthorizedAccessException>(() =>
            service.Edit(comment.Id, Guid.NewGuid(), "Changed", comment.CreatedAt.AddMinutes(1)));
    }

    [TestMethod]
    public void Remove_ByModerator_PreservesCommentForThreadContinuity()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(repo, new AvailableTargets());
        var target = CommentTarget.Node(Guid.NewGuid());
        var root = service.AddTopLevel(target, Guid.NewGuid(), "Root", DateTimeOffset.UtcNow);
        var reply = service.Reply(root.Id, Guid.NewGuid(), "Reply", root.CreatedAt.AddMinutes(1));

        service.Remove(root.Id, Guid.NewGuid(), true, root.CreatedAt.AddMinutes(2));

        Assert.AreEqual(CommentStatus.RemovedByModerator, root.Status);
        Assert.AreEqual(reply.Id, service.GetThread(target)[1].Comment.Id);
    }

    [TestMethod]
    public void Mutations_WhenTargetUnavailable_AreRejectedButThreadRemainsReadable()
    {
        var repo = new MemoryRepository();
        var availability = new AvailableTargets();
        var service = new CommentService(repo, availability);
        var target = CommentTarget.Node(Guid.NewGuid());
        var comment = service.AddTopLevel(target, Guid.NewGuid(), "Root", DateTimeOffset.UtcNow);
        availability.Available = false;

        Assert.Throws<InvalidOperationException>(() =>
            service.Edit(comment.Id, comment.AuthorParticipantId, "Changed", comment.CreatedAt.AddMinutes(1)));
        Assert.HasCount(1, service.GetThread(target));
    }

    private sealed class AvailableTargets : ICommentTargetAvailability
    {
        public bool Available { get; set; } = true;
        public bool IsAvailable(CommentTarget target) => Available;
    }

    private sealed class MemoryRepository : ICommentRepository
    {
        private readonly Dictionary<CommentId, Comment> _items = [];

        public Comment? GetById(CommentId id) => _items.GetValueOrDefault(id);

        public IReadOnlyCollection<Comment> GetByTarget(CommentTarget target) =>
            _items.Values.Where(x => x.Target == target).ToList();

        public void Save(Comment comment) => _items[comment.Id] = comment;
    }
}
