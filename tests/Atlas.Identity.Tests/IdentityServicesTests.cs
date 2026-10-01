using System.Security.Claims;
using Atlas.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: DoNotParallelize]

namespace Atlas.Identity.Tests;

[TestClass]
public sealed class IdentityServicesTests
{
    [TestMethod]
    public async Task Services_resolve_framework_account_managers_policies_and_reject_invalid_actors_without_sql()
    {
        var services = new ServiceCollection().AddAtlasIdentity("Server=localhost;Database=ModelOnly;Integrated Security=True");
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        var scoped = scope.ServiceProvider;
        Assert.IsNotNull(scoped.GetRequiredService<UserManager<AtlasIdentityUser>>());
        Assert.IsNotNull(scoped.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
        Assert.IsInstanceOfType<AtlasSignInManager>(scoped.GetRequiredService<SignInManager<AtlasIdentityUser>>());
        var options = scoped.GetRequiredService<IOptions<IdentityOptions>>().Value;
        Assert.IsTrue(options.SignIn.RequireConfirmedEmail);
        Assert.IsTrue(options.User.RequireUniqueEmail);
        Assert.AreEqual(12, options.Password.RequiredLength);
        var policies = scoped.GetRequiredService<IAuthorizationPolicyProvider>();
        foreach (var name in new[] { AtlasPolicies.Member, AtlasPolicies.GlobalModerator, AtlasPolicies.Administrator })
            Assert.IsNotNull(await policies.GetPolicyAsync(name));
        var accounts = scoped.GetRequiredService<AtlasAccounts>();
        Assert.IsNull(await accounts.ResolveParticipantAsync(new ClaimsPrincipal()));
        var malformed = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "malformed") }, "test"));
        Assert.IsNull(await accounts.ResolveParticipantAsync(malformed));
    }
}
