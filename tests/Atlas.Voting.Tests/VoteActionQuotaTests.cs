using Atlas.Contracts.Observability;
using Atlas.Contracts.Operations;
using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests;

[TestClass]
public sealed class VoteActionQuotaTests
{
    private static readonly DateTimeOffset At = DateTimeOffset.Parse("2026-10-19T12:00:00Z");

    [TestMethod]
    public void ExceedingQuotaExplainsLimitAndNextSlotWithoutChangingVote()
    {
        using var clock = AtlasTime.Use(new Clock(At));
        var store = new QuotaStore(VoteActionQuotaPolicy.Default.Limit);
        var actor = new ParticipantId(Guid.NewGuid());
        var target = new NodeVoteTarget(Guid.NewGuid());
        var cast = new CastVote(store, new VoteMutationPolicy(new Eligible()));

        // Spend all 60 slots at noon; changing an existing vote also spends a slot.
        var vote = cast.Execute(target, actor, 7);
        for (var action = 1; action < VoteActionQuotaPolicy.Default.Limit; action++)
            cast.Execute(target, actor, action % 2 == 0 ? 7 : 8);
        var previousValue = vote.Value.Value;
        var previousUpdatedAt = vote.UpdatedAt;

        var rejected = Assert.ThrowsExactly<VoteActionQuotaExceededException>(
            () => cast.Execute(target, actor, 9));

        // Keep the complete user-facing wording visible here, including the UTC retry time.
        const string expectedMessage =
            "Voting action limit reached: 60 actions per 10 minutes. Try again at 2026-10-19 12:10:00 UTC.";
        Assert.AreEqual(expectedMessage, rejected.Message);
        Console.WriteLine(rejected.Message);
        Assert.AreEqual(0, rejected.Allowance.Remaining);
        Assert.AreEqual(TimeSpan.FromMinutes(10), rejected.Allowance.RetryAfter);
        Assert.AreEqual(EventReason.QuotaExceeded, Telemetry.Reason(rejected));

        // Rejection must neither alter the vote nor add a successful action to the ledger.
        Assert.AreEqual(previousValue, vote.Value.Value);
        Assert.AreEqual(previousUpdatedAt, vote.UpdatedAt);
        Assert.HasCount(60, store.History);
    }

    [TestMethod]
    public void RollingWindowExpiresExactlyAndTighteningUsesNewestActions()
    {
        var policy = new VoteActionQuotaPolicy(2, TimeSpan.FromMinutes(10));
        DateTimeOffset[] history = [At.AddMinutes(-9), At.AddMinutes(-5), At.AddMinutes(-1)];
        var full = policy.Evaluate(history, At);
        Assert.AreEqual(0, full.Remaining);
        Assert.AreEqual(At.AddMinutes(5), full.NextSlotAt);
        Assert.AreEqual(TimeSpan.FromMinutes(5), full.RetryAfter);
        Assert.AreEqual(1, policy.Evaluate(history, At.AddMinutes(5)).Remaining);
        Assert.AreEqual(2, policy.Evaluate(history, At.AddMinutes(9)).Remaining);
    }

    [TestMethod]
    public void CastChangeAndUndoShareSlotsAcrossBothTargetKindsWithoutRefund()
    {
        using var clock = AtlasTime.Use(new Clock(At));
        var store = new QuotaStore(4);
        var actor = new ParticipantId(Guid.NewGuid());
        var other = new ParticipantId(Guid.NewGuid());
        var cast = new CastVote(store, new VoteMutationPolicy(new Eligible()));
        var undo = new UndoVote(store, new VoteMutationPolicy(new Eligible()));
        var node = new NodeVoteTarget(Guid.NewGuid());
        var reaction = new NodeReactionVoteTarget(Guid.NewGuid());
        var current = cast.Execute(node, actor, 7); // one slot
        cast.Execute(reaction, actor, 1);          // second slot
        cast.Execute(node, actor, 8);              // third slot
        Assert.IsTrue(undo.Execute(reaction, actor)); // fourth slot; no refund
        var changedAt = current.UpdatedAt;
        var rejected = Assert.ThrowsExactly<VoteActionQuotaExceededException>(() => cast.Execute(node, actor, 9));
        Assert.AreEqual(EventReason.QuotaExceeded, Telemetry.Reason(rejected));
        Assert.AreEqual(8, current.Value.Value);
        Assert.AreEqual(changedAt, current.UpdatedAt);
        Assert.AreEqual(0, store.GetActionAllowance(actor).Remaining);
        Assert.AreEqual(4, store.GetActionAllowance(other).Remaining);
        Assert.IsFalse(undo.Execute(reaction, actor)); // no current vote: no-op, no slot
        Assert.AreEqual(4, store.History.Count);
    }

    [TestMethod]
    public void InvalidVoteDoesNotConsumeAllowanceAndExhaustedUndoKeepsCurrentVote()
    {
        using var clock = AtlasTime.Use(new Clock(At));
        var store = new QuotaStore(1);
        var actor = new ParticipantId(Guid.NewGuid());
        var target = new NodeVoteTarget(Guid.NewGuid());
        var cast = new CastVote(store, new VoteMutationPolicy(new Eligible()));
        var undo = new UndoVote(store, new VoteMutationPolicy(new Eligible()));
        Assert.Throws<ArgumentException>(() => cast.Execute(target, actor, 999));
        Assert.IsEmpty(store.History);
        var vote = cast.Execute(target, actor, 5);
        Assert.ThrowsExactly<VoteActionQuotaExceededException>(() => undo.Execute(target, actor));
        Assert.AreSame(vote, store.GetByParticipantAndTarget(actor, target));
        Assert.HasCount(1, store.History);
    }

    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
    private sealed class Eligible : IVotingEligibility
    {
        public bool IsEligible(ParticipantId participantId) => true;
        public bool IsAvailable(VoteTarget target) => true;
    }
    // Single-threaded test adapter; SQL integration tests establish actual transactional coordination.
    private sealed class QuotaStore(int limit) : IVoteRepository, IVoteActionQuota
    {
        private readonly InMemoryVoteRepository _votes = new();
        public List<(Guid Actor, DateTimeOffset At)> History { get; } = [];
        public VoteActionAllowance GetActionAllowance(ParticipantId actor) =>
            new VoteActionQuotaPolicy(limit, TimeSpan.FromMinutes(10)).Evaluate(History.Where(x => x.Actor == actor.Id).Select(x => x.At), AtlasTime.UtcNow);
        public void EnsureActionAllowed(ParticipantId actor)
        {
            var allowance = GetActionAllowance(actor);
            if (allowance.Remaining == 0) throw new VoteActionQuotaExceededException(allowance);
        }
        public T ExecuteVoteAction<T>(ParticipantId actor, Func<T> operation) => operation();
        public void Save(Vote vote)
        {
            EnsureActionAllowed(vote.ParticipantId);
            _votes.Save(vote);
            History.Add((vote.ParticipantId.Id, AtlasTime.UtcNow));
        }
        public void Delete(VoteId id)
        {
            var vote = _votes.GetById(id);
            if (vote is null) return;
            EnsureActionAllowed(vote.ParticipantId);
            _votes.Delete(id);
            History.Add((vote.ParticipantId.Id, AtlasTime.UtcNow));
        }
        public Vote? GetById(VoteId id) => _votes.GetById(id);
        public Vote? GetByParticipantAndTarget(ParticipantId actor, VoteTarget target) => _votes.GetByParticipantAndTarget(actor, target);
        public IReadOnlyCollection<Vote> GetTargetVotes(VoteTarget target) => _votes.GetTargetVotes(target);
    }
}
