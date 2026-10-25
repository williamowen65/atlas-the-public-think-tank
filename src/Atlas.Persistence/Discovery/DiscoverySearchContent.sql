-- Search text is a SqlParameter; filter tokens have already been removed by the web parser.
-- UNION avoids duplicate nodes when title, introduction and several blocks all match.
SELECT n.Id AS NodeId
FROM dbo.NodeRows n
WHERE FREETEXT((n.Title, n.ShortDescription), @search, LANGUAGE 1033)
UNION
SELECT n.Id AS NodeId
FROM dbo.NodeRows n
JOIN dbo.DocumentBlocks link ON link.DocumentId = n.DescriptionId
JOIN dbo.BlockRows b ON b.Id = link.BlockId
WHERE FREETEXT(b.SearchText, @search, LANGUAGE 1033)
