-- Every hit is ranked against the same corpus and the same indexed column.
SELECT d.ResultId, d.NodeId, d.Kind, hits.[RANK] AS Score
FROM FREETEXTTABLE(dbo.SearchDocuments, SearchText, @search, LANGUAGE 1033) hits
JOIN dbo.SearchDocuments d ON d.SearchKey = hits.[KEY]
