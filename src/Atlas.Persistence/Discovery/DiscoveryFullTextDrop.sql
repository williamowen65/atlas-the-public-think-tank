IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.CommentRows'))
    DROP FULLTEXT INDEX ON dbo.CommentRows;
IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.BlockRows'))
    DROP FULLTEXT INDEX ON dbo.BlockRows;
IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.NodeRows'))
    DROP FULLTEXT INDEX ON dbo.NodeRows;
IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'AtlasDiscovery')
    DROP FULLTEXT CATALOG AtlasDiscovery;
