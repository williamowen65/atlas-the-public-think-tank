using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests
{
    /// <summary>
    /// Models competing commands handled by separate Voting application
    /// service instances that share one persistence store.
    ///
    /// Separate CastVote and repository objects approximate independently
    /// constructed service instances. The shared store represents the
    /// database or other persistence system through which horizontally
    /// scaled instances must preserve Voting invariants.
    ///
    /// These tests intentionally describe required distributed-safe behavior
    /// and are expected to fail until persistence provides an atomic
    /// participant-target operation.
    /// </summary>
    [TestClass]
    public sealed class VotingServiceInstanceConcurrencyTests
    {
        /// <summary>
        /// Models two Voting service instances casting the first vote for the
        /// same participant and target.
        ///
        /// Each CastVote has its own in-process operation gate, and each
        /// repository adapter is a separate object. The coordinated shared
        /// store makes both adapters observe "no current vote" before either
        /// saves. A correct persistence-level mechanism must allow only one
        /// current participant-target record to survive.
        /// </summary>
        [TestMethod]
        [TestCategory("Concurrency")]
        [TestCategory("ServiceInstances")]
        public async Task CastVote_SeparateServiceInstances_StoreOneCurrentVote()
        {
            using var store =
                new CoordinatedSharedVoteStore();

            var firstRepository =
                new SharedStoreVoteRepository(store);

            var secondRepository =
                new SharedStoreVoteRepository(store);

            var firstCastVote =
                CreateCastVote(firstRepository);

            var secondCastVote =
                CreateCastVote(secondRepository);

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
                firstRepository.GetTargetVotes(target),
                "Separate Voting service instances created duplicate current votes for one participant-target pair.");
        }

        /// <summary>
        /// Verifies the externally visible aggregate after the same
        /// separate-instance race.
        ///
        /// Even when requests are handled by independently constructed
        /// service and repository objects, one participant must contribute
        /// exactly one current rating to the shared target summary.
        /// </summary>
        [TestMethod]
        [TestCategory("Concurrency")]
        [TestCategory("ServiceInstances")]
        public async Task GetVoteSummary_AfterSeparateServiceInstancesRace_CountsParticipantOnce()
        {
            using var store =
                new CoordinatedSharedVoteStore();

            var firstRepository =
                new SharedStoreVoteRepository(store);

            var secondRepository =
                new SharedStoreVoteRepository(store);

            var firstCastVote =
                CreateCastVote(firstRepository);

            var secondCastVote =
                CreateCastVote(secondRepository);

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
                new GetVoteSummary(firstRepository).Execute(
                    target);

            Assert.AreEqual(
                1,
                summary.VoteCount,
                "Separate service instances gave one participant more than one unit of influence.");

            Assert.IsTrue(
                summary.AverageVote is 4.0 or 10.0,
                "The summary should reflect whichever single competing vote was accepted.");
        }

        private static CastVote CreateCastVote(
            IVoteRepository repository)
        {
            var eligibility =
                new AlwaysEligibleVotingContext();

            return new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));
        }

        private static async Task CastCompetingVotes(
            CastVote firstCastVote,
            CastVote secondCastVote,
            VoteTarget target,
            ParticipantId participantId)
        {
            var firstRequest = Task.Run(() =>
                firstCastVote.Execute(
                    target,
                    participantId,
                    4));

            var secondRequest = Task.Run(() =>
                secondCastVote.Execute(
                    target,
                    participantId,
                    10));

            await Task.WhenAll(
                firstRequest,
                secondRequest);
        }

        /// <summary>
        /// Represents shared persistence used by separate repository adapters.
        ///
        /// The collection gate protects the store's technical integrity. The
        /// lookup barrier deterministically exposes the business race by
        /// ensuring that both service instances finish their first lookup
        /// before either is allowed to save.
        /// </summary>
        private sealed class CoordinatedSharedVoteStore :
            IDisposable
        {
            internal object Gate { get; } = new();
            internal List<Vote> Votes { get; } = [];
            internal Barrier CompetingLookups { get; } = new(2);
            internal int LookupCount;

            public void Dispose()
            {
                CompetingLookups.Dispose();
            }
        }

        /// <summary>
        /// Represents one service instance's repository adapter over shared
        /// persistence. Adapter objects do not share local state; they
        /// coordinate only through the supplied shared store.
        /// </summary>
        private sealed class SharedStoreVoteRepository :
            IVoteRepository
        {
            private readonly CoordinatedSharedVoteStore _store;

            public SharedStoreVoteRepository(
                CoordinatedSharedVoteStore store)
            {
                _store = store;
            }

            public Vote? GetByParticipantAndTarget(
                ParticipantId participantId,
                VoteTarget voteTarget)
            {
                Vote? existingVote;

                lock (_store.Gate)
                {
                    existingVote = _store.Votes.SingleOrDefault(vote =>
                        vote.ParticipantId.Id == participantId.Id &&
                        vote.Target.Id == voteTarget.Id &&
                        vote.Target.GetType() == voteTarget.GetType());
                }

                var lookupNumber =
                    Interlocked.Increment(
                        ref _store.LookupCount);

                if (lookupNumber <= 2 &&
                    !_store.CompetingLookups.SignalAndWait(
                        TimeSpan.FromSeconds(10)))
                {
                    throw new TimeoutException(
                        "Both service-instance vote lookups did not reach the shared-store test barrier.");
                }

                return existingVote;
            }

            public void Save(Vote vote)
            {
                lock (_store.Gate)
                {
                    var existingIndex = _store.Votes.FindIndex(
                        existingVote => existingVote.Id == vote.Id);

                    if (existingIndex >= 0)
                    {
                        _store.Votes[existingIndex] = vote;
                    }
                    else
                    {
                        _store.Votes.Add(vote);
                    }
                }
            }

            public void Delete(VoteId id)
            {
                lock (_store.Gate)
                {
                    _store.Votes.RemoveAll(vote =>
                        vote.Id == id);
                }
            }

            public Vote? GetById(VoteId id)
            {
                lock (_store.Gate)
                {
                    return _store.Votes.SingleOrDefault(vote =>
                        vote.Id == id);
                }
            }

            public IReadOnlyCollection<Vote> GetTargetVotes(
                VoteTarget target)
            {
                lock (_store.Gate)
                {
                    return _store.Votes
                        .Where(vote =>
                            vote.Target.Id == target.Id &&
                            vote.Target.GetType() == target.GetType())
                        .ToArray();
                }
            }
        }

        /// <summary>
        /// Keeps these tests focused on persistence concurrency rather than
        /// eligibility or target-availability decisions.
        /// </summary>
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
