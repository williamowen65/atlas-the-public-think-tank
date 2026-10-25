CREATE OR ALTER PROCEDURE dbo.AtlasRefreshCommentSearch @ids dbo.AtlasSearchIds READONLY
AS
BEGIN
    SET NOCOUNT ON;
    -- Serialize projection upserts for the same result without replacing its full-text key.
    DECLARE @documents TABLE (ResultId uniqueidentifier PRIMARY KEY, NodeId uniqueidentifier, SearchText nvarchar(max));
    INSERT @documents (ResultId, NodeId, SearchText)
    SELECT c.Id, c.TargetId, c.Body FROM dbo.CommentRows c JOIN @ids ids ON ids.Id = c.Id
    WHERE c.TargetKind = N'Node' AND c.Status = N'Active' AND c.RemovedAt IS NULL;
    DELETE target FROM dbo.SearchDocuments target
    JOIN @ids ids ON ids.Id = target.ResultId
    WHERE target.Kind = 1 AND NOT EXISTS (SELECT 1 FROM @documents d WHERE d.ResultId = target.ResultId);
    UPDATE target WITH (UPDLOCK, HOLDLOCK)
    SET SearchText = d.SearchText, NodeId = d.NodeId
    FROM dbo.SearchDocuments target JOIN @documents d ON d.ResultId = target.ResultId
    WHERE target.Kind = 1 AND (target.SearchText COLLATE Latin1_General_100_BIN2 <> d.SearchText COLLATE Latin1_General_100_BIN2 OR target.NodeId <> d.NodeId);
    INSERT dbo.SearchDocuments (ResultId, NodeId, Kind, SearchText)
    SELECT d.ResultId, d.NodeId, 1, d.SearchText FROM @documents d
    WHERE NOT EXISTS (SELECT 1 FROM dbo.SearchDocuments target WITH (UPDLOCK, HOLDLOCK)
                      WHERE target.Kind = 1 AND target.ResultId = d.ResultId);
END;
