CREATE OR ALTER TRIGGER dbo.AtlasSearchTrackComments ON dbo.CommentRows
AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ids dbo.AtlasSearchIds;
    INSERT @ids (Id) SELECT Id FROM inserted UNION SELECT Id FROM deleted;
    EXEC dbo.AtlasRefreshCommentSearch @ids;
END;
