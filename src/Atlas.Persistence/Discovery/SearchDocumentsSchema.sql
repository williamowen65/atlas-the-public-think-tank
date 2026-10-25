-- Rebuildable search projection, deliberately independent of EF migration history.
IF OBJECT_ID(N'dbo.SearchDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SearchDocuments (
        SearchKey bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_SearchDocuments PRIMARY KEY,
        ResultId uniqueidentifier NOT NULL,
        NodeId uniqueidentifier NOT NULL,
        Kind int NOT NULL,
        SearchText nvarchar(max) NOT NULL,
        CONSTRAINT CK_SearchDocuments_Kind CHECK (Kind IN (0, 1))
    );
    CREATE UNIQUE INDEX IX_SearchDocuments_Result ON dbo.SearchDocuments(Kind, ResultId);
    CREATE INDEX IX_SearchDocuments_Node ON dbo.SearchDocuments(NodeId);
END;
IF TYPE_ID(N'dbo.AtlasSearchIds') IS NULL
    EXEC(N'CREATE TYPE dbo.AtlasSearchIds AS TABLE (Id uniqueidentifier NOT NULL PRIMARY KEY)');
