using Atlas.Moderation;
using Atlas.Persistence;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity;

public static class IdentityServices
{
    /// <summary>Registers the SQL account infrastructure. Hosts supply endpoints, email delivery, and key storage.</summary>
    public static IServiceCollection AddAtlasIdentity(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.AddDbContext<AtlasDataContext>(options => options.UseSqlServer(connectionString));
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddIdentity<AtlasIdentityUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequiredLength = 12;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddEntityFrameworkStores<AtlasDataContext>()
            .AddSignInManager<AtlasSignInManager>()
            .AddDefaultTokenProviders();
        services.AddScoped<AtlasAccounts>();
        services.AddScoped<IModeratorAuthorization, IdentityModeratorAuthorization>();
        services.AddScoped<IAuthorizationHandler, AtlasRoleHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AtlasPolicies.Member, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new AtlasRoleRequirement(AtlasRoles.Member, AtlasRoles.GlobalModerator, AtlasRoles.Administrator)));
            options.AddPolicy(AtlasPolicies.GlobalModerator, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new AtlasRoleRequirement(AtlasRoles.GlobalModerator, AtlasRoles.Administrator)));
            options.AddPolicy(AtlasPolicies.Administrator, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new AtlasRoleRequirement(AtlasRoles.Administrator)));
        });
        // Recheck active profile on every cookie request in addition to framework security-stamp validation.
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.Events.OnValidatePrincipal = async context =>
            {
                await SecurityStampValidator.ValidatePrincipalAsync(context);
                if (context.Principal is not null && await context.HttpContext.RequestServices
                        .GetRequiredService<AtlasAccounts>().ResolveParticipantAsync(context.Principal) is null)
                    context.RejectPrincipal();
            };
        });
        return services;
    }
}
