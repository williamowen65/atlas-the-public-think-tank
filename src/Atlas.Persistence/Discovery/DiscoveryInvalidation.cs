using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Discovery;

/// <summary>Classifies pending EF writes by the cached projections they can change.</summary>
internal static class DiscoveryInvalidation
{
    /// <summary>Returns which generations SaveChanges must advance in its write transaction.</summary>
    public static (bool Page, bool Detail) Changes(AtlasDataContext db)
    {
        // Include property edits even when EF has not yet detected them automatically.
        // This only classifies changes; SaveChanges performs the atomic revision update.
        db.ChangeTracker.DetectChanges();
        var page = false;
        var detail = false;
        var writes = db.ChangeTracker.Entries()
            .Where(entry => entry.State is
                EntityState.Added or EntityState.Modified or EntityState.Deleted);
        foreach (var entry in writes)
        {
            if (entry.Entity is VoteRow vote)
            {
                page = true;
                // A node importance vote changes ordering/statistics, not cached display text.
                // Inspect both target types in case a vote was moved between kinds.
                // Reaction votes affect the reaction score displayed on the card.
                detail |= vote.TargetType != "Node" ||
                    entry.Property(nameof(VoteRow.TargetType)).OriginalValue as string != "Node";
            }
            else if (entry.Entity is NodeRow or NodeParentRow or NodeRequestedTypeRow or NodeTypeRow or
                DocumentRow or DocumentBlockRow or BlockRow or ParticipantRow or CommunityRow or
                CommunityNodeRow or NodeReactionRow or ReactionDefinitionRow or ModerationCaseRow or CommentRow)
            {
                // Conservative, database-wide invalidation includes related labels,
                // ancestor paths and moderation notices, not just the node itself.
                page = true;
                detail = true;
            }
        }
        // Unlisted rows (for example notification delivery) do not affect Discovery.
        return (page, detail);
    }
}
