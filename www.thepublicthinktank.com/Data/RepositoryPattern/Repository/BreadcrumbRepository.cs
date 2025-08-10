using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
{
    public class BreadcrumbRepository : IBreadcrumbRepository
    {

        private ApplicationDbContext _context;
        public BreadcrumbRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Breadcrumb_ReadVM>> GetBreadcrumbPagedAsync(Guid? contentId)
        {
            var breadcrumbs = new List<Breadcrumb_ReadVM>();
            if (contentId == null)
            {
                return breadcrumbs;
            }

            // First check if the content exists as an Issue
            var issue = await _context.Issues
                .Include(i => i.ParentIssue)
                .Include(i => i.ParentSolution)
                .FirstOrDefaultAsync(i => i.IssueID == contentId);

            if (issue != null)
            {
                breadcrumbs.Add(new Breadcrumb_ReadVM
                {
                    Title = issue.Title,
                    ContentID = issue.IssueID,
                    ContentType = ContentType.Issue
                });

                // If this issue has a parent issue, recursively get its breadcrumbs
                if (issue.ParentIssueID.HasValue)
                {
                    var parentBreadcrumbs = await GetBreadcrumbPagedAsync(issue.ParentIssueID.Value);
                    breadcrumbs.AddRange(parentBreadcrumbs);
                }
                // If this issue has a parent solution, recursively get its breadcrumbs
                else if (issue.ParentSolutionID.HasValue)
                {
                    var parentBreadcrumbs = await GetBreadcrumbPagedAsync(issue.ParentSolutionID.Value);
                    breadcrumbs.AddRange(parentBreadcrumbs);
                }

                return breadcrumbs;
            }

            // If not an issue, check if it's a solution
            var solution = await _context.Solutions
                .Include(s => s.ParentIssue)
                .FirstOrDefaultAsync(s => s.SolutionID == contentId);

            if (solution != null)
            {
                breadcrumbs.Add(new Breadcrumb_ReadVM
                {
                    Title = solution.Title,
                    ContentID = solution.SolutionID,
                    ContentType = ContentType.Solution
                });

                // Solutions have parent issues, so get the breadcrumbs for the parent issue
                if (solution.ParentIssueID != Guid.Empty)
                {
                    var parentBreadcrumbs = await GetBreadcrumbPagedAsync(solution.ParentIssueID);
                    breadcrumbs.AddRange(parentBreadcrumbs);
                }

                return breadcrumbs;
            }

            // If we get here, no content was found with the given ID
            return breadcrumbs;
        }
    }
}

