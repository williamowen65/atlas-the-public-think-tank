using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class AffordableHousing : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Affordable Housing",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 17),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = Homelessness.ContentId // Making this a sub-issue of Homelessness
                };
            }
        }

        public static Guid ContentId = new Guid("a5d9c7e8-3b2f-47a1-9c5e-8f6d4b2a1c3d");
        public string content =
          "A major contributor to homelessness is the chronic shortage of affordable housing, particularly in high-demand urban areas. " +
          "The supply of housing units that are both available and affordable to low-income individuals and families has not kept pace " +
          "with demand, creating intense competition, long waitlists, and rising homelessness rates.\n\n" +

          "Key challenges include:\n\n" +

          "Zoning restrictions and land-use policies that limit multi-family or low-income housing construction.\n\n" +

          "High construction and land costs, especially in urban centers, which discourage affordable development.\n\n" +

          "Reduced federal and state investment in public and subsidized housing over recent decades.\n\n" +

          "Community opposition (NIMBYism) that stalls or blocks housing projects aimed at lower-income populations.\n\n" +

          "Insufficient incentives for private developers to build units affordable to extremely low-income tenants.\n\n" +

          "Without enough affordable housing, shelters and transitional programs are stretched thin, and individuals exiting " +
          "homelessness often have nowhere to go. Solutions may include reforming zoning laws, expanding housing trust funds, " +
          "increasing public-private partnerships, and scaling up supportive housing models.";

        public IssueVote[] issueVotes { get; } = new IssueVote[] { };
    }
}