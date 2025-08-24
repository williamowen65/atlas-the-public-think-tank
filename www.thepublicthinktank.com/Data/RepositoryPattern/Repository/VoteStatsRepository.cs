using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.Database;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
{
    public class VoteStatsRepository : IVoteStatsRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public VoteStatsRepository(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider
            )
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }



        /// <summary>
        /// Represents a cacheable content vote entity
        /// </summary>
        /// <param name="issueId"></param>
        /// <remarks>
        /// The votes are keyed by UserID for ease of lookup of a users vote on content
        /// </remarks>
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
                ContentType = ContentType.Issue,
                AverageVote = averageVote,
                TotalVotes = totalVotes,
                IssueVotes = votes.Select(v => new Vote_Cacheable_ReadVM
                {
                    VoteID = v.VoteID,
                    UserID = v.UserID,
                    VoteValue = v.VoteValue,
                    CreatedAt = v.CreatedAt,
                    ModifiedAt = v.ModifiedAt,
                    
                }).ToDictionary(v => v.UserID)
            };
        }

        /// <summary>
        /// Represents a cacheable content vote entity
        /// </summary>
        /// <param name="solutionId"></param>
        /// <remarks>
        /// The votes are keyed by UserID for ease of lookup of a users vote on content
        /// </remarks>
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
                ContentType = ContentType.Solution,
                AverageVote = averageVote,
                TotalVotes = totalVotes,
                SolutionVotes = votes.Select(v => new Vote_Cacheable_ReadVM
                {
                    VoteID = v.VoteID,
                    UserID = v.UserID,
                    VoteValue = v.VoteValue,
                    CreatedAt = v.CreatedAt,
                    ModifiedAt = v.ModifiedAt,

                }).ToDictionary(v => v.UserID)
            };
        }

        public async Task<Vote_Cacheable_ReadVM?> UpsertIssueVote(UserVote_Issue_UpsertVM model, AppUser user)
        {


            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var issueRepository = services.GetRequiredService<IIssueRepository>();

            

                // Check that issue exist
                IssueRepositoryViewModel? issue = await issueRepository.GetIssueById(model.IssueID);

                if (issue == null)
                {
                    throw new Exception($"Issue does not exist by the id: {model.IssueID}");
                }

                // NOTE: There should be a composite/compound key on IssueVotes: UserID + IssueID

                // Check for existing vote from this user on this issue
                IssueVote? existingVote = await _context.IssueVotes
                    .Where(v => v.UserID == user.Id && v.IssueID == model.IssueID)
                    .FirstOrDefaultAsync();

                Vote_Cacheable_ReadVM issueVote;

                if (existingVote != null)
                {
                  
                        // Update existing vote
                        existingVote.VoteValue = model.VoteValue;
                        existingVote.ModifiedAt = DateTime.UtcNow;
                        _context.IssueVotes.Update(existingVote);

                        // Convert to Vote_Cacheable_ReadVM
                        issueVote = new Vote_Cacheable_ReadVM
                        {
                            VoteID = existingVote.VoteID,
                            UserID = existingVote.UserID,
                            VoteValue = existingVote.VoteValue,
                            CreatedAt = existingVote.CreatedAt,
                            ModifiedAt = existingVote.ModifiedAt,
                            
                        };
                    
                }
                else
                {
                    // Create new vote
                    var newVote = new IssueVote
                    {
                        VoteID = Guid.NewGuid(),
                        IssueID = model.IssueID,
                        UserID = user.Id,
                        VoteValue = model.VoteValue,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    };

                    await _context.IssueVotes.AddAsync(newVote);

                    // Convert to Vote_Cacheable_ReadVM
                    issueVote = new Vote_Cacheable_ReadVM
                    {
                        VoteID = newVote.VoteID,
                        UserID = newVote.UserID,
                        VoteValue = newVote.VoteValue,
                        CreatedAt = newVote.CreatedAt,
                        ModifiedAt = newVote.ModifiedAt
                    };
                }

                await _context.SaveChangesAsync();

                return issueVote;
           
        }

        public async Task<Vote_Cacheable_ReadVM?> UpsertSolutionVote(UserVote_Solution_UpsertVM model, AppUser user)
        {

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var solutionRepository = services.GetRequiredService<ISolutionRepository>();



            // Check that issue exist
            SolutionRepositoryViewModel? solution = await solutionRepository.GetSolutionById(model.SolutionID);

            if (solution == null)
            {
                throw new Exception($"Solution does not exist by the id: {model.SolutionID}");
            }

            // NOTE: There should be a composite/compound key on IssueVotes: UserID + IssueID

            // Check for existing vote from this user on this issue
            SolutionVote? existingVote = await _context.SolutionVotes
                .Where(v => v.UserID == user.Id && v.SolutionID == model.SolutionID)
                .FirstOrDefaultAsync();

            Vote_Cacheable_ReadVM solutionVote;

            if (existingVote != null)
            {

                // Update existing vote
                existingVote.VoteValue = model.VoteValue;
                existingVote.ModifiedAt = DateTime.UtcNow;
                _context.SolutionVotes.Update(existingVote);

                // Convert to Vote_Cacheable_ReadVM
                solutionVote = new Vote_Cacheable_ReadVM
                {
                    VoteID = existingVote.VoteID,
                    UserID = existingVote.UserID,
                    VoteValue = existingVote.VoteValue,
                    CreatedAt = existingVote.CreatedAt,
                    ModifiedAt = existingVote.ModifiedAt
                };

            }
            else
            {
                // Create new vote
                var newVote = new SolutionVote
                {
                    VoteID = Guid.NewGuid(),
                    SolutionID = model.SolutionID,
                    UserID = user.Id,
                    VoteValue = model.VoteValue,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _context.SolutionVotes.AddAsync(newVote);

                // Convert to Vote_Cacheable_ReadVM
                solutionVote = new Vote_Cacheable_ReadVM
                {
                    VoteID = newVote.VoteID,
                    UserID = newVote.UserID,
                    VoteValue = newVote.VoteValue,
                    CreatedAt = newVote.CreatedAt,
                    ModifiedAt = newVote.ModifiedAt
                };
            }

            await _context.SaveChangesAsync();

            return solutionVote;
        }
    }
}
