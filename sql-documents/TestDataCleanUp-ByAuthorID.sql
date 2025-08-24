-- Script to delete all content created by a specific user
-- First identifies all root issues by the user, then recursively deletes each tree

-- User parameter

DECLARE @UserID uniqueidentifier = '49294663-7E59-4AA8-7649-08DDE3405379'; -- Set the AppUser ID whose content you want to delete


-- Temporary storage for root issues
IF OBJECT_ID('tempdb..#UserRootIssues') IS NOT NULL
    DROP TABLE #UserRootIssues;

-- Find all top-level issues created by the user (no parent content)
SELECT IssueID
INTO #UserRootIssues
FROM Issues
WHERE AuthorID = @UserID
  AND ParentIssueID IS NULL
  AND ParentSolutionID IS NULL;

-- Debug: Show root issues found
SELECT * FROM #UserRootIssues;

-- Process each root issue
DECLARE @CurrentIssueID uniqueidentifier;

-- Cursor to iterate through each root issue
DECLARE RootIssue_Cursor CURSOR FOR
SELECT IssueID FROM #UserRootIssues;

OPEN RootIssue_Cursor;
FETCH NEXT FROM RootIssue_Cursor INTO @CurrentIssueID;


WHILE @@FETCH_STATUS = 0
BEGIN
    --PRINT 'Processing root issue: ' + CONVERT(VARCHAR(36), @CurrentIssueID);
    
    -- For each root issue, run the existing cleanup logic
    -- ========================================
    -- Build hierarchy and dump it into a temp table
    -- ========================================

       IF OBJECT_ID('tempdb..#ContentHierarchy') IS NOT NULL
        DROP TABLE #ContentHierarchy;

    ;WITH ContentHierarchy (IssueID, SolutionID, Level, Path) AS (
        -- Base case: root issue
        SELECT
            i.IssueID,
            CAST(NULL AS uniqueidentifier) AS SolutionID,
            0 AS Level,
            CAST(CONVERT(varchar(36), i.IssueID) AS varchar(max)) AS Path
        FROM Issues i
        WHERE i.IssueID = @CurrentIssueID

        UNION ALL

        -- Child issues of issues
        SELECT
            child.IssueID,
            CAST(NULL AS uniqueidentifier),
            h.Level + 1,
            h.Path + '>' + CONVERT(varchar(36), child.IssueID)
        FROM ContentHierarchy h
        JOIN Issues child ON child.ParentIssueID = h.IssueID
        WHERE h.IssueID IS NOT NULL

        UNION ALL

        -- Solutions of issues
        SELECT
            CAST(NULL AS uniqueidentifier),
            s.SolutionID,
            h.Level + 1,
            h.Path + '>' + CONVERT(varchar(36), s.SolutionID)
        FROM ContentHierarchy h
        JOIN Solutions s ON s.ParentIssueID = h.IssueID
        WHERE h.IssueID IS NOT NULL

        UNION ALL

        -- Child issues of solutions
        SELECT
            i.IssueID,
            CAST(NULL AS uniqueidentifier),
            h.Level + 1,
            h.Path + '>' + CONVERT(varchar(36), i.IssueID)
        FROM ContentHierarchy h
        JOIN Issues i ON i.ParentSolutionID = h.SolutionID
        WHERE h.SolutionID IS NOT NULL
    )
    SELECT *
    INTO #ContentHierarchy
    FROM ContentHierarchy;

    -- Debug (uncomment to check hierarchy before deleting)
    SELECT * FROM #ContentHierarchy ORDER BY Level DESC;
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

    IF OBJECT_ID('issues.IssueVotes', 'U') IS NOT NULL
    BEGIN
        DELETE iv
        FROM issues.IssueVotes iv
        JOIN #ContentHierarchy h ON h.IssueID = iv.IssueID;
    END

    -- Delete solution votes
    IF OBJECT_ID('solutions.SolutionVotes', 'U') IS NOT NULL
    BEGIN
        DELETE sv
        FROM solutions.SolutionVotes sv
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
    -- Loop through the entries - delete from bottom up
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

    -- Clean up temp table for this iteration
    DROP TABLE #ContentHierarchy;
    
    -- Get next root issue
    FETCH NEXT FROM RootIssue_Cursor INTO @CurrentIssueID;

END

-- Clean up cursor
CLOSE RootIssue_Cursor;
DEALLOCATE RootIssue_Cursor;

-- Clean up temp tables
DROP TABLE #UserRootIssues;

PRINT 'User content deletion complete';