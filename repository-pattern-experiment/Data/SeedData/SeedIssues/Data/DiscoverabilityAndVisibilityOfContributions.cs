using System;
using System.Collections.Generic;
using repository_pattern_experiment.Data.SeedData.SeedSolutions.Data;
using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;
using static repository_pattern_experiment.Data.SeedData.SeedIds;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class DiscoverabilityAndVisibilityOfContributions : SeedIssueContainer
    {
        public static Guid ContentId = new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e");

        public string content =
            "How can high-quality ideas from everyday users be surfaced without being buried by noise or popularity bias?\n\n" +
            "In collaborative platforms like Atlas, ensuring that high-quality contributions receive appropriate visibility is crucial for maintaining user engagement and facilitating problem-solving.\n\n" +

            "Currently, many platforms struggle with this challenge: valuable content can be buried while sensationalist or low-quality content rises to prominence. This undermines the collective intelligence of online communities and discourages thoughtful participation.\n\n" +

            "Key questions include:\n\n" +

            "- How can we design discovery mechanisms that surface valuable content without creating perverse incentives?\n\n" +

            "- What balance should be struck between algorithmic and human curation?\n\n" +

            "- How can we ensure that new contributors have a fair chance at visibility while still maintaining quality standards?\n\n" +

            "- What metrics beyond simple engagement best indicate the actual value of contributions?\n\n" +

            "These challenges are particularly relevant for a platform like Atlas that aims to harness collective intelligence for problem-solving rather than simply maximizing engagement.";

        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Discoverability and Visibility of Contributions",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 6, 15),
                    AuthorID = SeedUserFive.user.Id, // Using centralized user ID
                    ScopeID = Scopes.Global, // Using centralized scope ID
                    ParentSolutionID = AtlasThePublicThinkTank.ContentId // Making this a sub-issue of Atlas solution
                };
            }
        }

        public IssueVote[] issueVotes { get; } = {
            new IssueVote
            {
                VoteID = new Guid("a4b7c9e1-2d3f-4a5b-6c7d-8e9f0a1b2c3d"),
                IssueID = ContentId,
                UserID = SeedUserOne.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 6, 16)
            },
            new IssueVote
            {
                VoteID = new Guid("b5c8d0e2-3f4a-5b6c-7d8e-9f0a1b2c3d4e"),
                IssueID = ContentId,
                UserID = SeedUserThree.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 6, 16)
            },
            new IssueVote
            {
                VoteID = new Guid("c6d9e0f3-4a5b-6c7d-8e9f-0a1b2c3d4e5f"),
                IssueID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 6, 17)
            }
        };
    }
}