using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace atlas_the_public_think_tank.Middleware
{
    public class PageRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public PageRequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            // Use a custom category for the logger
            _logger = loggerFactory.CreateLogger("PageRequestLogger");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // List of static resource extensions to ignore
            var staticExtensions = new[]
            {
                ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot", ".map"
            };

            // Only log if the request is NOT for a static resource and is a GET request
            if (!string.IsNullOrEmpty(path)
                && !staticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                && context.Request.Method == "GET")
            {
                _logger.LogInformation($"\r{new string('=', 19)}\nRazor Page requested: {path}\n{new string('=', 19)}");
            }

            await _next(context);
        }
    }


    /// <summary>
    ///  Registering this middleware as an extension method
    /// </summary>
    public static class PageRequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UsePageRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PageRequestLoggingMiddleware>();
        }
    }
}