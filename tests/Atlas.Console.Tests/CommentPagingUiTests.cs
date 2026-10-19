using Atlas.Comments.Comments;
using Atlas.ConsoleApp.Comments;
using Atlas.Participants.Participants;

namespace Atlas.Console.Tests;

[TestClass]
[DoNotParallelize] // Redirects process-wide input/output.
public sealed class CommentPagingUiTests
{
    [TestMethod]
    public void ReadOnlyBrowserLoadsOnlyViewedPagesAndReplyBranches()
    {
        var target = CommentTarget.Node(Guid.NewGuid());
        var repo = new Pages(target);
        repo.Root.RemoveByAuthor(repo.Root.AuthorParticipantId, repo.Root.CreatedAt.AddSeconds(1));
        var inputBefore = System.Console.In;
        var outputBefore = System.Console.Out;
        using var input = new StringReader("2\n1\n4\n4\n3\n4\n0\n");
        using var output = new StringWriter();
        try
        {
            System.Console.SetIn(input); System.Console.SetOut(output);
            CommentCommands.ReadOnly(target, repo, new Participants());
        }
        finally { System.Console.SetIn(inputBefore); System.Console.SetOut(outputBefore); }
        Assert.HasCount(5, repo.Requests);
        Assert.IsNull(repo.Requests[0].ParentId);
        Assert.AreEqual(repo.Root.Id, repo.Requests[1].ParentId);
        Assert.IsNull(repo.Requests[2].ParentId);
        Assert.AreEqual(repo.Cursor, repo.Requests[3].After);
        Assert.IsNull(repo.Requests[4].After);
        StringAssert.Contains(output.ToString(), "COMMENTS (read only)");
        StringAssert.Contains(output.ToString(), "Direct replies");
        Assert.DoesNotContain("1. Reply", output.ToString());
        Assert.DoesNotContain("private-removed-canary", output.ToString());
        StringAssert.Contains(output.ToString(), "[removed:");
    }

    private sealed class Participants : IParticipantRepository
    {
        public Participant? GetById(ParticipantId id) => null;
        public IReadOnlyCollection<Participant> GetAll() => [];
        public void Save(Participant item) => throw new AssertFailedException("Read-only browser must not save participants.");
    }
    private sealed class Pages(CommentTarget target) : ICommentRepository
    {
        public Comment Root { get; } = new(target, null, Guid.NewGuid(), "private-removed-canary", DateTimeOffset.UtcNow);
        public CommentCursor Cursor => new(Root.CreatedAt, Root.Id);
        public List<CommentPageRequest> Requests { get; } = [];
        public Task<CommentPage> ReadPageAsync(CommentPageRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(request.ParentId is null && request.After is null
                ? new CommentPage([Root], Cursor) : new CommentPage([], null));
        }
        public Comment? GetById(CommentId id) => throw new AssertFailedException("Browser should use comments from its current page.");
        public IReadOnlyCollection<Comment> GetByTarget(CommentTarget target) => throw new AssertFailedException("Browser must not load a full thread.");
        public int CountByTarget(CommentTarget target) => throw new AssertFailedException("Browser does not count the full thread.");
        public void Save(Comment comment) => throw new AssertFailedException("Read-only browser must not mutate comments.");
    }
}
