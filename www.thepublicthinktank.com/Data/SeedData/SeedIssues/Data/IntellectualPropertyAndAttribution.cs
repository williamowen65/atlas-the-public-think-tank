using System;
using System.Collections.Generic;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class IntellectualPropertyAndAttribution : SeedIssueContainer
    {
        public static Guid ContentId = new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a");

        public string content =
            "How do you ensure contributors are credited appropriately, especially if their ideas are developed or repurposed by others?\n\n" +

            "In collaborative problem-solving environments like Atlas, ideas often evolve through iterative refinement and combination with other perspectives. While this process is essential for developing robust solutions, it presents challenges for ensuring proper attribution and recognition of intellectual contributions.\n\n" +

            "Traditional intellectual property frameworks are often ill-suited for collaborative platforms where the goal is shared knowledge creation rather than exclusive ownership. Yet, proper attribution remains crucial for maintaining trust, encouraging participation, and respecting contributors' work.\n\n" +

            "Key questions include:\n\n" +

            "- What mechanisms can track the provenance of ideas as they evolve through collaborative refinement?\n\n" +

            "- How can we balance recognition of original contributors with acknowledgment of those who significantly develop or improve ideas?\n\n" +

            "- What role should automated systems play in tracking contributions versus relying on community norms and practices?\n\n" +

            "- How can attribution be made transparent without creating excessive overhead that impedes collaboration?\n\n" +

            "- What recourse should be available when contributors feel their contributions have been misattributed or used without proper credit?\n\n" +

            "Solving these challenges is essential for creating a collaborative environment where contributors feel their work is respected and valued, while still enabling the free flow of ideas necessary for collective problem-solving.";

        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Intellectual Property and Attribution",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 7, 25),
                    AuthorID = SeedUserThree.user.Id, // Using centralized user ID
                    ScopeID = Scopes.Global, // Using centralized scope ID
                    ParentSolutionID = AtlasThePublicThinkTank.ContentId // Making this a sub-issue of Atlas solution
                };
            }
        }

        public IssueVote[] issueVotes { get; } = {
            new IssueVote
            {
                VoteID = new Guid("d1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a"),
                IssueID = ContentId,
                UserID = SeedUserOne.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 7, 26)
            },
            new IssueVote
            {
                VoteID = new Guid("e2f3a4b5-c6d7-8e9f-0a1b-2c3d4e5f6a7b"),
                IssueID = ContentId,
                UserID = SeedUserFour.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 7, 26)
            },
            new IssueVote
            {
                VoteID = new Guid("f3a4b5c6-d7e8-9f0a-1b2c-3d4e5f6a7b8c"),
                IssueID = ContentId,
                UserID = SeedUserSix.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 7, 27)
            },
            new IssueVote
            {
                VoteID = new Guid("a4b5c6d7-e8f9-0a1b-2c3d-4e5f6a7b8c9d"),
                IssueID = ContentId,
                UserID = SeedUserEight.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 7, 27)
            },
            new IssueVote
            {
                VoteID = new Guid("b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e"),
                IssueID = ContentId,
                UserID = SeedUserTen.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 7, 28)
            },
            new IssueVote
            {
                VoteID = new Guid("c6d7e8f9-a0b1-2c3d-4e5f-6a7b8c9d0e1f"),
                IssueID = ContentId,
                UserID = SeedUserEleven.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 7, 28)
            }
        };
    }
}