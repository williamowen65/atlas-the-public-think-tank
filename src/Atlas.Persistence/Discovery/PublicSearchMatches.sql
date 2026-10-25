-- Relevance remains SQL Server full-text relevance. Passage recognition is a separate signal.
-- TRANSLATE handles common punctuation; repeated REPLACE collapses whitespace runs up to 32768.
-- CHARINDEX treats the normalized, word-bounded phrase literally (no query syntax or wildcards).
SELECT d.ResultId, d.NodeId, d.Kind, hits.[RANK] AS Score,
       CASE WHEN @phrase IS NOT NULL AND CHARINDEX(@phrase, p15.Text COLLATE Latin1_General_100_CI_AI_SC) > 0
            THEN 1 ELSE 0 END AS PhraseMatch
FROM FREETEXTTABLE(dbo.SearchDocuments, SearchText, @search, LANGUAGE 1033) hits
JOIN dbo.SearchDocuments d ON d.SearchKey = hits.[KEY]
CROSS APPLY (SELECT N' ' + TRANSLATE(d.SearchText, @punctuation, @spaces) + N' ' AS Text) p0
CROSS APPLY (SELECT REPLACE(p0.Text, N'  ', N' ') AS Text) p1
CROSS APPLY (SELECT REPLACE(p1.Text, N'  ', N' ') AS Text) p2
CROSS APPLY (SELECT REPLACE(p2.Text, N'  ', N' ') AS Text) p3
CROSS APPLY (SELECT REPLACE(p3.Text, N'  ', N' ') AS Text) p4
CROSS APPLY (SELECT REPLACE(p4.Text, N'  ', N' ') AS Text) p5
CROSS APPLY (SELECT REPLACE(p5.Text, N'  ', N' ') AS Text) p6
CROSS APPLY (SELECT REPLACE(p6.Text, N'  ', N' ') AS Text) p7
CROSS APPLY (SELECT REPLACE(p7.Text, N'  ', N' ') AS Text) p8
CROSS APPLY (SELECT REPLACE(p8.Text, N'  ', N' ') AS Text) p9
CROSS APPLY (SELECT REPLACE(p9.Text, N'  ', N' ') AS Text) p10
CROSS APPLY (SELECT REPLACE(p10.Text, N'  ', N' ') AS Text) p11
CROSS APPLY (SELECT REPLACE(p11.Text, N'  ', N' ') AS Text) p12
CROSS APPLY (SELECT REPLACE(p12.Text, N'  ', N' ') AS Text) p13
CROSS APPLY (SELECT REPLACE(p13.Text, N'  ', N' ') AS Text) p14
CROSS APPLY (SELECT REPLACE(p14.Text, N'  ', N' ') AS Text) p15
