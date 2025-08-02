using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class DeliberativeAndCivilDiscourseDesign : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = AmplificationOfPoliticalPolarizationAndExtremism.ContentId,
                    Title = "Encourage Engagement, Not Escalation",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 5, 27),
                    AuthorID = SeedUserEight.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("f7c6b5d9-2a48-47e3-83c1-a5b9d2e7f038");
        public string content =
            "Social media platforms should redesign their interaction systems to prioritize deliberative and civil discourse " +
            "over confrontational exchanges that fuel polarization. By restructuring the fundamental ways users engage with " +
            "political content and each other, platforms can create environments that reward thoughtful engagement rather " +
            "than escalation and outrage.\n\n" +

            "Key elements of this solution include:\n\n" +

            "- Structured discussion formats that encourage thoughtful exchanges: Replace simple comment threads with " +
            "frameworks that prompt users to identify points of agreement before expressing disagreement, articulate " +
            "underlying values, and respond to specific aspects of others' arguments rather than engaging in sweeping dismissals\n\n" +

            "- Expanded interaction options beyond binary reactions: Move beyond like/dislike buttons to include nuanced " +
            "response options such as 'thoughtful point,' 'changed my perspective,' 'well-evidenced,' or 'respectfully " +
            "disagree,' rewarding substance over mere emotional reactions\n\n" +

            "- Cooling-off periods and reflection prompts: Introduce brief delays before publishing responses to heated " +
            "political content, with optional reflection prompts asking users to consider whether their comment advances " +
            "the conversation and how it might be received\n\n" +

            "- Community recognition systems for bridge-building: Develop reputation systems that highlight and reward users " +
            "who consistently engage constructively across political divides, elevating their contributions in discussions\n\n" +

            "- Collaborative features that incentivize finding common ground: Create special formats for issues that encourage " +
            "users from different viewpoints to collaboratively draft statements of shared principles or potential compromises\n\n" +

            "- Friction for escalation patterns: Add increasing levels of friction (time delays, additional prompts) when " +
            "conversation patterns show signs of unproductive escalation, without blocking communication entirely\n\n" +

            "Implementation would require significant user experience research and iterative design, with transparent metrics " +
            "tracking improvements in discourse quality. Platforms could introduce these features in opt-in communities " +
            "initially, gradually expanding as positive outcomes are demonstrated.\n\n" +

            "This approach fundamentally changes incentive structures that currently reward divisiveness. By designing " +
            "interaction systems that make thoughtful engagement easier and more satisfying than performative conflict, " +
            "platforms can foster environments where users experience the genuine intellectual and social rewards of " +
            "constructive political discourse rather than the hollow dopamine hits of tribal combat.";

        public SolutionVote[] solutionVotes { get; } = {
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 28),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 28),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 29),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 29),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 5, 30),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 30),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 31),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 31),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 6, 1),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 6, 1),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 6, 2),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f2a3b4c5-d6e7-4f8a-9b0c-1d2e3f4a5b6c"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 6, 2),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 6, 3),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b4c5d6e7-f8a9-4b0c-1d2e-3f4a5b6c7d8e"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 6, 3),
               }
        };
    }
}