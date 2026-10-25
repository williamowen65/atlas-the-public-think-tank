SELECT hits.[KEY] AS ResultId, hits.[KEY] AS NodeId, CAST(0 AS int) AS Kind, hits.[RANK] AS Score
FROM FREETEXTTABLE(dbo.NodeRows, (Title, ShortDescription), @search, LANGUAGE 1033) hits
UNION ALL
SELECT n.Id AS ResultId, n.Id AS NodeId, CAST(0 AS int) AS Kind, hits.[RANK] AS Score
FROM FREETEXTTABLE(dbo.BlockRows, SearchText, @search, LANGUAGE 1033) hits
JOIN dbo.DocumentBlocks link ON link.BlockId = hits.[KEY]
JOIN dbo.NodeRows n ON n.DescriptionId = link.DocumentId
UNION ALL
SELECT c.Id AS ResultId, c.TargetId AS NodeId, CAST(1 AS int) AS Kind, hits.[RANK] AS Score
FROM FREETEXTTABLE(dbo.CommentRows, Body, @search, LANGUAGE 1033) hits
JOIN dbo.CommentRows c ON c.Id = hits.[KEY]
WHERE c.TargetKind = N'Node' AND c.Status = N'Active' AND c.RemovedAt IS NULL
