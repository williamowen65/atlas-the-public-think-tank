SELECT IssueID AS ContentID, ContentType = 'Issue', CreatedAt FROM Issues
UNION ALL
SELECT SolutionID AS ContentID, ContentType = 'Solution', CreatedAt FROM Solutions

ORDER BY ContentType

