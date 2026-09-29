using atlas_the_public_think_tank.Models.Cacheable;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common.ContentItemVote
{
  
    public class ContentItemVotes_ReadVM : ContentItem_VoteStats_Cacheable
    {
        /// <summary>
        /// IssueVotes are stored as a map in memory for ease of update in the cache.
        /// </summary>
        public Dictionary<Guid, Vote_Cacheable> GenericContentVotes { get; set; } = new Dictionary<Guid, Vote_Cacheable>();
    }
}
