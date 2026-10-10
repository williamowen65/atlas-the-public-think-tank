using Atlas.Voting;
using Atlas.Voting.Target;
using Atlas.Voting.Votes;
namespace Atlas.Voting.Tests;
[TestClass]
public sealed class ValidationBoundaryTests
{
    [TestMethod]
    public void VoteConsumersRejectDefaultIdentityAndNullParticipant()
    {
        var now = DateTimeOffset.UtcNow;
        var target = new NodeVoteTarget(Guid.NewGuid());
        Assert.Throws<ArgumentNullException>(() => new Vote(target, null!, 1));
        Assert.Throws<ArgumentException>(() => Vote.Reconstitute(default, target, new ParticipantId(Guid.NewGuid()), 1, now, now));
        Assert.Throws<ArgumentNullException>(() => Vote.Reconstitute(VoteId.New(), target, null!, 1, now, now));
    }
}
