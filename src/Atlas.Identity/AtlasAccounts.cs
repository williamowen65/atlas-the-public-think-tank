using System.Data;
using System.Security.Claims;
using Atlas.Participants.Participants;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity;

public sealed record AccountRegistration(IdentityResult Result, ParticipantId? ParticipantId);

/// <summary>Coordinates account/profile creation. Credential operations remain owned by UserManager.</summary>
public sealed class AtlasAccounts(AtlasDataContext database, UserManager<AtlasIdentityUser> users)
{
    /// <summary>Creates a new Member, profile, and role link atomically; callers cannot choose IDs or roles.</summary>
    public async Task<AccountRegistration> RegisterAsync(string email, string password, string displayName,
        CancellationToken cancellationToken = default)
    {
        Participant participant;
        try { participant = new Participant(displayName, DateTimeOffset.UtcNow); }
        catch (ArgumentException)
        {
            return Failure("InvalidDisplayName", "A valid display name is required.");
        }
        if (string.IsNullOrWhiteSpace(email))
            return Failure("InvalidEmail", "A valid email address is required.");
        var user = new AtlasIdentityUser
        {
            Id = participant.Id.Value, UserName = email.Trim(), Email = email.Trim()
        };
        // The participant-name check and all Identity saves share this SQL transaction.
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        try
        {
            if (await database.ParticipantRows.AnyAsync(row => row.DisplayName == participant.DisplayName,
                    cancellationToken))
                return Failure("DuplicateDisplayName", "That display name is already in use.");
            database.ParticipantRows.Add(new ParticipantRow
            {
                Id = participant.Id.Value, DisplayName = participant.DisplayName, Bio = participant.Bio,
                IsActive = participant.IsActive, CreatedAt = participant.CreatedAt, UpdatedAt = participant.UpdatedAt
            });
            await database.SaveChangesAsync(cancellationToken);
            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded) return new(result, null);
            result = await users.AddToRoleAsync(user, AtlasRoles.Member);
            if (!result.Succeeded) return new(result, null);
            await transaction.CommitAsync(cancellationToken);
            return new(IdentityResult.Success, participant.Id);
        }
        finally
        {
            // A failed registration rolls back on disposal. Remove rolled-back tracked state as well,
            // so a later SaveChanges in this request cannot accidentally persist it.
            database.ChangeTracker.Clear();
        }
    }

    /// <summary>Translates an authenticated framework principal to an active, persisted Participant.</summary>
    public async Task<ParticipantId?> ResolveParticipantAsync(ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (principal.Identity?.IsAuthenticated != true) return null;
        if (!Guid.TryParse(users.GetUserId(principal), out var id) || id == Guid.Empty) return null;
        var user = await users.GetUserAsync(principal);
        if (user is null || !await users.IsEmailConfirmedAsync(user) || await users.IsLockedOutAsync(user))
            return null;
        return await database.ParticipantRows.AsNoTracking()
            .AnyAsync(row => row.Id == user.Id && row.IsActive, cancellationToken)
                ? new ParticipantId(user.Id) : null;
    }

    private static AccountRegistration Failure(string code, string description) =>
        new(IdentityResult.Failed(new IdentityError { Code = code, Description = description }), null);
}
