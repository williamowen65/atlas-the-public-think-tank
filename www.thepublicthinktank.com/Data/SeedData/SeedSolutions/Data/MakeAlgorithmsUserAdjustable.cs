using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class MakeAlgorithmsUserAdjustable : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = AmplificationOfPoliticalPolarizationAndExtremism.ContentId,
                    Title = "Make Algorithms User-Adjustable",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 5, 15),
                    AuthorID = SeedUserThree.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e");
        public string content =
            "Social media platforms should empower users with direct control over the algorithms that determine what content " +
            "they see, specifically designed to mitigate political polarization and exposure to extremist content. This " +
            "solution puts decision-making power back in users' hands rather than defaulting to engagement-maximizing " +
            "algorithms that often amplify divisive content.\n\n" +

            "The key feature would be a transparent, user-friendly control panel offering adjustable settings including:\n\n" +

            "- Political diversity sliders: Users could set preferences for seeing content across the political spectrum " +
            "rather than only views that align with their existing positions\n\n" +

            "- Content variety controls: Options to balance news sources, opinion pieces, and user discussions from " +
            "different perspectives\n\n" +

            "- Fact-checking intensity: Adjustable settings for how prominently fact-checking information appears alongside " +
            "political content\n\n" +

            "- Source credibility thresholds: Ability to set minimum credibility standards for news sources in one's feed\n\n" +

            "- Tone preferences: Options to prioritize measured, substantive political discussions over inflammatory rhetoric\n\n" +

            "- Contextual depth settings: Controls for showing more in-depth background on complex political issues rather " +
            "than simplified, polarizing summaries\n\n" +

            "These controls would be accompanied by periodic feedback showing users metrics about their content diet, " +
            "such as political diversity scores, emotional tone analysis, and source variety statistics. Optional " +
            "recommendations could suggest small adjustments to experience more balanced political discourse.\n\n" +

            "Implementation would include educational onboarding to help users understand how their choices affect " +
            "their information ecosystem, default settings designed for balanced exposure, and continuous refinement " +
            "based on research about what settings most effectively reduce polarization while maintaining user satisfaction.\n\n" +

            "By transferring algorithm control from platform to user, this solution directly addresses the systemic " +
            "incentives that currently reward divisive content. It preserves free expression while creating pathways " +
            "for users to intentionally construct healthier information environments that promote understanding across " +
            "political divides rather than deepening them.";

        public SolutionVote[] solutionVotes { get; } = {
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e8f9a0b7-6c3d-48e4-a2b9-f5d8c6e3a1b0"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 16),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f9a0b1c8-7d4e-49f5-b3c0-a6e9d7f4b2c1"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 16),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a0b1c2d9-8e5f-40a6-c4d1-b7f0e8a5d3c2"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 17),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b1c2d3e0-9f6a-41b7-d5e2-c8a1f9b6e4d3"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 5, 17),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c2d3e4f1-0a7b-42c8-e6f3-d9b2a0c7f5e4"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 18),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d3e4f5a2-1b8c-43d9-f7a4-e0c3b1d8a6f5"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 18),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e4f5a6b3-2c9d-44e0-a8b5-f1d4c2e9b7a6"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 19),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f5a6b7c4-3d0e-45f1-b9c6-a2e5d3f0c8b7"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 19),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a6b7c8d5-4e1f-46a2-c0d7-b3f6e4a1d9c8"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 5, 20),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b7c8d9e6-5f2a-47b3-d1e8-c4a7f5b2e0d9"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 20),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c8d9e0f7-6a3b-48c4-e2f9-d5b8a6c3f1e0"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 21),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d9e0f1a8-7b4c-49d5-f3a0-e6c9b7d4a2f1"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 21),
               }
        };
    }
}