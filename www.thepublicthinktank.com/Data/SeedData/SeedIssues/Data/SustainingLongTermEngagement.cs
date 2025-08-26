using System;
using System.Collections.Generic;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class SustainingLongTermEngagement : SeedIssueContainer
    {
        public static Guid ContentId = new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3");

        public string content =
            "What strategies can maintain user interest and participation beyond the initial launch or viral phase?\n\n" +

            "While many platforms experience strong initial engagement, sustaining meaningful participation over time remains a significant challenge. For Atlas to effectively leverage collective intelligence for problem-solving, it must overcome the tendency toward declining engagement that affects most collaborative platforms.\n\n" +

            "Traditional social media often relies on addictive design patterns to maintain engagement, but these approaches frequently lead to shallow interaction rather than thoughtful participation. A platform focused on collaborative problem-solving requires different strategies to sustain long-term community involvement.\n\n" +

            "Key questions include:\n\n" +

            "- How can the platform create meaningful progression systems that reward deepening contribution without gamifying in ways that distort participation?\n\n" +

            "- What feedback mechanisms best help users understand their impact and the value of their contributions?\n\n" +

            "- How can community rituals and regular events create sustainable rhythms of participation?\n\n" +

            "- What role should real-world impact and implementation of solutions play in maintaining motivation?\n\n" +

            "- How can the platform support different modes of engagement that accommodate varying levels of time commitment and expertise?\n\n" +

            "- What governance structures allow the community to evolve with changing needs while maintaining coherent purpose?\n\n" +

            "- How can the platform encourage meaningful relationships between participants that strengthen commitment to the community?\n\n" +

            "Addressing these challenges requires balancing intrinsic and extrinsic motivations while creating structures that support sustained, meaningful participation without burnout or disillusionment.";

        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Sustaining Long-Term Engagement",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 8, 20),
                    AuthorID = SeedUserEight.user.Id, // Using centralized user ID
                    ScopeID = Scopes.Global, // Using centralized scope ID
                    ParentSolutionID = AtlasThePublicThinkTank.ContentId // Making this a sub-issue of Atlas solution
                };
            }
        }

        public IssueVote[] issueVotes { get; } = {
            new IssueVote
            {
                VoteID = new Guid("f1e2d3c4-b5a6-4798-87b9-d0c1e2f3a4b5"),
                IssueID = ContentId,
                UserID = SeedUserOne.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 21)
            },
            new IssueVote
            {
                VoteID = new Guid("a9b8c7d6-e5f4-4312-b1a0-9d8c7b6a5f4e"),
                IssueID = ContentId,
                UserID = SeedUserThree.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 8, 21)
            },
            new IssueVote
            {
                VoteID = new Guid("c7b8a9d0-5e2f-4983-a1b0-c9d8e7f6a5b4"),
                IssueID = ContentId,
                UserID = SeedUserFive.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 8, 22)
            },
            new IssueVote
            {
                VoteID = new Guid("e5f4d3c2-b1a0-4675-8392-c1d0b9a8f7e6"),
                IssueID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 8, 22)
            },
            new IssueVote
            {
                VoteID = new Guid("9a8b7c6d-5f4e-4312-b0a9-1c2d3e4f5a6b"),
                IssueID = ContentId,
                UserID = SeedUserNine.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 8, 23)
            },
            new IssueVote
            {
                VoteID = new Guid("3c4d5e6f-7a8b-4921-83c0-f5e4d3c2b1a0"),
                IssueID = ContentId,
                UserID = SeedUserTwelve.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 23)
            },
            new IssueVote
            {
                VoteID = new Guid("2d3e4f5a-6b7c-4809-a1b2-c3d4e5f6a7b8"),
                IssueID = ContentId,
                UserID = SeedUserFourteen.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 8, 24)
            }
        };
    }
}