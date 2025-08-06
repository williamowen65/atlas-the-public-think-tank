using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class OceanAcidification : SeedIssueContainer
    {
        public Issue issue {
            get {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Ocean Acidification",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 16),
                    ParentIssueID = ClimateChange.ContentId,
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            } 
        }

        public static Guid ContentId = new Guid("9a7e6cbd-8c1f-4d3f-a07f-3d8c9c2bfc44");

        public string content =
            "Ocean acidification is one of the less visible but most serious consequences of climate change. It occurs " +
            "when the oceans absorb excess carbon dioxide (CO₂) from the atmosphere—a process that has accelerated rapidly " +
            "due to human activities like burning fossil fuels and deforestation. When CO₂ dissolves in seawater, it forms " +
            "carbonic acid, which lowers the ocean's pH. Since the start of the industrial era, the ocean has become " +
            "approximately 30% more acidic, marking a significant shift in its chemistry over a relatively short period.\n\n" +

            "This acidification disrupts marine ecosystems, particularly affecting organisms that rely on calcium carbonate " +
            "to build their shells and skeletons—such as corals, oysters, clams, and some plankton. As the pH drops, it becomes " +
            "harder for these organisms to form and maintain their structures, leading to weaker shells and slower growth. " +
            "Coral reefs, often referred to as the 'rainforests of the sea,' are especially vulnerable. Weakened coral skeletons " +
            "make reef systems more prone to collapse, which threatens the biodiversity they support and the millions of people " +
            "who depend on them for food, tourism, and coastal protection.\n\n" +

            "The effects ripple through the entire food chain. When small shell-forming organisms struggle to survive, larger " +
            "animals that feed on them—like fish and whales—also face risks. This destabilization can lead to a decline in fish " +
            "populations, which directly impacts global food security. Moreover, many communities, especially in island and coastal " +
            "regions, rely heavily on healthy marine ecosystems for their livelihoods. Thus, ocean acidification is not just an " +
            "environmental issue—it’s an economic and humanitarian one as well.";

        public IssueVote[] issueVotes { get; } = new IssueVote[] { };

    }
}
