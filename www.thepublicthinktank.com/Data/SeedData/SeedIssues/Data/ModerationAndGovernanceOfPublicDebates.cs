using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;
using System;
using System.Collections.Generic;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class ModerationAndGovernanceOfPublicDebates : SeedIssueContainer
    {
        public static Guid ContentId = new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e");

        public string content =
            "What kind of moderation is required to keep discourse civil, inclusive, and focused—without being overly censorious?\n\n" +

            "Creating an environment for productive problem-solving requires balancing freedom of expression with the need for respectful, constructive dialogue. Traditional moderation approaches often struggle with this balance, either allowing harmful behavior that drives away valuable contributors or implementing restrictions that stifle legitimate discussion.\n\n" +

            "For a platform like Atlas that aims to harness collective intelligence, this challenge is particularly critical. The governance model must support robust debate while preventing the toxicity that plagues many online spaces.\n\n" +

            "Key questions include:\n\n" +

            "- How can moderation systems distinguish between passionate disagreement and harmful behavior?\n\n" +

            "- What role should community governance play versus centralized moderation?\n\n" +

            "- How can moderation decisions be made transparent and accountable?\n\n" +

            "- What escalation paths should exist when users disagree with moderation decisions?\n\n" +

            "- How can the platform's design itself encourage constructive behavior and reduce the need for active moderation?\n\n" +

            "- What metrics can measure the health of discourse without creating perverse incentives?\n\n" +

            "Developing effective governance models is essential for creating an environment where diverse perspectives can contribute to solving complex problems without descending into unproductive conflict.";

        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Moderation and Governance of Public Debates",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 8, 10),
                    AuthorID = SeedUserSix.user.Id, // Using centralized user ID
                    ScopeID = scope.ScopeID,
                    ParentSolutionID = AtlasThePublicThinkTank.ContentId // Making this a sub-issue of Atlas solution
                };
            }
        }

        public Scope scope
        {
            get
            {
                return new Scope()
                {
                    ScopeID = new Guid("1032b6b2-f0f4-437c-9bb8-b386016bbdc7"),
                    Scales = { Scale.Global, Scale.Community },
                    Domains = { Domain.Cultural, Domain.Political, Domain.Technological },
                    EntityTypes = { EntityType.Organization, EntityType.Person },
                    Timeframes = { Timeframe.LongTerm, Timeframe.ShortTerm },
                    Boundaries = { },
                };
            }
        }
        public IssueVote[] issueVotes { get; } = {
            new IssueVote
            {
                VoteID = new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"),
                IssueID = ContentId,
                UserID = SeedUserOne.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 11)
            },
            new IssueVote
            {
                VoteID = new Guid("d2e3f4a5-b6c7-8d9e-0f1a-2b3c4d5e6f7a"),
                IssueID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 8, 11)
            },
            new IssueVote
            {
                VoteID = new Guid("e3f4a5b6-c7d8-9e0f-1a2b-3c4d5e6f7a8b"),
                IssueID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 8, 12)
            },
            new IssueVote
            {
                VoteID = new Guid("f4a5b6c7-d8e9-0f1a-2b3c-4d5e6f7a8b9c"),
                IssueID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 8, 12)
            },
            new IssueVote
            {
                VoteID = new Guid("a5b6c7d8-e9f0-1a2b-3c4d-5e6f7a8b9c0d"),
                IssueID = ContentId,
                UserID = SeedUserNine.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 13)
            },
            new IssueVote
            {
                VoteID = new Guid("b6c7d8e9-f0a1-2b3c-4d5e-6f7a8b9c0d1e"),
                IssueID = ContentId,
                UserID = SeedUserEleven.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 8, 13)
            },
            new IssueVote
            {
                VoteID = new Guid("c7d8e9f0-a1b2-3c4d-5e6f-7a8b9c0d1e2f"),
                IssueID = ContentId,
                UserID = SeedUserThirteen.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 14)
            }
        };
    }
}