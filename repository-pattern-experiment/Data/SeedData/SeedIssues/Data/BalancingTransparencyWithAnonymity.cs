using System;
using System.Collections.Generic;
using repository_pattern_experiment.Data.SeedData.SeedSolutions.Data;
using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;
using static repository_pattern_experiment.Data.SeedData.SeedIds;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class BalancingTransparencyWithAnonymity : SeedIssueContainer
    {
        public static Guid ContentId = new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0");

        public string content =
            "Should users be able to post anonymously or pseudonymously? How does that affect accountability and trust?\n\n" +

            "The question of identity and attribution on collaborative platforms presents a fundamental tension between competing values. On one hand, anonymity and pseudonymity can enable participation from vulnerable populations, protect against retaliation, and allow ideas to be evaluated on their merits rather than their source. On the other hand, these practices can reduce accountability, enable harassment, and potentially undermine trust in the system.\n\n" +

            "For a platform like Atlas that aims to foster collective problem-solving, navigating this tension is particularly important. The credibility of solutions may depend on transparent expertise, while the diversity of perspectives may require protecting contributors' identities in some contexts.\n\n" +

            "Key questions include:\n\n" +

            "- What granular options between full identification and complete anonymity might provide appropriate balance for different contexts?\n\n" +

            "- How can reputation systems function effectively when identities may be fluid or concealed?\n\n" +

            "- What verification mechanisms might establish credibility without requiring full identity disclosure?\n\n" +

            "- How can platforms prevent abuse of anonymity while preserving its benefits for legitimate uses?\n\n" +

            "- What community norms and technical systems can establish trust in contributions despite potential identity concealment?\n\n" +

            "- How might different types of content or actions require different levels of identity verification?\n\n" +

            "Balancing these considerations requires thoughtful design that respects both the values of transparency and the legitimate needs for privacy and protection in online discourse.";

        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Balancing Transparency with Anonymity",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 8, 25),
                    AuthorID = SeedUserTen.user.Id, // Using centralized user ID
                    ScopeID = Scopes.Global, // Using centralized scope ID
                    ParentSolutionID = AtlasThePublicThinkTank.ContentId // Making this a sub-issue of Atlas solution
                };
            }
        }

        public IssueVote[] issueVotes { get; } = {
            new IssueVote
            {
                VoteID = new Guid("d1c0b9a8-7f6e-4352-91a0-c8d7b6a5f4e3"),
                IssueID = ContentId,
                UserID = SeedUserOne.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 8, 26)
            },
            new IssueVote
            {
                VoteID = new Guid("e2d1c0b9-8a7f-4463-a2b1-d9e8c7f6a5b4"),
                IssueID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 26)
            },
            new IssueVote
            {
                VoteID = new Guid("f3e2d1c0-9b8a-4574-b3c2-e0f9d8a7b6c5"),
                IssueID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 8, 27)
            },
            new IssueVote
            {
                VoteID = new Guid("a4f3e2d1-c0b9-4685-c4d3-f1a0e9b8c7d6"),
                IssueID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 8, 27)
            },
            new IssueVote
            {
                VoteID = new Guid("b5a4f3e2-d1c0-4796-d5e4-a2b1f0c9d8e7"),
                IssueID = ContentId,
                UserID = SeedUserEight.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 8, 28)
            },
            new IssueVote
            {
                VoteID = new Guid("c6b5a4f3-e2d1-4807-e6f5-b3c2a1d0e9f8"),
                IssueID = ContentId,
                UserID = SeedUserEleven.user.Id,
                VoteValue = 6,
                CreatedAt = new DateTime(2024, 8, 28)
            },
            new IssueVote
            {
                VoteID = new Guid("d7c6b5a4-f3e2-4918-f7a6-c4d3b2e1f0a9"),
                IssueID = ContentId,
                UserID = SeedUserThirteen.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 29)
            },
            new IssueVote
            {
                VoteID = new Guid("e8d7c6b5-a4f3-4029-a8b7-d5e4c3f2a1b0"),
                IssueID = ContentId,
                UserID = SeedUserFifteen.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 8, 29)
            }
        };
    }
}