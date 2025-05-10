-- Required seed data that the application needs to function properly
-- This file should be ran on build in both production and Development

-- One reason this is required is because of a database "workaround" I came up with
-- in an effort to be able to combine all votes into a single votes table.
-- The Gist of it is that The UserVote uses a CompoundKey for All of its arguments.
-- This makes it essentially, one vote per voteable item.
-- But compound keys are made of primary keys, and primary keys cannot be null. 
-- Therefore, a "System" entry for forums and solutions and comments needs to exist (ID = 0)
-- When votes are cast to UserVotes, instead of writing "null" for columns where the vote didn't apply. 
-- you could write 0 (the system user). And that would make the compound key work, and you could ignore the ID 0

CREATE UNIQUE INDEX IX_UserVotes_UserID_ForumID
ON forums.UserVotes (UserID, ForumID)
WHERE ForumID IS NOT NULL AND ForumSolutionID IS NULL AND CommentID IS NULL;

CREATE UNIQUE INDEX IX_UserVotes_UserID_ForumSolutionID
ON forums.UserVotes (UserID, ForumSolutionID)
WHERE ForumID IS NULL AND ForumSolutionID IS NOT NULL AND CommentID IS NULL;

CREATE UNIQUE INDEX IX_UserVotes_UserID_CommentID
ON forums.UserVotes (UserID, CommentID)
WHERE ForumID IS NULL AND ForumSolutionID IS NULL AND CommentID IS NOT NULL;
