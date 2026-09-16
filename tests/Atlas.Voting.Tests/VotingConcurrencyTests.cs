using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests
{
    /// <summary>
    /// Verifies that competing vote commands using the same CastVote
    /// operation preserve one current vote per participant-target pair.
    ///
    /// These tests model concurrent callers inside one application process
    /// that share one CastVote instance and therefore one operation gate.
    /// Separate service-instance behavior is covered by
    /// VotingServiceInstanceConcurrencyTests.
    /// </summary>
    [TestClass]
    public sealed class VotingConcurrencyTests
    {
        /// <summary>
        /// Models two request handlers casting a first vote through the same
        /// CastVote instance.
        ///
        /// Both tasks are made ready before the starting gate opens. CastVote
        /// must serialize its complete lookup-and-save operation so that the
        /// second request observes and changes the first request's vote rather
        /// than creating a second current vote.
        /// </summary>
        [TestMethod]
        [TestCategory("Concurrency")]
        public async Task CastVote_CompetingFirstVotesThroughSameInstance_StoresOneCurrentVote()
        {
            var repository =
                new ThreadSafeTestVoteRepository();

            var castVote =
                CreateCastVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            await CastCompetingVotes(
                castVote,
                target,
                participantId);

            Assert.HasCount(
                1,
                repository.GetTargetVotes(target),
                "Competing requests created duplicate current votes for one participant-target pair.");
        }

        /// <summary>
        /// Verifies the aggregate consequence of protecting the shared
        /// CastVote operation.
        ///
        /// One participant must contribute one current rating even when two
        /// calls begin together. The final value may be either competing
        /// value because scheduling determines which request completes last.
        /// </summary>
        [TestMethod]
        [TestCategory("Concurrency")]
        public async Task GetVoteSummary_AfterCompetingVotesThroughSameInstance_CountsParticipantOnce()
        {
            var repository =
                new ThreadSafeTestVoteRepository();

            var castVote =
                CreateCastVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            await CastCompetingVotes(
                castVote,
                target,
                participantId);

            var summary =
                new GetVoteSummary(repository).Execute(
                    target);

            Assert.AreEqual(
                1,
                summary.VoteCount,
                "One participant should contribute only one unit of influence.");

            Assert.IsTrue(
                summary.AverageVote is 4.0 or 10.0,
                "The summary should reflect whichever competing vote completed last.");
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

        /// <summary>
        /// Makes both tasks ready before releasing them toward the same
        /// CastVote instance. Coordination happens before Execute so the test
        /// does not require both callers to enter a section that correct
        /// synchronization intentionally allows only one caller to enter.
        /// </summary>
        private static async Task CastCompetingVotes(
            CastVote castVote,
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

                return castVote.Execute(
                    target,
                    participantId,
                    4);
            });

            var secondRequest = Task.Run(() =>
            {
                ready.Signal();
                startGate.Wait();

                return castVote.Execute(
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

        /// <summary>
        /// Keeps the test collection structurally safe under concurrent
        /// access without enforcing participant-target uniqueness itself.
        /// The CastVote operation under test remains responsible for
        /// serializing the check-and-save sequence in this scenario.
        /// </summary>
        private sealed class ThreadSafeTestVoteRepository :
            IVoteRepository
        {
            private readonly object _gate = new();
            private readonly List<Vote> _votes = [];

            public Vote? GetByParticipantAndTarget(
                ParticipantId participantId,
                VoteTarget voteTarget)
            {
                lock (_gate)
                {
                    return _votes.SingleOrDefault(vote =>
                        vote.ParticipantId.Id == participantId.Id &&
                        vote.Target.Id == voteTarget.Id &&
                        vote.Target.GetType() == voteTarget.GetType());
                }
            }

            public void Save(Vote vote)
            {
                lock (_gate)
                {
                    var existingIndex = _votes.FindIndex(
                        existingVote => existingVote.Id == vote.Id);

                    if (existingIndex >= 0)
                    {
                        _votes[existingIndex] = vote;
                    }
                    else
                    {
                        _votes.Add(vote);
                    }
                }
            }

            public void Delete(VoteId id)
            {
                lock (_gate)
                {
                    _votes.RemoveAll(vote =>
                        vote.Id == id);
                }
            }

            public Vote? GetById(VoteId id)
            {
                lock (_gate)
                {
                    return _votes.SingleOrDefault(vote =>
                        vote.Id == id);
                }
            }

            public IReadOnlyCollection<Vote> GetTargetVotes(
                VoteTarget target)
            {
                lock (_gate)
                {
                    return _votes
                        .Where(vote =>
                            vote.Target.Id == target.Id &&
                            vote.Target.GetType() == target.GetType())
                        .ToArray();
                }
            }
        }

        /// <summary>
        /// Keeps the test focused on concurrency by allowing both requests
        /// through the Slice 3 eligibility and availability policy.
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
