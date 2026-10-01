using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Identity;

/// <summary>Adds profile eligibility to Identity's confirmation, lockout, password, and 2FA checks.</summary>
public sealed class AtlasSignInManager(
    UserManager<AtlasIdentityUser> users, IHttpContextAccessor accessor,
    IUserClaimsPrincipalFactory<AtlasIdentityUser> claimsFactory, IOptions<IdentityOptions> options,
    ILogger<SignInManager<AtlasIdentityUser>> logger, IAuthenticationSchemeProvider schemes,
    IUserConfirmation<AtlasIdentityUser> confirmation, AtlasDataContext database)
    : SignInManager<AtlasIdentityUser>(users, accessor, claimsFactory, options, logger, schemes, confirmation)
{
    public override async Task<bool> CanSignInAsync(AtlasIdentityUser user) =>
        await base.CanSignInAsync(user) && await database.ParticipantRows.AsNoTracking()
            .AnyAsync(row => row.Id == user.Id && row.IsActive);
}
