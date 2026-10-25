-- No GO: EF executes this batch with suppressTransaction: true.
IF COALESCE(FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'), 0) <> 1
    THROW 51000, 'Atlas search requires SQL Server Full-Text Search. Install the feature before applying this migration.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_languages WHERE lcid = 1033)
    THROW 51000, 'Atlas search requires English full-text language resources (LCID 1033).', 1;
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'AtlasDiscovery')
    EXEC(N'CREATE FULLTEXT CATALOG AtlasDiscovery WITH ACCENT_SENSITIVITY = OFF');
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.NodeRows'))
    EXEC(N'CREATE FULLTEXT INDEX ON dbo.NodeRows
        (Title LANGUAGE 1033, ShortDescription LANGUAGE 1033)
        KEY INDEX PK_NodeRows ON AtlasDiscovery
        WITH CHANGE_TRACKING AUTO, STOPLIST = SYSTEM');
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.BlockRows'))
    EXEC(N'CREATE FULLTEXT INDEX ON dbo.BlockRows (SearchText LANGUAGE 1033)
        KEY INDEX PK_BlockRows ON AtlasDiscovery
        WITH CHANGE_TRACKING AUTO, STOPLIST = SYSTEM');
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.CommentRows'))
    EXEC(N'CREATE FULLTEXT INDEX ON dbo.CommentRows (Body LANGUAGE 1033)
        KEY INDEX PK_CommentRows ON AtlasDiscovery
        WITH CHANGE_TRACKING AUTO, STOPLIST = SYSTEM');
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.SearchDocuments'))
    EXEC(N'CREATE FULLTEXT INDEX ON dbo.SearchDocuments (SearchText LANGUAGE 1033)
        KEY INDEX PK_SearchDocuments ON AtlasDiscovery
        WITH CHANGE_TRACKING AUTO, STOPLIST = SYSTEM');
