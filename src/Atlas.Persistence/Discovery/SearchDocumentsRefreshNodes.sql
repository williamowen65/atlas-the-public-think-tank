CREATE OR ALTER PROCEDURE dbo.AtlasRefreshNodeSearch @ids dbo.AtlasSearchIds READONLY
AS
BEGIN
    SET NOCOUNT ON;
    -- Serialize projection upserts for the same result without replacing its full-text key.
    DECLARE @documents TABLE (ResultId uniqueidentifier PRIMARY KEY, NodeId uniqueidentifier, SearchText nvarchar(max));
    INSERT @documents (ResultId, NodeId, SearchText)
    SELECT n.Id, n.Id, CONCAT(CONVERT(nvarchar(max), n.Title), N' ', n.ShortDescription, N' ', blocks.SearchText)
    FROM dbo.NodeRows n JOIN @ids ids ON ids.Id = n.Id
    OUTER APPLY (
        SELECT STRING_AGG(CONVERT(nvarchar(max), b.SearchText), N' ') WITHIN GROUP (ORDER BY link.Position) AS SearchText
        FROM dbo.DocumentBlocks link JOIN dbo.BlockRows b ON b.Id = link.BlockId
        WHERE link.DocumentId = n.DescriptionId
    ) blocks;
    DELETE target FROM dbo.SearchDocuments target
    JOIN @ids ids ON ids.Id = target.ResultId
    WHERE target.Kind = 0 AND NOT EXISTS (SELECT 1 FROM @documents d WHERE d.ResultId = target.ResultId);
    UPDATE target WITH (UPDLOCK, HOLDLOCK)
    SET SearchText = d.SearchText, NodeId = d.NodeId
    FROM dbo.SearchDocuments target JOIN @documents d ON d.ResultId = target.ResultId
    WHERE target.Kind = 0 AND (target.SearchText COLLATE Latin1_General_100_BIN2 <> d.SearchText COLLATE Latin1_General_100_BIN2 OR target.NodeId <> d.NodeId);
    INSERT dbo.SearchDocuments (ResultId, NodeId, Kind, SearchText)
    SELECT d.ResultId, d.NodeId, 0, d.SearchText FROM @documents d
    WHERE NOT EXISTS (SELECT 1 FROM dbo.SearchDocuments target WITH (UPDLOCK, HOLDLOCK)
                      WHERE target.Kind = 0 AND target.ResultId = d.ResultId);
END;
