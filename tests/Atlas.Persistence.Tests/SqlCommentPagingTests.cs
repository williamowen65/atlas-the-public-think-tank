using Atlas.Application.Storage;
using Atlas.Comments.Comments;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class SqlCommentPagingTests
{
    [TestMethod]
    public async Task RootAndReplyPagesStayScopedPreservePlaceholdersAndDoNotLoadDescendants()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false);
        var target = CommentTarget.Node(database.AddNode());
        var other = CommentTarget.Node(database.AddNode());
        var author = database.AddParticipant();
        var repo = new SqlCommentRepository(database.Open);
        var at = DateTimeOffset.UtcNow.AddMinutes(-1);
        var roots = Enumerable.Range(0, 5).Select(i => new Comment(target, null, author, $"Root {i}", at.AddSeconds(i))).ToArray();
        foreach (var root in roots) repo.Save(root);
        var reply = new Comment(target, roots[0].Id, author, "Reply", at.AddSeconds(6));
        repo.Save(reply);
        repo.Save(new Comment(target, reply.Id, author, "Grandchild loaded separately", at.AddSeconds(7)));
        repo.Save(new Comment(other, null, author, "Other discussion", at));
        roots[0].RemoveByAuthor(author, at.AddSeconds(8)); repo.Save(roots[0]);
        var first = await repo.ReadPageAsync(new CommentPageRequest(target, size: 2));
        CollectionAssert.AreEqual(roots.Take(2).Select(c => c.Id).ToArray(), first.Items.Select(c => c.Id).ToArray());
        Assert.IsTrue(first.Items[0].IsRemoved);
        Assert.IsNotNull(first.Next);
        var second = await repo.ReadPageAsync(new CommentPageRequest(target, after: first.Next, size: 2));
        var last = await repo.ReadPageAsync(new CommentPageRequest(target, after: second.Next, size: 2));
        Assert.HasCount(1, last.Items); Assert.IsNull(last.Next);
        var replies = await repo.ReadPageAsync(new CommentPageRequest(target, roots[0].Id));
        Assert.HasCount(1, replies.Items); Assert.AreEqual(reply.Id, replies.Items[0].Id);
        var grandchildren = await repo.ReadPageAsync(new CommentPageRequest(target, reply.Id));
        Assert.HasCount(1, grandchildren.Items);
        Assert.AreEqual(7, repo.CountByTarget(target));
        Assert.IsEmpty((await repo.ReadPageAsync(new CommentPageRequest(other, roots[0].Id))).Items);
    }

    [TestMethod]
    public async Task TimestampTiesAndInsertedRowsDoNotRepeatEarlierPages()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true, installFullText: false);
        var target = CommentTarget.Node(database.AddNode());
        var author = database.AddParticipant();
        var at = DateTimeOffset.UtcNow.AddMinutes(-1);
        var repo = new SqlCommentRepository(database.Open);
        for (var i = 0; i < 55; i++) repo.Save(new Comment(target, null, author, $"Tied {i}", at));
        var first = await repo.ReadPageAsync(new CommentPageRequest(target, size: 20));
        repo.Save(new Comment(target, null, author, "New later comment", at.AddSeconds(1)));
        var ids = first.Items.Select(c => c.Id).ToList();
        var cursor = first.Next;
        while (cursor is not null)
        {
            var page = await repo.ReadPageAsync(new CommentPageRequest(target, after: cursor, size: 20));
            Assert.IsLessThanOrEqualTo(20, page.Items.Count);
            ids.AddRange(page.Items.Select(c => c.Id)); cursor = page.Next;
        }
        Assert.HasCount(56, ids);
        Assert.AreEqual(56, ids.Distinct().Count());
    }

    [TestMethod]
    public async Task CancelledReadDoesNotNeedDatabaseWork()
    {
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        var repo = new SqlCommentRepository(() => throw new AssertFailedException("Cancellation should precede context creation."));
        await Assert.ThrowsAsync<OperationCanceledException>(() => repo.ReadPageAsync(
            new CommentPageRequest(CommentTarget.Node(Guid.NewGuid())), cancelled.Token));
    }
}
