using Atlas.Participants.Participants;
namespace Atlas.Participants.Tests;
[TestClass]
public sealed class ValidationBoundaryTests
{
    [TestMethod]
    public void EarlierEditsAndDeactivationPreserveProfile()
    {
        var now = DateTimeOffset.UtcNow;
        var item = new Participant("Name", "Bio", now);
        Assert.Throws<ArgumentException>(() => item.UpdateProfile("New", "New bio", now.AddMinutes(-1)));
        Assert.Throws<ArgumentException>(() => item.Deactivate(now.AddMinutes(-1)));
        Assert.AreEqual("Name", item.DisplayName);
        Assert.AreEqual("Bio", item.Bio);
        Assert.IsTrue(item.IsActive);
        Assert.AreEqual(now, item.UpdatedAt);
        item.Deactivate(now);
        item.Deactivate(now.AddMinutes(-1));
    }
}
