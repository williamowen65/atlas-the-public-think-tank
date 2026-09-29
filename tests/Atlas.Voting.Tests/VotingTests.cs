using Atlas.Voting.Target;
using Atlas.Voting.Value;
using Atlas.Voting.Votes;
using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;

namespace Atlas.Voting.Tests
{
    [TestClass]
    public sealed class VotingTests
    {
        [TestMethod]
        public void CastVote_SavesValidVote()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = CreateCastVote(repository);

            var target = new NodeVoteTarget(Guid.NewGuid());
            var participantId = new ParticipantId(Guid.NewGuid());

            var vote = castVote.Execute(
                target,
                participantId,
                7);

            Assert.AreSame(
                vote,
                repository.GetById(vote.Id));
        }

        /// <summary>
        /// Verifies that casting again changes the existing current vote
        /// without creating duplicate participant-target influence.
        /// </summary>
        [TestMethod]
        public void CastVote_WhenParticipantAlreadyVoted_ChangesExistingVote()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = CreateCastVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            var originalVote = castVote.Execute(
                target,
                participantId,
                7);

            var originalCreatedAt = originalVote.CreatedAt;
            var originalUpdatedAt = originalVote.UpdatedAt;

            var changedVote = castVote.Execute(
                target,
                participantId,
                9);

            Assert.AreEqual(
                originalVote.Id,
                changedVote.Id);

            Assert.AreEqual(
                9,
                changedVote.Value.Value);

            Assert.AreEqual(
                originalCreatedAt,
                changedVote.CreatedAt);

            Assert.IsTrue(
                changedVote.UpdatedAt > originalUpdatedAt);

