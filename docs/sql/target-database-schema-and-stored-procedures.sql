/*
 Script
 Create the Public Think Tank database
 Insert test data
 Create database objects
 Perform test queries
 
 Author: William Owen
	Created with Git Version Control


General TODO's 
	Change all sp_ procedures to modified prefix to speed up time takes to find procedure
		stp, stpr... proc_...sproc_.... etc anything  
 */
/**********************
 
 Initialize Database
 
 **********************/
USE MASTER;
DROP DATABASE IF EXISTS ThePublicThinkTank;
CREATE DATABASE ThePublicThinkTank;
GO
USE ThePublicThinkTank;
GO
-- Initialize Schemas
CREATE SCHEMA users;
GO
CREATE SCHEMA forums;
GO
/**********************
 
 Initialize Tables
 
 **********************/
CREATE TABLE users.Users (
	UserID INT IDENTITY PRIMARY KEY,
	FirstName VARCHAR(40) NOT NULL,
	LastName VARCHAR(40) NOT NULL,
	UserName VARCHAR(20) NOT NULL,
	Password VARCHAR(64) NOT NULL,
	-- Use HASHBYTES to enter password
	Email VARCHAR(200) NOT NULL,
	IsEmailConfirmed BIT NOT NULL DEFAULT 'False'
);

GO
-- INSERTING a NULL ID entry.
SET IDENTITY_INSERT users.Users ON;
INSERT users.Users (
	UserID, 
	FirstName,
	LastName,
	UserName,
	Password,
	Email,
	IsEmailConfirmed)
VALUES (0, 'System', 'System', 'System', 'System', 'System', 'TRUE');
SET IDENTITY_INSERT users.Users OFF;
PRINT 'Created the "System" user'
GO

-- These are top level "subreddits"
-- for more info, search for "-- NOTE: Categories"
CREATE TABLE forums.Categories (
	CategoryID INT IDENTITY PRIMARY KEY,
	CategoryName VARCHAR(100)
		CHECK (LEN(LTRIM(RTRIM(CategoryName))) > 0)
);
-- Fixed number of scopes
-- Every Forum/Problem is assigned a scope
CREATE TABLE forums.Scopes (
	ScopeID INT IDENTITY PRIMARY KEY,
	ScopeName VARCHAR(100)
		CHECK (LEN(LTRIM(RTRIM(ScopeName))) > 0)
);
-- INSERTING a NULL ID entry.
SET IDENTITY_INSERT forums.Scopes ON;
INSERT forums.Scopes (ScopeID)
VALUES (0);
SET IDENTITY_INSERT forums.Scopes OFF;
PRINT 'Created the "System" scope'

-- Any problem, solution, comment can be blocked if it is "illegal"
CREATE TABLE forums.BlockedContent (
	BlockedContentID INT IDENTITY PRIMARY KEY,
	ReasonID SMALLINT -- REFERENCES
);

-- INSERTING a NULL ID entry.
SET IDENTITY_INSERT forums.BlockedContent ON;
INSERT forums.BlockedContent (BlockedContentID)
VALUES (0);
SET IDENTITY_INSERT forums.BlockedContent OFF;
PRINT 'Created the "System" BlockedContent'


-- These are "problems" which may relate to each other
-- One problem may be a sub-problem of a another problem.
-- Top problems are considered "the subreddit"
--		constitute a "main forum"
--		Nested problems are like a "nested sub-reddit"
--			Give the content a type of organization 
--			as some solutions might reveal other sub problems.
-- Forums, like Comments table can be self-referencing
--      This allows for sub problems... 
CREATE TABLE forums.Forums (
	Title VARCHAR(200)
		CHECK (LEN(LTRIM(RTRIM(Title))) > 0),
	ForumID INT IDENTITY PRIMARY KEY,
	-- possibly needs to be set higher... 
	Description VARCHAR(max)
		CHECK (LEN(LTRIM(RTRIM(Description))) > 0),
	-- NumberOfVotes -- Calculated ?
	-- Average -- Calculated ?
	CreatedAt datetime2 DEFAULT GETDATE(),
	ModifiedAt datetime2,
	AuthorID INT NOT NULL REFERENCES users.Users (UserID),
	ScopeID INT NOT NULL REFERENCES forums.Scopes (ScopeID),
	ParentForumID INT REFERENCES forums.Forums (ForumID),
	BlockedContentID INT REFERENCES forums.BlockedContent (BlockedContentID) -- Status -- TBD (does problem have valid solutions?/are the solutions being fundraised?)
);
-- INSERTING a NULL ID entry.
SET IDENTITY_INSERT forums.Forums ON;
INSERT forums.Forums (ForumID, AuthorID, ScopeID)
VALUES (0, 0, 0);
SET IDENTITY_INSERT forums.Forums OFF;
PRINT 'Created the "System" forum'

-- Each problem can have many solutions
CREATE TABLE forums.Solutions (
	SolutionID INT IDENTITY PRIMARY KEY,
	ForumID INT REFERENCES forums.Forums (ForumID),
	Title VARCHAR(300) NOT NULL
		CHECK (LEN(LTRIM(RTRIM(Title))) > 0),
	Description VARCHAR(max) NOT NULL
		CHECK (LEN(LTRIM(RTRIM(Description))) > 0),
	-- SolutionRating -- calculated
	-- NumberOfVotes -- calculated
	-- Average -- calculated
	AuthorID INT REFERENCES users.Users (UserID),
	CreatedAt datetime2,
	ModifiedAt datetime2,
	BlockedContentID INT REFERENCES forums.BlockedContent (BlockedContentID)
);
-- INSERTING a NULL ID entry.
SET IDENTITY_INSERT forums.Solutions ON;
INSERT forums.Solutions (
	SolutionID,
	ForumID,
	AuthorID,
	BlockedContentID, 
	Title,
	Description)
