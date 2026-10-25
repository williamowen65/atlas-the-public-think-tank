CREATE OR ALTER TRIGGER dbo.AtlasSearchTrackLinks ON dbo.DocumentBlocks
AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ids dbo.AtlasSearchIds;
    INSERT @ids (Id) SELECT n.Id FROM dbo.NodeRows n
    WHERE n.DescriptionId IN (SELECT DocumentId FROM inserted UNION SELECT DocumentId FROM deleted);
    EXEC dbo.AtlasRefreshNodeSearch @ids;
END;
