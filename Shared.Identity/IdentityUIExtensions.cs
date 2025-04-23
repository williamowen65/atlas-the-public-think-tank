using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shared.DbContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Shared.Identity
{
    public static class IdentityUIExtensions
    {
        /// <summary>
        /// Adds shared identity services and UI to the application
        /// </summary>
        public static IServiceCollection AddSharedIdentityUI(this IServiceCollection services, 
            IConfiguration configuration, 
            Action<IdentityOptions>? configureIdentity = null)
        {
            // Configure services for identity
            var identityBuilder = services
                .AddDefaultIdentity<IdentityUser>(options => 
                {
                    // Default options
                    options.SignIn.RequireConfirmedAccount = true;
                    
                    // Apply caller's configuration if provided
                    configureIdentity?.Invoke(options);
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI(); // Explicitly add default UI components

            // Configure RazorViewEngine to find views in this library
            services.Configure<RazorViewEngineOptions>(options =>
            {
                // Add view locations for Identity areas
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
                
                // Add view locations for partials in the Identity area
                options.AreaViewLocationFormats.Add("/Areas/Identity/Pages/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/Identity/Pages/Shared/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/Identity/Pages/Account/Shared/{0}.cshtml");
            });

            return services;
        }

        /// <summary>
        /// Configures database for Identity using shared DbContext
        /// </summary>
        public static IServiceCollection AddSharedIdentityDatabase(this IServiceCollection services, 
            IConfiguration configuration,
            string connectionStringName = "DefaultConnection")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString(connectionStringName)));
            
            services.AddDatabaseDeveloperPageExceptionFilter();
                
            return services;
        }

        /// <summary>
        /// Maps the shared Identity UI routes and adds authentication middleware
        /// </summary>
        public static IApplicationBuilder UseSharedIdentityUI(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Ensure Identity area routes are registered
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "Identity",
                    areaName: "Identity",
                    pattern: "Identity/{controller=Home}/{action=Index}/{id?}");
            });
            
            return app;
        }
    }
}