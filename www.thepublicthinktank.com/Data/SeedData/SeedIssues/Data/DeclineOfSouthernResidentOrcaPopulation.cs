using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class DeclineOfSouthernResidentOrcaPopulation : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Decline of Southern Resident Orca Population",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 18),
                    ParentIssueID = CriticalDeclineOfEndangeredSpecies.ContentId, // Make this a sub-issue
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("d6c9a5b4-3e2f-47d1-8c7a-5b9e6f4d3c2a");
        public string content =
            "The Southern Resident orca population—a distinct and culturally significant group of killer whales " +
            "in the Pacific Northwest—is in critical decline. Once a thriving community, their numbers have dropped " +
            "to dangerously low levels, with fewer than 75 individuals remaining. This decline signals more than " +
            "just the loss of a species—it reflects a broader ecological crisis in the Salish Sea and surrounding " +
            "marine environments.\n\n" +

            "The core issue is multifaceted. Southern Resident orcas face severe food shortages, particularly a " +
            "decline in Chinook salmon—their primary prey—due to overfishing, damming of rivers, and habitat " +
            "degradation. They are also threatened by increasing underwater noise from commercial vessels, which " +
            "interferes with their ability to communicate and hunt. Additionally, toxic pollutants accumulate in " +
            "their bodies, compromising their immune and reproductive systems over time.\n\n" +

            "This is not a natural decline—it is a direct result of human impact. Without urgent and coordinated " +
            "intervention, this unique and deeply intelligent population risks extinction within our lifetime. " +
            "The loss would not only be ecological but cultural, especially for Indigenous communities who view " +
            "the orcas as relatives and symbols of environmental stewardship.\n\n" +

            "Saving the Southern Residents requires bold action: restoring salmon habitats, reducing vessel noise, " +
            "regulating pollution, and rethinking regional development. Their survival is a test of our willingness " +
            "to protect vulnerable ecosystems and to act before it's too late.";

        public IssueVote[] issueVotes { get; } = new IssueVote[] { };
    }
}