-- spUserVotes_INSERT is at beginning of the file, before spForums_INSERT
CREATE PROC spUserVotes_INSERT
    @ForumID INT,
    @ForumSolutionID INT,
    @CommentID INT,
    @UserID INT,
    @Vote INT
AS
INSERT forums.UserVotes
    (
    ForumID,
    ForumSolutionID,
    CommentID,
    UserID,
    Vote
    )
VALUES 
    (
    @ForumID,
    @ForumSolutionID,
    @CommentID,
    @UserID,
    @Vote
    )
PRINT 'Created a user vote on some content'

INSERT users.UserHistory (
    UserID,
    Action,
    Link,
    ForumID,
    UserVote
) VALUES (
    @UserID,
    'User voted on some content',
    NULL,
    @ForumID,
    @Vote
    )
Print 'Added a user history entry';
-- END spUserVotes_INSERT

GO

-- spForums_INSERT - Update parameter name from @Description to @Content
CREATE PROC spForums_INSERT 
    @ForumID INT,
    @Title VARCHAR(200),
    @Content VARCHAR(max),  
    @ContentStatus INT,
    @AuthorID VARCHAR(50),
    @ScopeID INT,
    @ParentForumID INT,
    @BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Forums ON;
INSERT forums.Forums (
        ForumID,
        Title,
        Content, 
        ContentStatus,
        AuthorID,
        ScopeID,
        ParentForumID,
        BlockedContentID,
        CreatedAt
    )
VALUES (
    @ForumID,
    @Title,
    @Content, 
    @ContentStatus,
    @AuthorID,
    @ScopeID,
    @ParentForumID,
    @BlockedContentID,
    GETDATE()
    );
SET IDENTITY_INSERT forums.Forums OFF;
Print 'Added a problem';


INSERT users.UserHistory (
    UserID,
    Action,
    Link,
    ForumID
) VALUES (
    @AuthorID,
    'Created a forum',
    'link-to-content',
    @ForumID
    )
Print 'Added a user history entry';


-- BY default, there need to be an existing vote for the forum b/c of how the Read Forums works with the votes table.
-- NOTE: An error is thrown when creating the DB b/c spUserVotes_INSERT has not been created yet
EXEC spUserVotes_INSERT
    @ForumID = @ForumID,
    @ForumSolutionID = 0,
    @CommentID = 0,
    @UserID = @AuthorID,
    @Vote = 10;

RETURN @ForumID;
-- END spForums_INSERT

GO


-- spForums_UPDATE 
CREATE PROC spForums_UPDATE 
    @ForumID INT,
    @Title VARCHAR(200),
    @Content VARCHAR(max),  
    @ContentStatus INT,
    @AuthorID VARCHAR(50),
    @ScopeID INT,
    @ParentForumID INT,
    @BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Forums ON;
UPDATE forums.Forums 
SET 
    Title = @Title,
    Content = @Content,  
    ContentStatus = @ContentStatus,
    ModifiedAt = GETDATE(),
    AuthorID = @AuthorID,
    ScopeID = @ScopeID,
    BlockedContentID = @BlockedContentID
    
WHERE ForumID = @ForumID;
SET IDENTITY_INSERT forums.Forums OFF;
Print 'Edited an existing problem';

INSERT users.UserHistory (
    UserID,
    Action,
    Link,
    ForumID
) VALUES (
    @AuthorID,
    'Edited a forum',
    NULL,
    @ForumID
    )
Print 'Added a user history entry';
-- END spForums_UPDATE

GO

-- spComments_INSERT
CREATE PROC spComments_INSERT 
    @CommentID INT,
    @Comment VARCHAR(3000),
    @ForumID INT,
    @SolutionID INT,
    @AuthorID VARCHAR(50),
    @ParentCommentID INT,
    @BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Comments ON;
INSERT forums.Comments
    (CommentID,
    Comment,
    ForumID,
    ForumSolutionID,
    AuthorID,
    ParentCommentID,
    BlockedContentID,
    CreatedAt
    )
VALUES 
    ( @CommentID,
     @Comment,
     @ForumID,
     @SolutionID,
     @AuthorID,
     @ParentCommentID,
     @BlockedContentID,
     GETDATE())
