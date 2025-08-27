using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class TransparentAlgorithmSettings : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId,
                    Title = "Transparent Algorithm Settings",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 2, 10),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("a2c7d49e-5f38-41b6-9e76-8c429d5b1f83");
        public string content =
            "Let users toggle what affects their feed: recency, variety, depth, etc.\n\n" +
            "Social media platforms should implement transparent algorithm settings that allow users to have direct control " +
            "over what content appears in their feeds. This solution would enable individuals to customize their experience " +
            "based on personal preferences and values, rather than being subject to black-box algorithms optimized solely for " +
            "engagement metrics.\n\n" +

            "Key features would include:\n\n" +

            "- Adjustable content preferences: Users could set sliders for content recency vs. relevance, content diversity, " +
            "topic depth, and the balance between content from connections versus broader sources\n\n" +

            "- Explicit content filters: Clear options to filter sensitive topics, controversial content, or specific categories " +
            "based on personal comfort levels\n\n" +

            "- Algorithm transparency documentation: Plain-language explanations of how the algorithm works and what factors " +
            "influence content selection\n\n" +

            "- Usage insights: Data visualizations showing users how their content consumption patterns affect what they see\n\n" +

            "This approach would return agency to users, reduce algorithm-driven echo chambers, and build trust through " +
            "transparency. Platforms implementing such settings would differentiate themselves as more ethical and user-centric, " +
            "potentially attracting individuals concerned about digital wellbeing and algorithmic manipulation.";

        public SolutionVote[] solutionVotes { get; } = new SolutionVote[] { };
    }
}