VALUES (0, 0, 0, 0, 'System', 'System');
SET IDENTITY_INSERT forums.Solutions OFF;
PRINT 'Created the "System" solution'


-- A self referencing table to support nested comments
--		Composite primary/foriegn keys
CREATE TABLE forums.Comments (
	CommentID INT IDENTITY PRIMARY KEY,
	ForumID INT REFERENCES forums.Forums (ForumID),
	ForumSolutionID INT REFERENCES forums.Solutions (SolutionID),
	Comment VARCHAR(3000) NOT NULL
		CHECK (LEN(LTRIM(RTRIM(Comment))) > 0),  -- NOTE I used Chat to find out how to make this CHECK work.
	AuthorID INT REFERENCES users.Users (UserID),
	CreatedAt datetime2,
	ModifiedAt datetime2,
	ParentCommentID INT REFERENCES forums.Comments (CommentID),
	BlockedContentID INT REFERENCES forums.BlockedContent (BlockedContentID)
	
);
-- INSERTING a NULL ID entry.
SET IDENTITY_INSERT forums.Comments ON;
INSERT forums.Comments (
	CommentID,
	Comment,
	ForumID,
	AuthorID,
	BlockedContentID)
VALUES (0, 'System', 0, 0, 0);
SET IDENTITY_INSERT forums.Comments OFF;
PRINT 'Created the "System" comment'


CREATE TABLE forums.UserVotes (
	ForumID INT REFERENCES forums.Forums (ForumID),
	ForumSolutionID INT REFERENCES forums.Solutions (SolutionID),
	CommentID INT REFERENCES forums.Comments (CommentID),
	UserID INT,
	Vote INT
		CHECK (Vote BETWEEN 1 AND 10),
	PRIMARY KEY(UserID, ForumID, ForumSolutionID, CommentID)
);

-- INSERTING a NULL ID entry.
-- SET IDENTITY_INSERT forums.UserVotes ON;
-- INSERT forums.UserVotes (
-- 	ForumID,
-- 	ForumSolutionID,
-- 	CommentID,
-- 	UserID,
-- 	Vote)
-- VALUES (0, 0, 0, 0, 0);
-- SET IDENTITY_INSERT forums.UserVotes OFF;
-- PRINT 'Created the "System" UserVotes'

-- Many to Many table for category "tags"
CREATE TABLE forums.ForumsCategories (
	CategoryID INT REFERENCES forums.Categories (CategoryID),
	ForumID INT REFERENCES forums.Forums (ForumID),
	PRIMARY KEY (CategoryID, ForumID)
);

/*
	users.UserHistory is created last b/c it will have reference to most actions available
 */
CREATE TABLE users.UserHistory (
	UserHistoryID INT IDENTITY PRIMARY KEY,
	UserID INT REFERENCES users.Users (UserID),
	Action VARCHAR(200) NOT NULL,
	-- REFERENCES users.Actions
	Link VARCHAR(200) NULL,
	ForumID INT NULL DEFAULT NULL REFERENCES forums.Forums (ForumID),
	ForumSolutionID INT NULL DEFAULT NULL REFERENCES forums.Solutions (SolutionID),
	CommentID INT NULL DEFAULT NULL REFERENCES forums.Comments (CommentID),
	UserVote INT,
	Timestamp datetime2 DEFAULT GETDATE()
);
GO
/*
 
 Create other database objects
 
 -- When a user creates forums, solutions, or votes, records that action in user history
 --		This is implemented for forums, and comments
 --		It still needed to happen for solutions and votes
 -- Category generation. If a problem is posted without a category, logic will run to either give it a category, or set it as a category
 Stored procedures for all data inserts.

A few STORED PROCEDUREs for CRUD
	Actions for Insert and Update: forums, comments, solutions, and voting
		Record actions in users.UserHistory
		Automate the creation of categories on forums.
	Read actions accept arguments

Create and Update
	spForums_INSERT
	spForums_UPDATE
	spSolutions_INSERT
	spSolutions_UPDATE
	spComments_INSERT
	spComments_UPDATE
	spUserVotes_INSERT
	TODO spUserVotes_UPDATE
Read
	spGetForums
		- by category (Categories are like tags on forums)
			- User should be able to combine these tags in a search
		- by scope
		- by Votes (Ascending/Descending)
			- maybe also by range?
		- By user
		- By text search
			- This would use a split-text technique.
	spGetComments
		- by Forum
		- by Solution
		- Some sort abilities (vote, date, etc)
	spGetSolutions
		- by Forum
		- by scope
		- Some sort abilities (vote, date, etc)
	spGetUserHistory
		- by user
	spGetCategories
		- get list of all categories
		- TODO: future task -> users can pin categories (pinned by index).
			- Get the users pinned categories

*/
GO
print '';
print 'Creating spForums_INSERT';
print 'Ignore error about spUserVotes_INSERT';
print '';
GO

