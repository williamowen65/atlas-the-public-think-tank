namespace Atlas.Identity;

/// <summary>
/// Exposes the Atlas Participant established for the current authenticated user.
///
/// This is an application/host boundary, not a domain service. Hosts implement it from
/// their authentication mechanism: the Console session today, and a request-scoped
/// ClaimsPrincipal/HttpContext implementation in the future API.
///
/// Domain operations should continue to receive explicit actor IDs when authorization
/// is part of the domain rule; they must not depend on ambient authentication state.
/// </summary>
public interface IUserContext
{
    Guid ParticipantId { get; }
}
