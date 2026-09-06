using atlas_the_public_think_tank.Models.Cacheable;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote
{


    public class IssueVotes_Cacheable_ReadVM : ContentItem_VoteStats_Cacheable
    {
        /// <summary>
        /// IssueVotes are stored as a map in memory for ease of update in the cache.
        /// The Guid key is the userId for quick look up of vote by user id
        /// </summary>
        public Dictionary<Guid, Vote_Cacheable> IssueVotes { get; set; } = new Dictionary<Guid, Vote_Cacheable>();
    }

}
