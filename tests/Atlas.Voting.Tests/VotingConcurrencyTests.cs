using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;

namespace Atlas.Voting.Tests
{
    /// <summary>
    /// Verifies that competing vote commands using the same CastVote
    /// operation preserve one current vote per participant-target pair.
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
            using var startGate =
                new ManualResetEventSlim(false);

            var firstRequest = Task.Run(() =>
            {
                startGate.Wait();

                return castVote.Execute(
                    target,
                    participantId,
                    4);
            });

            var secondRequest = Task.Run(() =>
            {
                startGate.Wait();

                return castVote.Execute(
                    target,
                    participantId,
                    10);
            });

            startGate.Set();

            await Task.WhenAll(
                firstRequest,
                secondRequest);
        }

        /// <summary>
        /// Thread-safe test repository used to observe the result of competing
        /// CastVote commands without introducing collection corruption.
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
                Vote? existingVote;

                lock (_gate)
                {
                    existingVote = _votes.SingleOrDefault(vote =>
                        vote.ParticipantId.Id == participantId.Id &&
                        vote.Target.Id == voteTarget.Id &&
                        vote.Target.GetType() == voteTarget.GetType());
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
