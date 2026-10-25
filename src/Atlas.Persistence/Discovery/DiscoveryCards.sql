-- Breadcrumbs are now bounded EF hop reads in DiscoveryBreadcrumbReader.
-- No recursive path traversal occurs in this summary query.
-- Ordered block/reaction/community text also uses STRING_AGG WITHIN GROUP; preserving
-- that order avoids changing cross-block descriptions and established card formatting.
-- EF handles ordinary type/author/status/date/comment metadata in ReadCards instead.
-- The child-type JSON remains with these summaries to avoid another database round trip;
-- that part could be LINQ in a separate batch, but is not inherently SQL-only.
-- Public display cards for @ids, supplied as a JSON array of selected node GUIDs.
-- Ranking/paging happens in SqlDiscoveryReadQuery before this batch is called.
-- This query returns display fields, not tracked EF entities or domain aggregates.
-- OUTER APPLY attaches optional per-node summaries without dropping an empty card.
-- Selected constrains output to the page; related ancestors/children supply metadata.
WITH Selected AS (
    SELECT Id FROM OPENJSON(@ids) WITH (Id uniqueidentifier '$')
), PublicNodes AS (
    -- Resolve public labels and the latest active exclusion. Hidden titles/bodies
    -- become notices both on selected cards and in their ancestor breadcrumbs.
    SELECT n.*, COALESCE(t.Name, N'Unknown') AS TypeName,
           CAST(CASE WHEN m.Id IS NULL THEN 0 ELSE 1 END AS bit) AS IsHidden,
           CONCAT(N'[Hidden by moderator: ', CASE m.PublicReason
               WHEN 1 THEN N'Spam'
               WHEN 2 THEN N'Harassment'

               WHEN 3 THEN N'Unsafe content'
               WHEN 4 THEN N'Off topic' ELSE N'Other' END, N']') AS Notice
    FROM NodeRows n
    LEFT JOIN NodeTypeRows t ON t.Id = n.TypeId
    OUTER APPLY (
        SELECT TOP(1) Id, PublicReason
        FROM ModerationCaseRows
        WHERE NodeId = n.Id AND Status = 2 AND VisibilityRestoredAt IS NULL
        ORDER BY DecidedAt DESC, Id
    ) m
)
-- Assemble one card per selected ID. Empty summaries receive Console-friendly defaults.
SELECT n.Id AS NodeId,
       CASE WHEN n.IsHidden = 1 THEN n.Notice ELSE n.Title END AS Title,
       CASE WHEN n.IsHidden = 1 THEN n.Notice WHEN n.DescriptionId IS NULL THEN N'Description document not found.'
            ELSE COALESCE(description.Text, N'No description has been provided.') END AS Description,
       COALESCE(children.Text, N'None') AS SubNodes,
       CASE
           WHEN reactions.Text IS NULL THEN N'—'
           ELSE CONCAT(reactions.Text,
               CASE WHEN reactionCount.Value > 3
                   THEN CONCAT(N' +', reactionCount.Value - 3)
                   ELSE N'' END)
       END AS Reactions,
       COALESCE(communities.Text, N'None') AS Communities,
       n.IsHidden
FROM Selected s
    JOIN PublicNodes n ON n.Id = s.Id
