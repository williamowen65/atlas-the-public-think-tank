using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using Microsoft.EntityFrameworkCore;
using static atlas_the_public_think_tank.Data.SeedData.SeedIds;

namespace atlas_the_public_think_tank.Data.SeedData.SeedVotes
{
    public static class SeedIssueVotes
    {
        public static IssueVote[] SeedIssuesVoteData = SeedIssues.SeedIssues.SeedIssuesDataContainers
            .SelectMany(container => container.issueVotes)
            .ToArray();

        public static void Seed(ApplicationDbContext context)
        {
            var existingVoteIds = context.IssueVotes.Select(v => v.VoteID).ToHashSet();
            var newVotes = SeedIssuesVoteData.Where(v => !existingVoteIds.Contains(v.VoteID))
                // Guaranteeing that the entity doesn't contain navigation properties
                .Select(iv => new IssueVote()
                {
                    VoteValue = iv.VoteValue,
                    IssueID = iv.IssueID,
                    VoteID = iv.VoteID,
                    UserID = iv.UserID,
                    CreatedAt = iv.CreatedAt,
                    ModifiedAt = iv.ModifiedAt
                 })
                .ToList();

            if (newVotes.Any())
            {
                context.IssueVotes.AddRange(newVotes);
            }
        }
    }

    public static class SeedSolutionVotes
    {
        public static SolutionVote[] SeedSolutionVoteData = SeedSolutions.SeedSolutions.SeedSolutionDataContainers
            .SelectMany(container => container.solutionVotes)
            .ToArray();

        public static void Seed(ApplicationDbContext context)
        {
            var existingVoteIds = context.SolutionVotes.Select(v => v.VoteID).ToHashSet();
            var newVotes = SeedSolutionVoteData.Where(v => !existingVoteIds.Contains(v.VoteID))
                // Guaranteeing that the entity doesn't contain navigation properties
                .Select(sv => new SolutionVote()
                {

                    VoteValue = sv.VoteValue,
                    SolutionID = sv.SolutionID,
                    VoteID = sv.VoteID,
                    UserID = sv.UserID,
                    CreatedAt = sv.CreatedAt,
                    ModifiedAt = sv.ModifiedAt
                })
                .ToList();

            if (newVotes.Any())
            {
                context.SolutionVotes.AddRange(newVotes);
            }
        }
    }
}