using Atlas.Communities.Communities;

namespace Atlas.Communities.Tests;

[TestClass]
public sealed class ValidationTests
{
    [TestMethod]
    public void ReconstitutionRejectsUndefinedStatus()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentOutOfRangeException>(() => Community.Reconstitute(
            CommunityId.New(), "Community", "", Guid.NewGuid(), (CommunityStatus)999, now, now));
    }
}
