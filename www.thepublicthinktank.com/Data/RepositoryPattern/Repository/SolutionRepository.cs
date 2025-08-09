using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
{
    public class SolutionRepository : ISolutionRepository
    {

        private ApplicationDbContext _context;
        public SolutionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<SolutionRepositoryViewModel?> GetSolutionById(Guid id)
        {
            Solution? solution = await _context.Solutions
             .Include(i => i.Scope)
             .FirstOrDefaultAsync(i => i.SolutionID == id);

            if (solution == null)
                return null;

            return new SolutionRepositoryViewModel
            {
                Id = solution.SolutionID,
                ParentIssueID = solution.ParentIssueID,
                ContentStatus = solution.ContentStatus,
                Scope = solution.Scope,
                CreatedAt = solution.CreatedAt,
                ModifiedAt = solution.ModifiedAt,
                Title = solution.Title,
                Content = solution.Content,
                AuthorID = solution.AuthorID,
            };
        }
        public Task AddSolutionAsync(Solution solution, Guid parentIssueId)
        {
            throw new NotImplementedException();
          
        }

    }
}
