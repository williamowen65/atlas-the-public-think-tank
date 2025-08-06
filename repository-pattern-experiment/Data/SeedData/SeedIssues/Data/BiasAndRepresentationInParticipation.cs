using System;
using System.Collections.Generic;
using repository_pattern_experiment.Data.SeedData.SeedSolutions.Data;
using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;
using static repository_pattern_experiment.Data.SeedData.SeedIds;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class BiasAndRepresentationInParticipation : SeedIssueContainer
    {
        public static Guid ContentId = new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2");

        public string content =
            "How can the platform ensure diverse voices are heard and prevent dominance by already-privileged demographics?\n\n" +

            "Collaborative platforms often inadvertently reproduce or amplify existing societal inequalities in who participates and whose contributions receive attention. For a platform like Atlas that aims to leverage collective intelligence to solve complex problems, ensuring diverse participation is not just a matter of fairness but also essential for developing comprehensive, effective solutions.\n\n" +

            "Many current platforms struggle with representation issues across dimensions like gender, race, socioeconomic status, disability, geographic location, and educational background. These disparities limit the range of perspectives and expertise available to address challenges.\n\n" +

            "Key questions include:\n\n" +

            "- What design features can reduce barriers to participation for underrepresented groups?\n\n" +

            "- How can discovery algorithms be designed to surface valuable contributions from diverse participants rather than reinforcing existing visibility advantages?\n\n" +

            "- What metrics should be tracked to identify representation gaps without creating privacy concerns?\n\n" +

            "- How can the platform encourage inclusive dialogue without tokenizing contributors from underrepresented groups?\n\n" +

            "- What community norms and moderation approaches can prevent behaviors that disproportionately drive away participants from marginalized groups?\n\n" +

            "- How can the platform's structure acknowledge and address the different resources (time, technical access, etc.) available to different potential participants?\n\n" +

            "Addressing these challenges requires thoughtful design at all levels—from technical infrastructure to community governance—to create an environment where diverse perspectives can meaningfully contribute to problem-solving.";

        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Bias and Representation in Participation",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 8, 15),
                    AuthorID = SeedUserFour.user.Id, // Using centralized user ID
                    ScopeID = Scopes.Global, // Using centralized scope ID
                    ParentSolutionID = AtlasThePublicThinkTank.ContentId // Making this a sub-issue of Atlas solution
                };
            }
        }

        public IssueVote[] issueVotes { get; } = {
            new IssueVote
            {
                VoteID = new Guid("e8f9a0b1-c2d3-4e5f-6a7b-8c9d0e1f2a3b"),
                IssueID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 8, 16)
            },
            new IssueVote
            {
                VoteID = new Guid("f9a0b1c2-d3e4-5f6a-7b8c-9d0e1f2a3b4c"),
                IssueID = ContentId,
                UserID = SeedUserThree.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 16)
            },
            new IssueVote
            {
                VoteID = new Guid("a0b1c2d3-e4f5-6a7b-8c9d-0e1f2a3b4c5d"),
                IssueID = ContentId,
                UserID = SeedUserFive.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 8, 17)
            },
            new IssueVote
            {
                VoteID = new Guid("b1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e"),
                IssueID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 8, 17)
            },
            new IssueVote
            {
                VoteID = new Guid("c2d3e4f5-a6b7-8c9d-0e1f-2a3b4c5d6e7f"),
                IssueID = ContentId,
                UserID = SeedUserEight.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 8, 18)
            },
            new IssueVote
            {
                VoteID = new Guid("d3e4f5a6-b7c8-9d0e-1f2a-3b4c5d6e7f8a"),
                IssueID = ContentId,
                UserID = SeedUserTen.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 18)
            },
            new IssueVote
            {
                VoteID = new Guid("e4f5a6b7-c8d9-0e1f-2a3b-4c5d6e7f8a9b"),
                IssueID = ContentId,
                UserID = SeedUserEleven.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 8, 19)
            },
            new IssueVote
            {
                VoteID = new Guid("f5a6b7c8-d9e0-1f2a-3b4c-5d6e7f8a9b0c"),
                IssueID = ContentId,
                UserID = SeedUserThirteen.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 19)
            }
        };
    }
}