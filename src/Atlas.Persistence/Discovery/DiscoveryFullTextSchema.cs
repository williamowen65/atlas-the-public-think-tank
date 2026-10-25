namespace Atlas.Persistence.Discovery;

/// <summary>SQL shared by the developer-generated migration and fresh-model SQL test databases.</summary>
public static class DiscoveryFullTextSchema
{
    // Keep poll/chart IDs and non-public fields out of the index. Persisting this expression
    // lets SQL Server maintain searchable block text with the original block in one write.
    public const string BlockTextExpression = """
        CONVERT(nvarchar(max), CASE [Kind]
            WHEN N'markdown' THEN COALESCE([Markdown], N'')
            WHEN N'image' THEN CONCAT([AltText], N' ', [Caption])
            WHEN N'video' THEN CONCAT([Caption], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)
            WHEN N'link-preview' THEN CONCAT([Title], N' ', [Description], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)
            WHEN N'chart-reference' THEN COALESCE([Title], N'')
            ELSE N'' END)
        """;

    /// <summary>Run after adding BlockRows.SearchText, outside the migration transaction.</summary>
    public static string CreateSql => SqlDiscoveryReadQuery.ReadResource("DiscoveryFullTextCreate.sql");
    /// <summary>Run before dropping BlockRows.SearchText, outside the migration transaction.</summary>
    public static string DropSql => SqlDiscoveryReadQuery.ReadResource("DiscoveryFullTextDrop.sql");
}
