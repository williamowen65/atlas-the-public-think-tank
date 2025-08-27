using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class MoodBubbles : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId,
                    Title = "Mood Bubbles",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 3, 5),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("e7d9c6b2-3a5f-4182-9e08-51bd72af46c3");
        public string content =
            "Let users filter feed by emotional tone — e.g., calm, curious, motivated.\n\n" +
            "Social media platforms should implement a 'Mood Bubbles' feature that allows users to curate their content " +
            "experience based on desired emotional states. Unlike traditional content filters that focus on topics or " +
            "sources, this solution would categorize content by its likely emotional impact on viewers.\n\n" +

            "Key features would include:\n\n" +

            "- Emotional tone filtering: Users could select from emotional states like 'calm', 'curious', 'motivated', " +
            "'inspired', 'joyful', or 'reflective' to match their current needs or desired mood\n\n" +

            "- AI-powered content analysis: Content would be analyzed for emotional tone using natural language processing " +
            "and image recognition to identify its likely emotional impact\n\n" +

            "- Personalized calibration: The system would learn individual user responses to content over time, " +
            "recognizing that different people may react differently to the same content\n\n" +

            "- Mood scheduling: Users could set different emotional preferences for different times of day " +
            "(e.g., 'motivated' in the morning, 'calm' in the evening)\n\n" +

            "This approach would transform social media from a source of unpredictable emotional stimuli to a tool " +
            "for intentional emotional well-being. It acknowledges that content consumption affects mental state and " +
            "gives users agency in managing this impact. Platforms adopting this feature would position themselves as " +
            "leaders in supporting digital wellness and emotional intelligence.";

        public SolutionVote[] solutionVotes { get; } = new SolutionVote[] {
            new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("2f8a7d5e-4b9c-48e1-a036-7c53f4e8d12b"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 6,
                    CreatedAt = new DateTime(2024, 1, 18),
               },
        };
    }
}