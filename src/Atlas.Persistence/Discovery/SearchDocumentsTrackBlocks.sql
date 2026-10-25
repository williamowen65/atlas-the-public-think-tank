CREATE OR ALTER TRIGGER dbo.AtlasSearchTrackBlocks ON dbo.BlockRows
AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ids dbo.AtlasSearchIds;
    INSERT @ids (Id) SELECT DISTINCT n.Id FROM dbo.NodeRows n
    JOIN dbo.DocumentBlocks link ON link.DocumentId = n.DescriptionId
    WHERE link.BlockId IN (SELECT Id FROM inserted UNION SELECT Id FROM deleted);
    EXEC dbo.AtlasRefreshNodeSearch @ids;
END;
