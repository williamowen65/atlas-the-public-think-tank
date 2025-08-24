using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
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


        public async Task<Issue_ReadVM?> AddIssueAsync(Issue issue)
        {
            // Add the issue to the database
            await _context.Issues.AddAsync(issue);
            await _context.SaveChangesAsync();

            // Create and return the view model
            return await Read.Issue(issue.IssueID, new ContentFilter());
        }

        public async Task<Issue_ReadVM?> UpdateIssueAsync(Issue issue)
        {
            _context.Issues.Update(issue);
            await _context.SaveChangesAsync();

            return await Read.Issue(issue.IssueID, new ContentFilter());

        }
    }
}
