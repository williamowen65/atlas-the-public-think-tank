using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shared.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
                .AddEntityFrameworkStores<ApplicationDbContext>();

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
            
            return app;
        }
    }
}