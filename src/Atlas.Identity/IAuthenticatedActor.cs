namespace Atlas.Identity;

/// <summary>
/// Exposes the Atlas Participant ID established for the current authenticated actor.
///
/// This is an application/host authentication boundary, not a complete user profile
/// and not a domain service. Hosts implement it from their authentication mechanism:
/// the Console session today, and a request-scoped ClaimsPrincipal/HttpContext
/// implementation in the future API.
///
/// Domain operations continue to receive explicit actor IDs when authorization is part
/// of the domain rule; they must not depend on ambient authentication state.
/// </summary>
public interface IAuthenticatedActor
{
    Guid ParticipantId { get; }
}
