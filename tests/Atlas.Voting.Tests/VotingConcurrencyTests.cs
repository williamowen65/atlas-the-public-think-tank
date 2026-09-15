using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests
{
    /// <summary>
    /// Demonstrates the current check-then-save race for competing commands
    /// that use the same participant-target pair.
    ///
    /// These tests intentionally describe the required safe behavior and are
    /// expected to fail until Slice 4 adds an atomic concurrency mechanism.
    /// </summary>
    [TestClass]
    public sealed class VotingConcurrencyTests
    {
        /// <summary>
        /// Models two request handlers casting a first vote for the same
        /// participant and target.
        ///
        /// The coordinated repository pauses both commands after each has
        /// observed that no vote exists. Both are then released to save. This
        /// proves that application-level lookup followed by a separate save
        /// cannot by itself enforce one current participant-target vote.
        /// </summary>
        [TestMethod]
        [TestCategory("Concurrency")]
        public async Task CastVote_CompetingFirstVotesForSameParticipantTarget_StoresOneCurrentVote()
        {
            var repository =
                new CoordinatedFirstVoteRepository();

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
        /// Models the aggregate consequence of the same race.
        ///
        /// A participant should contribute one current rating to a target.
        /// When both competing inserts survive, GetVoteSummary counts that
        /// participant twice and averages two values instead of reporting the
        /// single accepted current vote.
        /// </summary>
        [TestMethod]
        [TestCategory("Concurrency")]
        public async Task GetVoteSummary_AfterCompetingFirstVotes_CountsParticipantOnce()
        {
            var repository =
                new CoordinatedFirstVoteRepository();

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
                "The duplicate race gave one participant more than one unit of influence.");

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
            CastVote castVote,
            VoteTarget target,
            ParticipantId participantId)
        {
            var firstRequest = Task.Run(() =>
                castVote.Execute(
                    target,
                    participantId,
                    4));

            var secondRequest = Task.Run(() =>
                castVote.Execute(
                    target,
                    participantId,
                    10));

            await Task.WhenAll(
                firstRequest,
                secondRequest);
        }

        /// <summary>
        /// Test double that makes the race reproducible.
        ///
        /// Each competing lookup reads under a lock and then waits at the
        /// barrier. Neither request may proceed to Save until both lookups
        /// have returned the same "no current vote" observation. Save itself
        /// is locked only to keep the test collection structurally safe; it
        /// intentionally provides no participant-target uniqueness rule.
        /// </summary>
        private sealed class CoordinatedFirstVoteRepository :
            IVoteRepository,
            IDisposable
        {
            private readonly object _gate = new();
            private readonly List<Vote> _votes = [];
            private readonly Barrier _competingLookups = new(2);
            private int _lookupCount;

            public Vote? GetByParticipantAndTarget(
                ParticipantId participantId,
                VoteTarget voteTarget)
            {
                Vote? existingVote;

                lock (_gate)
                {
                    existingVote = _votes.SingleOrDefault(vote =>
                        vote.ParticipantId.Id == participantId.Id &&
                        vote.Target.Id == voteTarget.Id &&
                        vote.Target.GetType() == voteTarget.GetType());
                }

                var lookupNumber =
                    Interlocked.Increment(ref _lookupCount);

                if (lookupNumber <= 2 &&
                    !_competingLookups.SignalAndWait(
                        TimeSpan.FromSeconds(10)))
                {
                    throw new TimeoutException(
                        "Both competing vote lookups did not reach the test barrier.");
                }

                return existingVote;
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

            public void Dispose()
            {
                _competingLookups.Dispose();
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