            Assert.HasCount(
                1,
                repository.GetTargetVotes(target));
        }

        /// <summary>
        /// Verifies that different participants can vote on the
        /// same target independently.
        /// </summary>
        [TestMethod]
        public void CastVote_WithDifferentParticipants_SavesBothVotes()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = CreateCastVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var firstParticipantId =
                new ParticipantId(Guid.NewGuid());

            var secondParticipantId =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                firstParticipantId,
                8);

            castVote.Execute(
                target,
                secondParticipantId,
                4);

            var targetVotes =
                repository.GetTargetVotes(target);

            Assert.HasCount(
                2,
                targetVotes);

            var firstVote =
                repository.GetByParticipantAndTarget(
                    firstParticipantId,
                    target);

            var secondVote =
                repository.GetByParticipantAndTarget(
                    secondParticipantId,
                    target);

            Assert.IsNotNull(firstVote);
            Assert.IsNotNull(secondVote);

            Assert.AreEqual(
                8,
                firstVote.Value.Value);

            Assert.AreEqual(
                4,
                secondVote.Value.Value);
        }

        /// <summary>
        /// Verifies that one participant may vote independently
        /// on different targets.
        /// </summary>
        [TestMethod]
        public void CastVote_SameParticipantDifferentTargets_SavesBothVotes()
        {
            var repository =
                new InMemoryVoteRepository();

            var castVote =
                CreateCastVote(repository);

            var participantId =
                new ParticipantId(Guid.NewGuid());

            var firstTarget =
                new NodeVoteTarget(Guid.NewGuid());

            var secondTarget =
                new NodeVoteTarget(Guid.NewGuid());

            castVote.Execute(
                firstTarget,
                participantId,
                8);

            castVote.Execute(
                secondTarget,
                participantId,
                6);

            Assert.HasCount(
                1,
                repository.GetTargetVotes(firstTarget));

            Assert.HasCount(
                1,
                repository.GetTargetVotes(secondTarget));
        }

        /// <summary>
        /// Verifies that an unvoted target reports zero votes and no average.
        /// </summary>
        [TestMethod]
        public void GetVoteSummary_WithNoVotes_ReturnsEmptySummary()
        {
            var repository = new InMemoryVoteRepository();
            var getVoteSummary = new GetVoteSummary(repository);

            var summary = getVoteSummary.Execute(
                new NodeVoteTarget(Guid.NewGuid()));

            Assert.AreEqual(
                0,
                summary.VoteCount);

            Assert.IsNull(
                summary.AverageVote);

            Assert.IsNull(
                summary.CurrentParticipantVote);
        }

        /// <summary>
        /// Verifies that the summary includes only votes belonging
        /// to the requested target.
        /// </summary>
        [TestMethod]
        public void GetVoteSummary_WithMultipleTargets_AggregatesRequestedTarget()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = CreateCastVote(repository);
            var getVoteSummary = new GetVoteSummary(repository);

            var requestedTarget =
                new NodeVoteTarget(Guid.NewGuid());

            var otherTarget =
                new NodeVoteTarget(Guid.NewGuid());

            castVote.Execute(
                requestedTarget,
                new ParticipantId(Guid.NewGuid()),
                8);

            castVote.Execute(
                requestedTarget,
                new ParticipantId(Guid.NewGuid()),
                4);

            castVote.Execute(
                otherTarget,
                new ParticipantId(Guid.NewGuid()),
                10);

            var summary =
                getVoteSummary.Execute(requestedTarget);

            Assert.AreEqual(
                2,
                summary.VoteCount);

            Assert.AreEqual(
                6.0,
                summary.AverageVote);
        }

        /// <summary>
        /// Verifies that the summary reports only the requested
        /// participant's current vote.
        /// </summary>
        [TestMethod]
        public void GetVoteSummary_WithParticipant_ReturnsThatParticipantsVote()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = CreateCastVote(repository);
            var getVoteSummary = new GetVoteSummary(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var requestedParticipant =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                requestedParticipant,
                8);

            castVote.Execute(
                target,
                new ParticipantId(Guid.NewGuid()),
                4);

            var summary = getVoteSummary.Execute(
                target,
                requestedParticipant);

            Assert.AreEqual(
                8,
                summary.CurrentParticipantVote);
        }

        [TestMethod]
        public void Vote_WithNodeTarget_CreatesNodeImportanceRating()
        {
            var target = new NodeVoteTarget(Guid.NewGuid());
            var participantId = new ParticipantId(Guid.NewGuid());

            var vote = new Vote(target, participantId, 7);

            Assert.IsInstanceOfType<NodeImportanceRating>(vote.Value);
            Assert.AreEqual(7, vote.Value.Value);
        }

        [TestMethod]
        public void Vote_WithUnsupportedTarget_ThrowsException()
        {
            var unsupportedTarget = new VoteTarget(Guid.NewGuid());
            var participantId = new ParticipantId(Guid.NewGuid());

            Assert.Throws<ArgumentException>(() =>
            {
                new Vote(unsupportedTarget, participantId, 7);
            });
        }


        [TestMethod]
        public void Vote_WithEmptyGuid_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var target = new NodeVoteTarget(Guid.Empty);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                var participantId = new ParticipantId(Guid.Empty);
            });
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(10)]
        public void NodeImportanceRating_CanOnlyBeBetween0to10Inclusive(int value)
        {
            NodeImportanceRating nodeRating = new NodeImportanceRating(value);

            Assert.AreEqual(value,nodeRating.Value);

        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(11)]
        public void NodeImportanceRating_FailsOutside0to10Inclusive(int value)
        {
           
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                NodeImportanceRating nodeRating = new NodeImportanceRating(value);
            });
        }

        [TestMethod]
        public void NodeImportanceRating_ValueHasNoSetter()
        {
            var valueProperty = typeof(NodeImportanceRating)
                .GetProperty(nameof(NodeImportanceRating.Value));

            Assert.IsNotNull(valueProperty);
            Assert.IsNull(valueProperty.GetSetMethod(nonPublic: true));
        }

        /// <summary>
        /// Verifies that only the acting participant's vote is removed
        /// and that current aggregates immediately exclude it.
        /// </summary>
        [TestMethod]
        public void UndoVote_WhenOwnedVoteExists_RemovesItFromCurrentResults()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = CreateCastVote(repository);
            var undoVote = CreateUndoVote(repository);
            var getVoteSummary = new GetVoteSummary(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                participantId,
                8);

            castVote.Execute(
                target,
                new ParticipantId(Guid.NewGuid()),
                4);

            var removed = undoVote.Execute(
                target,
                participantId);

            var summary = getVoteSummary.Execute(
                target,
                participantId);

            Assert.IsTrue(removed);

            Assert.AreEqual(
                1,
                summary.VoteCount);

            Assert.AreEqual(
                4.0,
                summary.AverageVote);

            Assert.IsNull(
                summary.CurrentParticipantVote);

            Assert.HasCount(
                1,
                repository.GetTargetVotes(target));
        }

        /// <summary>
        /// Verifies that a participant cannot undo another
        /// participant's current vote.
        /// </summary>
        [TestMethod]
        public void UndoVote_WhenParticipantDoesNotOwnVote_LeavesVoteUnchanged()
        {
            var repository = new InMemoryVoteRepository();
            var castVote = CreateCastVote(repository);
            var undoVote = CreateUndoVote(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            castVote.Execute(
                target,
                new ParticipantId(Guid.NewGuid()),
                6);

            var removed = undoVote.Execute(
                target,
                new ParticipantId(Guid.NewGuid()));

            Assert.IsFalse(removed);

            Assert.HasCount(
                1,
                repository.GetTargetVotes(target));
        }

        /// <summary>
        /// Verifies that undo is idempotent when no current vote exists.
        /// </summary>
        [TestMethod]
        public void UndoVote_WhenVoteDoesNotExist_ReturnsFalse()
        {
            var repository = new InMemoryVoteRepository();
            var undoVote = CreateUndoVote(repository);

            var removed = undoVote.Execute(
                new NodeVoteTarget(Guid.NewGuid()),
                new ParticipantId(Guid.NewGuid()));

            Assert.IsFalse(removed);
        }

        [TestMethod]
        public void CastVote_WhenVoteChanges_RecalculatesSummary()
        {
            var repository =
                new InMemoryVoteRepository();

            var castVote =
                CreateCastVote(repository);

            var getVoteSummary =
                new GetVoteSummary(repository);

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var changingParticipant =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                changingParticipant,
                8);

            castVote.Execute(
                target,
                new ParticipantId(Guid.NewGuid()),
                4);

            castVote.Execute(
                target,
                changingParticipant,
                10);

            var summary =
                getVoteSummary.Execute(target);

            Assert.AreEqual(
                2,
                summary.VoteCount);

            Assert.AreEqual(
                7.0,
                summary.AverageVote);
        }

        /// <summary>
        /// Verifies that a missing authenticated actor cannot mutate votes.
        /// </summary>
        [TestMethod]
        public void CastVote_WithoutParticipant_ThrowsException()
        {
            var repository = new InMemoryVoteRepository();
            var eligibility = new TestVotingEligibility();
            var castVote = new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));

            Assert.Throws<ArgumentNullException>(() =>
            {
                castVote.Execute(
                    new NodeVoteTarget(Guid.NewGuid()),
                    null!,
                    7);
            });
        }

        /// <summary>
        /// Verifies that an ineligible participant cannot create a vote.
        /// </summary>
        [TestMethod]
        public void CastVote_WithIneligibleParticipant_RejectsCommand()
        {
            var repository = new InMemoryVoteRepository();
            var eligibility = new TestVotingEligibility
            {
                ParticipantsAreEligible = false
            };

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var castVote = new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));

            Assert.Throws<InvalidOperationException>(() =>
            {
                castVote.Execute(
                    target,
                    new ParticipantId(Guid.NewGuid()),
                    7);
            });

            Assert.HasCount(
                0,
                repository.GetTargetVotes(target));
        }

        /// <summary>
        /// Verifies that an unavailable target rejects a new vote.
        /// </summary>
        [TestMethod]
        public void CastVote_WithUnavailableTarget_RejectsNewVote()
        {
            var repository = new InMemoryVoteRepository();
            var eligibility = new TestVotingEligibility
            {
                TargetsAreAvailable = false
            };

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var castVote = new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));

            Assert.Throws<InvalidOperationException>(() =>
            {
                castVote.Execute(
                    target,
                    new ParticipantId(Guid.NewGuid()),
                    7);
            });

            Assert.HasCount(
                0,
                repository.GetTargetVotes(target));
        }

        /// <summary>
        /// Verifies that an unavailable target rejects changing its
        /// existing current vote.
        /// </summary>
        [TestMethod]
        public void CastVote_WhenTargetBecomesUnavailable_RejectsChange()
        {
            var repository = new InMemoryVoteRepository();
            var eligibility = new TestVotingEligibility();
            var castVote = new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                participantId,
                4);

            eligibility.TargetsAreAvailable = false;

            Assert.Throws<InvalidOperationException>(() =>
            {
                castVote.Execute(
                    target,
                    participantId,
                    9);
            });

            var existingVote =
                repository.GetByParticipantAndTarget(
                    participantId,
                    target);

            Assert.IsNotNull(existingVote);

            Assert.AreEqual(
                4,
                existingVote.Value.Value);
        }

        /// <summary>
        /// Verifies that an unavailable target freezes undo mutations.
        /// </summary>
        [TestMethod]
        public void UndoVote_WhenTargetBecomesUnavailable_RejectsUndo()
        {
            var repository = new InMemoryVoteRepository();
            var eligibility = new TestVotingEligibility();
            var castVote = new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));

            var undoVote = new UndoVote(
                repository,
                new VoteMutationPolicy(eligibility));

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            var vote = castVote.Execute(
                target,
                participantId,
                8);

            eligibility.TargetsAreAvailable = false;

            Assert.Throws<InvalidOperationException>(() =>
            {
                undoVote.Execute(
                    target,
                    participantId);
            });

            Assert.IsNotNull(
                repository.GetById(vote.Id));
        }

        /// <summary>
        /// Verifies that current summaries remain readable after the
        /// target becomes unavailable.
        /// </summary>
        [TestMethod]
        public void GetVoteSummary_WhenTargetBecomesUnavailable_RemainsReadable()
        {
            var repository = new InMemoryVoteRepository();
            var eligibility = new TestVotingEligibility();
            var castVote = new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));

            var target =
                new NodeVoteTarget(Guid.NewGuid());

            var participantId =
                new ParticipantId(Guid.NewGuid());

            castVote.Execute(
                target,
                participantId,
                8);

            eligibility.TargetsAreAvailable = false;

            var summary =
                new GetVoteSummary(repository).Execute(
                    target,
                    participantId);

            Assert.AreEqual(
                1,
                summary.VoteCount);

            Assert.AreEqual(
                8.0,
                summary.AverageVote);

            Assert.AreEqual(
                8,
                summary.CurrentParticipantVote);
        }

        /// <summary>
        /// Verifies that availability is evaluated for each target
        /// rather than inherited from a separate Node relationship.
        /// </summary>
        [TestMethod]
        public void CastVote_WithSeparateTargets_EvaluatesEachTargetIndependently()
        {
            var repository = new InMemoryVoteRepository();
            var eligibility = new TestVotingEligibility();
            var castVote = new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));

            var unavailableTarget =
                new NodeVoteTarget(Guid.NewGuid());

            var availableTarget =
                new NodeVoteTarget(Guid.NewGuid());

            eligibility.UnavailableTargetIds.Add(
                unavailableTarget.Id);

            var participantId =
                new ParticipantId(Guid.NewGuid());

            Assert.Throws<InvalidOperationException>(() =>
            {
                castVote.Execute(
                    unavailableTarget,
                    participantId,
                    5);
            });

            castVote.Execute(
                availableTarget,
                participantId,
                9);

            Assert.HasCount(
                0,
                repository.GetTargetVotes(unavailableTarget));

            Assert.HasCount(
                1,
                repository.GetTargetVotes(availableTarget));
        }

        private static CastVote CreateCastVote(
            IVoteRepository repository)
        {
            var eligibility =
                new TestVotingEligibility();

            return new CastVote(
                repository,
                new VoteMutationPolicy(eligibility));
        }

        private static UndoVote CreateUndoVote(
            IVoteRepository repository)
        {
            var eligibility =
                new TestVotingEligibility();

            return new UndoVote(
                repository,
                new VoteMutationPolicy(eligibility));
        }

        private sealed class TestVotingEligibility :
            IVotingEligibility
        {
            public bool ParticipantsAreEligible { get; set; } = true;

            public bool TargetsAreAvailable { get; set; } = true;

            public HashSet<Guid> UnavailableTargetIds { get; } = [];

            public bool IsEligible(
                ParticipantId participantId)
            {
                return ParticipantsAreEligible;
            }

            public bool IsAvailable(
                VoteTarget target)
            {
                return TargetsAreAvailable &&
                       !UnavailableTargetIds.Contains(target.Id);
            }
        }

    }
}
