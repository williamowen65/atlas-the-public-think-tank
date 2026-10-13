using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence.Discovery;

internal static class DiscoveryInvalidation
{
    public static (bool Page, bool Detail) Changes(AtlasDataContext db)
    {
        db.ChangeTracker.DetectChanges();
        var page = false;
        var detail = false;
        foreach (var entry in db.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Entity is VoteRow vote)
            {
                page = true;
                // A node importance vote changes ordering/statistics, not cached display text.
                detail |= vote.TargetType != "Node" || entry.Property(nameof(VoteRow.TargetType)).OriginalValue as string != "Node";
            }
            else if (entry.Entity is NodeRow or NodeParentRow or NodeRequestedTypeRow or NodeTypeRow or
                DocumentRow or DocumentBlockRow or BlockRow or ParticipantRow or CommunityRow or
                CommunityNodeRow or NodeReactionRow or ReactionDefinitionRow or ModerationCaseRow or CommentRow)
            { page = true; detail = true; }
        }
        return (page, detail);
    }
}
