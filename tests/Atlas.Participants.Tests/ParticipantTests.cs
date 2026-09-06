using Atlas.Participants.Participants;

namespace Atlas.Participants.Tests;

[TestClass]
public class ParticipantTests
{
    [TestMethod]
    public void Constructor_InitializesProfile()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var participant = new Participant(
            "  Alice  ",
            "  Interested in climate policy.  ",
            createdAt);

        Assert.AreNotEqual(Guid.Empty, participant.Id.Value);
        Assert.AreEqual("Alice", participant.DisplayName);
        Assert.AreEqual(
            "Interested in climate policy.",
            participant.Bio);
        Assert.IsTrue(participant.IsActive);
        Assert.AreEqual(createdAt, participant.CreatedAt);
        Assert.AreEqual(createdAt, participant.UpdatedAt);
    }

    [TestMethod]
    public void Constructor_WithoutBio_DefaultsToEmpty()
    {
        var participant = new Participant(
            "Alice",
            DateTimeOffset.UtcNow);

        Assert.AreEqual(string.Empty, participant.Bio);
    }

    [TestMethod]
    public void Constructor_WithBioOverMaximumLength_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new Participant(
                "Alice",
                new string('a', Participant.MaximumBioLength + 1),
                DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void UpdateProfile_ChangesNameBioAndTimestamp()
    {
        var participant = new Participant(
            "Alice",
            "Old bio",
            DateTimeOffset.UtcNow);
        var changedAt = participant.CreatedAt.AddMinutes(1);

        participant.UpdateProfile(
            "Alice Smith",
            "New bio",
            changedAt);

        Assert.AreEqual("Alice Smith", participant.DisplayName);
        Assert.AreEqual("New bio", participant.Bio);
        Assert.AreEqual(changedAt, participant.UpdatedAt);
    }

    [TestMethod]
    public void UpdateProfile_WithUnchangedValues_IsNoOp()
    {
        var participant = new Participant(
            "Alice",
            "Bio",
            DateTimeOffset.UtcNow);
        var originalUpdatedAt = participant.UpdatedAt;

        participant.UpdateProfile(
            participant.DisplayName,
            participant.Bio,
            originalUpdatedAt.AddMinutes(1));

        Assert.AreEqual(originalUpdatedAt, participant.UpdatedAt);
    }

    [TestMethod]
    public void UpdateProfile_WithInvalidBio_DoesNotPartiallyRename()
    {
        var participant = new Participant(
            "Alice",
            "Bio",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(
            () => participant.UpdateProfile(
                "Changed name",
                new string('a', Participant.MaximumBioLength + 1),
                participant.CreatedAt.AddMinutes(1)));

        Assert.AreEqual("Alice", participant.DisplayName);
        Assert.AreEqual("Bio", participant.Bio);
    }

    [TestMethod]
    public void Reconstitute_RestoresBio()
    {
        var id = ParticipantId.New();
        var createdAt = DateTimeOffset.UtcNow.AddHours(-1);
        var updatedAt = createdAt.AddMinutes(30);

        var participant = Participant.Reconstitute(
            id,
            "Alice",
            "Stored bio",
            isActive: true,
            createdAt,
            updatedAt);

        Assert.AreEqual(id, participant.Id);
        Assert.AreEqual("Stored bio", participant.Bio);
        Assert.AreEqual(updatedAt, participant.UpdatedAt);
    }
}
