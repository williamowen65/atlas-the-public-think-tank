using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common.ContentItemVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
{
    public class SolutionRepository : ISolutionRepository
    {

        private ApplicationDbContext _context;
        private static IServiceProvider? _serviceProvider;
        public SolutionRepository(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
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

        public async Task<List<ContentItem_ReadVM>?> GetSolutionVersionHistoryBySolutionVM(Solution_ReadVM solution)
        {

            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            using var methodScope = _serviceProvider.CreateScope();
            var services = methodScope.ServiceProvider;

            var _context = services.GetRequiredService<ApplicationDbContext>();
            var _appUserRepository = services.GetRequiredService<IAppUserRepository>();
            var _breadcrumbRepository = services.GetRequiredService<IBreadcrumbRepository>();


            // Pull all temporal versions of the solution with their period information
            var versionsWithPeriodStart = await _context.Solutions
                .TemporalAll()
                .Where(s => s.SolutionID == solution.SolutionID)
                .OrderBy(s => EF.Property<DateTime>(s, "PeriodStart"))
                .Select(s => new
                {
                    Solution = s,
                    PeriodStart = EF.Property<DateTime>(s, "PeriodStart")
                })
                .ToListAsync();

            List<ContentItem_ReadVM> contentItemVersions = new();

            foreach (var versionData in versionsWithPeriodStart)
            {
                var version = versionData.Solution;
                var versionPeriodStart = versionData.PeriodStart;

                AppUser_ReadVM? appUser = await _appUserRepository.GetAppUser(version.AuthorID);

                // Now use the captured versionPeriodStart in the scope query
                Scope? scope = await _context.Scopes
                    .TemporalAll()
                    .Where(s => s.ScopeID == version.ScopeID)
                    .Where(s =>
                        EF.Property<DateTime>(s, "PeriodStart") <= versionPeriodStart &&
                        EF.Property<DateTime>(s, "PeriodEnd") > versionPeriodStart)
                    .OrderByDescending(s => EF.Property<DateTime>(s, "PeriodStart"))
                    .FirstOrDefaultAsync();

                var contentItem = new ContentItem_ReadVM
                {
                    ContentID = version.SolutionID,
                    Title = version.Title,
                    Content = version.Content,
                    ContentType = ContentType.Solution,
                    ContentStatus = version.ContentStatus,
                    CreatedAt = version.CreatedAt,
                    ModifiedAt = version.ModifiedAt,
                    Author = appUser!,
                    BreadcrumbTags = await _breadcrumbRepository.GetBreadcrumbPagedAsync(version.ParentIssueID),
                    Scope = scope,
                    VoteStats = new ContentItemVotes_ReadVM
                    {
                        GenericContentVotes = solution.VoteStats.SolutionVotes,
                        AverageVote = solution.VoteStats.AverageVote,
                        ContentID = solution.VoteStats.ContentID,
                        TotalVotes = solution.VoteStats.TotalVotes,
                        ContentType = ContentType.Issue
                    }
                };

                contentItemVersions.Add(contentItem);
            }

            return contentItemVersions;
        }
    }
}
