CREATE OR ALTER TRIGGER dbo.AtlasSearchTrackNodes ON dbo.NodeRows
AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ids dbo.AtlasSearchIds;
    INSERT @ids (Id) SELECT Id FROM inserted UNION SELECT Id FROM deleted;
    EXEC dbo.AtlasRefreshNodeSearch @ids;
END;
