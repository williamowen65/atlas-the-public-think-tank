using Atlas.Persistence;

namespace Atlas.Application.Storage;

internal static partial class DemoData
{
    /// <summary>
    /// Deliberately nonzero ratings and voted reactions on moderated nodes.
    /// These are persisted historical votes, not permission to vote while hidden.
    /// </summary>
    private static void SeedModeratedNodeEngagement(AtlasDataContext database)
    {
        var timestamp = DateTimeOffset.Parse("2026-10-19T12:00:00Z");
        var goal = Guid.Parse("b1000000-0000-4000-8000-000000000003");
        var idea = Guid.Parse("b1000000-0000-4000-8000-000000000004");
        var descendant = Guid.Parse("b1000000-0000-4000-8000-000000000008");

        // Goal: 8 and 10 => average 9, count 2.
        // Idea: 5, 7 and 9 => average 7, count 3.
        // Hidden grandchild: 6 and 8 => average 7, count 2.
        var ratings = new (Guid Node, Guid Participant, int Value)[]
        {
            (goal, DemoParticipants.User01Id, 8),
            (goal, DemoParticipants.User02Id, 10),
            (idea, DemoParticipants.User01Id, 5),
            (idea, DemoParticipants.User02Id, 7),
            (idea, DemoParticipants.User03Id, 9),
            (descendant, DemoParticipants.User01Id, 6),
            (descendant, DemoParticipants.User03Id, 8)
        };
        for (var index = 0; index < ratings.Length; index++)
        {
            var rating = ratings[index];
            database.VoteRows.Add(new VoteRow
            {
                Id = Guid.Parse($"d2000000-0000-4000-8000-{101 + index:D12}"),
                ParticipantId = rating.Participant,
                TargetId = rating.Node,
                TargetType = "Node",
                Value = rating.Value,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            });
        }

        // Curated definitions already exist in SeedReactionDefinition.
        // Every association receives a +1 vote so its score is visibly nonzero.
        var associations = new (Guid Node, Guid Definition, Guid Participant)[]
        {
            (goal, Guid.Parse("40000000-0000-4000-8000-000000000001"), DemoParticipants.User01Id), // Promising
            (goal, Guid.Parse("40000000-0000-4000-8000-000000000007"), DemoParticipants.User02Id), // Practical
            (idea, Guid.Parse("40000000-0000-4000-8000-000000000006"), DemoParticipants.User01Id), // Innovative
            (idea, Guid.Parse("40000000-0000-4000-8000-000000000005"), DemoParticipants.User02Id), // Hopeful
            (descendant, Guid.Parse("40000000-0000-4000-8000-000000000010"), DemoParticipants.User03Id) // Uncertain
        };
        for (var index = 0; index < associations.Length; index++)
        {
            var association = associations[index];
            var reactionId = Guid.Parse($"d1000000-0000-4000-8000-{101 + index:D12}");
            var row = new NodeReactionRow
            {
                Id = reactionId,
                NodeId = association.Node,
                ReactionDefinitionId = association.Definition,
                AppliedByParticipantId = association.Participant,
                LifecycleState = "Active",
                Disposition = "Community",
                CreatedAt = timestamp,
                RemovedAt = null,
                AuditHistory = [new ReactionAuditRow
                {
                    Action = "Applied",
                    ActorParticipantId = association.Participant,
                    OccurredAt = timestamp,
                    LifecycleState = "Active",
                    Disposition = "Community",
                    RelatedNodeReactionId = null
                }]
            };
            AtlasDataContext.PrepareRow(row);
            database.NodeReactionRows.Add(row);
            database.VoteRows.Add(new VoteRow
            {
                Id = Guid.Parse($"d2000000-0000-4000-8000-{201 + index:D12}"),
                ParticipantId = association.Participant,
                TargetId = reactionId,
                TargetType = "NodeReaction",
                Value = 1,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            });
        }
    }
}
