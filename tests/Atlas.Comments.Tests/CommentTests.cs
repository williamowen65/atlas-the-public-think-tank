using Atlas.Contracts.Operations;
using Atlas.Comments.Comments;

namespace Atlas.Comments.Tests;

[TestClass]
public sealed class CommentTests
{
    [TestMethod]
    public void AddTopLevel_CreatesRootComment()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(
            repo,
            new AvailableTargets(),
            new NoModerators()
        );
        var target = CommentTarget.Node(Guid.NewGuid());

        var comment = service.AddTopLevel(
            target,
            Guid.NewGuid(),
            "First",
            DateTimeOffset.UtcNow
        );

        Assert.IsNull(comment.ParentCommentId);
        Assert.AreEqual(
            target,
            comment.Target
        );
    }

    [TestMethod]
    public void Reply_UsesExactlyOneParentAndInheritsTarget()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(
            repo,
            new AvailableTargets(),
            new AllModerators()
        );
        var target = CommentTarget.Node(Guid.NewGuid());
        var root = service.AddTopLevel(
            target,
            Guid.NewGuid(),
            "Root",
            DateTimeOffset.UtcNow
        );

        var reply = service.Reply(
            root.Id,
            Guid.NewGuid(),
            "Reply",
            root.CreatedAt.AddMinutes(1)
        );

        Assert.AreEqual(
            root.Id,
            reply.ParentCommentId
        );
        Assert.AreEqual(
            root.Target,
            reply.Target
        );
    }

    [TestMethod]
    public void Reply_WithMissingParent_IsRejected()
    {
        var service = new CommentService(
            new MemoryRepository(),
            new AvailableTargets(),
            new NoModerators()
        );
        Assert.Throws<InvalidOperationException>(() =>
            service.Reply(
                CommentId.New(),
                Guid.NewGuid(),
                "Reply",
                DateTimeOffset.UtcNow
            ));
    }

    [TestMethod]
    public void GetThread_ReturnsNestedParentChildOrder()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(
            repo,
            new AvailableTargets(),
            new NoModerators()
        );
        var target = CommentTarget.Node(Guid.NewGuid());
        var t = DateTimeOffset.UtcNow;
        var first = service.AddTopLevel(
            target,
            Guid.NewGuid(),
            "First",
            t
        );
        var reply = service.Reply(
            first.Id,
            Guid.NewGuid(),
            "Reply",
            t.AddMinutes(1)
        );
        var nested = service.Reply(
            reply.Id,
            Guid.NewGuid(),
            "Nested",
            t.AddMinutes(2)
        );
        var second = service.AddTopLevel(
            target,
            Guid.NewGuid(),
            "Second",
            t.AddMinutes(3)
        );

        var thread = service.GetThread(target);

        CollectionAssert.AreEqual(
            new[] {
                first.Id,
                reply.Id,
                nested.Id,
                second.Id
            },
            thread.Select(x => x.Comment.Id).ToArray()
        );
        CollectionAssert.AreEqual(
            new[] {
                0,
                1,
                2,
                0
            },
            thread.Select(x => x.Depth).ToArray()
        );
    }

    [TestMethod]
    public void Edit_ByNonAuthor_IsRejected()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(
            repo,
            new AvailableTargets(),
            new NoModerators()
        );
        var comment = service.AddTopLevel(
            CommentTarget.Node(Guid.NewGuid()),
            Guid.NewGuid(),
            "Original",
            DateTimeOffset.UtcNow
        );

        Assert.Throws<UnauthorizedAccessException>(() =>
            service.Edit(
                comment.Id,
                Guid.NewGuid(),
                "Changed",
                comment.CreatedAt.AddMinutes(1)
            ));
    }

    [TestMethod]
    public void Remove_ByModerator_PreservesCommentForThreadContinuity()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(
            repo,
            new AvailableTargets(),
            new AllModerators()
        );
        var target = CommentTarget.Node(Guid.NewGuid());
        var root = service.AddTopLevel(
            target,
            Guid.NewGuid(),
            "Root",
            DateTimeOffset.UtcNow
        );
        var reply = service.Reply(
            root.Id,
            Guid.NewGuid(),
            "Reply",
            root.CreatedAt.AddMinutes(1)
        );

        service.Remove(
            root.Id,
            Guid.NewGuid(),
            root.CreatedAt.AddMinutes(2)
        );

        Assert.AreEqual(
            CommentStatus.RemovedByModerator,
            root.Status
        );
        Assert.AreEqual(
            reply.Id,
            service.GetThread(target)[1].Comment.Id
        );
    }

    [TestMethod]
    public void Mutations_WhenTargetUnavailable_AreRejectedButThreadRemainsReadable()
    {
        var repo = new MemoryRepository();
        var availability = new AvailableTargets();
        var service = new CommentService(
            repo,
            availability,
            new NoModerators()
        );
        var target = CommentTarget.Node(Guid.NewGuid());
        var comment = service.AddTopLevel(
            target,
            Guid.NewGuid(),
            "Root",
            DateTimeOffset.UtcNow
        );
        availability.Available = false;

        Assert.Throws<InvalidOperationException>(() =>
            service.Edit(
                comment.Id,
                comment.AuthorParticipantId,
                "Changed",
                comment.CreatedAt.AddMinutes(1)
            ));
        Assert.HasCount(
            1,
            service.GetThread(target)
        );
    }

    /// <summary>Rejection must preserve the comment, not merely throw an exception.</summary>
    [TestMethod]
    public void OutsiderEditAndRemovePreserveBodyStatusAndSaveCount()
    {
        var repo = new MemoryRepository();
        var service = new CommentService(repo, new AvailableTargets(), new NoModerators());
        var root = service.AddTopLevel(CommentTarget.Node(Guid.NewGuid()), Guid.NewGuid(), "Original", DateTimeOffset.UtcNow);
        var before = (root.Body, root.Status, root.UpdatedAt);
        var outsider = Guid.NewGuid();
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.Edit(root.Id, outsider, "Forbidden", root.CreatedAt.AddMinutes(1)));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.Remove(root.Id, outsider, root.CreatedAt.AddMinutes(1)));
        Assert.AreEqual(before, (root.Body, root.Status, root.UpdatedAt));
        Assert.AreEqual(1, repo.SaveCalls);
    }

    /// <summary>An unavailable target rejects new comments and replies without growing the thread.</summary>
    [TestMethod]
    public void UnavailableTargetRejectsCreationAndReplyWithoutSaving()
    {
        var repo = new MemoryRepository();
        var availability = new AvailableTargets();
        var service = new CommentService(repo, availability, new NoModerators());
        var target = CommentTarget.Node(Guid.NewGuid());
        var root = service.AddTopLevel(target, Guid.NewGuid(), "Original", DateTimeOffset.UtcNow);
        availability.Available = false;
        Assert.ThrowsExactly<InvalidOperationException>(() => service.AddTopLevel(target, root.AuthorParticipantId, "Spam", root.CreatedAt.AddMinutes(1)));
        Assert.ThrowsExactly<InvalidOperationException>(() => service.Reply(root.Id, root.AuthorParticipantId, "Spam", root.CreatedAt.AddMinutes(1)));
        Assert.AreEqual(1, repo.SaveCalls);
        Assert.HasCount(1, service.GetThread(target));
    }

    /// <summary>Depth is valid discussion structure, not an abuse verdict or a recursion limit.</summary>
    [TestMethod]
    public void DeepThreadRemainsReadableInOrderWithoutRecursion()
    {
        var repo = new MemoryRepository();
        var target = CommentTarget.Node(Guid.NewGuid());
        var actor = Guid.NewGuid();
        var at = DateTimeOffset.UtcNow;
        CommentId? parent = null;
        const int count = 20_000;
        for (var index = 0; index < count; index++)
        {
            var comment = new Comment(target, parent, actor, index.ToString(), at.AddTicks(index));
            repo.Save(comment);
            parent = comment.Id;
        }
        var service = new CommentService(repo, new AvailableTargets(), new NoModerators());
        var thread = service.GetThread(target);
        Assert.HasCount(count, thread);
        Assert.AreEqual(0, thread[0].Depth);
        Assert.AreEqual(count - 1, thread[^1].Depth);
        Assert.AreEqual((count - 1).ToString(), thread[^1].Comment.Body);
        Assert.AreEqual(count, repo.SaveCalls); // Reading adds no writes.
    }

    [TestMethod]
    public void OrphanedStoredThreadIsReportedInsteadOfSilentlyOmittingComments()
    {
        var repo = new MemoryRepository();
        var target = CommentTarget.Node(Guid.NewGuid());
        repo.Save(new Comment(target, CommentId.New(), Guid.NewGuid(), "Orphan", DateTimeOffset.UtcNow));
        var service = new CommentService(repo, new AvailableTargets(), new NoModerators());
        Assert.ThrowsExactly<InvalidOperationException>(() => service.GetThread(target));
        Assert.AreEqual(1, repo.SaveCalls);
    }

    [TestMethod]
    public void DisconnectedStoredCycleFailsWithoutHidingCorruptReplies()
    {
        var repo = new MemoryRepository();
        var target = CommentTarget.Node(Guid.NewGuid());
        var actor = Guid.NewGuid();
        var at = DateTimeOffset.UtcNow;
        var first = CommentId.New();
        var second = CommentId.New();
        repo.Save(new Comment(target, null, actor, "Valid root", at));
        repo.Save(Comment.Reconstitute(first, target, second, actor, "A", CommentStatus.Active, at, at, null));
        repo.Save(Comment.Reconstitute(second, target, first, actor, "B", CommentStatus.Active, at, at, null));
        var service = new CommentService(repo, new AvailableTargets(), new NoModerators());
        Assert.ThrowsExactly<InvalidOperationException>(() => service.GetThread(target));
        Assert.AreEqual(3, repo.SaveCalls);
    }

    private sealed class AvailableTargets : ICommentTargetAvailability
    {
        public bool Available { get; set; } = true;
        public bool IsAvailable(CommentTarget target) => Available;
    }

    private sealed class MemoryRepository : ICommentRepository, IReferenceLookup
    {
        public bool IsAvailable(
            string kind,
            Guid id,
            bool requireActive
        ) => id != Guid.Empty;

        private readonly Dictionary<CommentId, Comment> _items = [];

        public Comment? GetById(CommentId id) => _items.GetValueOrDefault(id);

        public IReadOnlyCollection<Comment> GetByTarget(CommentTarget target) =>
            _items.Values.Where(x => x.Target == target).ToList();

        public int SaveCalls { get; private set; }
        public void Save(Comment comment) { _items[comment.Id] = comment; SaveCalls++; }
    }
    private sealed class NoModerators : ICommentModeratorAuthorization
    {
        public bool CanModerate(Guid participantId) => false;
    }
    private sealed class AllModerators : ICommentModeratorAuthorization
    {
        public bool CanModerate(Guid participantId) => true;
    }
}
