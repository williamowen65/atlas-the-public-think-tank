using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
 
using atlas_the_public_think_tank.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup.TestingData
{
    public static class Issues
    {

        public static Issue TestIssue1 { get; } = new Issue
        {
            IssueID = Guid.NewGuid(),
            Title = "Test Issue",
            Content = "This is a test issue for testing",
            CreatedAt = DateTime.Now,
            AuthorID = Users.TestUser1.Id,
            ScopeID = Scopes.GlobalScope.ScopeID,
            ContentStatus = ContentStatus.Published
        };
        public static Issue TestIssue2 { get; } = new Issue
        {
            IssueID = Guid.NewGuid(),
            Title = "Test Sub Issue",
            Content = "This is a test sub-issue for testing",
            CreatedAt = DateTime.Now,
            ParentIssueID = TestIssue1.IssueID,
            AuthorID = Users.TestUser1.Id,
            ScopeID = Scopes.GlobalScope.ScopeID,
            ContentStatus = ContentStatus.Published
        };
        public static Issue TestIssue3 { get; } = new Issue
        {
            IssueID = Guid.NewGuid(),
            Title = "Test Sub Issue",
            Content = "This is a test sub-issue for testing",
            CreatedAt = DateTime.Now,
            ParentIssueID = TestIssue1.IssueID,
            AuthorID = Users.TestUser1.Id,
            ScopeID = Scopes.GlobalScope.ScopeID,
            ContentStatus = ContentStatus.Published
        };
        public static Issue TestIssue4 { get; } = new Issue
        {
            IssueID = Guid.NewGuid(),
            Title = "Test Sub Issue",
            Content = "This is a test sub-issue for testing",
            CreatedAt = DateTime.Now,
            ParentIssueID = TestIssue1.IssueID,
            AuthorID = Users.TestUser1.Id,
            ScopeID = Scopes.GlobalScope.ScopeID,
            ContentStatus = ContentStatus.Published
        };
        public static Issue TestIssue5 { get; } = new Issue
        {
            IssueID = Guid.NewGuid(),
            Title = "Test Sub Issue",
            Content = "This is a test sub-issue for testing",
            CreatedAt = DateTime.Now,
            ParentIssueID = TestIssue1.IssueID,
            AuthorID = Users.TestUser1.Id,
            ScopeID = Scopes.GlobalScope.ScopeID,
            ContentStatus = ContentStatus.Published
        };

        public static void CreateTestIssue(ApplicationDbContext db, Issue issue)
        {
            db.Issues.Add(issue);
        }
    
    
    }
}
