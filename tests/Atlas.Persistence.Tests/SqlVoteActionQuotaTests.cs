using Atlas.Application.Storage;
using Atlas.Contracts.Operations;
using Atlas.Voting;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Persistence.Tests;

/// <summary>Fresh-model evidence while the developer-generated migration is pending; not upgrade-path evidence.</summary>
[TestClass]
public sealed class SqlVoteActionQuotaTests
{
    [TestMethod]
    public void CastChangeUndoPersistConsumptionAcrossRepositoryInstancesAndExpire()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true);
        var at = DateTimeOffset.UtcNow;
        using var clock = AtlasTime.Use(new Clock(at));
        var actor = new ParticipantId(database.AddParticipant());
        var other = new ParticipantId(database.AddParticipant());
        var node = new NodeVoteTarget(database.AddNode());
        var reaction = new NodeReactionVoteTarget(database.AddReaction());
        var policy = new VoteActionQuotaPolicy(3, TimeSpan.FromMinutes(10));
        var repo = new SqlVoteRepository(database.Open, policy);
        var cast = new CastVote(repo, new VoteMutationPolicy(new Eligible()));
        var undo = new UndoVote(repo, new VoteMutationPolicy(new Eligible()));
        cast.Execute(node, actor, 5);
        cast.Execute(node, actor, 6);
        Assert.IsTrue(undo.Execute(node, actor));
        Assert.IsFalse(undo.Execute(node, actor)); // no-op even while exhausted
        var reopened = new SqlVoteRepository(database.Open, policy);
        Assert.AreEqual(0, reopened.GetActionAllowance(actor).Remaining);
        Assert.AreEqual(3, reopened.GetActionAllowance(other).Remaining);
        Assert.ThrowsExactly<VoteActionQuotaExceededException>(() =>
            new CastVote(reopened, new VoteMutationPolicy(new Eligible())).Execute(reaction, actor, 1));
        using (var check = database.Open())
        {
            Assert.AreEqual(3, check.VoteActionQuotaRows.Count());
            Assert.AreEqual(0, check.VoteRows.Count());
        }
        using var later = AtlasTime.Use(new Clock(at.AddMinutes(10)));
        cast.Execute(reaction, actor, 1);
        Assert.AreEqual(2, repo.GetActionAllowance(actor).Remaining);
    }

    [TestMethod]
    public void InvalidValuesAndOuterRollbackConsumeNothingAndBackdatingCannotBypass()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true);
        var actor = new ParticipantId(database.AddParticipant());
        var target = new NodeVoteTarget(database.AddNode());
        var repo = new SqlVoteRepository(database.Open, new VoteActionQuotaPolicy(1, TimeSpan.FromMinutes(10)));
        var cast = new CastVote(repo, new VoteMutationPolicy(new Eligible()));
        Assert.Throws<ArgumentException>(() => cast.Execute(target, actor, 999));
        Assert.ThrowsExactly<InvalidOperationException>(() => OperationBoundary.Execute(repo, () =>
        {
            cast.Execute(target, actor, 5);
            throw new InvalidOperationException("Injected transaction failure");
        }));
        using (var check = database.Open())
        {
            Assert.AreEqual(0, check.VoteRows.Count());
            Assert.AreEqual(0, check.VoteActionQuotaRows.Count());
        }
        // Direct persistence still enforces the policy, using accounting time rather than vote time.
        repo.Save(new Vote(target, actor, 5, DateTimeOffset.UtcNow.AddDays(-1)));
        Assert.AreEqual(0, repo.GetActionAllowance(actor).Remaining);
        var existing = repo.GetByParticipantAndTarget(actor, target)!;
        Assert.ThrowsExactly<VoteActionQuotaExceededException>(() => repo.Delete(existing.Id));
        Assert.IsNotNull(repo.GetById(existing.Id));
    }

    [TestMethod]
    public async Task CompetingRequestsCannotBothConsumeTheLastSlot()
    {
        using var database = SqlTestDatabase.Create(useCurrentModel: true);
        var actor = new ParticipantId(database.AddParticipant());
        var first = new NodeVoteTarget(database.AddNode());
        var second = new NodeVoteTarget(database.AddNode());
        var policy = new VoteActionQuotaPolicy(1, TimeSpan.FromMinutes(10));
        using var start = new Barrier(2);
        async Task<bool> Attempt(NodeVoteTarget target) => await Task.Run(() =>
        {
            start.SignalAndWait();
            try
            {
                var repo = new SqlVoteRepository(database.Open, policy);
                new CastVote(repo, new VoteMutationPolicy(new Eligible())).Execute(target, actor, 5);
                return true;
            }
            catch (VoteActionQuotaExceededException) { return false; }
        });
        var results = await Task.WhenAll(Attempt(first), Attempt(second));
        Assert.AreEqual(1, results.Count(success => success));
        using var check = database.Open();
        Assert.AreEqual(1, check.VoteRows.Count());
        Assert.AreEqual(1, check.VoteActionQuotaRows.Count());
    }

    private sealed class Clock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
    // SQL repositories independently validate real participant and target references.
    private sealed class Eligible : IVotingEligibility
    {
        public bool IsEligible(ParticipantId participantId) => true;
        public bool IsAvailable(VoteTarget target) => true;
    }
}