SET IDENTITY_INSERT forums.Comments OFF;
Print 'Created a comment on a problem';

INSERT users.UserHistory (
    UserID,
    Action,
    Link,
    ForumID,
    CommentID
) VALUES (
    @AuthorID,
    'Created a comment on a forum',
    NULL,
    @ForumID,
    @CommentID
    )
Print 'Added a user history entry';
-- END spComments_INSERT

GO

-- spComments_UPDATE
CREATE PROC spComments_UPDATE 
     @CommentID INT,
    @Comment VARCHAR(3000),
    @ForumID INT,
    @SolutionID INT,
    @AuthorID VARCHAR(50),
    @ParentCommentID INT,
    @BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Comments ON;
UPDATE forums.Comments 
SET 
    ForumID = @ForumID,
    ForumSolutionID = @SolutionID,
    Comment = @Comment,
    AuthorID = @AuthorID,
    ModifiedAt = GETDATE(),
    ParentCommentID = @ParentCommentID,
    BlockedContentID = @BlockedContentID
WHERE CommentID = @CommentID;
SET IDENTITY_INSERT forums.Comments OFF;
Print 'Edited an existing comment on a forum';

INSERT users.UserHistory (
    UserID,
    Action,
    Link,
    ForumID,
    CommentID
) VALUES (
    @AuthorID,
    'Edited an existing comment on a forum',
    NULL,
    @ForumID,
    @CommentID
    )
Print 'Added a user history entry';
-- END spComments_UPDATE

GO


-- spSolutions_INSERT
CREATE PROC spSolutions_INSERT 
    @SolutionID INT,
    @ForumID INT,
    @Title VARCHAR(400),
    @Description VARCHAR(max),
    @AuthorID VARCHAR(50),
    @BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Solutions ON;
INSERT forums.Solutions
    (
    SolutionID,
    ForumID,
    Title,
    Description,
    AuthorID,
    BlockedContentID,
    CreatedAt
    )
VALUES 
    (
    @SolutionID,
    @ForumID,
    @Title,
    @Description,
    @AuthorID,
    @BlockedContentID,
    GETDATE()
    )

SET IDENTITY_INSERT forums.Solutions OFF;
Print 'Created a solution on a problem';

INSERT users.UserHistory (
    UserID,
    Action,
    Link,
    ForumID,
    ForumSolutionID
) VALUES (
    @AuthorID,
    'Created a solution on a problem',
    NULL,
    @ForumID,
    @SolutionID
    )
Print 'Added a user history entry';
-- END spSolutions_INSERT

GO

-- spSolutions_UPDATE
CREATE PROC spSolutions_UPDATE 
    @SolutionID INT,
    @ForumID INT,
    @Title VARCHAR(400),
    @Description VARCHAR(max),
    @AuthorID VARCHAR(50),
    @BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Solutions ON;
UPDATE forums.Solutions 
SET 
    Title = @Title,
    Description = @Description,
    AuthorID = @AuthorID,
    ModifiedAt = GETDATE(),
    BlockedContentID = @BlockedContentID
WHERE SolutionID = @SolutionID;
SET IDENTITY_INSERT forums.Solutions OFF;
Print 'Edited an existing solution on a forum';

INSERT users.UserHistory (
    UserID,
    Action,
    Link,
    ForumID,
    ForumSolutionID
) VALUES (
    @AuthorID,
    'Edited an existing solution on a forum',
    NULL,
    @ForumID,
    @SolutionID
    )
Print 'Added a user history entry';
-- END spSolutions_UPDATE

GO


-- spUserVotes_UPDATE
CREATE PROC spUserVotes_UPDATE 
    @vote int,
    @UserID int, 
    @ForumID int, 
    @ForumSolutionID int, 
    @CommentID int
AS 
UPDATE forums.UserVotes 
SET vote = @vote
WHERE 
    UserID = @UserID
    AND ForumID = @ForumID
    AND ForumSolutionID = @ForumSolutionID
    AND CommentID = @CommentID;
Print 'Edited an existing user vote';

