using Microsoft.EntityFrameworkCore;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.RepositoryPattern.Repository
{
    public class IssueRepository : IIssueRepository
    {
        private ApplicationDbContext _context;
        public IssueRepository(ApplicationDbContext context) { 
            _context = context;
        }
        public async Task<IssueRepositoryViewModel?> GetIssueById(Guid id)
        {
            Issue? issue = await _context.Issues
                .FirstOrDefaultAsync(i => i.IssueID == id);

            if (issue == null)
                return null;

            return new IssueRepositoryViewModel
            {
                Id = issue.IssueID,
                Title = issue.Title,
                Content = issue.Content
            };
        }


        public Task AddIssueAsync(Issue issue, Guid? parentIssueId, Guid? parentSolutionId)
        {
            return Task.CompletedTask;
        }
    }
}
