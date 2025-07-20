using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class SpreadOfMisinformationAndEchoChambers : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Spread of Misinformation and Echo Chambers",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 4, 5),
                    AuthorID = SeedUserFour.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId // Making it a sub-issue of the social media platforms issue
                };
            }
        }

        public static Guid ContentId = new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f");
        public string content =
            "The digital information ecosystem has become increasingly vulnerable to the rapid spread of false or misleading " +
            "content. Social media platforms, by design, can amplify misinformation at unprecedented speeds and scales, reaching " +
            "millions of users before corrections can catch up. This creates a troubling dynamic where falsehoods often travel " +
            "faster and reach wider audiences than verified facts.\n\n" +

            "Simultaneously, personalization algorithms create 'filter bubbles' and 'echo chambers' that limit exposure to " +
            "diverse viewpoints. These systems, designed to maximize engagement by showing users content similar to what " +
            "they've previously interacted with, inadvertently reinforce existing beliefs and minimize contradictory information. " +
            "Users become progressively isolated in information environments that reflect and amplify their existing views, " +
            "making them more susceptible to misleading content that aligns with their preconceptions.\n\n" +

            "The combination of these factors has serious implications for democratic societies. Public discourse increasingly " +
            "operates from divergent factual foundations, making consensus-building and collaborative problem-solving more " +
            "difficult. Trust in institutions, expertise, and shared sources of information continues to erode. And heightened " +
            "polarization driven by separate information realities threatens social cohesion and democratic functioning.\n\n" +

            "Addressing this challenge requires multifaceted approaches involving platform design changes, media literacy " +
            "initiatives, regulatory frameworks, and innovations in content verification. Finding solutions that balance " +
            "free expression with information integrity remains one of the most urgent challenges in our digital media " +
            "environment.";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1d2e3f4a-5b6c-47d8-9e0f-1a2b3c4d5e6f"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 4, 6),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2e3f4a5b-6c7d-48e9-a0b1-2c3d4e5f6a7b"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 4, 7),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3f4a5b6c-7d8e-49fa-b1c2-3d4e5f6a7b8c"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 4, 8),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4a5b6c7d-8e9f-40ab-c2d3-4e5f6a7b8c9d"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 4, 9),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5b6c7d8e-9f0a-41bc-d3e4-5f6a7b8c9d0e"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 4, 10),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6c7d8e9f-0a1b-42cd-e4f5-6a7b8c9d0e1f"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 4, 11),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7d8e9f0a-1b2c-43de-f5a6-7b8c9d0e1f2a"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 4, 12),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8e9f0a1b-2c3d-44ef-a6b7-8c9d0e1f2a3b"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 4, 13),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9f0a1b2c-3d4e-45fa-b7c8-9d0e1f2a3b4c"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 4, 14),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("0a1b2c3d-4e5f-46ab-c8d9-0e1f2a3b4c5d"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 4, 15),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1b2c3d4e-5f6a-47bc-d90e-1f2a3b4c5d6e"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 4, 16),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2c3d4e5f-6a7b-48cd-0e1f-2a3b4c5d6e7f"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 4, 17),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3d4e5f6a-7b8c-49de-1f2a-3b4c5d6e7f8a"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 4, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4e5f6a7b-8c9d-40ef-2a3b-4c5d6e7f8a9b"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 4, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5f6a7b8c-9d0e-41fa-3b4c-5d6e7f8a9b0c"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 4, 20),
               },
        };
    }
}
