using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace Shared.UI
{
    public static class SharedUIExtensions
    {
        /// <summary>
        /// Adds services required for the Shared UI components
        /// </summary>
        public static IServiceCollection AddSharedUI(this IServiceCollection services)
        {
            // Add any services needed by the Shared UI here
            return services;
        }

        /// <summary>
        /// Configures the application to use the static files from Shared.UI project
        /// </summary>
        public static IApplicationBuilder UseSharedUI(this IApplicationBuilder app)
        {
            // Get the embedded file provider from the Shared.UI assembly
            var assembly = typeof(SharedUIExtensions).Assembly;
            var fileProvider = new EmbeddedFileProvider(assembly, "Shared.UI.wwwroot");

            // Use the static files from the embedded resource
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = "/shared-ui"
            });

            return app;
        }
    }
}