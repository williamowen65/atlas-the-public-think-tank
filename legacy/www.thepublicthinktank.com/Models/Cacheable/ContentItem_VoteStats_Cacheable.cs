using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Models.Cacheable
{
    /// <summary>
    /// A generic ViewModel for the reading the vote content (issues/solutions/comments)
    /// </summary>
    public class ContentItem_VoteStats_Cacheable
    {
        public Guid ContentID { get; set; }

        /// <summary>
        /// ContentType is track here to help with examining data, but this property isn't 
        /// used in the DOM. ContentType is available in the DOM, but that content type is from a parent
        /// </summary>
        /// <remarks>
        /// This adds some extra data to the cache, but makes the cache more readable.
        /// </remarks>
        public ContentType ContentType { get; set; }

        //public string UserId { get; set; } // This is provided by ASP.NET Identity
        public int TotalVotes { get; set; } = 0;
        public double AverageVote { get; set; } = 0;
    }
}
