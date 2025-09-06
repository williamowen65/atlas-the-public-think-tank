using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers
{
    public class DiffCheckers
    {
        public static bool AreScopesDifferent(Scope scopeA, Scope scopeB)
        {
            if (scopeA == null || scopeB == null) return scopeA != scopeB;

            bool boundariesDiff = !scopeA.Boundaries.OrderBy(x => x)
                .SequenceEqual(scopeB.Boundaries.OrderBy(x => x));
            bool domainsDiff = !scopeA.Domains.OrderBy(x => x)
                .SequenceEqual(scopeB.Domains.OrderBy(x => x));
            bool entityTypesDiff = !scopeA.EntityTypes.OrderBy(x => x)
                .SequenceEqual(scopeB.EntityTypes.OrderBy(x => x));
            bool scalesDiff = !scopeA.Scales.OrderBy(x => x)
                .SequenceEqual(scopeB.Scales.OrderBy(x => x));
            bool timeframesDiff = !scopeA.Timeframes.OrderBy(x => x)
                .SequenceEqual(scopeB.Timeframes.OrderBy(x => x));

            return boundariesDiff || domainsDiff || entityTypesDiff || scalesDiff || timeframesDiff;
        }


        public static bool AreIssuesDifferent(Issue issueA, Issue issueB)
        {
            if (issueA == null || issueB == null) return issueA != issueB;

            // Compare scalar properties
            if (issueA.IssueID != issueB.IssueID) return true;
            if (issueA.ParentIssueID != issueB.ParentIssueID) return true;
            if (issueA.ParentSolutionID != issueB.ParentSolutionID) return true;
            if (issueA.Title != issueB.Title) return true;
            if (issueA.Content != issueB.Content) return true;
            if (issueA.ContentStatus != issueB.ContentStatus) return true;
            if (issueA.AuthorID != issueB.AuthorID) return true;
            if (issueA.CreatedAt != issueB.CreatedAt) return true;
            // Don't compare modified at
            //if (issueA.ModifiedAt != issueB.ModifiedAt) return true;
            if (issueA.BlockedContentID != issueB.BlockedContentID) return true;
            if (issueA.ScopeID != issueB.ScopeID) return true;
       

            return false;
        }

        public static bool AreSolutionsDifferent(Solution solutionA, Solution solutionB)
        {
            if (solutionA == null || solutionB == null) return solutionA != solutionB;

            // Compare scalar properties
            if (solutionA.SolutionID != solutionB.SolutionID) return true;
            if (solutionA.ParentIssueID != solutionB.ParentIssueID) return true;
            if (solutionA.Title != solutionB.Title) return true;
            if (solutionA.Content != solutionB.Content) return true;
            if (solutionA.ContentStatus != solutionB.ContentStatus) return true;
            if (solutionA.AuthorID != solutionB.AuthorID) return true;
            if (solutionA.CreatedAt != solutionB.CreatedAt) return true;
            // Don't compare ModifiedAt unless you need to track edits
            // if (solutionA.ModifiedAt != solutionB.ModifiedAt) return true;
            if (solutionA.BlockedContentID != solutionB.BlockedContentID) return true;
            if (solutionA.ScopeID != solutionB.ScopeID) return true;

            return false;
        }


    }
}
