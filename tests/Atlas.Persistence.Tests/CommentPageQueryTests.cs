using Atlas.Application.Storage;
using Atlas.Comments.Comments;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

[TestClass]
public sealed class CommentPageQueryTests
{
    [TestMethod]
    public void SqlContainsBoundedTopAndKeysetFiltersWithoutOffsetOrDescendantTraversal()
    {
        using var db = new AtlasDataContext(new DbContextOptionsBuilder<AtlasDataContext>()
            .UseSqlServer("Server=localhost;Database=QueryOnly;Integrated Security=True;TrustServerCertificate=True").Options);
        var request = new CommentPageRequest(CommentTarget.Node(Guid.NewGuid()), CommentId.New(),
            new CommentCursor(DateTimeOffset.Parse("2026-10-19T12:00:00Z"), CommentId.New()), size: 50);
        var sql = SqlCommentRepository.BuildPageQuery(db, request).ToQueryString();
        Console.WriteLine(sql);
        StringAssert.Contains(sql, "TOP(");
        StringAssert.Contains(sql, "51"); // One extra sibling determines Next without COUNT or a second query.
        StringAssert.Contains(sql, "[ParentCommentId]");
        StringAssert.Contains(sql, "[TargetKind]");
        StringAssert.Contains(sql, "[TargetId]");
        StringAssert.Contains(sql, "ORDER BY");
        Assert.DoesNotContain("OFFSET", sql);
        Assert.DoesNotContain("COUNT(", sql);
        var index = db.Model.FindEntityType(typeof(CommentRow))!.GetIndexes()
            .Single(item => item.GetDatabaseName() == "IX_CommentRows_SiblingPage");
        CollectionAssert.AreEqual(new[] { "TargetKind", "TargetId", "ParentCommentId", "CreatedAt", "Id" },
            index.Properties.Select(property => property.Name).ToArray());
    }
}
