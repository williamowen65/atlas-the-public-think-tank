using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Tests;

internal static class DiscoverySqlFixture
{
    public static void Seed(SqlTestDatabase fixture, params DiscoveryCandidate[] candidates)
    {
        using var db = fixture.Open();
        var now = DateTimeOffset.UtcNow;
        var authorIds = candidates.Select(c => c.AuthorParticipantId).Distinct().ToArray();
        var voterCount = Math.Max(1, candidates.Select(c => c.VoteCount).DefaultIfEmpty().Max());
        var voters = Enumerable.Range(0, voterCount).Select(_ => Guid.NewGuid()).ToArray();
        foreach (var id in authorIds.Concat(voters).Distinct()) db.ParticipantRows.Add(new ParticipantRow { Id = id, DisplayName = "Author", Bio = "", IsActive = true, CreatedAt = now, UpdatedAt = now });
        foreach (var id in candidates.Select(c => c.NodeTypeId).Distinct()) db.NodeTypeRows.Add(new NodeTypeRow { Id = id, Name = "Idea", Description = "", IsSystemDefined = true, CreatedAt = now, UpdatedAt = now });
        foreach (var id in candidates.SelectMany(c => c.CommunityIds).Distinct()) db.CommunityRows.Add(new CommunityRow { Id = id, Name = id.ToString(), Description = "", OwnerParticipantId = authorIds[0], Status = "Active", CreatedAt = now, UpdatedAt = now });
        foreach (var id in candidates.SelectMany(c => c.ReactionDefinitionIds).Distinct()) db.ReactionDefinitionRows.Add(new ReactionDefinitionRow { Id = id, Text = id.ToString(), NormalizedText = id.ToString(), Description = "", Emoji = "🐋", CreatedByParticipantId = authorIds[0], CreatedAt = now, UpdatedAt = now });
        foreach (var candidate in candidates)
        {
            var doc = Guid.NewGuid(); var block = Guid.NewGuid();
            db.BlockRows.Add(new BlockRow { Id = block, Kind = "markdown", Markdown = candidate.SearchableContent, CreatedAt = candidate.CreatedAt, UpdatedAt = candidate.UpdatedAt });
            db.DocumentRows.Add(new DocumentRow { Id = doc, CreatedAt = candidate.CreatedAt, UpdatedAt = candidate.UpdatedAt, Blocks = [new DocumentBlockRow { DocumentId = doc, Position = 0, BlockId = block }] });
            db.NodeRows.Add(new NodeRow { Id = candidate.NodeId, Title = candidate.Title, TypeId = candidate.NodeTypeId, AuthorId = candidate.AuthorParticipantId, DescriptionId = doc, Status = candidate.IsArchived ? "Archived" : "Active", CreatedAt = candidate.CreatedAt, UpdatedAt = candidate.UpdatedAt });
            foreach (var id in candidate.CommunityIds) db.CommunityNodeRows.Add(new CommunityNodeRow { NodeId = candidate.NodeId, CommunityId = id, AssociatedByParticipantId = candidate.AuthorParticipantId, AssociatedAt = now });
            foreach (var id in candidate.ReactionDefinitionIds) db.NodeReactionRows.Add(new NodeReactionRow { Id = Guid.NewGuid(), NodeId = candidate.NodeId, ReactionDefinitionId = id, AppliedByParticipantId = candidate.AuthorParticipantId, LifecycleState = "Active", Disposition = "Normal", CreatedAt = now });
            for (var i = 0; i < candidate.VoteCount; i++) db.VoteRows.Add(new VoteRow { Id = Guid.NewGuid(), TargetId = candidate.NodeId, TargetType = "Node", ParticipantId = voters[i], Value = (int)Math.Floor(candidate.AverageVote ?? 0) + (i < (int)Math.Round(((candidate.AverageVote ?? 0) % 1) * candidate.VoteCount) ? 1 : 0), CreatedAt = now, UpdatedAt = now });
            if (candidate.IsModerationExcluded) db.ModerationCaseRows.Add(new ModerationCaseRow { Id = Guid.NewGuid(), NodeId = candidate.NodeId, ReporterId = candidate.AuthorParticipantId, Reason = "Spam", ReportedTitle = candidate.Title, Status = 2, PublicReason = 1, CreatedAt = now });
        }
        db.SaveChanges();
    }
}
