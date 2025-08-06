using repository_pattern_experiment.Data.SeedData.SeedIssues.Data;
using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedSolutions.Data
{
    public class AnnotationMode : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId,
                    Title = "Annotation Mode",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 4, 18),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0");
        public string content =
            "Users can mark up each other's posts with constructive inline comments.\n\n" +
            "Social media platforms should implement an 'Annotation Mode' that allows users to provide contextual, " +
            "paragraph-specific feedback directly on content. This solution would transform standard commenting from " +
            "a sequential list of reactions into a more nuanced system of collaborative engagement with specific " +
            "parts of posts.\n\n" +

            "Key features would include:\n\n" +

            "- Inline annotation tools: Users could highlight specific text, images, or video segments and attach " +
            "comments directly to those elements\n\n" +

            "- Constructive guidance frameworks: Prompts encouraging specific types of feedback (e.g., asking clarifying " +
            "questions, providing relevant sources, offering alternative perspectives)\n\n" +

            "- Author control settings: Content creators could enable different levels of annotation privileges, from " +
            "open public annotation to limited trusted circles\n\n" +

            "- Quality filtering: Algorithms and community moderation to surface the most constructive annotations while " +
            "minimizing low-quality or antagonistic responses\n\n" +

            "- Contextual view options: Readers could toggle between viewing content with or without annotations, or " +
            "filter by annotation type\n\n" +

            "This approach would transform social media interactions from performative posturing to collaborative " +
            "knowledge building. By focusing on specific parts of content rather than generalized reactions, " +
            "annotations would encourage more thoughtful engagement and reduce misunderstandings. Content creators " +
            "would receive more useful feedback, and readers would benefit from additional context and perspective. " +
            "The annotation layer would serve as a bridge between original content and discussion, creating a more " +
            "interconnected and meaningful discourse environment.";

        public SolutionVote[] solutionVotes { get; } = {
             new SolutionVote
            {
                VoteID = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                SolutionID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 6,
                CreatedAt = new DateTime(2024, 4, 19)
            },
            new SolutionVote
            {
                VoteID = new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"),
                SolutionID = ContentId,
                UserID = SeedUserThree.user.Id,
                VoteValue = 6,
                CreatedAt = new DateTime(2024, 4, 19)
            },
            new SolutionVote
            {
                VoteID = new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"),
                SolutionID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 4,
                CreatedAt = new DateTime(2024, 4, 20)
            },
            new SolutionVote
            {
                VoteID = new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"),
                SolutionID = ContentId,
                UserID = SeedUserFive.user.Id,
                VoteValue = 3,
                CreatedAt = new DateTime(2024, 4, 20)
            },
            new SolutionVote
            {
                VoteID = new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b"),
                SolutionID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 4, 21)
            },
            new SolutionVote
            {
                VoteID = new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c"),
                SolutionID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 5,
                CreatedAt = new DateTime(2024, 4, 21)
            },
            new SolutionVote
            {
                VoteID = new Guid("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d"),
                SolutionID = ContentId,
                UserID = SeedUserEight.user.Id,
                VoteValue = 4,
                CreatedAt = new DateTime(2024, 4, 22)
            },
            new SolutionVote
            {
                VoteID = new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"),
                SolutionID = ContentId,
                UserID = SeedUserNine.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 4, 22)
            },
            new SolutionVote
            {
                VoteID = new Guid("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f"),
                SolutionID = ContentId,
                UserID = SeedUserTen.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 4, 23)
            },
            new SolutionVote
            {
                VoteID = new Guid("d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a"),
                SolutionID = ContentId,
                UserID = SeedUserEleven.user.Id,
                VoteValue = 4,
                CreatedAt = new DateTime(2024, 4, 23)
            },
            new SolutionVote
            {
                VoteID = new Guid("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b"),
                SolutionID = ContentId,
                UserID = SeedUserTwelve.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 4, 24)
            },
            new SolutionVote
            {
                VoteID = new Guid("f2a3b4c5-d6e7-4f8a-9b0c-1d2e3f4a5b6c"),
                SolutionID = ContentId,
                UserID = SeedUserThirteen.user.Id,
                VoteValue = 6,
                CreatedAt = new DateTime(2024, 4, 25)
            }

        };
    }
}