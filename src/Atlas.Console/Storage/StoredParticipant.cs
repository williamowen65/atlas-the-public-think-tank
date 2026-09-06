namespace Atlas.ConsoleApp.Storage;

public sealed class StoredParticipant
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
