using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

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
                    ScopeID = scope.ScopeID
                };
            }
        }

        public Scope scope
        {
            get
            {
                return new Scope()
                {
                    ScopeID = new Guid("fa86f462-e7ed-47b4-8c86-c6cc5bfc02e8"),
                    Scales = { Scale.Global },
                    Domains = { Domain.Political, Domain.Cultural }, // Civil discourse design
                    EntityTypes = { EntityType.Person, EntityType.Organization },
                    Timeframes = { Timeframe.ShortTerm, Timeframe.LongTerm },
                    Boundaries = { },
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
               
        };
    }
}