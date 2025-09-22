using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common.ContentItemVote;
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

        public static ContentItem_ReadVM ConvertIssue_ReadVMToContentItem_ReadVM(Issue_ReadVM vm)
        {
            if (vm == null) return null;

            return new ContentItem_ReadVM
            {
                ContentID = vm.IssueID,
                Title = vm.Title,
                Content = vm.Content,
                CreatedAt = vm.CreatedAt,
                ModifiedAt = vm.ModifiedAt,
                VersionHistoryCount = vm.VersionHistoryCount,
                LastActivity = vm.LastActivity,
                ContentStatus = vm.ContentStatus,
                BlockedContentID = vm.BlockedContentID,
                BreadcrumbTags = vm.BreadcrumbTags,
                Author = vm.Author,
                Comments = vm.Comments,
                Scope = vm.Scope,
                BlockedContent = vm.BlockedContent,
                VoteStats = new ContentItemVotes_ReadVM
                {
                    GenericContentVotes = vm.VoteStats.IssueVotes
                },
                ContentType = ContentType.Issue,
                PaginatedSubIssues = vm.PaginatedSubIssues,
                PaginatedSolutions = vm.PaginatedSolutions
            };
        }

        public static ContentItem_ReadVM ConvertSolution_ReadVMToContentItem_ReadVM(Solution_ReadVM vm)
        {
            if (vm == null) return null;

            return new ContentItem_ReadVM
            {
                ContentID = vm.SolutionID,
                Title = vm.Title,
                Content = vm.Content,
                CreatedAt = vm.CreatedAt,
                ModifiedAt = vm.ModifiedAt,
                VersionHistoryCount = vm.VersionHistoryCount,
                LastActivity = vm.LastActivity,
                ContentStatus = vm.ContentStatus,
                BlockedContentID = vm.BlockedContentID,
                BreadcrumbTags = vm.BreadcrumbTags,
                Author = vm.Author,
                Comments = vm.Comments,
                Scope = vm.Scope,
                BlockedContent = vm.BlockedContent,
                VoteStats = new ContentItemVotes_ReadVM
                {
                    GenericContentVotes = vm.VoteStats.SolutionVotes
                },
                ContentType = ContentType.Solution,
                PaginatedSubIssues = vm.PaginatedSubIssues,
                PaginatedSolutions = null // Solutions do not have sub-solutions
            };
        }

        //public static Issue_ReadVM ConvertIssueToIssue_ReadVM(Issue issue)
        //{
        //    if (issue == null) return null;

            //    return new Issue_ReadVM
            //    {
            //        IssueID = issue.IssueID,
            //        ParentIssueID = issue.ParentIssueID,
            //        ParentSolutionID = issue.ParentSolutionID,
            //        Title = issue.Title,
            //        Content = issue.Content,
            //        Scope = issue.Scope,
            //        ScopeID = issue.ScopeID ?? Guid.Empty,
            //        Author = null, // Set if you have an Author mapping
            //        CreatedAt = issue.CreatedAt,
            //        ModifiedAt = issue.ModifiedAt,
            //        ContentStatus = issue.ContentStatus,
            //        BlockedContentID = issue.BlockedContentID,
            //        VoteStats = null, // Set if you have VoteStats mapping
            //        ParentIssue = null, // Set if you want to map ParentIssue
            //        ParentSolution = null, // Set if you want to map ParentSolution
            //        PaginatedSubIssues = null, // Set if you want to map ChildIssues
            //        PaginatedSolutions = null // Set if you want to map Solutions
            //    };
            //}


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


        public static IssueRepositoryViewModel ConvertIssueToIssueRepositoryViewModel(Issue issue)
        {
            if (issue == null) return null;

            return new IssueRepositoryViewModel
            {
                Id = issue.IssueID,
                ParentIssueID = issue.ParentIssueID,
                ParentSolutionID = issue.ParentSolutionID,
                Title = issue.Title,
                Content = issue.Content,
                AuthorID = issue.AuthorID, // from ContentItem base
                Scope = issue.Scope,
                ContentStatus = issue.ContentStatus,
                CreatedAt = issue.CreatedAt,
                ModifiedAt = issue.ModifiedAt
            };
        }

        public static SolutionRepositoryViewModel ConvertSolutionToSolutionRepositoryViewModel(Solution solution)
        {
            if (solution == null) return null;

            return new SolutionRepositoryViewModel
            {
                Id = solution.SolutionID,
                ParentIssueID = solution.ParentIssueID,
                Title = solution.Title,
                Content = solution.Content,
                AuthorID = solution.AuthorID,
                Scope = solution.Scope,
                ContentStatus = solution.ContentStatus,
                CreatedAt = solution.CreatedAt,
                ModifiedAt = solution.ModifiedAt
            };
        }
    }
}

