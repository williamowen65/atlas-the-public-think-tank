-- The webapp won't have "Cascade on delete" because we don't want deleting a node to delete all sub-issues/solutions.
-- There is no deleting content from the app. There may be an archive for nested trees.
--
-- This script enables cascading deletes for development.
-- You can pass a parentIssueId or parentSolutionId as parameters.
--
-- This implements recursive deletion where bottom-most nodes are deleted first,
-- then the next layer up, until the root node can be deleted.
-- Required due to complex nestings like: issue > issue > issue > solution > issue > issue > solution

-- Declare parameter for the starting point (comment out the one you're not using)
DECLARE @RootIssueID uniqueidentifier = 'FD5A19E1-7394-432C-144E-08DDE0B53C5A'; -- Set the Issue ID you want to delete with all descendants
DECLARE @RootSolutionID uniqueidentifier = NULL; -- Set the Solution ID you want to delete with all descendants

-- ========================================
-- Build hierarchy and dump it into a temp table
-- ========================================
IF OBJECT_ID('tempdb..#ContentHierarchy') IS NOT NULL
    DROP TABLE #ContentHierarchy;

;WITH ContentHierarchy AS (
    -- Base case: root issue
    SELECT 
        i.IssueID,
        NULL AS SolutionID,
        0 AS Level,
        CAST(i.IssueID AS VARCHAR(MAX)) AS Path
    FROM Issues i
    WHERE i.IssueID = @RootIssueID AND @RootIssueID IS NOT NULL

    UNION ALL

    -- Base case: root solution
    SELECT 
        NULL AS IssueID,
        s.SolutionID,
        0 AS Level,
        CAST(s.SolutionID AS VARCHAR(MAX)) AS Path
    FROM Solutions s
    WHERE s.SolutionID = @RootSolutionID AND @RootSolutionID IS NOT NULL

    UNION ALL

    -- Child issues of issues
    SELECT 
        child.IssueID,
        NULL,
        h.Level + 1,
        h.Path + '>' + CAST(child.IssueID AS VARCHAR(MAX))
    FROM ContentHierarchy h
    JOIN Issues child ON child.ParentIssueID = h.IssueID
    WHERE h.IssueID IS NOT NULL

    UNION ALL

    -- Solutions of issues
    SELECT 
        NULL,
        s.SolutionID,
        h.Level + 1,
        h.Path + '>' + CAST(s.SolutionID AS VARCHAR(MAX))
    FROM ContentHierarchy h
    JOIN Solutions s ON s.ParentIssueID = h.IssueID
    WHERE h.IssueID IS NOT NULL

    UNION ALL

    -- Child issues of solutions
    SELECT 
        i.IssueID,
        NULL,
        h.Level + 1,
        h.Path + '>' + CAST(i.IssueID AS VARCHAR(MAX))
    FROM ContentHierarchy h
    JOIN Issues i ON i.ParentSolutionID = h.SolutionID
    WHERE h.SolutionID IS NOT NULL
)

SELECT *
INTO #ContentHierarchy
FROM ContentHierarchy;

-- ========================================
-- Debug (uncomment to check hierarchy before deleting)
-- ========================================
SELECT * FROM #ContentHierarchy ORDER BY Level DESC;

-- ========================================
-- Deletes (safe order: votes, categories, history, comments, then content bottom-up)
-- ========================================

-- Delete votes
-- Delete comment votes
IF OBJECT_ID('dbo.CommentVotes', 'U') IS NOT NULL
BEGIN
    DELETE cv
    FROM CommentVotes cv
    JOIN UserComment c ON cv.CommentID = c.CommentID
    JOIN #ContentHierarchy h ON 
        (h.IssueID = c.IssueID AND c.IssueID IS NOT NULL) OR 
        (h.SolutionID = c.SolutionID AND c.SolutionID IS NOT NULL);
END

-- Delete issue votes
IF OBJECT_ID('Issues.IssueVotes', 'U') IS NOT NULL
BEGIN
    DELETE iv
    FROM Issues.IssueVotes iv
    JOIN #ContentHierarchy h ON h.IssueID = iv.IssueID;
END

-- Delete solution votes
IF OBJECT_ID('Solutions.SolutionVotes', 'U') IS NOT NULL
BEGIN
    DELETE sv
    FROM Solutions.SolutionVotes sv
    JOIN #ContentHierarchy h ON h.SolutionID = sv.SolutionID;
END

-- Delete issue categories
IF OBJECT_ID('issues.IssuesCategories', 'U') IS NOT NULL
BEGIN
    DELETE ic
    FROM issues.IssuesCategories ic
    JOIN #ContentHierarchy h ON h.IssueID = ic.IssueID;
END

-- Delete solution categories
IF OBJECT_ID('solutions.SolutionsCategories', 'U') IS NOT NULL
BEGIN
    DELETE sc
    FROM solutions.SolutionsCategories sc
    JOIN #ContentHierarchy h ON h.SolutionID = sc.SolutionID;
END

-- Delete user history
IF OBJECT_ID('users.UserHistory', 'U') IS NOT NULL
BEGIN
    DELETE uh
    FROM users.UserHistory uh
    JOIN #ContentHierarchy h ON 
        (h.IssueID = uh.IssueID AND uh.IssueID IS NOT NULL) OR 
        (h.SolutionID = uh.SolutionID AND h.SolutionID IS NOT NULL);
END

-- Delete comments
IF OBJECT_ID('dbo.Comments', 'U') IS NOT NULL
BEGIN
    DELETE c
    FROM Comments c
    WHERE EXISTS (
        SELECT 1
        FROM #ContentHierarchy h
        WHERE (h.IssueID = c.IssueID AND c.IssueID IS NOT NULL) 
           OR (h.SolutionID = c.SolutionID AND c.SolutionID IS NOT NULL)
    );
END



-- Loop through the entries
-- check if there is an issueID or SolutionID.
-- which ever one there is, delete it.
-- continue until all are processed.
DECLARE @MaxLevel INT, @CurrentLevel INT;

-- Find deepest level
SELECT @MaxLevel = MAX(Level) FROM #ContentHierarchy;
SET @CurrentLevel = @MaxLevel;

-- Loop from deepest level back to root
WHILE @CurrentLevel >= 0
BEGIN
    -- Delete solutions at this level
    DELETE s
    FROM Solutions s
    JOIN #ContentHierarchy h ON h.SolutionID = s.SolutionID
    WHERE h.Level = @CurrentLevel
      AND h.SolutionID IS NOT NULL;

    -- Delete issues at this level
    DELETE i
    FROM Issues i
    JOIN #ContentHierarchy h ON h.IssueID = i.IssueID
    WHERE h.Level = @CurrentLevel
      AND h.IssueID IS NOT NULL;

    -- Move up one level
    SET @CurrentLevel = @CurrentLevel - 1;
END




-- ========================================
-- Cleanup
-- ========================================
DROP TABLE #ContentHierarchy;
