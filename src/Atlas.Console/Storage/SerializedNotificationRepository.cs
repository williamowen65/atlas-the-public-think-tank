using System.Text.Json;
using Atlas.Notifications;

namespace Atlas.ConsoleApp.Storage;

/// <summary>Prototype file adapter; SQL must enforce the occurrence/recipient unique key atomically.</summary>
public sealed class SerializedNotificationRepository(string notificationsPath, string preferencesPath) : INotificationRepository
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private List<Notification> Read() => SerializedStorage.Read<List<Notification>>(notificationsPath, _options) ?? [];
    public Notification? Find(Guid id) => Read().SingleOrDefault(item => item.Id == id);
    public Notification? FindOccurrence(Guid occurrenceId, Guid recipientId) =>
        Read().SingleOrDefault(item => item.OccurrenceId == occurrenceId && item.RecipientParticipantId == recipientId);
    public IReadOnlyList<Notification> Page(Guid recipientId, int offset, int limit) =>
        Read().Where(item => item.RecipientParticipantId == recipientId && item.InAppVisible && item.DismissedAt is null)
            .OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id)
            .Skip(offset).Take(limit).ToList();
    public void Save(Notification item)
    {
        var items = Read();
        var index = items.FindIndex(existing => existing.Id == item.Id);
        if (index < 0) items.Add(item); else items[index] = item;
        SerializedStorage.Write(notificationsPath, items, _options);
    }
    public NotificationPreferences Preferences(Guid participantId) =>
        (SerializedStorage.Read<List<NotificationPreferences>>(preferencesPath, _options) ?? [])
            .SingleOrDefault(item => item.ParticipantId == participantId)
        ?? new NotificationPreferences { ParticipantId = participantId };
    public void SavePreferences(NotificationPreferences preferences)
    {
        var items = SerializedStorage.Read<List<NotificationPreferences>>(preferencesPath, _options) ?? [];
        var index = items.FindIndex(item => item.ParticipantId == preferences.ParticipantId);
        if (index < 0) items.Add(preferences); else items[index] = preferences;
        SerializedStorage.Write(preferencesPath, items, _options);
    }
}
