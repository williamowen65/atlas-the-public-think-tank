 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Models
{
    public class ContentIndexEntry
    {
        public Guid ContentId { get; set; }
        public ContentType ContentType { get; set; }

        // The below properties are filterable properties.
        // They can all be optional and depend on the specific query

        public double AverageVote { get; set; }
        public int TotalVotes { get; set; }
        public DateTime CreatedAt { get; set; }


        /// <summary>
        /// WeightedScore: score = (averageVote * totalVotes) / (totalVotes + k)
        /// </summary>
        /// <remarks>
        /// k = tuning parameter (how much you penalize low-vote content) <br/>
        /// Content with a low vote gets discounted more <br/>
        /// </remarks>
        public double WeightedScore { get; set; }

        /// <summary>
        /// score = (v / (v + m)) * R + (m / (v + m)) * C
        /// </summary>
        /// <remarks>
        /// R = average vote for the item <br/>
        /// V = number of votes for the item <br/>
        /// m = minimum votes required for credibility (eg, 10, 20) <br/>
        /// C = mean vote across all content <br/>
        /// </remarks>
        public double BayesianAverage { get; set; }


    }

    public class ContentIdentifier
    {
        public Guid Id { get; set; }
        public ContentType Type { get; set; }
    }


    public class ApplicationInsightsSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public bool EnableSendBeacon { get; set; } = true;
    }

  
}
