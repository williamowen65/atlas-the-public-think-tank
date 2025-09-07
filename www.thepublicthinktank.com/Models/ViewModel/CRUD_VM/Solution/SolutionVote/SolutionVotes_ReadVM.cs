using atlas_the_public_think_tank.Models.Cacheable;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote
{

    public class SolutionVotes_ReadVM : ContentItem_VoteStats_Cacheable
    {
        /// <summary>
        /// IssueVotes are stored as a map in memory for ease of update in the cache.
        /// </summary>
        public Dictionary<Guid, Vote_Cacheable> SolutionVotes { get; set; } = new Dictionary<Guid, Vote_Cacheable>();
    }

}
