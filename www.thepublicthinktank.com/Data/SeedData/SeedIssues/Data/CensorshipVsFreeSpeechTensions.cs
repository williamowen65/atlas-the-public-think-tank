using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class CensorshipVsFreeSpeechTensions : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Censorship vs. Free Speech Tensions",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 5, 15),
                    AuthorID = SeedUserNine.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId // Making it a sub-issue of the social media platforms issue
                };
            }
        }

        public static Guid ContentId = new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a");
        public string content =
            "Social media platforms face an increasingly difficult balancing act between limiting harmful content and " +
            "preserving freedom of expression. This tension represents one of the most fundamental challenges in digital " +
            "governance today, with significant implications for public discourse, safety, and democracy.\n\n" +

            "On one side, platforms have a responsibility to address genuine harms that can occur in unmoderated spaces—including " +
            "harassment, incitement to violence, exploitation of vulnerable groups, and coordinated disinformation campaigns. " +
            "Research increasingly links unmoderated harmful content to real-world consequences, from psychological damage to " +
            "political violence. Many users expect platforms to provide some level of protection against these harms.\n\n" +

            "On the other side, content moderation decisions inevitably involve subjective judgments about what constitutes " +
            "harmful speech. Critics argue that excessive moderation risks removing legitimate political discourse, artistic " +
            "expression, or marginalized voices. There are valid concerns about corporate entities having broad powers to " +
            "determine acceptable speech, particularly given their global reach across diverse cultural contexts and " +
            "political systems.\n\n" +

            "This dilemma is complicated by several factors: the massive scale of content requiring moderation; the " +
            "limitations of automated systems in understanding context and nuance; the varying cultural and legal standards " +
            "across different countries; and the financial incentives that may influence platform governance decisions. " +
            "Disagreements about appropriate content policies often reflect deeper philosophical differences about the " +
            "relative importance of harm prevention versus expressive freedom.\n\n" +

            "Finding sustainable approaches to this challenge requires grappling with fundamental questions about the " +
            "nature and limits of free expression in digital contexts. Should platforms be treated as public forums with " +
            "minimal restrictions, or as curated spaces with clearer boundaries? How can moderation systems achieve greater " +
            "transparency, consistency, and accountability? And what role should governments, civil society, and users " +
            "themselves play in developing and implementing content governance frameworks that balance competing values?";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a1b2c3d4-5e6f-47a8-9b0c-1d2e3f4a5b6c"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 3,
                    CreatedAt = new DateTime(2024, 5, 16),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b2c3d4e5-6f7a-48b9-0c1d-2e3f4a5b6c7d"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 17),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c3d4e5f6-7a8b-49c0-1d2e-3f4a5b6c7d8e"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 2,
                    CreatedAt = new DateTime(2024, 5, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d4e5f6a7-8b9c-40d1-2e3f-4a5b6c7d8e9f"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e5f6a7b8-9c0d-41e2-3f4a-5b6c7d8e9f0a"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 4,
                    CreatedAt = new DateTime(2024, 5, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f6a7b8c9-0d1e-42f3-4a5b-6c7d8e9f0a1b"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 5, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a7b8c9d0-1e2f-43a4-5b6c-7d8e9f0a1b2c"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 1,
                    CreatedAt = new DateTime(2024, 5, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b8c9d0e1-2f3a-44b5-6c7d-8e9f0a1b2c3d"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c9d0e1f2-3a4b-45c6-7d8e-9f0a1b2c3d4e"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d0e1f2a3-4b5c-46d7-8e9f-0a1b2c3d4e5f"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 2,
                    CreatedAt = new DateTime(2024, 5, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e1f2a3b4-5c6d-47e8-9f0a-1b2c3d4e5f6a"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 6,
                    CreatedAt = new DateTime(2024, 5, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f2a3b4c5-6d7e-48f9-0a1b-2c3d4e5f6a7b"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 3,
                    CreatedAt = new DateTime(2024, 5, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a3b4c5d6-7e8f-49a0-1b2c-3d4e5f6a7b8c"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b4c5d6e7-8f9a-40b1-2c3d-4e5f6a7b8c9d"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 4,
                    CreatedAt = new DateTime(2024, 5, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c5d6e7f8-9a0b-41c2-3d4e-5f6a7b8c9d0e"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 5, 30),
               },
        };
    }
}