-- Render content blocks in document order, separated by blank lines.
-- Hidden nodes do not expose their description blocks.
OUTER APPLY (
    SELECT STRING_AGG(CONVERT(nvarchar(max), CASE b.Kind
        WHEN N'markdown' THEN COALESCE(b.Markdown, N'') COLLATE Latin1_General_100_CI_AI_SC
        WHEN N'image' THEN CONCAT(N'Image: ', b.AltText, N' (', b.Url COLLATE Latin1_General_100_CI_AI_SC, N')')
        WHEN N'video' THEN CONCAT(N'Video: ', b.Caption, N' (', b.Url COLLATE Latin1_General_100_CI_AI_SC, N')')
        WHEN N'link-preview' THEN CONCAT(N'Link: ', b.Title, N' (', b.Url COLLATE Latin1_General_100_CI_AI_SC, N') — ', b.Description)
        WHEN N'poll-reference' THEN N'Poll'
        WHEN N'chart-reference' THEN CONCAT(N'Chart: ', b.Title) ELSE b.Kind COLLATE Latin1_General_100_CI_AI_SC END), NCHAR(10) + NCHAR(10))
        WITHIN GROUP (ORDER BY link.Position) AS Text
    FROM DocumentBlocks link
    JOIN BlockRows b ON b.Id = link.BlockId WHERE link.DocumentId = n.DescriptionId AND n.IsHidden = 0
) description
-- Include requested child types even when their count is zero.
-- Return JSON so C# can apply the shared count-aware pluralization rules.
OUTER APPLY (
    SELECT (SELECT k.TypeId AS TypeId, COALESCE(t.Name, N'Unknown') AS Name, k.Count, COALESCE(t.AutoPluralize, CAST(1 AS bit)) AS AutoPluralize
    FROM (SELECT types.TypeId, COUNT(child.Id) AS Count FROM (
            SELECT requested.TypeId FROM NodeRequestedTypes requested WHERE requested.NodeId = n.Id
            UNION SELECT c.TypeId FROM NodeParents p
    JOIN NodeRows c ON c.Id = p.NodeId WHERE p.ParentNodeId = n.Id
         ) types

    LEFT JOIN NodeRows child ON child.TypeId = types.TypeId AND EXISTS (SELECT 1 FROM NodeParents p WHERE p.NodeId = child.Id AND p.ParentNodeId = n.Id)
         GROUP BY types.TypeId) k
    LEFT JOIN NodeTypeRows t ON t.Id = k.TypeId
    ORDER BY t.Name FOR JSON PATH) AS Text
) children
-- Display up to three active, unsuppressed reactions with recorded votes.
-- Canonical GUID text makes tied creation times deterministic.
OUTER APPLY (
    SELECT STRING_AGG(CONVERT(nvarchar(max), r.Label), N', ') WITHIN GROUP (ORDER BY r.CreatedAt, r.SortId) AS Text
    FROM (SELECT TOP(3) CONCAT(d.Emoji, N' ', d.Text, N' (', votes.Score, N')') AS Label, nr.CreatedAt,
            CONVERT(char(36), nr.Id) COLLATE Latin1_General_100_BIN2 AS SortId
          FROM NodeReactionRows nr
    JOIN ReactionDefinitionRows d ON d.Id = nr.ReactionDefinitionId
          CROSS APPLY (SELECT SUM(Value) AS Score FROM VoteRows v WHERE v.TargetType = N'NodeReaction' AND v.TargetId = nr.Id) votes
          WHERE nr.NodeId = n.Id AND nr.LifecycleState = N'Active' AND d.IsSuppressed = 0 AND votes.Score IS NOT NULL
          ORDER BY nr.CreatedAt, CONVERT(char(36), nr.Id) COLLATE Latin1_General_100_BIN2) r
) reactions
-- Count the same eligible reactions so the card can append the +N remainder.
OUTER APPLY (
    SELECT COUNT(*) AS Value FROM NodeReactionRows nr
    JOIN ReactionDefinitionRows d ON d.Id = nr.ReactionDefinitionId
    WHERE nr.NodeId = n.Id AND nr.LifecycleState = N'Active' AND d.IsSuppressed = 0
      AND EXISTS (SELECT 1 FROM VoteRows v WHERE v.TargetType = N'NodeReaction' AND v.TargetId = nr.Id)
) reactionCount

-- Collect associated community names in name order.
OUTER APPLY (
    SELECT STRING_AGG(CONVERT(nvarchar(max), c.Name), N', ') WITHIN GROUP (ORDER BY c.Name) AS Text
    FROM CommunityNodeRows link
    JOIN CommunityRows c ON c.Id = link.CommunityId WHERE link.NodeId = n.Id
) communities