INSERT users.UserHistory (
    UserID,
    Action,
    Link,
    ForumID,
    ForumSolutionID
) VALUES (
    @UserID,
    'Edited an existing user vote',
    NULL,
    @ForumID,
    @ForumSolutionID
    )
Print 'Added a user history entry';
-- END spUserVotes_UPDATE

GO

-- spForumsCategories_INSERT
CREATE PROC spForumsCategories_INSERT
    @CategoryName VARCHAR(100),
    @ForumID INT
AS

DECLARE @ForumName varchar(200);
SELECT @ForumName = title FROM forums.Forums WHERE ForumID = @ForumID;

print 'Trying to add a Category to forum(' + CONVERT(varchar, @ForumID) + '):' + @CategoryName + ' to ' + @ForumName;
DECLARE @CategoryID int;
SELECT  @CategoryID = CategoryID 
FROM forums.Categories WHERE CategoryName = @CategoryName;

IF @CategoryID IS NULL -- No category found
    BEGIN
    print 'No Category found -- creating ' + @CategoryName
    -- Create new category
    INSERT forums.Categories (CategoryName)
    VALUES (@CategoryName);
    SET @CategoryID = @@IDENTITY;
    PRINT 'Created a new category'
    END;

INSERT forums.ForumsCategories (
    CategoryID,
    ForumID
) VALUES (
    @CategoryID,
    @ForumID
);
DECLARE @ThisCategory VARCHAR(100);
SELECT @ThisCategory = CategoryName FROM forums.Categories WHERE CategoryID = @CategoryID
Print 'Assigned ForumID: ' + CONVERT(varchar, @ForumID) + ' to CategoryID: ' + CONVERT(varchar, @CategoryID) + ' ('+ @ThisCategory +')'
-- END spForumsCategories_INSERT

GO

-- sp_GetDatabaseStats
CREATE PROCEDURE dbo.sp_GetDatabaseStats
AS
BEGIN

    PRINT 'Running Audit: sp_GetDatabaseStats';
    -- Create a temporary table to store the results
    CREATE TABLE #Results (
        TableName VARCHAR(255) NOT NULL,
        "RowCount" INT NOT NULL,
        ColumnCount INT NOT NULL
    );

    -- Construct the dynamic SQL query
    DECLARE @SQLQuery NVARCHAR(MAX) = N'';

    -- Generate the dynamic SQL to gather statistics from all tables
    SELECT @SQLQuery = STRING_AGG(
        N'INSERT INTO #Results (TableName, "RowCount", ColumnCount) ' +
        N'SELECT ''' + s.name + '.' + t.name + ''', ' +
        N'(SELECT COUNT(*) FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ') AS "RowCount", ' +
        N'(SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = ''' + s.name + ''' AND TABLE_NAME = ''' + t.name + ''') AS ColumnCount ' +
        N'FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';'
    , ' ')
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id;

    -- Execute the generated SQL
    EXEC sp_executesql @SQLQuery;

    -- Create the final results table to store extended data
    DROP TABLE IF EXISTS DatabaseStats;
    CREATE TABLE DatabaseStats (
        TableName VARCHAR(255), 
        "RowCount" INT, 
        ColumnCount INT, 
        DatumCount INT,
        PrimaryKey VARCHAR(MAX),
        ForeignKey VARCHAR(MAX)
    );

    -- Insert data into the final table with calculated columns and foreign/primary keys
    INSERT INTO DatabaseStats (TableName, "RowCount", ColumnCount, DatumCount, PrimaryKey, ForeignKey)
    SELECT DISTINCT 
        TableName, 
        "RowCount", 
        ColumnCount, 
        DatumCount = "RowCount" * ColumnCount,
        PrimaryKey = dbo.GetPrimaryKeyColumns(TableName),
        ForeignKey = dbo.GetForeignKeyColumns(TableName)
    FROM #Results;

    -- Select the final results
    SELECT * FROM DatabaseStats;

    -- Clean up the temporary table
    DROP TABLE #Results;

    -- Optionally, print the dynamic SQL for debugging purposes
    -- PRINT @SQLQuery;
END;
-- END sp_GetDatabaseStats