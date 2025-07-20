using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class CanSocialMediaPlatformsBeBetter : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Can Social Media Platforms Be Better?",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 18),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e");
        public string content =
            "As these platforms become integral to how people connect, communicate, and access information, many challenges " +
            "persist that raise critical questions. How can social media companies improve transparency around their content " +
            "moderation policies to ensure fairness and consistency? Are their algorithms designed in ways that prioritize " +
            "user well-being over engagement and profit?\n\n" +

            "What responsibilities do social media sites have in combating misinformation, hate speech, and harmful content " +
            "without infringing on free expression? How can they better protect user privacy and data security amid growing " +
            "concerns over surveillance and misuse?\n\n" +

            "Moreover, how might social media platforms address the mental health impacts linked to prolonged use, especially " +
            "among young and vulnerable populations? And importantly, how can they create safer, more inclusive online " +
            "communities where harassment and abuse are minimized?\n\n" +

            "These questions point to deep systemic issues in the design, governance, and business models of social media " +
            "platforms. Addressing them is essential for building digital spaces that truly support healthy public discourse, " +
            "individual rights, and social cohesion.";



        public IssueVote[] issueVotes { get; } = {
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c7d58e3a-9f21-47b6-a12d-8e94bc67f531"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 18), // Use a fixed date instead of DateTime.Now
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d8e94b67-f531-42a5-b38c-1a9d7e58e32f"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 18),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e9f05c78-a642-43b6-c49d-2b0e8f69f43a"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 19),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f0a16d89-b753-44c7-d50e-3c1f9a70a54b"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 19),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a1b27e90-c864-45d8-e61f-4d2a0b81b65c"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 20),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b2c38f01-d975-46e9-f72a-5e3b1c92c76d"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 20),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c3d49a12-e086-47fa-a83b-6f4c2d03d87e"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 21),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d4e50b23-f197-48ab-b94c-7a5d3e14e98f"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 6,
                    CreatedAt = new DateTime(2024, 1, 21),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e5f61c34-a208-49bc-ca5d-8b6e4f25f09a"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 22),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f6a72d45-b319-40cd-db6e-9c7f5a36a10b"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 22),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a7b83e56-c420-41de-ec7f-0d8a6b47b21c"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 23),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b8c94f67-d531-42ef-fd8a-1e9b7c58c32d"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 23),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c9d05a78-e642-43fa-ae9b-2f0c8d69d43e"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 24),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d0e16b89-f753-44ab-bf0c-3a1d9e70e54f"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 24),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e1f27c90-a864-45bc-ca1d-4b2e0f81f65a"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 25),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f2a38d01-b975-46cd-db2e-5c3f1a92a76b"),
                    UserID = SeedUserSixteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 25),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a3b49e12-c086-47de-ec3f-6d4a2b03b87c"),
                    UserID = SeedUserSeventeen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 26),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b4c50f23-d197-48ef-fd4a-7e5b3c14c98d"),
                    UserID = SeedUserEighteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 26),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c5d61a34-e208-49fa-ae5b-8f6c4d25d09e"),
                    UserID = SeedUserNineteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 27),
               },
              new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d6e72b45-f319-40ab-bf6c-9a7d5e36e10f"),
                    UserID = SeedUserTwenty.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 27),
               },
        };
    
    }


}