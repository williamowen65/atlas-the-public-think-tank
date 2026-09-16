using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests
{
    /// <summary>
    /// Verifies the real Voting-owned in-memory repository implementation,
    /// rather than a test repository, under competing CastVote operations.
    /// </summary>
    [TestClass]
    public sealed class VotingConcurrencyTests
    {
        [TestMethod]
        [TestCategory("Concurrency")]
        public async Task InMemoryRepository_SeparateCastVoteInstances_StoreOneCurrentVote()
        {
            var repository =
                new InMemoryVoteRepository();

            var firstCastVote =
                CreateCastVote(repository);

            var secondCastVote =
                CreateCastVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            await CastCompetingVotes(
                firstCastVote,
                secondCastVote,
                target,
                participantId);

            Assert.HasCount(
                1,
                repository.GetTargetVotes(target),
                "The real in-memory repository created duplicate current votes.");
        }

        [TestMethod]
        [TestCategory("Concurrency")]
        public async Task InMemoryRepository_AfterCompetingWrites_CountsParticipantOnce()
        {
            var repository =
                new InMemoryVoteRepository();

            var firstCastVote =
                CreateCastVote(repository);

            var secondCastVote =
                CreateCastVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            await CastCompetingVotes(
                firstCastVote,
                secondCastVote,
                target,
                participantId);

            var summary =
                new GetVoteSummary(repository).Execute(
                    target);

            Assert.AreEqual(
                1,
                summary.VoteCount);

            Assert.IsTrue(
                summary.AverageVote is 4.0 or 10.0);
        }

        private static CastVote CreateCastVote(
            IVoteRepository repository)
        {
            return new CastVote(
                repository,
                new VoteMutationPolicy(
                    new AlwaysEligibleVotingContext()));
        }

        private static async Task CastCompetingVotes(
            CastVote firstCastVote,
            CastVote secondCastVote,
            VoteTarget target,
            ParticipantId participantId)
        {
            using var ready =
                new CountdownEvent(2);

            using var startGate =
                new ManualResetEventSlim(false);

            var firstRequest = Task.Run(() =>
            {
                ready.Signal();
                startGate.Wait();

                return firstCastVote.Execute(
                    target,
                    participantId,
                    4);
            });

            var secondRequest = Task.Run(() =>
            {
                ready.Signal();
                startGate.Wait();

                return secondCastVote.Execute(
                    target,
                    participantId,
                    10);
            });

            if (!ready.Wait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException(
                    "Both competing vote tasks did not become ready.");
            }

            startGate.Set();

            await Task.WhenAll(
                firstRequest,
                secondRequest);
        }

        private sealed class AlwaysEligibleVotingContext :
            IVotingEligibility
        {
            public bool IsEligible(
                ParticipantId participantId)
            {
                return true;
            }

            public bool IsAvailable(
                VoteTarget target)
            {
                return true;
            }
        }
    }
}
