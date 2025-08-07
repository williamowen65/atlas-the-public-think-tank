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
        /// Represents a cachable unit of an issue
        /// </summary>
        public async Task<IssueRepositoryViewModel?> GetIssueById(Guid id)
        {
            Issue? issue = await _context.Issues
                .Include(i => i.Scope)
                .Include(i => i.Author)
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
                Author = new AppUser_ContentItem_ReadVM
                {
                    Id = issue.Author.Id,
                    UserName = issue.Author.UserName!,
                    email = issue.Author.Email!
                }
            };
        }


        public Task AddIssueAsync(Issue issue, Guid? parentIssueId, Guid? parentSolutionId)
        {
            return Task.CompletedTask;
        }
    }
}
