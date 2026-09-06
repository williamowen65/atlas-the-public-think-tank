using Atlas.Participants.Participants;
using Atlas.Participants.Profiles;

namespace Atlas.Participants.Tests;

[TestClass]
public class UpdateParticipantProfileTests
{
    [TestMethod]
    public void Execute_WhenActorOwnsProfile_UpdatesAndSaves()
    {
        var repository = new FakeParticipantRepository();
        var participant = new Participant(
            "Alice",
            "Old bio",
            DateTimeOffset.UtcNow);
        repository.Save(participant);
        var workflow = new UpdateParticipantProfile(repository);
        var changedAt = participant.CreatedAt.AddMinutes(1);

        var updated = workflow.Execute(
            participant.Id,
            participant.Id,
            "Alice Smith",
            "New bio",
            changedAt);

        Assert.AreEqual("Alice Smith", updated.DisplayName);
        Assert.AreEqual("New bio", updated.Bio);
        Assert.AreEqual(changedAt, updated.UpdatedAt);
        Assert.AreEqual(2, repository.SaveCount);
    }

    [TestMethod]
    public void Execute_WhenActorDoesNotOwnProfile_ThrowsWithoutSaving()
    {
        var repository = new FakeParticipantRepository();
        var participant = new Participant(
            "Bob",
            "Bob's bio",
            DateTimeOffset.UtcNow);
        repository.Save(participant);
        var workflow = new UpdateParticipantProfile(repository);

        Assert.Throws<UnauthorizedAccessException>(
            () => workflow.Execute(
                ParticipantId.New(),
                participant.Id,
                "Changed Bob",
                "Changed bio",
                DateTimeOffset.UtcNow));

        Assert.AreEqual("Bob", participant.DisplayName);
        Assert.AreEqual("Bob's bio", participant.Bio);
        Assert.AreEqual(1, repository.SaveCount);
    }

    [TestMethod]
    public void Execute_WhenProfileDoesNotExist_Throws()
    {
        var repository = new FakeParticipantRepository();
        var participantId = ParticipantId.New();
        var workflow = new UpdateParticipantProfile(repository);

        Assert.Throws<InvalidOperationException>(
            () => workflow.Execute(
                participantId,
                participantId,
                "Alice",
                "Bio",
                DateTimeOffset.UtcNow));

        Assert.AreEqual(0, repository.SaveCount);
    }

    private sealed class FakeParticipantRepository
        : IParticipantRepository
    {
        private readonly Dictionary<ParticipantId, Participant>
            _participants = [];

        public int SaveCount { get; private set; }

        public IReadOnlyCollection<Participant> GetAll()
        {
            return _participants.Values.ToList();
        }

        public Participant? GetById(ParticipantId id)
        {
            return _participants.GetValueOrDefault(id);
        }

        public void Save(Participant participant)
        {
            _participants[participant.Id] = participant;
            SaveCount++;
        }
    }
}
