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
        // Register AtlasDataContext as the EF Core DbContext used by Identity and the Atlas SQL repositories.
        // AddDbContext makes a context available through dependency injection for each service/request scope,
        // while UseSqlServer tells EF Core which database provider and connection this context should use.
        services.AddDbContext<AtlasDataContext>(options => options.UseSqlServer(connectionString));

        // Identity and its supporting framework services use ILogger<T> for diagnostic and security-related logging.
        services.AddLogging();

        // Makes the current HTTP request available to services through IHttpContextAccessor. The console does not
        // have an HTTP request, but the future web host and several ASP.NET Core Identity services expect this facility.
        services.AddHttpContextAccessor();

        // Data Protection provides ASP.NET Core's cryptographic key system. Identity uses it to protect data such as
        // confirmation/reset tokens and authentication-related state. A production host must configure durable/shared keys.
        services.AddDataProtection();

        // Register ASP.NET Core Identity. AtlasIdentityUser is the persisted Identity user type defined in
        // Atlas.Persistence.Identity; IdentityRole<Guid> is the framework role type. Both use Guid primary keys.
        // The options below establish Atlas-wide account rules rather than business-domain permissions.
        services.AddIdentity<AtlasIdentityUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequiredLength = 12;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            // Store Identity users, roles, claims, logins, and tokens through AtlasDataContext in SQL Server.
            .AddEntityFrameworkStores<AtlasDataContext>()
            // Use Atlas's SignInManager subclass so Atlas-specific profile eligibility participates in sign-in.
            .AddSignInManager<AtlasSignInManager>()
            // Supply the framework token generators used for operations such as email confirmation and password reset.
            .AddDefaultTokenProviders();

        // Scoped services get one instance per dependency-injection scope. In a web application that normally means
        // one instance per HTTP request; the console explicitly creates scopes around account/session operations.
        // AtlasAccounts coordinates Identity accounts with Atlas Participant profiles.
        services.AddScoped<AtlasAccounts>();
        // Global moderation checks are backed by persisted Identity roles rather than a configured participant-ID list.
        services.AddScoped<IModeratorAuthorization, IdentityModeratorAuthorization>();
        // AtlasRoleHandler evaluates the custom AtlasRoleRequirement used by the authorization policies below.
        services.AddScoped<IAuthorizationHandler, AtlasRoleHandler>();

        // Named policies give callers readable authorization boundaries instead of scattering role-name checks.
        services.AddAuthorization(options =>
        {
            // Member: normal authenticated Atlas access. Member, GlobalModerator, and Administrator all satisfy it
            // because the two privileged roles are also allowed to perform ordinary member actions.
            options.AddPolicy(AtlasPolicies.Member, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new AtlasRoleRequirement(AtlasRoles.Member, AtlasRoles.GlobalModerator, AtlasRoles.Administrator)));

            // GlobalModerator: Atlas-wide moderation access. Administrators inherit this capability; ordinary Members do not.
            options.AddPolicy(AtlasPolicies.GlobalModerator, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new AtlasRoleRequirement(AtlasRoles.GlobalModerator, AtlasRoles.Administrator)));

            // Administrator: highest global Identity policy. Only a persisted Administrator role satisfies it.
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
