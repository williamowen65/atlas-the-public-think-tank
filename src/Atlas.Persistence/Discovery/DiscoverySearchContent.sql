-- EF composes this SELECT into its search predicate; it does not materialize all content.
-- Keep this part in SQL because the current provider does not express the required
-- ordered STRING_AGG WITHIN GROUP across block fields through the LINQ query here.
-- Order matters: a phrase may span adjacent fields or adjacent document blocks.
-- Replacing this with Any(block.Contains(...)) would change those search results.
SELECT n.Id AS NodeId, COALESCE(content.Text, N'') AS Text
FROM NodeRows n
OUTER APPLY (
    SELECT STRING_AGG(CONVERT(nvarchar(max), CASE b.Kind
        WHEN N'markdown' THEN COALESCE(b.Markdown, N'')
        WHEN N'image' THEN CONCAT(b.AltText, N' ', b.Caption)
        WHEN N'video' THEN CONCAT(b.Caption, N' ', b.Url COLLATE Latin1_General_100_CI_AI_SC)
        WHEN N'link-preview' THEN CONCAT(b.Title, N' ', b.Description, N' ', b.Url COLLATE Latin1_General_100_CI_AI_SC)
        WHEN N'chart-reference' THEN COALESCE(b.Title, N'')
        ELSE N'' END), N' ')
        WITHIN GROUP (ORDER BY link.Position) AS Text
    FROM DocumentBlocks link
    JOIN BlockRows b ON b.Id = link.BlockId
    WHERE link.DocumentId = n.DescriptionId
) content
