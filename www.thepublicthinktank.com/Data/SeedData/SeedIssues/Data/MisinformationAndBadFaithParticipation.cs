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
    public class MisinformationAndBadFaithParticipation : SeedIssueContainer
    {
        public static Guid ContentId = new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e");

        public string content =
            "How can the Atlas platform prevent or mitigate users who post misleading information, trolls, or coordinated disinformation efforts?\n\n" +

            "Traditional social media platforms struggle with combating misinformation and bad faith participation without resorting to heavy-handed moderation that risks stifling legitimate discourse. This challenge is particularly acute for a platform like Atlas that aims to foster collaborative problem-solving.\n\n" +

            "Key questions include:\n\n" +

            "- What verification mechanisms can be implemented that balance accuracy with accessibility?\n\n" +

            "- How can the platform's reputation system be designed to reward good-faith participation while discouraging manipulation?\n\n" +

            "- What role should community moderation play versus automated systems?\n\n" +

            "- How can the platform distinguish between honest mistakes and deliberate misinformation?\n\n" +

            "- What safeguards can prevent coordinated manipulation campaigns while protecting privacy?\n\n" +

            "Addressing these challenges is essential for maintaining the integrity of discussions and ensuring that Atlas fulfills its potential as a space for meaningful collaborative problem-solving rather than becoming another vector for misinformation.";

        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Misinformation and Bad Faith Participation",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 7, 5),
                    AuthorID = SeedUserSeven.user.Id, // Using centralized user ID
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
                    ScopeID = new Guid("ff976803-e30a-45d6-a74d-4bcbb024a513"),
                    Scales = { Scale.Global },
                    Domains = { Domain.Technological, Domain.Cultural, Domain.Political },
                    EntityTypes = { EntityType.Organization, EntityType.Person },
                    Timeframes = { Timeframe.LongTerm, Timeframe.ShortTerm },
                    Boundaries = { },
                };
            }
        }

        public IssueVote[] issueVotes { get; } = {
            new IssueVote
            {
                VoteID = new Guid("e1f2a3b4-c5d6-7e8f-9a0b-1c2d3e4f5a6b"),
                IssueID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 7, 6)
            },
            new IssueVote
            {
                VoteID = new Guid("f2e3d4c5-b6a7-8f9e-0a1b-2c3d4e5f6a7b"),
                IssueID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 7, 6)
            },
            new IssueVote
            {
                VoteID = new Guid("a3b4c5d6-e7f8-9a0b-1c2d-3e4f5a6b7c8d"),
                IssueID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 7, 7)
            },
            new IssueVote
            {
                VoteID = new Guid("b4c5d6e7-f8a9-0b1c-2d3e-4f5a6b7c8d9e"),
                IssueID = ContentId,
                UserID = SeedUserNine.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 7, 7)
            },
            new IssueVote
            {
                VoteID = new Guid("c5d6e7f8-a9b0-1c2d-3e4f-5a6b7c8d9e0f"),
                IssueID = ContentId,
                UserID = SeedUserTwelve.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 7, 8)
            }
        };
    }
}