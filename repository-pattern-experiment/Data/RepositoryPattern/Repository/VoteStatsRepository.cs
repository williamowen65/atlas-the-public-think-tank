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
                IssueVotes = votes.Select(v => new Vote_ReadVM
                {
                    VoteID = v.VoteID,
                    UserID = v.UserID,
                    VoteValue = v.VoteValue,
                    CreatedAt = v.CreatedAt,
                    ModifiedAt = v.ModifiedAt
                }).ToDictionary(v => v.VoteID)
            };
        }

        /// <summary>
        /// Represents a cacheable content vote entity
        /// </summary>
        /// <param name="solutionId"></param>
        /// <returns></returns>
        public async Task<UserVote_Solution_ReadVM> GetSolutionVoteStats(Guid solutionId)
        {
            // Retrieve vote data for the issue
            var votes = await _context.SolutionVotes
                .OfType<SolutionVote>()
                .Where(v => v.SolutionID == solutionId)
                .ToListAsync();

            double averageVote = votes.Any() ? votes.Average(v => v.VoteValue) : 0;
            int totalVotes = votes.Count;

            return new UserVote_Solution_ReadVM
            {
                ContentID = solutionId,
                AverageVote = averageVote,
                TotalVotes = totalVotes,
                SolutionVotes = votes.Select(v => new Vote_ReadVM
                {
                    VoteID = v.VoteID,
                    UserID = v.UserID,
                    VoteValue = v.VoteValue,
                    CreatedAt = v.CreatedAt,
                    ModifiedAt = v.ModifiedAt
                }).ToDictionary(v => v.VoteID)
            };
        }


    }
}
