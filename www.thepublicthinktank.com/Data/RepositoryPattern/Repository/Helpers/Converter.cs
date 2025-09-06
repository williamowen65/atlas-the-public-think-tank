using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers
{
    public class Converter
    {

        public static Issue ConvertIssue_ReadVMToIssue(Issue_ReadVM vm)
        {
            if (vm == null) return null;

            var issue = new Issue
            {
                IssueID = vm.IssueID,
                ParentIssueID = vm.ParentIssueID,
                ParentSolutionID = vm.ParentSolutionID,
                Title = vm.Title,
                Content = vm.Content,
                CreatedAt = vm.CreatedAt,
                ModifiedAt = vm.ModifiedAt,
                ContentStatus = vm.ContentStatus,
                BlockedContentID = vm.BlockedContentID,
                ScopeID = vm.Scope?.ScopeID ?? Guid.Empty,
                AuthorID = vm.Author?.Id ?? Guid.Empty,
                // Navigation properties left null by default
                // Collections (ChildIssues, Solutions, IssueVotes, IssueTags) left null by default
            };

            return issue;
        }


        public static Solution ConvertSolution_ReadVMToSolution(Solution_ReadVM vm)
        {
            if (vm == null) return null;

            var solution = new Solution
            {
                SolutionID = vm.SolutionID,
                ParentIssueID = vm.ParentIssueID,
                Title = vm.Title,
                Content = vm.Content,
                CreatedAt = vm.CreatedAt,
                ModifiedAt = vm.ModifiedAt,
                ContentStatus = vm.ContentStatus,
                BlockedContentID = vm.BlockedContentID,
                ScopeID = vm.Scope?.ScopeID ?? Guid.Empty,
                AuthorID = vm.Author?.Id ?? Guid.Empty,
                // Navigation properties and collections left null by default
            };

            return solution;
        }
    }
}

