using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class ConsumptionTracker : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId,
                    Title = "Consumption Tracker",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 3, 22),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d");
        public string content =
            "Quietly shows users how long they've been passive vs. active contributors.\n\n" +
            "Social media platforms should implement a 'Consumption Tracker' that provides users with insights into their " +
            "engagement patterns. This solution would offer a subtle but impactful way to increase awareness of passive " +
            "consumption versus active participation on the platform.\n\n" +

            "Key features would include:\n\n" +

            "- Time visualization: An unobtrusive dashboard showing the balance between time spent scrolling/viewing versus " +
            "time spent creating content, commenting, or otherwise contributing\n\n" +

            "- Weekly insights: Gentle notifications or summaries that highlight engagement patterns without judgment, " +
            "celebrating contribution milestones\n\n" +

            "- Contribution diversity metrics: Tracking different types of engagement (e.g., original posts, thoughtful " +
            "comments, sharing educational content) to recognize various forms of valuable contribution\n\n" +

            "- Customizable goals: Optional, user-defined targets for desired participation levels that align with personal " +
            "digital wellbeing objectives\n\n" +

            "- Privacy controls: Full user control over tracking data, with options to pause tracking or delete history\n\n" +

            "Unlike addictive engagement metrics, this solution emphasizes quality of engagement rather than quantity. " +
            "By making users aware of their consumption-to-contribution ratio, platforms could encourage more mindful " +
            "usage patterns and foster a community of active participants rather than passive consumers. This approach " +
            "also empowers users to make informed decisions about their digital habits without employing manipulative " +
            "design techniques.";

        public SolutionVote[] solutionVotes { get; } = {
            new SolutionVote
            {
                VoteID = new Guid("5d7c8f3a-2e6b-4c1d-9a3f-0e8d7b5c6a4f"),
                SolutionID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 2,
                CreatedAt = new DateTime(2024, 3, 23)
            },
            new SolutionVote
            {
                VoteID = new Guid("6a3b5c8d-1e7f-4a9b-8c5d-2e4f6a7b8c9d"),
                SolutionID = ContentId,
                UserID = SeedUserThree.user.Id,
                VoteValue = 1,
                CreatedAt = new DateTime(2024, 3, 24)
            },
            new SolutionVote
            {
                VoteID = new Guid("7b4c6d5e-3f2a-1b9c-8d7e-6f5a4b3c2d1e"),
                SolutionID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 3,
                CreatedAt = new DateTime(2024, 3, 24)
            },
            new SolutionVote
            {
                VoteID = new Guid("8c5d6e7f-4a3b-2c1d-9e8f-7a6b5c4d3e2f"),
                SolutionID = ContentId,
                UserID = SeedUserFive.user.Id,
                VoteValue = 5,
                CreatedAt = new DateTime(2024, 3, 25)
            },
            new SolutionVote
            {
                VoteID = new Guid("9d6e7f8a-5b4c-3d2e-0f1a-8b7c6d5e4f3a"),
                SolutionID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 2,
                CreatedAt = new DateTime(2024, 3, 25)
            },
            new SolutionVote
            {
                VoteID = new Guid("0e7f8a9b-6c5d-4e3f-1a2b-9c8d7e6f5a4b"),
                SolutionID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 4,
                CreatedAt = new DateTime(2024, 3, 26)
            },
            new SolutionVote
            {
                VoteID = new Guid("1f8a9b0c-7d6e-5f4a-2b3c-0d9e8f7a6b5c"),
                SolutionID = ContentId,
                UserID = SeedUserEight.user.Id,
                VoteValue = 1,
                CreatedAt = new DateTime(2024, 3, 26)
            },
            new SolutionVote
            {
                VoteID = new Guid("2a9b0c1d-8e7f-6a5b-3c4d-1e0f9a8b7c6d"),
                SolutionID = ContentId,
                UserID = SeedUserNine.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 3, 27)
            },
            new SolutionVote
            {
                VoteID = new Guid("3b0c1d2e-9f8a-7b6c-4d5e-2f1a0b9c8d7e"),
                SolutionID = ContentId,
                UserID = SeedUserTen.user.Id,
                VoteValue = 2,
                CreatedAt = new DateTime(2024, 3, 27)
            },
            new SolutionVote
            {
                VoteID = new Guid("4c1d2e3f-0a9b-8c7d-5e6f-3a2b1c0d9e8f"),
                SolutionID = ContentId,
                UserID = SeedUserEleven.user.Id,
                VoteValue = 3,
                CreatedAt = new DateTime(2024, 3, 28)
            }
        };
    }
}