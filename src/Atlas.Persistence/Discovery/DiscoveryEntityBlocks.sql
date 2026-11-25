-- Discovery missing blocks
SELECT b.Id, CASE b.Kind
        WHEN N'sectioned' THEN COALESCE(b.CompositionSearchText, N'') COLLATE Latin1_General_100_CI_AI_SC
        WHEN N'markdown' THEN COALESCE(b.Markdown, N'') COLLATE Latin1_General_100_CI_AI_SC
        WHEN N'image' THEN CONCAT(N'Image: ', b.AltText, N' (', b.Url COLLATE Latin1_General_100_CI_AI_SC, N')')
        WHEN N'video' THEN CONCAT(N'Video: ', b.Caption, N' (', b.Url COLLATE Latin1_General_100_CI_AI_SC, N')')
        WHEN N'link-preview' THEN CONCAT(N'Link: ', b.Title, N' (', b.Url COLLATE Latin1_General_100_CI_AI_SC, N') — ', b.Description)
        WHEN N'poll-reference' THEN N'Poll'
        WHEN N'chart-reference' THEN CONCAT(N'Chart: ', b.Title) ELSE b.Kind COLLATE Latin1_General_100_CI_AI_SC END AS Description,
       CASE WHEN COALESCE(DATALENGTH(b.Markdown), 0) + COALESCE(DATALENGTH(b.Composition), 0)
            + COALESCE(DATALENGTH(b.Url), 0) + COALESCE(DATALENGTH(b.AltText), 0)
            + COALESCE(DATALENGTH(b.Caption), 0) + COALESCE(DATALENGTH(b.Title), 0)
            + COALESCE(DATALENGTH(b.Description), 0) <= 65536 THEN (
           SELECT b.Id AS id, b.Kind AS kind,
            b.Markdown AS [content.markdown], b.Composition AS [content.composition],
            b.Url AS [content.url], b.AltText AS [content.altText],
            b.Caption AS [content.caption],
            CASE WHEN b.Kind = N'image' THEN b.Description END AS [content.layout],
            b.Title AS [content.title],
            CASE WHEN b.Kind = N'link-preview' THEN b.Description END AS [content.description],
            CASE WHEN b.Kind = N'poll-reference' THEN b.ReferenceId END AS [content.pollId],
            CASE WHEN b.Kind = N'chart-reference' THEN b.ReferenceId END AS [content.chartId]
           FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
       ) END AS PreviewJson
FROM OPENJSON(@ids) WITH (Id uniqueidentifier '$') requested
JOIN BlockRows b ON b.Id = requested.Id
