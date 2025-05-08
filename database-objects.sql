-- This file will create the Stored Procedures and Functions related to the DB


-- Database Functions for The Public Think Tank

-- Function to get category name by ID
CREATE FUNCTION fnGetCategoryNameByID (
    @CategoryID INT
) 
RETURNS VARCHAR(255)
AS
BEGIN
    DECLARE @CategoryName VARCHAR(255);
    
    SELECT @CategoryName = CategoryName 
    FROM forums.Categories
    WHERE CategoryID = @CategoryID;

    RETURN @CategoryName;
END;
GO

-- Function to get forums with filtering options
CREATE FUNCTION fnGetForums(
    @ForumID int,
    @UserID int,
    @CategoryID int,
    @ParentForumID int
)
RETURNS table
RETURN
    -- CTE to prevent duplication when combining with votes
    WITH ForumCategories AS (
        SELECT 
            f.ForumID,
            STRING_AGG(c.CategoryName, ', ') AS Categories
        FROM forums.Forums f
        LEFT JOIN forums.ForumsCategories fc ON f.ForumID = fc.ForumID
        LEFT JOIN forums.Categories c ON fc.CategoryID = c.CategoryID
        GROUP BY f.ForumID
    )
    SELECT 
        f.*,
        fc.Categories,
        COUNT(uv.Vote) AS TotalVotes,
        AVG(CAST(uv.Vote AS FLOAT)) AS AverageVote
    FROM forums.Forums f
    LEFT JOIN ForumCategories fc ON f.ForumID = fc.ForumID
    LEFT JOIN forums.UserVotes uv ON f.ForumID = uv.ForumID
    WHERE 
        (@ForumID IS NULL OR f.ForumID = @ForumID) 
        AND (@ParentForumID IS NULL OR f.ParentForumID = @ParentForumID)
    GROUP BY
        f.ForumID,
        f.Title,
        f.Description,
        f.CreatedAt,
        f.ModifiedAt,
        f.AuthorID,
        f.ScopeID,
        f.ParentForumID,
        f.BlockedContentID,
        fc.Categories
    HAVING (@CategoryID IS NULL OR fc.Categories LIKE '%' + dbo.fnGetCategoryNameByID(@CategoryID) +'%');
GO

-- Function to get comments with optional filtering
CREATE FUNCTION fnGetComments()
RETURNS table
RETURN
    SELECT 
        c.*,
        COUNT(uv.Vote) AS TotalVotes,
        AVG(CAST(uv.Vote AS FLOAT)) AS AverageVote
    FROM forums.Comments c
    LEFT JOIN forums.UserVotes uv ON c.CommentID = uv.CommentID
    WHERE (c.CommentID != 0)
    GROUP BY
        c.CommentID,
        c.ForumID,
        c.ForumSolutionID,
        c.Comment,
        c.AuthorID,
        c.CreatedAt,
        c.ModifiedAt,
        c.ParentCommentID,
        c.BlockedContentID;
GO

-- Function to get solutions with filtering options
CREATE FUNCTION fnGetSolutions(
    @ForumID INT,
    @SolutionID INT,
    @AuthorID INT
)
RETURNS table
RETURN 
    SELECT 
        s.*,
        COUNT(uv.Vote) AS TotalVotes,
        AVG(CAST(uv.Vote AS FLOAT)) AS AverageVote
    FROM forums.Solutions s
    LEFT JOIN forums.UserVotes uv ON s.SolutionID = uv.ForumSolutionID
    WHERE 
        (@ForumID IS NULL OR s.ForumID = @ForumID)
        AND (@SolutionID IS NULL OR s.SolutionID = @SolutionID)
        AND (@AuthorID IS NULL OR s.AuthorID = @AuthorID)
    GROUP BY
        s.SolutionID,
        s.ForumID,
        s.Title,
        s.Content,
        s.AuthorID,
        s.CreatedAt,
        s.ModifiedAt,
        s.BlockedContentID;
GO

-- Function to get a user's votes
CREATE FUNCTION fnGetUsersVotes(
    @UserID int
)
RETURNS table
RETURN
    SELECT * FROM forums.UserVotes
    WHERE UserID = @UserID;
GO

-- Function to get user history with optional filtering
CREATE FUNCTION fnGetUserHistory(
    @UserID int
)
RETURNS table
RETURN
    SELECT *
    FROM users.UserHistory
    WHERE (@UserID IS NULL OR UserID = @UserID);
GO

-- Helper function for database statistics to get primary key columns
CREATE FUNCTION dbo.GetPrimaryKeyColumns(
    @TableIdentifier VARCHAR(255) -- Format: SchemaName.TableName
)
RETURNS VARCHAR(MAX)
AS
BEGIN
    DECLARE @PrimaryKeys VARCHAR(MAX) = '';
    DECLARE @SchemaName VARCHAR(128);
    DECLARE @TableName VARCHAR(128);
    
    -- Split the table identifier into schema and table name
    SELECT 
        @SchemaName = PARSENAME(@TableIdentifier, 2),
        @TableName = PARSENAME(@TableIdentifier, 1);
    
    -- Get primary key columns
    SELECT @PrimaryKeys = STRING_AGG(c.name, ', ')
    FROM sys.indexes i
    JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    JOIN sys.tables t ON i.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE i.is_primary_key = 1
        AND s.name = @SchemaName
        AND t.name = @TableName;
    
    RETURN @PrimaryKeys;
END;
GO




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

-- spForums_INSERT - 
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
    NULL,
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
    @Content VARCHAR(max),
    @ContentStatus INT,
    @AuthorID VARCHAR(50),
    @BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Solutions ON;
INSERT forums.Solutions
    (
    SolutionID,
    ForumID,
    Title,
    Content,
    ContentStatus,
    AuthorID,
    BlockedContentID,
    CreatedAt
    )
VALUES 
    (
    @SolutionID,
    @ForumID,
    @Title,
    @Content,
    @ContentStatus,
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
    @UserID VARCHAR(50), 
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
