using Atlas.Contracts.Notifications.V1;
using Atlas.ConsoleApp.Storage;
using Atlas.Notifications;


namespace Atlas.Persistence.Tests;
[TestClass]
public sealed class NotificationFlowTests
{
    [TestMethod]
    public void Duplicate_event_and_restart_preserve_one_read_notification()
    {
        using var database = SqlTestDatabase.Create();
            var repository = new SqlNotificationRepository(database.Open);
            var service = new NotificationService(repository, []);
            var recipient = database.AddParticipant();
            var request = new NotificationRequestedV1(Guid.NewGuid(), recipient, database.AddParticipant(),
                "NodeCommented", "Node", Guid.NewGuid(), DateTimeOffset.UtcNow);
            var first = service.Handle(request)!;
            Assert.AreEqual(first.Id, service.Handle(request)!.Id);
            service.MarkRead(recipient, first.Id);
            var reloaded = new SqlNotificationRepository(database.Open).Page(recipient, 0, 10);
            Assert.HasCount(1, reloaded);
            Assert.IsNotNull(reloaded[0].ReadAt);
            Assert.Throws<UnauthorizedAccessException>(() => service.Dismiss(Guid.NewGuid(), first.Id));
            service.Dismiss(recipient, first.Id);
            Assert.HasCount(0, repository.Page(recipient, 0, 10));
        
    }
    [TestMethod]
    public void Preferences_control_channels_and_failed_delivery_is_recorded()
    {
        using var database = SqlTestDatabase.Create();

            var repository = new SqlNotificationRepository(database.Open);
            var recipient = database.AddParticipant();
            var preferences = repository.Preferences(recipient);
            preferences.DiscussionInApp = false;
            preferences.DiscussionEmail = true;
            repository.SavePreferences(preferences);
            var service = new NotificationService(repository, [new FailingEmail(), new SimulatedDelivery(DeliveryChannel.Push)]);
            var item = service.Handle(new NotificationRequestedV1(Guid.NewGuid(), recipient, database.AddParticipant(),
                "NodeCommented", "Node", Guid.NewGuid(), DateTimeOffset.UtcNow))!;
            Assert.HasCount(0, service.Page(recipient, 0, 10));
            Assert.HasCount(1, item.DeliveryAttempts);
            Assert.AreEqual(DeliveryStatus.Failed, item.DeliveryAttempts[0].Status);
            Assert.AreEqual(DeliveryChannel.Email, item.DeliveryAttempts[0].Channel);
            Assert.IsNotNull(repository.Find(item.Id));
        
    }
    private sealed class FailingEmail : INotificationDelivery
    {
        public DeliveryChannel Channel => DeliveryChannel.Email;
        public void Send(Notification item) => throw new InvalidOperationException("simulated outage");
    }
}