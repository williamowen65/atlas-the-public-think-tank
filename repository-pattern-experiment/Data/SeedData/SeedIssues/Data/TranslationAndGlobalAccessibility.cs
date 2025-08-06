using System;
using System.Collections.Generic;
using repository_pattern_experiment.Data.SeedData.SeedSolutions.Data;
using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;
using static repository_pattern_experiment.Data.SeedData.SeedIds;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class TranslationAndGlobalAccessibility : SeedIssueContainer
    {
        public static Guid ContentId = new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2");

        public string content =
            "How can the think tank be accessible across languages, cultures, and digital literacy levels?\n\n" +

            "For a platform like Atlas to achieve its goal of harnessing collective intelligence for problem-solving, it must be accessible to diverse participants worldwide. However, current collaborative platforms often face significant barriers related to language, cultural context, and varying levels of digital literacy.\n\n" +

            "Language barriers can exclude valuable perspectives, while cultural differences in communication styles and norms may lead to misunderstandings or alienation. Additionally, complex interfaces and features can exclude participants with limited digital experience or access to technology.\n\n" +

            "Key questions include:\n\n" +

            "- What translation and localization approaches can make content accessible while preserving nuance and context?\n\n" +

            "- How can user interfaces be designed to be intuitive across cultural contexts and digital literacy levels?\n\n" +

            "- What alternative access methods could accommodate participants with limited internet connectivity or devices?\n\n" +

            "- How can the platform's information architecture accommodate different cultural frameworks for organizing knowledge?\n\n" +

            "- What community norms and facilitation approaches can bridge cultural differences in communication styles?\n\n" +

            "- How can content moderation be culturally sensitive while maintaining consistent standards?\n\n" +

            "- What technical solutions might reduce bandwidth requirements for participation?\n\n" +

            "Addressing these challenges is essential for building a truly global collaborative platform that can leverage diverse perspectives from around the world, rather than only those from privileged communities with high technological access and specific cultural backgrounds.";

        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Translation and Global Accessibility",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 8, 30),
                    AuthorID = SeedUserThree.user.Id, // Using centralized user ID
                    ScopeID = Scopes.Global, // Using centralized scope ID
                    ParentSolutionID = AtlasThePublicThinkTank.ContentId // Making this a sub-issue of Atlas solution
                };
            }
        }

        public IssueVote[] issueVotes { get; } = {
            new IssueVote
            {
                VoteID = new Guid("f9e8d7c6-b5a4-4312-9021-e8d7c6b5a4f3"),
                IssueID = ContentId,
                UserID = SeedUserOne.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 8, 31)
            },
            new IssueVote
            {
                VoteID = new Guid("a0f9e8d7-c6b5-4423-a132-f9e8d7c6b5a0"),
                IssueID = ContentId,
                UserID = SeedUserTwo.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 8, 31)
            },
            new IssueVote
            {
                VoteID = new Guid("b1a0f9e8-d7c6-4534-b243-a0f9e8d7c6b1"),
                IssueID = ContentId,
                UserID = SeedUserFive.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 9, 1)
            },
            new IssueVote
            {
                VoteID = new Guid("c2b1a0f9-e8d7-4645-c354-b1a0f9e8d7c2"),
                IssueID = ContentId,
                UserID = SeedUserSeven.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 9, 1)
            },
            new IssueVote
            {
                VoteID = new Guid("d3c2b1a0-f9e8-4756-d465-c2b1a0f9e8d3"),
                IssueID = ContentId,
                UserID = SeedUserNine.user.Id,
                VoteValue = 7,
                CreatedAt = new DateTime(2024, 9, 2)
            },
            new IssueVote
            {
                VoteID = new Guid("e4d3c2b1-a0f9-4867-e576-d3c2b1a0f9e4"),
                IssueID = ContentId,
                UserID = SeedUserTwelve.user.Id,
                VoteValue = 10,
                CreatedAt = new DateTime(2024, 9, 2)
            },
            new IssueVote
            {
                VoteID = new Guid("f5e4d3c2-b1a0-4978-f687-e4d3c2b1a0f5"),
                IssueID = ContentId,
                UserID = SeedUserFourteen.user.Id,
                VoteValue = 9,
                CreatedAt = new DateTime(2024, 9, 3)
            },
            new IssueVote
            {
                VoteID = new Guid("a6f5e4d3-c2b1-4089-a798-f5e4d3c2b1a6"),
                IssueID = ContentId,
                UserID = SeedUserSixteen.user.Id,
                VoteValue = 8,
                CreatedAt = new DateTime(2024, 9, 3)
            }
        };
    }
}