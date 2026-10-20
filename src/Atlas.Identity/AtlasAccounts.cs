using Atlas.Contracts.Observability;
using System.Data;
using System.Security.Claims;
using Atlas.Participants.Participants;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity;

public sealed record AccountRegistration(
    IdentityResult Result,
    ParticipantId? ParticipantId
);

/// <summary>Coordinates account/profile creation. Credential operations remain owned by UserManager.</summary>
public sealed class AtlasAccounts(
    AtlasDataContext database,
    UserManager<AtlasIdentityUser> users
)
{
    /// <summary>
    /// Creates a new Member, Participant profile, and role link atomically; callers cannot choose IDs or roles.
    /// Registration intentionally leaves the Identity email unconfirmed. In the future REST API/web registration
    /// flow, the host will use UserManager to generate an email-confirmation token and an email provider will send
    /// the confirmation link. The confirmation endpoint will then pass that token to UserManager.ConfirmEmailAsync.
    /// AtlasAccounts owns coordinated account/profile creation; email delivery and HTTP confirmation belong to the host.
    /// The console's --confirm-email command is only a local-development substitute for that future delivery flow.
    /// </summary>
    public async Task<AccountRegistration> RegisterAsync(string email, string password, string displayName, CancellationToken cancellationToken = default)
    {
        using var operation = Telemetry.BeginOperation();
        try
        {
            var result = await RegisterCoreAsync(email, password, displayName, cancellationToken);
            Telemetry.Record(Telemetry.Create(EventKind.Registration, result.ParticipantId?.Value,
                result.Result.Succeeded ? EventOutcome.Succeeded : EventOutcome.Denied,
                result.Result.Succeeded ? EventReason.None : EventReason.InvalidInput));
            return result;
        }
        catch (Exception error)
        {
            Telemetry.Record(Telemetry.Create(EventKind.Registration, outcome: EventOutcome.Failed, reason: Telemetry.Reason(error)));
            throw;
        }
    }

    private async Task<AccountRegistration> RegisterCoreAsync(
        string email,
        string password,
        string displayName,
        CancellationToken cancellationToken = default
    )
    {
        Participant participant;
        try { participant = new Participant(
            displayName,
            AtlasTime.UtcNow
        ); }
        catch (ArgumentException)
        {
            return Failure(
                "InvalidDisplayName",
                "A valid display name is required."
            );
        }
        if (string.IsNullOrWhiteSpace(email))
            return Failure(
                "InvalidEmail",
                "A valid email address is required."
            );
        var user = new AtlasIdentityUser
        {
            Id = participant.Id.Value,
            UserName = email.Trim(),
            Email = email.Trim()
        };
        // The participant-name check and all Identity saves share this SQL transaction.
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken
        );
        try
        {
            if (await database.ParticipantRows.AnyAsync(
                row => row.DisplayName == participant.DisplayName,
                cancellationToken
            ))
                return Failure(
                    "DuplicateDisplayName",
                    "That display name is already in use."
                );
            database.ParticipantRows.Add(new ParticipantRow
            {
                Id = participant.Id.Value,
                DisplayName = participant.DisplayName,
                Bio = participant.Bio,
                IsActive = participant.IsActive,
                CreatedAt = participant.CreatedAt,
                UpdatedAt = participant.UpdatedAt
            });
            await database.SaveChangesAsync(cancellationToken);
            var result = await users.CreateAsync(
                user,
                password
            );
            if (!result.Succeeded) return new(
                result,
                null
            );
            result = await users.AddToRoleAsync(
                user,
                AtlasRoles.Member
            );
            if (!result.Succeeded) return new(
                result,
                null
            );
            await transaction.CommitAsync(cancellationToken);
            database.PublishCommittedTelemetry(transaction.TransactionId);
            return new(
                IdentityResult.Success,
                participant.Id
            );
        }
        finally
        {
            // A failed registration rolls back on disposal. Remove rolled-back tracked state as well,
            // so a later SaveChanges in this request cannot accidentally persist it.
            database.ChangeTracker.Clear();
        }
    }

    public const string DeletedDisplayName = "Deleted user";

    /// <summary>Remove credentials and identifying profile fields, retaining contribution references.</summary>
    public async Task<IdentityResult> DeleteAccountAsync(Guid userId, string password, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || !await users.CheckPasswordAsync(user, password))
            return Failure("CredentialsRejected", "Account closure could not be completed.").Result;
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var profile = await database.ParticipantRows.SingleOrDefaultAsync(p => p.Id == userId && p.IsActive, cancellationToken);
            if (profile is null) return Failure("InactiveProfile", "Account closure could not be completed.").Result;
            // Do not strand a community with no active owner. Ownership transfer is a separate policy/workflow.
            if (await database.CommunityRows.AnyAsync(c => c.OwnerParticipantId == userId, cancellationToken))
                return Failure("CommunityOwnership", "Transfer community ownership before closing the account.").Result;
            profile.DisplayName = DeletedDisplayName;
            profile.Bio = string.Empty;
            profile.IsActive = false;
            profile.UpdatedAt = AtlasTime.UtcNow;
            // Identity removes credential/contact data and its dependent role/login/token records.
            // The Participant row remains so posts, comments and audit references remain valid.
            var result = await users.DeleteAsync(user);
            if (!result.Succeeded) return result;
            await database.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            database.PublishCommittedTelemetry(transaction.TransactionId);
            return IdentityResult.Success;
        }
        finally { database.ChangeTracker.Clear(); }
    }

    /// <summary>Recovery must not reactivate a closed/inactive Atlas profile.</summary>
    public Task<bool> HasActiveProfileAsync(Guid userId, CancellationToken cancellationToken = default) =>
        database.ParticipantRows.AsNoTracking().AnyAsync(row => row.Id == userId && row.IsActive, cancellationToken);

    /// <summary>Translates an authenticated framework principal to an active, persisted Participant.</summary>
    public async Task<ParticipantId?> ResolveParticipantAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default
    )
    {
        if (principal.Identity?.IsAuthenticated != true) return null;
        if (!Guid.TryParse(
            users.GetUserId(principal),
            out var id
        ) || id == Guid.Empty) return null;
        var user = await users.GetUserAsync(principal);
        if (user is null || !await users.IsEmailConfirmedAsync(user) || await users.IsLockedOutAsync(user))
            return null;
        return await database.ParticipantRows.AsNoTracking()
            .AnyAsync(
                row => row.Id == user.Id && row.IsActive,
                cancellationToken
            )
                ? new ParticipantId(user.Id) : null;
    }

    private static AccountRegistration Failure(
        string code,
        string description
    ) =>
        new(
            IdentityResult.Failed(new IdentityError {
                Code = code,
                Description = description
            }),
            null
        );
}
