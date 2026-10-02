using Atlas.Contracts.Notifications.V1;
using Atlas.Notifications;


namespace Atlas.Notifications.Tests;
[TestClass]
public sealed class NotificationFlowTests
{
    [TestMethod]
    public void Duplicate_event_preserves_one_notification_and_read_state()
    {
            var repository = new MemoryNotifications();
            var service = new NotificationService(repository, []);
            var recipient = Guid.NewGuid();
            var request = new NotificationRequestedV1(Guid.NewGuid(), recipient, Guid.NewGuid(),
                "NodeCommented", "Node", Guid.NewGuid(), DateTimeOffset.UtcNow);
            var first = service.Handle(request)!;
            Assert.AreEqual(first.Id, service.Handle(request)!.Id);
            service.MarkRead(recipient, first.Id);
            var reloaded = repository.Page(recipient, 0, 10);
            Assert.HasCount(1, reloaded);
            Assert.IsNotNull(reloaded[0].ReadAt);
            Assert.Throws<UnauthorizedAccessException>(() => service.Dismiss(Guid.NewGuid(), first.Id));
            service.Dismiss(recipient, first.Id);
            Assert.HasCount(0, repository.Page(recipient, 0, 10));
        
    }
    [TestMethod]
    public void Preferences_control_channels_and_failed_delivery_is_recorded()
    {

            var repository = new MemoryNotifications();
            var recipient = Guid.NewGuid();
            var preferences = repository.Preferences(recipient);
            preferences.DiscussionInApp = false;
            preferences.DiscussionEmail = true;
            repository.SavePreferences(preferences);
            var service = new NotificationService(repository, [new FailingEmail(), new SimulatedDelivery(DeliveryChannel.Push)]);
            var item = service.Handle(new NotificationRequestedV1(Guid.NewGuid(), recipient, Guid.NewGuid(),
                "NodeCommented", "Node", Guid.NewGuid(), DateTimeOffset.UtcNow))!;
            Assert.HasCount(0, service.Page(recipient, 0, 10));
            Assert.HasCount(1, item.DeliveryAttempts);
            Assert.AreEqual(DeliveryStatus.Failed, item.DeliveryAttempts[0].Status);
            Assert.AreEqual(DeliveryChannel.Email, item.DeliveryAttempts[0].Channel);
            Assert.IsNotNull(repository.Find(item.Id));
        
    }
    [TestMethod]
    public void Preferences_are_owned_by_the_authenticated_participant()
    {
        var repository = new MemoryNotifications();
        var service = new NotificationService(repository, []);
        var owner = Guid.NewGuid();
        var outsider = Guid.NewGuid();

        var preferences = service.Preferences(owner, owner);
        preferences.DiscussionEmail = true;
        service.SavePreferences(owner, preferences);

        Assert.IsTrue(service.Preferences(owner, owner).DiscussionEmail);
        Assert.Throws<UnauthorizedAccessException>(() => service.Preferences(outsider, owner));
        Assert.Throws<UnauthorizedAccessException>(() => service.SavePreferences(outsider, preferences));
    }

    private sealed class FailingEmail : INotificationDelivery
    {
        public DeliveryChannel Channel => DeliveryChannel.Email;
        public void Send(Notification item) => throw new InvalidOperationException("simulated outage");
    }
    private sealed class MemoryNotifications : INotificationRepository
    {
        private readonly Dictionary<Guid, Notification> _items = [];
        private readonly Dictionary<Guid, NotificationPreferences> _preferences = [];
        public Notification? Find(Guid id) => _items.GetValueOrDefault(id);
        public Notification? FindOccurrence(Guid occurrence, Guid recipient) => _items.Values.SingleOrDefault(item => item.OccurrenceId == occurrence && item.RecipientParticipantId == recipient);
        public IReadOnlyList<Notification> Page(Guid recipient, int offset, int limit) => _items.Values.Where(item => item.RecipientParticipantId == recipient && item.InAppVisible && item.DismissedAt == null).Skip(offset).Take(limit).ToList();
        public void Save(Notification item) => _items[item.Id] = item;
        public NotificationPreferences Preferences(Guid id) => _preferences.GetValueOrDefault(id) ?? new NotificationPreferences { ParticipantId = id };
        public void SavePreferences(NotificationPreferences item) => _preferences[item.ParticipantId] = item;
    }
}