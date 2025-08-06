using repository_pattern_experiment.Data.SeedData.SeedIssues.Data;
using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedSolutions.Data
{
    public class DownRankPersonalAttacksAndPerformativeOutrage : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = DeliberativeAndCivilDiscourseDesign.ContentId,
                    Title = "Down-Rank Personal Attacks and Performative Outrage",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 5, 20),
                    AuthorID = SeedUserFour.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647");
        public string content =
            "Social media platforms should implement content ranking systems that algorithmically de-prioritize personal " +
            "attacks and performative outrage while elevating substantive political discourse. This solution addresses " +
            "a core driver of political polarization: the current algorithmic preference for emotionally charged, " +
            "divisive content over reasoned discussion.\n\n" +

            "The approach would involve several key components:\n\n" +

            "- Natural language processing systems trained to distinguish between substantive political arguments and " +
            "content that primarily consists of character attacks, inflammatory rhetoric, or performative moral outrage\n\n" +

            "- Algorithmic adjustments that reduce the visibility of posts containing high levels of personal attacks " +
            "or outrage-baiting language in feeds and recommendation systems\n\n" +

            "- Corresponding promotion of content that addresses political topics with substantive arguments, evidence, " +
            "and respectful engagement with opposing viewpoints\n\n" +

            "- Transparency metrics showing users the percentage of 'high substance' versus 'high outrage' content in " +
            "their feeds, with optional tools to further adjust these ratios\n\n" +

            "- Regular public reporting on platform-wide trends in discourse quality and the effectiveness of " +
            "ranking interventions\n\n" +

            "Implementation would require careful design to avoid political bias, with regular auditing by diverse " +
            "stakeholders to ensure the system doesn't inadvertently suppress legitimate political speech. Crucially, " +
            "this approach doesn't remove or censor any content—it simply adjusts visibility based on discourse quality " +
            "rather than engagement potential.\n\n" +

            "The benefits would be substantial: reduced amplification of extremist rhetoric, decreased incentives for " +
            "politicians and media outlets to engage in inflammatory messaging, and the creation of social media " +
            "environments more conducive to constructive political discourse. By shifting algorithmic incentives away " +
            "from outrage and toward substance, platforms can help reverse the polarization cycle while still preserving " +
            "a diverse range of political viewpoints.";

        public SolutionVote[] solutionVotes { get; } = {
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f9e2d1c0-4b7a-49f8-b3a5-c6d9e8f7b2a1"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 21),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a0f3e2d1-5c8b-40a9-c4b6-d7e0f9a8c3b2"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 21),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b1a4f3e2-6d9c-41b0-d5c7-e8f1a0b9d4c3"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 22),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c2b5a4f3-7e0d-42c1-e6d8-f9a2b1c0e5d4"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 5, 22),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d3c6b5a4-8f1e-43d2-f7e9-a0b3c2d1f6e5"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 23),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e4d7c6b5-9a2f-44e3-a8f0-b1c4d3e2a7f6"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 23),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f5e8d7c6-0b3a-45f4-b9a1-c2d5e4f3b8a7"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 24),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a6f9e8d7-1c4b-46a5-c0b2-d3e6f5a4c9b8"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 24),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b7a0f9e8-2d5c-47b6-d1c3-e4f7a6b5d0c9"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 5, 25),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c8b1a0f9-3e6d-48c7-e2d4-f5a8b7c6e1d0"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 25),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d9c2b1a0-4f7e-49d8-f3e5-a6b9c8d7f2e1"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 26),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e0d3c2b1-5a8f-40e9-a4f6-b7c0d9e8a3f2"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 26),
               }
        };
    }
}