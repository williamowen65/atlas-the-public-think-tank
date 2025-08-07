using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using repository_pattern_experiment.Data.RepositoryPattern.IRepository;
using repository_pattern_experiment.Models.Database;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.Repository
{
    public class VoteStatsRepository : IVoteStatsRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VoteStatsRepository(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

      



        /// <summary>
        /// Returns the vote stats on a specific content item
        /// </summary>
        /// <remarks>
        /// May or may not include the active users vote on this current item.
        /// </remarks>
        public async Task<UserVote_Generic_Cacheable_ReadVM?> getContentVoteStats(Guid id)
        {
            // First check if ID exists in Issues
            var issue = await _context.Issues.FirstOrDefaultAsync(i => i.IssueID == id);
            if (issue != null)
            {
                return await GetIssueVoteStats(id);
            }
            return null;
        }

        /// <summary>
        /// Represents a cacheable content vote entity
        /// </summary>
        /// <param name="issueId"></param>
        /// <returns></returns>
        public async Task<UserVote_Issue_ReadVM> GetIssueVoteStats(Guid issueId)
        {
            // Retrieve vote data for the issue
            var votes = await _context.IssueVotes
                .OfType<IssueVote>()
                .Where(v => v.IssueID == issueId)
                .ToListAsync();

            double averageVote = votes.Any() ? votes.Average(v => v.VoteValue) : 0;
            int totalVotes = votes.Count;

            return new UserVote_Issue_ReadVM
            {
                ContentID = issueId,
                AverageVote = averageVote,
                TotalVotes = totalVotes,
                IssueVotes = votes.Select(v => new IssueVote_ReadVM
                {
                    VoteID = v.VoteID,
                    UserID = v.UserID,
                    VoteValue = v.VoteValue,
                    CreatedAt = v.CreatedAt,
                    ModifiedAt = v.ModifiedAt
                }).ToList()
            };
        }

        public async Task<int?> GetActiveUserIssueVote(Guid issueId)
        {
            int? userVote = null;

            // Get current user if authenticated
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);

            if (user != null)
            {
                var existingVote = await _context.IssueVotes
                    .OfType<IssueVote>()
                    .FirstOrDefaultAsync(v => v.UserID == user.Id && v.IssueID == issueId);

                if (existingVote != null)
                {
                    userVote = existingVote.VoteValue;
                }
            }
            return userVote;
        }

    }
}
