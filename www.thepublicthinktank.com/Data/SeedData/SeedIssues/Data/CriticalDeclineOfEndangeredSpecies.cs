using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class CriticalDeclineOfEndangeredSpecies : SeedIssueContainer
    {
        public Issue issue {
            get {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Critical Decline of Endangered Species",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 16),
                    ParentIssueID = ClimateChange.ContentId,
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            } 
        }

        public static Guid ContentId = new Guid("e8f2a395-c16d-48c1-b31c-d7c5a622b2f5");
        public string content =
            "Across the globe, we are witnessing a dramatic and accelerating decline in biodiversity. Endangered " +
            "species—animals, plants, and entire ecosystems—are disappearing at an alarming rate due to human " +
            "activities such as habitat destruction, pollution, climate change, poaching, and the introduction " +
            "of invasive species.\n\n" +

            "This crisis is not just about losing individual species—it is about the collapse of entire ecological " +
            "networks. When keystone species vanish, food chains unravel, pollination fails, water systems destabilize, " +
            "and the natural balance that supports life on Earth begins to erode. The loss of biodiversity undermines " +
            "the health of ecosystems we all depend on—for clean air, fertile soil, stable climate, and even medical " +
            "breakthroughs.\n\n" +

            "The issue is urgent and deeply systemic. Current extinction rates are estimated to be 1,000 times higher " +
            "than the natural background rate, a pace not seen since the last mass extinction event. Yet, many species " +
            "are disappearing silently, without ever being studied or even discovered.\n\n" +

            "Without immediate and sustained global action, we risk not only irreversible ecological damage but also " +
            "profound consequences for human survival. Protecting endangered species means preserving the interconnected " +
            "web of life. It demands stronger conservation laws, habitat restoration, indigenous land stewardship, and " +
            "a commitment to shifting our relationship with nature—from exploitation to stewardship.";
        public IssueVote[] issueVotes { get; } = new IssueVote[] { };
    }

}
