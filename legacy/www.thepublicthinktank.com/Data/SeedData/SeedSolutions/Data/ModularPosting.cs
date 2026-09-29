using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class ModularPosting : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId,
                    Title = "Modular Posting",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 5, 12),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
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
                    ScopeID = new Guid("c65ca915-45d4-4975-929b-9a79199dc51f"),
                    Scales = { Scale.Global },
                    Domains = { Domain.Technological, Domain.Cultural },
                    EntityTypes = { EntityType.Organization },
                    Timeframes = { Timeframe.ShortTerm },
                    Boundaries = { },
                };
            }
        }

        public static Guid ContentId = new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a");
        public static string content =
            "Posts are made of blocks — Problem, Evidence, Opinion, Ask — to improve clarity.\n\n" +
            "Social media platforms should implement 'Modular Posting' as a structured content creation framework " +
            "that breaks posts into distinct, labeled components. This solution would transform the standard " +
            "free-form posting format into a more organized approach that helps both creators and readers " +
            "distinguish between different types of information.\n\n" +

            "Key modules would include:\n\n" +

            "- Problem: A clearly defined issue or question being addressed\n\n" +

            "- Evidence: Factual information, data, or sources supporting claims\n\n" +

            "- Opinion: Clearly marked personal perspectives or interpretations\n\n" +

            "- Ask: Specific calls to action, questions for discussion, or requests\n\n" +

            "Additional features would include:\n\n" +

            "- Visual differentiation: Each module would have distinct styling to make the post structure immediately " +
            "apparent to readers\n\n" +

            "- Flexible ordering: Users could arrange modules in the sequence that best serves their communication goals\n\n" +

            "- Optional modules: Not all posts would require all module types, allowing flexibility while maintaining structure\n\n" +

            "- Advanced filtering: Readers could filter content based on module types (e.g., 'show me posts with evidence')\n\n" +

            "This approach would significantly improve content clarity by helping users distinguish between facts, " +
            "opinions, and requests. It would encourage more thoughtful content creation by prompting users to " +
            "consider different aspects of their communication. For readers, modular posts would enable faster " +
            "comprehension and more effective evaluation of information. This structure would be particularly " +
            "valuable for complex topics where mixing different types of information often leads to misunderstandings " +
            "and unnecessary conflicts.";

        public SolutionVote[] solutionVotes { get; } = {
            new SolutionVote
            {
                VoteID = new Guid("f1e2d3c4-b5a6-47a8-9b0c-1d2e3f4a5b6c"),
                SolutionID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 5, 14)
            },
            new SolutionVote
            {
                VoteID = new Guid("e2d3c4f1-a5b6-47a8-9b0c-1d2e3f4a5b6d"),
                SolutionID = ContentId,
                UserID = SeedUserThree.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 5, 14)
            },
            new SolutionVote
            {
                VoteID = new Guid("d3c4f1e2-b6a5-47a8-9b0c-1d2e3f4a5b6e"),
                SolutionID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 5, 15)
            },
            new SolutionVote
            {
                VoteID = new Guid("c4f1e2d3-a5b6-47a8-9b0c-1d2e3f4a5b6f"),
                SolutionID = ContentId,
                UserID = SeedUserFive.user.Id,
                VoteValue = 6,
                CreatedAt = new DateTime(2024, 5, 15)
            },
            new SolutionVote
            {
                VoteID = new Guid("b5a6c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7a"),
                SolutionID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 5, 16)
            },
            new SolutionVote
            {
                VoteID = new Guid("a6b5c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7b"),
                SolutionID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 5, 16)
            },
            new SolutionVote
            {
                VoteID = new Guid("96b5c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7c"),
                SolutionID = ContentId,
                UserID = SeedUserEight.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 5, 17)
            }
        };
    }
}