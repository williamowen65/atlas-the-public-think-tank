using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class AtlasThePublicThinkTank : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId,
                    Title = "Atlas - The Public Think Tank",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 15),
                    AuthorID = SeedUserOne.user.Id, // Using centralized user ID
                    ScopeID = scope.ScopeID,
                };
            }
        }

        public static Guid ContentId = new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19");
        public string content =
            "Atlas: The Public Think Tank represents a paradigm shift in how social media platforms function. While traditional platforms prioritize engagement metrics and advertising revenue, Atlas focuses on collaborative problem-solving and thoughtful discourse.\n\n" +

            "Key innovations include:\n\n" +

            "- Nuanced voting system: Instead of simplistic likes/dislikes, Atlas employs a 0-10 scale that encourages thoughtful evaluation of content quality and relevance\n\n" +

            "- Issue-solution framework: Content is organized around problems and their potential solutions, creating natural context for constructive discussion\n\n" +

            "- Transparency by design: Algorithm settings are fully adjustable by users, giving people control over what they see and why\n\n" +

            "- Community-driven development: The platform itself is treated as an evolving project that users can help improve\n\n" +

            "Atlas addresses many core problems with current social media: the amplification of divisive content, lack of nuance in discussions, and the prioritization of engagement over user wellbeing. By creating a space specifically designed for collaborative thinking and problem-solving, Atlas demonstrates that social platforms can be reimagined to better serve human needs.\n\n" +

            "This solution doesn't just critique existing social media—it offers a concrete alternative that shows how technology can be harnessed to connect people in more meaningful, productive ways.";

        public Scope scope
        {
            get
            {
                return new Scope()
                {
                    ScopeID = new Guid("3119d678-b8b9-4c6f-90f7-e35e5d446dfc"),
                    Scales = { Scale.Global },
                    Domains = { Domain.Technological, Domain.Political }, // Atlas platform
                    EntityTypes = { EntityType.Organization, EntityType.Government },
                    Timeframes = { Timeframe.LongTerm },
                    Boundaries = { },
                };
            }
        }
        public SolutionVote[] solutionVotes { get; } = {
            new SolutionVote
            {
                VoteID = new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                SolutionID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 1, 16)
            },
            new SolutionVote
            {
                VoteID = new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
                SolutionID = ContentId,
                UserID = SeedUserThree.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 16)
            },
            new SolutionVote
            {
                VoteID = new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"),
                SolutionID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 17)
            },
            new SolutionVote
            {
                VoteID = new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"),
                SolutionID = ContentId,
                UserID = SeedUserFive.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 17)
            },
            new SolutionVote
            {
                VoteID = new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"),
                SolutionID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 18)
            },
            new SolutionVote
            {
                VoteID = new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"),
                SolutionID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 18)
            },
            new SolutionVote
            {
                VoteID = new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"),
                SolutionID = ContentId,
                UserID = SeedUserEight.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 19)
            },
            new SolutionVote
            {
                VoteID = new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"),
                SolutionID = ContentId,
                UserID = SeedUserNine.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 19)
            },
            new SolutionVote
            {
                VoteID = new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"),
                SolutionID = ContentId,
                UserID = SeedUserTen.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 20)
            },
            new SolutionVote
            {
                VoteID = new Guid("abc12345-de67-89f0-1234-56789abcdef0"),
                SolutionID = ContentId,
                UserID = SeedUserEleven.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 1, 21)
            },
            new SolutionVote
            {
                VoteID = new Guid("bcd23456-ef78-90a1-2345-6789abcdef01"),
                SolutionID = ContentId,
                UserID = SeedUserTwelve.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 1, 21)
            },
            new SolutionVote
            {
                VoteID = new Guid("cde34567-f890-a1b2-3456-789abcdef012"),
                SolutionID = ContentId,
                UserID = SeedUserThirteen.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 22)
            },
            new SolutionVote
            {
                VoteID = new Guid("def45678-90a1-b2c3-4567-89abcdef0123"),
                SolutionID = ContentId,
                UserID = SeedUserFourteen.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 1, 22)
            },
            new SolutionVote
            {
                VoteID = new Guid("ef056789-1ab2-c3d4-5678-9abcdef01234"),
                SolutionID = ContentId,
                UserID = SeedUserFifteen.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 1, 23)
            },
            new SolutionVote
            {
                VoteID = new Guid("f0167890-2bc3-d4e5-6789-abcdef012345"),
                SolutionID = ContentId,
                UserID = SeedUserSixteen.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 1, 23)
            },
            new SolutionVote
            {
                VoteID = new Guid("01278901-3cd4-e5f6-789a-bcdef0123456"),
                SolutionID = ContentId,
                UserID = SeedUserOne.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 1, 24)
            },
            new SolutionVote
            {
                VoteID = new Guid("12389012-4de5-f607-89ab-cdef01234567"),
                SolutionID = ContentId,
                UserID = SeedUserSeventeen.user.Id,
                VoteValue = 5,
                CreatedAt = new DateTime(2024, 1, 24)
            },
            new SolutionVote
            {
                VoteID = new Guid("23490123-5ef6-0789-abcd-ef012345678a"),
                SolutionID = ContentId,
                UserID = SeedUserEighteen.user.Id,
                VoteValue = 5,
                CreatedAt = new DateTime(2024, 1, 25)
            },
            new SolutionVote
            {
                VoteID = new Guid("34501234-6f07-89ab-cdef-0123456789ab"),
                SolutionID = ContentId,
                UserID = SeedUserNineteen.user.Id,
                VoteValue = 5,
                CreatedAt = new DateTime(2024, 1, 25)
            },
            new SolutionVote
            {
                VoteID = new Guid("45612345-7a89-bcde-f012-3456789abcde"),
                SolutionID = ContentId,
                UserID = SeedUserTwenty.user.Id,
                VoteValue = 5,
                CreatedAt = new DateTime(2024, 1, 26)
            }
        };
    }
}
