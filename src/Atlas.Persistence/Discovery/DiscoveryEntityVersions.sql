-- Candidate IDs come from the cached page, and SQL verifies their current public visibility.
SELECT r.PageVersion, r.DetailVersion, COALESCE((
    SELECT n.Id AS NodeId, n.DescriptionId AS DocumentId,
           CONVERT(varchar(64), HASHBYTES('SHA2_256', n.Title), 2) AS TitleVersion,
           JSON_QUERY(COALESCE((
               SELECT b.Id, CONVERT(varchar(64), HASHBYTES('SHA2_256', (
                   SELECT b.Kind, b.Markdown, b.Composition, b.CompositionSearchText,
                          b.Url, b.AltText, b.Caption, b.Title, b.Description, b.ReferenceId
                   FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
               )), 2) AS Version
               FROM DocumentBlocks link
               JOIN BlockRows b ON b.Id = link.BlockId
               WHERE link.DocumentId = n.DescriptionId
               ORDER BY link.Position
               FOR JSON PATH
           ), N'[]')) AS Blocks
    FROM OPENJSON(@ids) WITH (Id uniqueidentifier '$') requested
    JOIN NodeRows n ON n.Id = requested.Id
    WHERE NOT EXISTS (
        SELECT 1 FROM ModerationCaseRows m
        WHERE m.NodeId = n.Id AND m.Status = 2 AND m.VisibilityRestoredAt IS NULL
    )
    FOR JSON PATH
), N'[]') AS EntitiesJson
FROM DiscoveryRevisions r
WHERE r.Id = 1
