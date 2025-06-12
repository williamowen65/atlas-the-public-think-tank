using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Services;

namespace atlas_the_public_think_tank;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var isTesting = builder.Environment.EnvironmentName == "Testing"
            || builder.Configuration["ASPNETCORE_ENVIRONMENT"] == "Testing";

        // =====================================
        // Dependency Injection (Service Setup)
        // =====================================

        if (!isTesting)
        { 
            // Retrieve the connection string from appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Register the application's database context (ApplicationDbContext) with the DI container
            // and configure it to use SQL Server with the retrieved connection string
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)
                       .EnableSensitiveDataLogging()); // TODO: Disable this logging in production
        }


        builder.Services.AddDefaultIdentity<AppUser>(options =>
            options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Add a developer-friendly exception filter for database-related errors
        // (helps provide detailed error information during development)
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Add MVC support (controllers with views) to the service container
        // This enables routing HTTP requests to controller actions
        builder.Services.AddControllersWithViews()
                   .AddJsonOptions(options =>
                   {
                       //options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                   });

        builder.Services.AddRazorPages();

        builder.Services.AddScoped<CRUD>(); // Register the CRUD service for dependency injection
        builder.Services.AddScoped<Issues>(); // Register the CRUD service for dependency injection
        builder.Services.AddScoped<Solutions>(); // Register the CRUD service for dependency injection
        builder.Services.AddScoped<BreadcrumbAccessor>(); // Register the CRUD service for dependency injection

        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();

        // =====================================
        // Middleware and Routing Configuration
        // =====================================

        // Log basic startup information
        app.Logger.LogInformation("Application starting up...");
        app.Logger.LogInformation($"Is Development Environment: {app.Environment.IsDevelopment()}");

        // Configure the HTTP request pipeline based on environment
        if (app.Environment.IsDevelopment())
        {
            // UeeMigrationsEndPoint provides a way to "apply pending migrations" from the browser
            app.UseMigrationsEndPoint();
            // if you view a page that should have migrations applied, instead of an error page,
            // you'll get up button to apply migrations
            // https://stackoverflow.com/questions/65389260/what-app-usemigrationsendpoint-does-in-net-core-web-application-startup-class#:~:text=This%20app.UseMigrationsEndPoint()%20is%20actually%20a%20very%20handy%20tool%20in%20development.

            // In development, show detailed database errors and exception pages
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // In production, use a generic error handler and enable HSTS (HTTP Strict Transport Security)
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts(); // Enforce HTTPS with a 30-day default duration
        }

        // Force redirect all HTTP requests to HTTPS
        app.UseHttpsRedirection();

        // Enable request routing
        app.UseRouting();

        app.UseAuthentication();
        // Enable authorization middleware (checks if users are authorized to access resources)
        app.UseAuthorization();

        // Map static assets (like CSS, JavaScript, images)
        app.MapStaticAssets();

        // Map default controller routes (e.g., HomeController -> /Home/Index)
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        // Map Razor Pages endpoints (for Razor Page files like .cshtml)
        app.MapRazorPages()
           .WithStaticAssets();

        // Start the application and begin listening for incoming HTTP requests
        app.Run();
    }
}