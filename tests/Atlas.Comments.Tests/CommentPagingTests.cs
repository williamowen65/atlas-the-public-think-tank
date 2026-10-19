using Atlas.Comments.Comments;

namespace Atlas.Comments.Tests;

[TestClass]
public sealed class CommentPagingTests
{
    private static readonly DateTimeOffset At = DateTimeOffset.Parse("2026-10-19T12:00:00Z");

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(51)]
    public void RejectsUnboundedPageSizes(int size) =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new CommentPageRequest(CommentTarget.Node(Guid.NewGuid()), size: size));

    [TestMethod]
    public void RejectsEmptyParentAndCursorIds()
    {
        var target = CommentTarget.Node(Guid.NewGuid());
        Assert.Throws<ArgumentException>(() => new CommentPageRequest(target, default(CommentId)));
        Assert.Throws<ArgumentException>(() => new CommentPageRequest(target, after: new CommentCursor(At, default)));
    }

    [TestMethod]
    public async Task RootReadDelegatesOnePageWithoutLoadingTheThread()
    {
        var target = CommentTarget.Node(Guid.NewGuid());
        var repo = new Pages();
        var request = new CommentPageRequest(target);
        var page = await Service(repo).GetPageAsync(request);
        Assert.AreSame(repo.Result, page);
        Assert.AreSame(request, repo.LastRequest);
        Assert.AreEqual(1, repo.PageCalls);
        Assert.AreEqual(0, repo.ParentCalls);
    }

    [TestMethod]
    public async Task RemovedParentCanBeExpandedButCrossTargetOrMissingParentsCannot()
    {
        var target = CommentTarget.Node(Guid.NewGuid());
        var author = Guid.NewGuid();
        var parent = new Comment(target, null, author, "Original", At);
        parent.RemoveByAuthor(author, At.AddSeconds(1));
        var repo = new Pages { Parent = parent };
        await Service(repo).GetPageAsync(new CommentPageRequest(target, parent.Id));
        Assert.AreEqual(1, repo.PageCalls);
        await Assert.ThrowsAsync<ArgumentException>(() => Service(repo).GetPageAsync(
            new CommentPageRequest(CommentTarget.Node(Guid.NewGuid()), parent.Id)));
        repo.Parent = null;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(repo).GetPageAsync(new CommentPageRequest(target, parent.Id)));
        Assert.AreEqual(1, repo.PageCalls);
    }

    [TestMethod]
    public async Task CancellationStopsBeforeRepositoryWorkAndIsForwardedToRead()
    {
        var repo = new Pages();
        using var stop = new CancellationTokenSource();
        stop.Cancel();
        var request = new CommentPageRequest(CommentTarget.Node(Guid.NewGuid()));
        await Assert.ThrowsAsync<OperationCanceledException>(() => Service(repo).GetPageAsync(request, stop.Token));
        Assert.AreEqual(0, repo.PageCalls);
        using var active = new CancellationTokenSource();
        await Service(repo).GetPageAsync(request, active.Token);
        Assert.AreEqual(active.Token, repo.LastToken);
    }

    private static CommentService Service(Pages repo) => new(repo, new Available(), new NoModerator());
    private sealed class Available : ICommentTargetAvailability { public bool IsAvailable(CommentTarget target) => true; }
    private sealed class NoModerator : ICommentModeratorAuthorization { public bool CanModerate(Guid id) => false; }
    private sealed class Pages : ICommentRepository
    {
        public Comment? Parent { get; set; }
        public CommentPage Result { get; } = new([], null);
        public int PageCalls { get; private set; }
        public int ParentCalls { get; private set; }
        public CommentPageRequest? LastRequest { get; private set; }
        public CancellationToken LastToken { get; private set; }
        public Comment? GetById(CommentId id) { ParentCalls++; return Parent?.Id == id ? Parent : null; }
        public IReadOnlyCollection<Comment> GetByTarget(CommentTarget target) => throw new AssertFailedException("Paging must not load a full thread.");
        public int CountByTarget(CommentTarget target) => throw new AssertFailedException("Paging does not need a total count.");
        public void Save(Comment item) => throw new AssertFailedException("Reads must not mutate comments.");
        public Task<CommentPage> ReadPageAsync(CommentPageRequest request, CancellationToken cancellationToken = default)
        { PageCalls++; LastRequest = request; LastToken = cancellationToken; return Task.FromResult(Result); }
    }
}
