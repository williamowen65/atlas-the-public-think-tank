using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class AdDrivenModelsIncentivizingOutrageAndEngagementAtAllCosts : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Ad-Driven Models Incentivizing Outrage and Engagement At All Costs",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 3, 10),
                    AuthorID = SeedUserFive.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId // Making it a sub-issue of the social media platforms issue
                };
            }
        }

        public static Guid ContentId = new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1");
        public string content =
            "Many major social media platforms operate on business models that prioritize user engagement and attention as the " +
            "primary metrics for success. These models rely on advertising revenue, which increases when users spend more time " +
            "on the platform and engage more frequently with content. This creates a fundamental misalignment between what's " +
            "profitable for platforms and what's healthy for individuals and society.\n\n" +

            "Research consistently shows that emotionally charged content—particularly material that triggers outrage, fear, " +
            "or divisiveness—generates significantly more engagement than neutral or positive content. Algorithms designed to " +
            "maximize engagement therefore tend to amplify the most provocative and polarizing voices, regardless of accuracy " +
            "or social value. This creates feedback loops where content creators are incentivized to produce increasingly " +
            "extreme material to maintain visibility.\n\n" +

            "The consequences of this system are far-reaching. Public discourse becomes dominated by the most inflammatory " +
            "perspectives rather than the most thoughtful ones. Complex issues are reduced to simplified, antagonistic narratives. " +
            "Users are pushed toward increasingly radical content through recommendation systems. And social cohesion suffers " +
            "as different groups are exposed to dramatically different information environments tailored to reinforce their " +
            "existing views.\n\n" +

            "Some argue that these outcomes aren't bugs but features of a system working exactly as designed—to capture and " +
            "monetize human attention regardless of the social cost. Addressing this issue requires fundamentally reimagining " +
            "the economic incentives that drive platform design, potentially through regulation, alternative business models, " +
            "or both. Without such changes, platforms may continue optimizing for engagement metrics that fail to account for " +
            "human and social well-being.";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2d3e4f5a-6b7c-48d9-9e0f-1a2b3c4d5e6f"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 11),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3e4f5a6b-7c8d-49e0-a1b2-3c4d5e6f7a8b"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 12),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4f5a6b7c-8d9e-40f1-b2c3-4d5e6f7a8b9c"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 3, 13),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5a6b7c8d-9e0f-41a2-c3d4-5e6f7a8b9c0d"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 14),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6b7c8d9e-0f1a-42b3-d4e5-6f7a8b9c0d1e"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 15),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7c8d9e0f-1a2b-43c4-e5f6-7a8b9c0d1e2f"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 16),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8d9e0f1a-2b3c-44d5-f6a7-8b9c0d1e2f3a"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 3, 17),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9e0f1a2b-3c4d-45e6-a7b8-9c0d1e2f3a4b"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("0f1a2b3c-4d5e-46f7-b8c9-0d1e2f3a4b5c"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 3, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1a2b3c4d-5e6f-47a8-c9d0-1e2f3a4b5c6d"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2b3c4d5e-6f7a-48b9-d0e1-2f3a4b5c6d7e"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3c4d5e6f-7a8b-49c0-e1f2-3a4b5c6d7e8f"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 3, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4d5e6f7a-8b9c-40d1-f2a3-4b5c6d7e8f9a"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5e6f7a8b-9c0d-41e2-a3b4-5c6d7e8f9a0b"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 3, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6f7a8b9c-0d1e-42f3-b4c5-6d7e8f9a0b1c"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 25),
               },
        };
    }
}
