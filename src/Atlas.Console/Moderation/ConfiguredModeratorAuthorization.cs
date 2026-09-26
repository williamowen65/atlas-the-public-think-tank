using Atlas.Moderation;

namespace Atlas.ConsoleApp.Moderation;

/// <summary>Prototype host configuration; production authentication must establish the actor.</summary>
internal sealed class ConfiguredModeratorAuthorization : IModeratorAuthorization
{
    private readonly HashSet<Guid> _moderators;

    public ConfiguredModeratorAuthorization(string? ids)
    {
        _moderators = (ids ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Guid.TryParse(value.Trim(), out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty).ToHashSet();
    }

    public bool IsAtlasModerator(Guid participantId) => _moderators.Contains(participantId);
}
