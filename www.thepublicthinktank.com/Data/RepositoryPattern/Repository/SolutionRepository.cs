using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;

using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using Microsoft.EntityFrameworkCore;

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
        public async Task<Solution_ReadVM> AddSolutionAsync(Solution solution)
        {
            // Add the issue to the database
            await _context.Solutions.AddAsync(solution);
            await _context.SaveChangesAsync();

            // Create and return the view model
            return await Read.Solution(solution.SolutionID, new ContentFilter());

        }

        public async Task<Solution_ReadVM> UpdateSolutionAsync(Solution solution)
        {
            _context.Solutions.Update(solution);
            await _context.SaveChangesAsync();

            return await Read.Solution(solution.SolutionID, new ContentFilter());
        }

        public async Task<int> GetSolutionVersionHistoryCount(Guid solutionID)
        {
            return await _context.Solutions
             .TemporalAll()
             .Where(s => s.SolutionID == solutionID)
             .CountAsync();
        }
    }
}
