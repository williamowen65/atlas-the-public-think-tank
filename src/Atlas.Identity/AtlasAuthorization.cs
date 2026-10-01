using Atlas.Moderation;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity;

public static class AtlasPolicies
{
    public const string Member = "Atlas.Member";
    public const string GlobalModerator = "Atlas.GlobalModerator";
    public const string Administrator = "Atlas.Administrator";
}

public sealed record AtlasRoleRequirement(params string[] Roles) : IAuthorizationRequirement;

/// <summary>Uses current stored roles and profile eligibility rather than trusting stale role claims.</summary>
public sealed class AtlasRoleHandler(AtlasAccounts accounts, AtlasDataContext database)
    : AuthorizationHandler<AtlasRoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        AtlasRoleRequirement requirement)
    {
        var participant = await accounts.ResolveParticipantAsync(context.User);
        if (participant is null) return;
        if (await (from membership in database.UserRoles
                   join role in database.Roles on membership.RoleId equals role.Id
                   where membership.UserId == participant.Value && requirement.Roles.Contains(role.Name!)
                   select membership).AnyAsync())
            context.Succeed(requirement);
    }
}

/// <summary>Infrastructure adapter for Atlas moderation. The host must supply the authenticated actor ID.</summary>
public sealed class IdentityModeratorAuthorization(AtlasDataContext database) : IModeratorAuthorization
{
    public bool IsAtlasModerator(Guid participantId) =>
        (from user in database.Users
         join participant in database.ParticipantRows on user.Id equals participant.Id
         join membership in database.UserRoles on user.Id equals membership.UserId
         join role in database.Roles on membership.RoleId equals role.Id
         where user.Id == participantId && participant.IsActive && user.EmailConfirmed &&
               (!user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow) &&
               (role.Name == AtlasRoles.GlobalModerator || role.Name == AtlasRoles.Administrator)
         select user.Id).Any();
}
