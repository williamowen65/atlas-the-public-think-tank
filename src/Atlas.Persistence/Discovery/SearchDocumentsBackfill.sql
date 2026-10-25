-- Install tracking before taking the initial snapshot so subsequent writes stay tracked.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @nodes dbo.AtlasSearchIds, @comments dbo.AtlasSearchIds;
INSERT @nodes (Id) SELECT Id FROM dbo.NodeRows UNION SELECT ResultId FROM dbo.SearchDocuments WHERE Kind = 0;
INSERT @comments (Id) SELECT Id FROM dbo.CommentRows UNION SELECT ResultId FROM dbo.SearchDocuments WHERE Kind = 1;
EXEC dbo.AtlasRefreshNodeSearch @nodes;
EXEC dbo.AtlasRefreshCommentSearch @comments;
COMMIT;
