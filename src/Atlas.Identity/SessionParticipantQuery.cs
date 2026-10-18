using Atlas.Participants.Participants;
using Atlas.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity;

/// <summary>Reload the Atlas profile linked to an Identity account without exposing EF rows to hosts.</summary>
public sealed class SessionParticipantQuery(AtlasDataContext database)
{
    public async Task<Participant> GetAsync(Guid userId)
    {
        var row = await database.ParticipantRows.AsNoTracking().SingleAsync(row => row.Id == userId);
        return Participant.Reconstitute(new ParticipantId(row.Id), row.DisplayName, row.Bio,
            row.IsActive, row.CreatedAt, row.UpdatedAt);
    }
}