/*
Create a new forum
*/
CREATE PROC spForums_INSERT 
	@ForumID INT,
	@Title VARCHAR(200),
	@Description VARCHAR(max),
	@AuthorID INT,
	@ScopeID INT,
	@ParentForumID INT,
	@BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Forums ON;
INSERT forums.Forums (
		ForumID,
		Title,
		Description,
		AuthorID,
		ScopeID,
		ParentForumID,
		BlockedContentID,
		CreatedAt
	)
VALUES (
	@ForumID,
	@Title,
	@Description,
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
print '';
print 'Creating fnGetCategoryNameByID';
print '';
GO

/*
	- by category (Categories are like tags on forums)
		- User should be able to combine these tags in a search
	- by scope
	- by Votes (Ascending/Descending)
		- maybe also by range?
	- By user
	- By text search
		- This would use a split-text technique.
*/
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
print '';
print 'Creating fnGetForums';
print '';
GO

/*
ChatGPT gave me some help on this. 
Undesired duplication of the category strings was solved by creating a CTE table
See other comment by the CTE
*/

CREATE FUNCTION fnGetForums(
	@ForumID int,
	@UserID int,
	@CategoryID int,
	@ParentForumID int
	)
	RETURNS table
RETURN
	-- Basically, this CTE does a query without factoring in the votes. This prevents duplication and gives you a result set to combine with later when votes are factored in.
	WITH ForumCategories AS (
		SELECT 
			f.ForumID,
			STRING_AGG(c.CategoryName, ', ') AS Categories
		FROM forums.Forums f
		JOIN forums.ForumsCategories fc ON f.ForumID = fc.ForumID
		JOIN forums.Categories c ON fc.CategoryID = c.CategoryID
		GROUP BY f.ForumID
	)
	SELECT 
		-- '%' + dbo.fnGetCategoryNameByID(@CategoryID)+'%' AS test,		
		fc.Categories,
		COUNT(uv.Vote) AS TotalVotes,
		CAST(AVG(CAST(uv.Vote AS DECIMAL(10,2))) AS DECIMAL(10,2)) AS AverageVote,
		f.*
	FROM forums.Forums f
	LEFT JOIN ForumCategories fc ON f.ForumID = fc.ForumID
	LEFT JOIN forums.UserVotes uv ON f.ForumID = uv.ForumID
	WHERE 
		(@ForumID IS NULL OR f.ForumID = @ForumID) 
		AND f.ForumID != 0
		AND (@UserID IS NULL OR f.AuthorID = @UserID)
		AND (uv.ForumSolutionID IS NULL OR uv.ForumSolutionID = 0)
		AND (uv.CommentID IS NULL OR uv.CommentID = 0)
		AND (@ParentForumID IS NULL OR f.ParentForumID = @ParentForumID)
	GROUP BY
		fc.Categories,
		f.ForumID,
		f.Title,
		f.Description,
		f.CreatedAt,
		f.ModifiedAt,
		f.AuthorID,
		f.ScopeID,
		f.ParentForumID,
		f.BlockedContentID
	HAVING (@CategoryID IS NULL OR fc.Categories LIKE '%' + dbo.fnGetCategoryNameByID(@CategoryID) +'%');

GO

/*
Update an existing forum
*/

CREATE PROC spForums_UPDATE 
	@ForumID INT,
	@Title VARCHAR(200),
	@Description VARCHAR(max),
	@AuthorID INT,
	@ScopeID INT,
	@ParentForumID INT,
	@BlockedContentID INT
AS 
SET IDENTITY_INSERT forums.Forums ON;
UPDATE forums.Forums 
SET 
	Title = @Title,
	Description = @Description,
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

/*
Create a comment on a Forum, Solution, or another comment
*/

CREATE PROC spComments_INSERT 
	@CommentID INT,
	@Comment VARCHAR(3000),
	@ForumID INT,
	@SolutionID INT,
	@AuthorID INT,
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

CREATE FUNCTION fnGetComments(
	-- @ForumID,
	-- @SolutionID,
	-- @ParentCommentID
	) returns table
RETURN
	SELECT 
		COUNT(uv.Vote) AS TotalVotes,
		CAST(AVG(CAST(uv.Vote AS DECIMAL(10,2))) AS DECIMAL(10,2)) AS AverageVote,
		c.*
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
		c.BlockedContentID

GO
/*
Update an existing comment with new text
*/

CREATE PROC spComments_UPDATE 
 	@CommentID INT,
	@Comment VARCHAR(3000),
	@ForumID INT,
	@SolutionID INT,
	@AuthorID INT,
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
-- END  spComments_UPDATE 
GO

/*
Create a solution in response to a forum
*/

CREATE PROC spSolutions_INSERT 
	@SolutionID INT,
	@ForumID INT,
	@Title VARCHAR(400),
	@Description VARCHAR(max),
	@AuthorID INT,
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
print '';
print 'Creating fnGetSolutions';
print '';
GO

CREATE FUNCTION fnGetSolutions(
	@ForumID INT,
	@SolutionID INT,
	@AuthorID INT
	)
	RETURNS table

RETURN 
	-- Basically, this CTE does a query without factoring in the votes. This prevents duplication and gives you a result set to combine with later when votes are factored in.
	WITH ForumCategories AS (
		SELECT 
			s.SolutionID,
			STRING_AGG(c.CategoryName, ', ') AS Categories
		FROM forums.Solutions s
		JOIN forums.ForumsCategories fc ON s.ForumID = fc.ForumID
		JOIN forums.Categories c ON fc.CategoryID = c.CategoryID
		GROUP BY s.ForumID,s.SolutionID
	)

	SELECT 
		fc.Categories,
		COUNT(uv.Vote) AS TotalVotes,
		CAST(AVG(CAST(uv.Vote AS DECIMAL(10,2))) AS DECIMAL(10,2)) AS AverageVote,
		s.* 
	FROM forums.Solutions s
	LEFT JOIN ForumCategories fc ON s.SolutionID = fc.SolutionID
	LEFT JOIN forums.UserVotes uv ON s.SolutionID = uv.ForumSolutionID

	WHERE 
		(@SolutionID IS NULL OR s.SolutionID = @SolutionID) 
		AND (@ForumID IS NULL OR s.ForumID = @ForumID) 
		AND (@AuthorID IS NULL OR s.AuthorID = @AuthorID) 
		AND s.SolutionID != 0
		AND (uv.CommentID IS NULL OR uv.CommentID = 0)
		AND s.SolutionID != 0
	GROUP BY
		fc.Categories,
	 	s.SolutionID,
		s.ForumID,
		s.Title,
		s.Description,
		s.AuthorID,
		s.CreatedAt,
		s.ModifiedAt,
		s.BlockedContentID


GO
print '';
print 'Creating spSolutions_UPDATE';
print '';
GO

/*
Update an existing solution
*/

CREATE PROC spSolutions_UPDATE 
	@SolutionID INT,
	@ForumID INT,
	@Title VARCHAR(400),
	@Description VARCHAR(max),
	@AuthorID INT,
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
-- END  spSolutions_UPDATE 
GO
print '';
print 'Creating spUserVotes_INSERT';
print '';
GO

/*
Vote on a forum, solution, or comment
*/

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
GO
-- ATM, just be able to get a specific users votes
CREATE FUNCTION fnGetUsersVotes(
	@UserID int
	)
	RETURNS table

RETURN
	SELECT * FROM forums.UserVotes
	WHERE UserID = @UserID;

GO

/*
Update an existing vote
	I would like to combine this with spUserVotes_INSERT into spUserVotes_UPSERT
*/

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
-- END  spUserVotes_UPDATE 


GO
/*
Create a new category for forums.
*/
--   Don't pass the @CategoryID, instead pass a CategoryName.
--   If it exist, use the ID for it. If not, then created a category for it and use that ID
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
GO

/* 
VIEWs
vwGetCategories - Gets all existing categories
	--- TODO Create a function, fnGetUsersCategories(UserID) -> A filtered version of the same list, or sorted by preference.
*/

CREATE VIEW vwGetCategories
AS
SELECT * FROM forums.Categories

GO

-- Get the top 10 percent of contributors to the Forums
-- TODO Create the same view for solutions and comments and votes.
DROP VIEW IF EXISTS forums.vwTopContributors;
GO
CREATE VIEW forums.vwTopContributors
AS
SELECT TOP 10 PERCENT u.FirstName, u.LastName, u.UserName, u.Email, COUNT(*) AS totalForums
FROM users.Users AS u
JOIN forums.Forums ON AuthorID = u.UserID
GROUP BY u.UserName,u.FirstName, u.LastName, u.Email
ORDER BY totalForums DESC

GO

DROP VIEW IF EXISTS forums.vwDivisiveContent
GO

/*
This view can be used to identify which forums are 
likely to be contentious or have a wide range of opinions.
Identifies forums with highly divisive content based on votes
by calculating metrics like the average vote and 
the standard deviation of votes to measure divisiveness
The Metrics
	Average Vote: Calculate the average vote for each forum.
	Standard Deviation: Calculate the standard deviation of votes to measure how spread out the votes are, indicating divisiveness.
*/
CREATE VIEW forums.vwDivisiveContent AS
SELECT 
    f.ForumID,
    f.Title,
    f.Description,
    COUNT(uv.Vote) AS TotalVotes,
    AVG(CAST(uv.Vote AS DECIMAL(10,2))) AS AverageVote,
    STDEV(CAST(uv.Vote AS DECIMAL(10,2))) AS VoteStdDev,
    CASE 
        WHEN STDEV(CAST(uv.Vote AS DECIMAL(10,2))) > 2 THEN 'Divisive'
        ELSE 'Non-Divisive'
    END AS Divisiveness
FROM 
    forums.Forums f
LEFT JOIN 
    forums.UserVotes uv ON f.ForumID = uv.ForumID
GROUP BY 
    f.ForumID, 
    f.Title, 
    f.Description
HAVING 
    COUNT(uv.Vote) > 1; -- Ensures that there is more than one vote to calculate divisiveness


GO


/*

A view that highlights active or trending content in forums

When this view is called, Additional sorting should be applied.
	ORDER BY cannot be used in VIEWs.....

SELECT *
FROM forums.vwHotContent
ORDER BY TotalUserActions DESC, LastActivityTimestamp DESC;


*/
DROP VIEW IF EXISTS forums.vwHotContent;
GO

CREATE VIEW forums.vwHotContent AS
SELECT
    f.ForumID,
    f.Title,
    f.Description,
    f.CreatedAt,
    f.ModifiedAt,
    f.AuthorID,
    f.ScopeID,
    f.ParentForumID,
    f.BlockedContentID,
    ISNULL(
        (SELECT COUNT(DISTINCT uh.UserHistoryID)
         FROM users.UserHistory uh
         WHERE uh.ForumID = f.ForumID
           AND uh.Action IN ('User voted on some content', 'Created a comment on a forum')
        ), 0) AS TotalUserActions, -- Counts distinct user actions related to the forum
    ISNULL(
        (SELECT COUNT(DISTINCT uv.UserID)
         FROM forums.UserVotes uv
         WHERE uv.ForumID = f.ForumID
        ), 0) AS TotalVotes, -- Counts distinct votes related to the forum
    CAST(AVG(CAST(uv.Vote AS DECIMAL(10,2))) AS DECIMAL(10,2)) AS AverageVote, -- Average vote score
    ISNULL(
        (SELECT MAX(uh.Timestamp)
         FROM users.UserHistory uh
         WHERE uh.ForumID = f.ForumID
           AND uh.Action IN ('User voted on some content', 'Created a comment on a forum')
        ), f.CreatedAt) AS LastActivityTimestamp -- The last activity timestamp
FROM 
    forums.Forums f
LEFT JOIN 
    forums.UserVotes uv ON f.ForumID = uv.ForumID
WHERE
    f.BlockedContentID IS NULL -- Only include non-blocked content
GROUP BY
    f.ForumID,
    f.Title,
    f.Description,
    f.CreatedAt,
    f.ModifiedAt,
    f.AuthorID,
    f.ScopeID,
    f.ParentForumID,
    f.BlockedContentID
HAVING 
    ISNULL(
        (SELECT COUNT(DISTINCT uh.UserHistoryID)
         FROM users.UserHistory uh
         WHERE uh.ForumID = f.ForumID
           AND uh.Action IN ('User voted on some content', 'Created a comment on a forum')
        ), 0) > 0 -- Include only forums with user actions
    OR ISNULL(
        (SELECT COUNT(DISTINCT uv.UserID)
         FROM forums.UserVotes uv
         WHERE uv.ForumID = f.ForumID
        ), 0) > 0; -- Include only forums with votes


GO



CREATE FUNCTION fnGetUserHistory(
	@UserID int
	)
	RETURNS table

RETURN
	SELECT 
		*
	FROM users.UserHistory
	WHERE (@UserID IS NULL OR UserID = @UserID)
GO
/*
Special Audit functions


This script is powerful for analyzing table structures 
and could be a valuable tool for database administrators
 or developers looking to audit or optimize a database.
*/

GO
-- helper function for sp_GetDatabaseStats

DROP FUNCTION IF EXISTS dbo.GetPrimaryKeyColumns;
GO
CREATE FUNCTION dbo.GetPrimaryKeyColumns
(
    @TableIdentifier VARCHAR(255) -- Format: SchemaName.TableName
)
RETURNS VARCHAR(MAX)
AS
BEGIN
    DECLARE @PrimaryKeys VARCHAR(MAX);

    -- Extract schema and table name from the input parameter
    DECLARE @SchemaName NVARCHAR(128);
    DECLARE @TableName NVARCHAR(128);
    
    -- Split the @TableIdentifier into schema and table name
    SET @SchemaName = PARSENAME(@TableIdentifier, 2);
    SET @TableName = PARSENAME(@TableIdentifier, 1);
    
    -- Query to get primary key columns as a comma-separated list
    SELECT @PrimaryKeys = STRING_AGG(COLUMN_NAME, ', ')
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = @SchemaName
      AND TABLE_NAME = @TableName
      AND CONSTRAINT_NAME = (
          SELECT CONSTRAINT_NAME
          FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
          WHERE TABLE_SCHEMA = @SchemaName
            AND TABLE_NAME = @TableName
            AND CONSTRAINT_TYPE = 'PRIMARY KEY'
      );
    
    RETURN @PrimaryKeys;
END;
GO
--  helper function for sp_GetDatabaseStats

-- Drop the function if it already exists
DROP FUNCTION IF EXISTS dbo.GetForeignKeyColumns;
GO

-- Create the function to return foreign key columns
CREATE FUNCTION dbo.GetForeignKeyColumns
(
    @TableIdentifier VARCHAR(255) -- Format: SchemaName.TableName
)
RETURNS VARCHAR(MAX)
AS
BEGIN
    DECLARE @ForeignKeys VARCHAR(MAX);

    -- Extract schema and table name from the input parameter
    DECLARE @SchemaName NVARCHAR(128);
    DECLARE @TableName NVARCHAR(128);
    
    -- Split the @TableIdentifier into schema and table name
    SET @SchemaName = PARSENAME(@TableIdentifier, 2);
    SET @TableName = PARSENAME(@TableIdentifier, 1);
    
    -- Query to get foreign key columns as a comma-separated list
    SELECT @ForeignKeys = STRING_AGG(COLUMN_NAME, ', ')
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS r
    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
        ON r.CONSTRAINT_NAME = k.CONSTRAINT_NAME
    WHERE k.TABLE_SCHEMA = @SchemaName
      AND k.TABLE_NAME = @TableName;

    RETURN @ForeignKeys;
END;


/*
Obtain a table of database tables with rowCount, ColumnCount, DatumCount, Primary Keys, Foreign Keys
*/

GO
DROP PROCEDURE IF EXISTS dbo.sp_GetDatabaseStats;
GO

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
GO


GO

/**************************
 
 INSERT DEMO DATA
 
 forums.Forums
 forums.Solutions 
 forums.Comments
 forums.UserVotesForums
 forums.BlockedContent
 forums.Scopes
 forums.Categories
 users.UserHistory
 users.Users
 
 The default way to enter data into tables will be via stored procedures
 
 **************************/
SET IDENTITY_INSERT forums.Scopes ON;
INSERT INTO forums.Scopes (ScopeID, ScopeName)
VALUES (1, 'Global'),
	(2, 'National'),
	(3, 'Local'),
	(4, 'Individual');
SET IDENTITY_INSERT forums.Scopes OFF;
Print 'Added Scopes';

-- NOTE: Categories
-- The categories added here are default categories.
-- New categories can be made via the STORED PROCEDURE "spForums_INSERT"
--     In this sp, the user specifies either an existing category or a new category.
-- Categories are like 'tags': A forum could have many.... This is the "forums.RelatesTo" table, or "forums.Forums_Categories"
SET IDENTITY_INSERT forums.Categories ON;
INSERT INTO forums.Categories (CategoryID, CategoryName)
VALUES 
	(1, 'Global Cooperation'),
	(2, 'Sustainable Development'),
	(3, 'Equitable Access'),
	(4, 'Innovation and Technology'),
	(5, 'Effective Governance'),
	(6, 'Education and Awareness'),
	(7, 'Cultural Understanding'),
	(8, 'Resilience and Adaptability');
SET IDENTITY_INSERT forums.Categories OFF;
Print 'Added Categories';


/*
 CREATE A USER
 - TODO create a stored procedure for this
 */

SET IDENTITY_INSERT users.Users ON;
INSERT INTO users.Users (
		UserID,
		FirstName,
		LastName,
		UserName,
		Password,
		Email,
		IsEmailConfirmed
	)
VALUES (
		1,
		'Cooper',
		'The Dog',
		'bark in ur face',
		HASHBYTES('SHA2_512', 'test-password'),
		'whoLetTheDogsOut@barker.com',
		'TRUE'
	),
	(
		2,
		'Leo',
		'The Cat',
		'Orange Prince',
		HASHBYTES('SHA2_512', 'test-password1'),
		'ILetTheDogsOut@meow.com',
		'TRUE'
	),
	(
		3,
		'William',
		'Owen',
		'islandhopper152',
		HASHBYTES('SHA2_512', 'test-password'),
		'islandhopper152@gmail.com',
		'TRUE'
	),
	(
		4,
		'James',
		'James Bond',
		'007',
		HASHBYTES('SHA2_512', 'drowssap'),
		'duhduhduuhduhduhduh007@yahoo.com',
		'TRUE'
	)
SET IDENTITY_INSERT users.Users OFF;
Print 'Added 3 Users'

-- EXEC dbo.sp_GetDatabaseStats;

/*
 Create an Example problem, solution, and demo votes
 */
DECLARE @forumID_INSERT int;

EXEC @forumID_INSERT = spForums_INSERT
		@ForumID = 2,
		@Title = 'The Public Think Tank: What features would improve it?',
		@Description = 'Social media can play a significant role in helping \
humanity reach goals for a cohesive and thriving global society, provided it \
is used effectively and responsibly. \
Here’s how social media can contribute:\
\
1. **Raising Awareness:**  \
   Social media platforms can spread awareness about important \
   issues such as climate change, inequality, and health \
   crises. Campaigns and informational content can reach large \
   audiences quickly and mobilize action.\
\
2. **Facilitating Communication:**  \
   Social media enables global communication and collaboration, \
   allowing people from different regions and backgrounds to \
   connect, share ideas, and work together on common goals.\
\
3. **Supporting Activism:**  \
   Social media has been a powerful tool for grassroots \
   movements and activism. It can help organize events, rally \
   support, and drive social and political change.\
\
4. **Crowdsourcing Solutions:**  \
   Platforms can be used to gather input and ideas from diverse \
   populations, leveraging collective intelligence to solve \
   problems and innovate.\
\
5. **Educational Outreach:**  \
   Social media can provide access to educational resources, \
   expert knowledge, and learning opportunities. It can also \
   support digital literacy and critical thinking.\
\
6. **Community Building:**  \
   Social media helps build and strengthen communities around \
   shared interests and causes. Online communities can offer \
   support, share resources, and amplify voices.\
\
7. **Promoting Transparency and Accountability:**  \
   Social media can be used to monitor and report on issues \
   such as corruption, environmental damage, and human rights \
   abuses. It provides a platform for holding individuals and \
   institutions accountable.\
\
8. **Disseminating Critical Information:**  \
   In times of crisis, social media can quickly disseminate \
   important information, provide updates, and coordinate \
   responses.\
\
However, there are challenges and risks associated with social \
media use:\
\
- **Misinformation and Fake News:**  \
  The spread of misinformation can undermine efforts and create \
  confusion. It’s crucial to verify information and promote \
  media literacy.\
\
- **Privacy and Security:**  \
  Ensuring user privacy and data security is essential to \
  maintaining trust and protecting individuals from harm.\
  \
- **Echo Chambers:**  \
  Social media can create echo chambers where users are exposed \
  only to information that reinforces their existing beliefs. \
  This can hinder constructive dialogue and understanding.\
\
- **Digital Divide:**  \
  Access to social media and digital technologies is uneven \
  across the globe, which can exacerbate inequalities.\
\
By addressing these challenges and using social media \
strategically, it can be a valuable tool in promoting positive \
change and helping humanity achieve its goals.',
		@AuthorID = 3,
		@ScopeID = 2,
		@ParentForumID = NULL,
		@BlockedContentID = NULL;

EXEC spForumsCategories_INSERT
	@CategoryName = 'Global Cooperation',
	@ForumID = @ForumID_INSERT;  -- The Public Think Tank

EXEC spForumsCategories_INSERT
	@CategoryName = 'Innovation and Technology',
	@ForumID = @ForumID_INSERT; -- The Public Think Tank

EXEC spSolutions_INSERT
		@SolutionID = 3,
		@ForumID = 2,
		@Title = 'Make sure to handle multiple languages',
		@Description = 'If the site is meant to be a global public think tank, then it should handle other language characters and be able to translate languages for different clients. (use nvarchar)',
		@AuthorID = 1,
		@BlockedContentID = NULL

EXEC spSolutions_INSERT
		@SolutionID = 2,
		@ForumID = 2,
		@Title = 'Add a more expressive way to explain problems and solutions',
		@Description = 'I''m, specifically thinking Google Docs platform would be grate to work with. Maybe there are ways to integrate them directly into HTML. \
		The only problems I can forsee are that the content of them might not be searchable from the Public Think Tank.',
		@AuthorID = 1,
		@BlockedContentID = NULL

-- EXEC spUserVotes_INSERT -- This was throwing an error about duplicate primary key..
-- 	@ForumID = 2,
-- 	@ForumSolutionID = 0,
-- 	@CommentID = 0,
-- 	@UserID = 3,
-- 	@Vote = 10;

EXEC spUserVotes_INSERT
	@ForumID = 2,
	@ForumSolutionID = 2,
	@CommentID = 0,
	@UserID = 3,
	@Vote = 10;

EXEC spUserVotes_INSERT
	@ForumID = 2,
	@ForumSolutionID = 2,
	@CommentID = 0,
	@UserID = 4,
	@Vote = 2;


EXEC @forumID_INSERT = spForums_INSERT
	@ForumID = 3,
	@Title = 'How can we restore populations of endangered species?',
	@Description = 'For example, the Southern Resident Orcas of the San Juan Islands',
	@AuthorID = 3,
	@ScopeID = 3,
	@ParentForumID = NULL,
	@BlockedContentID = NULL;

-- This automatically creates a new category which didn't exist before
EXEC spForumsCategories_INSERT
	@CategoryName = 'Biodiversity and Wildlife',
	@ForumID = @forumID_INSERT;

-- Creating sub problem
EXEC  @forumID_INSERT = spForums_INSERT
	@ForumID = 4,
	@Title = "How can we improve public education around endangered species?",
	@Description = "This seems like the way to generate public investment in biodiversity and wildlife.",
	@AuthorID = 4,
	@ScopeID = 2,
	@ParentForumID = 3 ,
	@BlockedContentID = NULL;

EXEC spForumsCategories_INSERT
	@CategoryName = 'Biodiversity and Wildlife',
	@ForumID = @forumID_INSERT;


EXEC @forumID_INSERT = spForums_INSERT
		@ForumID = 1,
		@Title = 'How can we reduce the risk and danger associated with driving on the road?',
		@Description = 'Every time I am on the road I see people driving with out regard to safety. I know there are stats on driving accidents and that they can be reduced. What are some ways we can make that happpen?',
		@AuthorID = 3,
		@ScopeID = 2,
		@ParentForumID = NULL,
		@BlockedContentID = NULL;

EXEC spForumsCategories_INSERT
	@CategoryName = 'Resilience and Adaptability',
	@ForumId = @forumID_INSERT; -- Dangers of driving
	
EXEC spForums_UPDATE 
		@ForumID = 1,
		@Title = 'How can we reduce the risk and danger associated with driving on the freeway?',
		@Description = 'Every time I am on the road I see people driving with out regard to safety. I know there are stats on driving accidents and that they can be reduced. What are some ways we can make that happpen?',
		@AuthorID = 3,
		@ScopeID = 2,
		@ParentForumID = NULL,
		@BlockedContentID = NULL;

EXEC spComments_INSERT
	@CommentID = 1,
	@Comment = 'Sounds like an idea the Mechatronics people could help build ',
	@ForumID = 1,
	@SolutionID = NULL,
	@AuthorID = 2,
	@ParentCommentID = NULL, 
	@BlockedContentID = NULL;

EXEC spComments_UPDATE
	@CommentID = 1,
	@Comment = 'Sounds like an idea the Mechatronics people could help build. Maybe we should inquire about that.',
	@ForumID = 1,
	@SolutionID = NULL,
	@AuthorID = 2,
	@ParentCommentID = NULL, 
	@BlockedContentID = NULL;

EXEC spUserVotes_INSERT
	@ForumID = 1,
	@ForumSolutionID = 0,
	@CommentID = 1,
	@UserID = 3,
	@Vote = 10;


	

EXEC spSolutions_INSERT
		@SolutionID = 1,
		@ForumID = 1,
		@Title = 'Add a device to cars which reports tail-gaters',
		@Description = 'The pieces of tech already exist, they just need to be assembled in the right way. We already have back up cams. Why can''t those also be used to take pictures of license plates tail gating you on the freeway, with automated reporting? If something like this was mainstream and people knew about it, there would be less tail gaters.',
		@AuthorID = 3,
		@BlockedContentID = NULL

EXEC spSolutions_UPDATE
	@SolutionID = 1,
	@ForumID = 1,
	@Title = 'Add a device to cars which automatically reports tail-gaters when on the freeway',
	@Description = 'The pieces of tech already exist, they just need to be assembled in the right way. We already have back up cams. Why can''t those also be used to take pictures of license plates tail gating you on the freeway, with automated reporting? If something like this was mainstream and people knew about it, there would be less tail gaters.',
	@AuthorID = 3,
	@BlockedContentID = NULL




EXEC spUserVotes_INSERT
	@ForumID = 1,
	@ForumSolutionID = 0,
	@CommentID = 0,
	@UserID = 2,
	@Vote = 10;

EXEC spUserVotes_INSERT
	@ForumID = 1,
	@ForumSolutionID = 0,
	@CommentID = 0,
	@UserID = 1,
	@Vote = 7;

EXEC spUserVotes_INSERT
	@ForumID = 1,
	@ForumSolutionID = 0,
	@CommentID = 0,
	@UserID = 4,
	@Vote = 6;

EXEC spUserVotes_INSERT
	@ForumID = 2,
	@ForumSolutionID = 0,
	@CommentID = 0,
	@UserID = 4,
	@Vote = 9;

EXEC spUserVotes_INSERT
	@ForumID = 1,
	@ForumSolutionID = 1,
	@CommentID = 0,
	@UserID = 2,
	@Vote = 8;

EXEC spUserVotes_INSERT
	@ForumID = 1,
	@ForumSolutionID = 1,
	@CommentID = 0,
	@UserID = 1,
	@Vote = 7;
	
PRINT '';
PRINT 'TEST: This duplicated vote is not allowed';
PRINT '      Should use spUserVotes_INSERT'
PRINT '';

EXEC spUserVotes_INSERT
	@ForumID = 1,
	@ForumSolutionID = 1,
	@CommentID = 0,
	@UserID = 1,
	@Vote = 7;


 
-- INSERT INTO forums.UserVotesForums (ForumID, UserID, Vote)
-- VALUES (1, 2, 10),
-- 	(1, 1, 6);
-- print 'Added some votes to UserVotesForums';
-- INSERT INTO forums.UserVotesSolutions (ForumSolutionID, ForumID, UserID, Vote)
-- VALUES (1, 1, 2, 8),
-- 	(1, 1, 1, 5);
-- print 'Added some votes to UserVotesSolutions';




/*
 Demo Queries
 */
-- Get paginated problems (by highest vote average, or number of votes, or created at, by scope, category.)
-- Get paginated solutions for problem (by highest vote average, or number of votes, or created at)
-- Get user history
-- Get all categories (auto generated by trigger (or SP) on forum creation )
--      This will require a stored procdure which accepts a category argument when creating a problem, which is not part of the schema. 
-- Get sub-problems
-- Get comments for a problem
-- Get comments for a solution

print '';
print 'Fetching data...'
print '';
-- Get all forums
SELECT * FROM dbo.fnGetForums(NULL, NULL, NULL, NULL);
-- Get all forums with the 'Global Cooperation' Category
SELECT * FROM dbo.fnGetForums(NULL, NULL, 1, NULL);
-- Get all forums with the 'Biodiversity and Wildlife' Category
SELECT * FROM dbo.fnGetForums(NULL, NULL, 9, NULL);
-- Get a forum by ID
SELECT * FROM dbo.fnGetForums(1, NULL, NULL, NULL);
-- Get a forum that are children of another problem
SELECT * FROM dbo.fnGetForums(NULL, NULL, NULL, 3);
-- GET a users history
SELECT * FROM dbo.fnGetUserHistory(3);
-- GET a users specific votes
SELECT * FROM dbo.fnGetUsersVotes(3)
SELECT * FROM dbo.fnGetUsersVotes(2)
-- GET Solutions for a specific problem
SELECT * FROM dbo.fnGetSolutions(2, NULL, NULL);
-- GET Solutions for a specific problem
SELECT * FROM dbo.fnGetSolutions(1, NULL, NULL);
-- Get a specific solution
SELECT * FROM dbo.fnGetSolutions(2, 3, NULL);
-- Get a solutions by specific user
SELECT * FROM dbo.fnGetSolutions(NULL, NULL, 3);
-- Get ALL Comments
SELECT * FROM dbo.fnGetComments();


-- GET ALL CATEGORIES
SELECT * FROM vwGetCategories;

print 'Testing a function to get a category by ID: ' + dbo.fnGetCategoryNameByID(1);

SELECT * FROM forums.vwTopContributors;
SELECT * FROM forums.vwDivisiveContent;


SELECT *
FROM forums.vwHotContent
ORDER BY TotalUserActions DESC, LastActivityTimestamp DESC;

-- Auditing stored procedure
EXEC dbo.sp_GetDatabaseStats;
