using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class ClimateChange : SeedIssueContainer
    {

        public Issue issue {
            get {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Climate Change Solutions",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 15),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            } 
        }

        public static Guid ContentId = new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc");
        public string content =
            "Climate change is no longer a distant threat—it is a present-day crisis reshaping our planet in real time. " +
            "Driven primarily by the burning of fossil fuels, deforestation, and unsustainable land use, climate change " +
            "is increasing global temperatures, disrupting weather patterns, and accelerating the frequency and intensity " +
            "of natural disasters.\n\n" +

            "The consequences are wide-reaching: rising sea levels threaten coastal communities, prolonged droughts " +
            "endanger food and water supplies, and extreme heat waves place vulnerable populations at serious risk. " +
            "Ecosystems are under immense stress, with species extinction accelerating as habitats are lost or altered " +
            "beyond recovery.\n\n" +

            "At its core, the issue is not just environmental—it is also social, economic, and moral. Climate change " +
            "disproportionately affects those who contribute the least to it: low-income communities, indigenous " +
            "populations, and developing nations often lack the resources to adapt or recover. Without urgent, " +
            "coordinated global action, these inequalities will deepen, and the window to prevent irreversible " +
            "damage will continue to close.\n\n" +

            "To confront this crisis, we must dramatically reduce greenhouse gas emissions, invest in renewable energy, " +
            "protect natural ecosystems, and build resilient infrastructure. The challenge is immense, but so is the " +
            "responsibility—and the opportunity—to shape a livable future for all.";


        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c7d58e3a-9f21-47b6-a12d-8e44bc67f531"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a1b24f8c-3e12-47d6-9e78-5f98c732a641"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b3c45d9e-5f21-48e7-af89-6f07d843b752"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d5e67f0a-7b23-49c8-bd90-7e18f934c863"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a6b78c9d-0e12-4f34-a556-8b90c123d974"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e1f2a3b4-5c67-4d89-ae01-2f34e567f085"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 5,
                    CreatedAt = new DateTime(2024, 1, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c9d0e1f2-3a45-4b67-cd89-0e12f345a196"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 6,
                    CreatedAt = new DateTime(2024, 1, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a6b7c8d9-e012-4f34-a567-8a90b123c207"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d4e5f6a7-b890-4c12-de34-5f67a890b318"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 4,
                    CreatedAt = new DateTime(2024, 1, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c1d2e3f4-a567-4b89-cd01-2e34f567a429"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 3,
                    CreatedAt = new DateTime(2024, 1, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a9a8b7c6-d543-4e21-fa09-8b76c543d530"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e2f3a4b5-c678-4d90-ef12-3a45b678c641"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a7b8c9d0-e123-4a45-bc67-8d90e123f752"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 2,
                    CreatedAt = new DateTime(2024, 1, 30),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a6b7c8d9-e012-4f34-ab56-7a90b123c863"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 1,
                    CreatedAt = new DateTime(2024, 1, 31),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f4a5b6c7-d890-4e12-af34-5a67b890c974"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 6,
                    CreatedAt = new DateTime(2024, 2, 1),
               },
        };
 
    }
}
