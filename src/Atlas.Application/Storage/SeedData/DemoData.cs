using System.Text.Json;
using Atlas.Persistence;

namespace Atlas.Application.Storage;

/// <summary>Opt-in demo records with fixed identities and timestamps.</summary>
internal static partial class DemoData
{
    public static void Seed(AtlasDataContext database)
    {
        SeedNode(database);
        SeedNodeType(database);
        SeedDocument(database);
        SeedBlock(database);
        SeedVote(database);
        SeedReactionDefinition(database);
        SeedNodeReaction(database);
        SeedCommunity(database);
        SeedCommunityMembership(database);
        SeedCommunityNode(database);
        SeedComment(database);
        SeedModerationCase(database);
        SeedModeratedNodeEngagement(database);
        SeedBreadcrumbDemo(database);
        SeedPublicThinkTank(database);
        SeedPublicThinkTankCommunities(database);
    }
    /// <summary>
    /// Run after initial demo rows are committed. Capture immutable report snapshots
    /// before applying the author revision fixtures.
    /// </summary>
    public static void SeedModerationRevision(AtlasDataContext database)
    {
        // Capture demo evidence before applying the author's sample revisions.
        foreach (var report in database.ModerationCaseRows.ToArray())
        {
            var reportedNode = database.NodeRows.Single(row => row.Id == report.NodeId);
            var content = (from link in database.Set<DocumentBlockRow>()
                           join reportedBlock in database.BlockRows on link.BlockId equals reportedBlock.Id
                           where link.DocumentId == reportedNode.DescriptionId
                           orderby link.Position
                           select new { link.Position, reportedBlock.Kind, reportedBlock.Markdown, reportedBlock.AltText,
                               reportedBlock.Caption, reportedBlock.Url, reportedBlock.Title, reportedBlock.Description, reportedBlock.ReferenceId }).ToArray();
            database.ModerationReportedSnapshotRows.Add(new ModerationReportedSnapshotRow
            {
                ModerationCaseId = report.Id,
                NodeId = report.NodeId,
                CapturedAt = report.CreatedAt,
                Title = reportedNode.Title,
                ShortDescription = reportedNode.ShortDescription,
                ContentJson = JsonSerializer.Serialize(content)
            });
        }
        var nodeId = Guid.Parse("b1000000-0000-4000-8000-000000000008");
        var blockId = Guid.Parse("b3000000-0000-4000-8000-000000000008");
        var caseId = Guid.Parse("c4000000-0000-4000-8000-000000000004");
        var node = database.NodeRows.Single(row => row.Id == nodeId);
        var block = database.BlockRows.Single(row => row.Id == blockId);
        var moderationCase = database.ModerationCaseRows.Single(row => row.Id == caseId);
        node.ShortDescription = "Explore whether ranked voting could help more candidates participate, with evidence from elections that already use it.";
        node.UpdatedAt = DateTimeOffset.Parse("2026-10-20T15:00:00Z");
        block.Markdown = "Could ranked voting make it easier for candidates to run for office? Compare candidate participation in elections with and without ranked voting, and consider other factors such as campaign costs and ballot access. The question is about election participation rather than an unrelated political debate.";
        block.UpdatedAt = node.UpdatedAt;
        moderationCase.ReviewRequestedAt = DateTimeOffset.Parse("2026-10-20T16:00:00Z");
        // A second review-request fixture, with its reported content already snapshotted.
        var secondNode = database.NodeRows.Single(row => row.Id == Guid.Parse("b1000000-0000-4000-8000-000000000004"));
        var secondBlock = database.BlockRows.Single(row => row.Id == Guid.Parse("b3000000-0000-4000-8000-000000000004"));
        secondNode.ShortDescription = "Compare successful approaches from other communities, including their limitations, before adapting them to new circumstances.";
        secondNode.UpdatedAt = DateTimeOffset.Parse("2026-10-20T15:30:00Z");
        secondBlock.Markdown += "\n\nAuthor revision: include the context, limitations, and evidence for each example before recommending it elsewhere.";
        secondBlock.UpdatedAt = secondNode.UpdatedAt;
        database.SaveChanges();
    }
}
