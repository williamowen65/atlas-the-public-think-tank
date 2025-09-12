using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class RoleplayThreads : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId,
                    Title = "Roleplay Threads",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 4, 5),
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
                    ScopeID = new Guid("26524034-13d3-4e9c-a361-ddf01271a71f"),
                    Scales = { Scale.Global },
                    Domains = { Domain.Cultural, Domain.Technological },
                    EntityTypes = { EntityType.Person, EntityType.Organization },
                    Timeframes = { Timeframe.ShortTerm },
                    Boundaries = { },
                };
            }
        }

        public static Guid ContentId = new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f");
        public string content =
            "People post as personas to explore ideas, not identities.\n\n" +
            "Social media platforms should implement 'Roleplay Threads' as dedicated spaces where users can temporarily " +
            "adopt different perspectives through clearly marked personas. This solution would create safe environments " +
            "for exploring diverse viewpoints without the social consequences of permanently associating those views with " +
            "one's personal identity.\n\n" +

            "Key features would include:\n\n" +

            "- Transparent persona creation: Users could create temporary personas with clear labels indicating they're " +
            "roleplay identities, not authentic personal accounts\n\n" +

            "- Perspective-based discussions: Threads would be organized around specific topics where users explicitly " +
            "adopt different philosophical, professional, or cultural perspectives\n\n" +

            "- Guided facilitation tools: Prompts and frameworks to help users explore ideas fairly and thoroughly from " +
            "multiple angles\n\n" +

            "- Civil discourse enforcement: Special moderation rules designed specifically for roleplay spaces to maintain " +
            "respectful exploration while allowing challenging conversations\n\n" +

            "- Knowledge-building focus: Emphasis on collaborative learning rather than performance or personal branding\n\n" +

            "This approach would transform social media from a place of rigid identity performance to a laboratory for " +
            "intellectual exploration and empathy development. By separating ideas from identities, platforms could foster " +
            "more nuanced conversations about complex topics without the polarization that occurs when views become tied to " +
            "personal brands. Roleplay Threads would provide a structured environment for users to practice perspective-taking " +
            "and develop a deeper understanding of different worldviews.";

        public SolutionVote[] solutionVotes { get; } = {
            new SolutionVote
            {
                VoteID = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-1e2f3a4b5c6d"),
                SolutionID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 4, 7)
            },
            new SolutionVote
            {
                VoteID = new Guid("b2c3d4e5-f6a7-4b8c-9d0e-2f3a4b5c6d7e"),
                SolutionID = ContentId,
                UserID = SeedUserThree.user.Id,
                VoteValue = 5,
                CreatedAt = new DateTime(2024, 4, 7)
            },
            new SolutionVote
            {
                VoteID = new Guid("c3d4e5f6-a7b8-4c9d-0e1f-3a4b5c6d7e8f"),
                SolutionID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 4, 8)
            },
            new SolutionVote
            {
                VoteID = new Guid("d4e5f6a7-b8c9-4d0e-1f2a-4b5c6d7e8f9a"),
                SolutionID = ContentId,
                UserID = SeedUserFive.user.Id,
                VoteValue = 4,
                CreatedAt = new DateTime(2024, 4, 9)
            },
            new SolutionVote
            {
                VoteID = new Guid("e5f6a7b8-c9d0-4e1f-2a3b-5c6d7e8f9a0b"),
                SolutionID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 4, 10)
            },
            new SolutionVote
            {
                VoteID = new Guid("f6a7b8c9-d0e1-4f2a-3b4c-6d7e8f9a0b1c"),
                SolutionID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 6,
                CreatedAt = new DateTime(2024, 4, 10)
            },
            new SolutionVote
            {
                VoteID = new Guid("a7b8c9d0-e1f2-4a3b-5c6d-7e8f9a0b1c2d"),
                SolutionID = ContentId,
                UserID = SeedUserEight.user.Id,
                VoteValue = 3,
                CreatedAt = new DateTime(2024, 4, 11)
            },
            new SolutionVote
            {
                VoteID = new Guid("b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e"),
                SolutionID = ContentId,
                UserID = SeedUserNine.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 4, 12)
            }
        };
    }
}