using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common.ContentItemVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
{
    public class IssueRepository : IIssueRepository
    {
        private ApplicationDbContext _context;
        private static IServiceProvider? _serviceProvider;
        public IssueRepository(ApplicationDbContext context, IServiceProvider serviceProvider) { 
            _context = context;
            _serviceProvider = serviceProvider;
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

            // Create and return the view model (also sets the cache)
            return await Read.Issue(issue.IssueID, new ContentFilter());
        }

        public async Task<Issue_ReadVM?> UpdateIssueAsync(Issue issue)
        {
            _context.Issues.Update(issue);
            await _context.SaveChangesAsync();

            return await Read.Issue(issue.IssueID, new ContentFilter());
        }

        public async Task<int> GetIssueVersionHistoryCount(Guid issueID)
        {
            return await _context.Issues
              .TemporalAll()
              .Where(i => i.IssueID == issueID)
              .CountAsync();
        }

        public async Task<List<ContentItem_ReadVM>?> GetIssueVersionHistoryByIssueVM(Issue_ReadVM issue)
        {

            if (_serviceProvider == null)
                throw new InvalidOperationException("Read class has not been initialized with a service provider.");

            using var methodScope = _serviceProvider.CreateScope();
            var services = methodScope.ServiceProvider;
            var _context = services.GetRequiredService<ApplicationDbContext>();
            var _appUserRepository = services.GetRequiredService<IAppUserRepository>();
            var _breadcrumbRepository = services.GetRequiredService<IBreadcrumbRepository>();


            var versionsWithPeriodStart = await _context.Issues
              .TemporalAll()
              .Where(i => i.IssueID == issue.IssueID)
              .OrderBy(i => EF.Property<DateTime>(i, "PeriodStart"))
              .Select(i => new
              {
                  Issue = i,
                  PeriodStart = EF.Property<DateTime>(i, "PeriodStart")
              })
              .ToListAsync();

            List<ContentItem_ReadVM> contentItemVersions = new();

            foreach (var versionData in versionsWithPeriodStart)
            {
                var version = versionData.Issue;
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
                    ContentID = version.IssueID,
                    Title = version.Title,
                    Content = version.Content,
                    ContentType = ContentType.Issue,
                    ContentStatus = version.ContentStatus,
                    CreatedAt = version.CreatedAt,
                    ModifiedAt = version.ModifiedAt,
                    Author = appUser!,
                    BreadcrumbTags = await _breadcrumbRepository.GetBreadcrumbPagedAsync(version.ParentIssueID ?? version.ParentSolutionID),
                    Scope = scope,
                    VoteStats = new ContentItemVotes_ReadVM
                    {
                        GenericContentVotes = issue.VoteStats.IssueVotes,
                        AverageVote = issue.VoteStats.AverageVote,
                        ContentID = issue.VoteStats.ContentID,
                        TotalVotes = issue.VoteStats.TotalVotes,
                        ContentType = ContentType.Issue
                    }
                };

                contentItemVersions.Add(contentItem);
            }

            return contentItemVersions;
        }
    }
}
