using Microsoft.EntityFrameworkCore;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.Database;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.Repository
{
    public class IssueRepository : IIssueRepository
    {
        private ApplicationDbContext _context;
        public IssueRepository(ApplicationDbContext context) { 
            _context = context;
        }

        /// <summary>
        /// Represents a cacheable unit of an issue
        /// </summary>
        /// <remarks>
        /// Author info is not cached with the issue, but cached via an AppUser Cache.
        /// This is meant to make updates to the cache easier. And not have duplicate data laying around.
        /// </remarks>
        public async Task<IssueRepositoryViewModel?> GetIssueById(Guid id)
        {
            Issue? issue = await _context.Issues
                .Include(i => i.Scope)
                .FirstOrDefaultAsync(i => i.IssueID == id);

            if (issue == null)
                return null;

            return new IssueRepositoryViewModel
            {
                Id = issue.IssueID,
                ParentIssueID = issue.ParentIssueID,
                ParentSolutionID = issue.ParentSolutionID,
                ContentStatus = issue.ContentStatus,
                Scope = issue.Scope,
                CreatedAt = issue.CreatedAt,
                ModifiedAt = issue.ModifiedAt,
                Title = issue.Title,
                Content = issue.Content,
                AuthorID = issue.AuthorID,
            };
        }


        public Task AddIssueAsync(Issue issue, Guid? parentIssueId, Guid? parentSolutionId)
        {
            return Task.CompletedTask;
        }
    }
}
