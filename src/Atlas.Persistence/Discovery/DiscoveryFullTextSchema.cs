using Microsoft.EntityFrameworkCore;
using Atlas.Discovery;

namespace Atlas.Persistence.Discovery;

/// <summary>SQL shared by the developer-generated migration and fresh-model SQL test databases.</summary>
public static class DiscoveryFullTextSchema
{
    // Keep poll/chart IDs and non-public fields out of the index. Persisting this expression
    // lets SQL Server maintain searchable block text with the original block in one write.
    // EF records this SQL verbatim in its snapshot. Normalize line endings so a migration
    // generated on Windows compares equal to the runtime model on Linux CI.
    public static readonly string BlockTextExpression = """
        CONVERT(nvarchar(max), CASE [Kind]
            WHEN N'markdown' THEN COALESCE([Markdown], N'')
            WHEN N'image' THEN CONCAT([AltText], N' ', [Caption])
            WHEN N'video' THEN CONCAT([Caption], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)
            WHEN N'link-preview' THEN CONCAT([Title], N' ', [Description], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)
            WHEN N'chart-reference' THEN COALESCE([Title], N'')
            ELSE N'' END)
        """.ReplaceLineEndings("\r\n");

    /// <summary>Do not let an empty-table query hide missing or disabled search infrastructure.</summary>
    internal static void EnsureAvailable(AtlasDataContext db, bool includeComments = false)
    {
        var available = db.Database.SqlQuery<int>($"""
            SELECT CASE WHEN COALESCE(FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'), 0) = 1
                AND EXISTS (SELECT 1 FROM sys.fulltext_languages WHERE lcid = 1033)
                AND (SELECT COUNT(*) FROM sys.fulltext_indexes
                    WHERE (object_id IN (OBJECT_ID(N'dbo.NodeRows'), OBJECT_ID(N'dbo.BlockRows'))
                        OR ({includeComments} = 1 AND object_id IN (OBJECT_ID(N'dbo.CommentRows'), OBJECT_ID(N'dbo.SearchDocuments'))))
                      AND is_enabled = 1) = { (includeComments ? 4 : 2) }
                THEN 1 ELSE 0 END AS [Value]
            """).Single();
        if (available != 1)
            throw new SearchUnavailableException(new InvalidOperationException(
                "Full-text support, English resources, or enabled search full-text indexes are missing. Rerun --setup-database."));
    }

    /// <summary>Run after adding BlockRows.SearchText, outside the migration transaction.</summary>
    public static string CreateSql => SqlDiscoveryReadQuery.ReadResource("DiscoveryFullTextCreate.sql");
    /// <summary>Run before dropping BlockRows.SearchText, outside the migration transaction.</summary>
    public static string DropSql => SqlDiscoveryReadQuery.ReadResource("DiscoveryFullTextDrop.sql");
}
