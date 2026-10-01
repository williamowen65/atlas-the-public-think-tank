using Microsoft.AspNetCore.Identity;

namespace Atlas.Persistence.Identity;

/// <summary>Credential record. Id is also the linked ParticipantId; profile data stays in Participants.</summary>
public sealed class AtlasIdentityUser : IdentityUser<Guid> { }

public static class AtlasRoles
{
    public const string Member = "Member";
    public const string GlobalModerator = "GlobalModerator";
    public const string Administrator = "Administrator";
}
