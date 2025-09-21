using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RawSQL;
using atlas_the_public_think_tank.Data.RepositoryPattern;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Middleware;
using atlas_the_public_think_tank.Migrations_NonEF;
using atlas_the_public_think_tank.Models;

using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using repository_pattern_experiment.Controllers;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace atlas_the_public_think_tank;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        /* See information on appsettings and environment variables: https://github.com/williamowen65/atlas-the-public-think-tank/wiki/3.2.%20Appsettings%20and%20Environment%20Variables */
        Console.WriteLine($"Using environment: {builder.Environment.EnvironmentName}");

        var connString = builder.Configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"Connection string being used: {connString}");

        // When running the test project, TestEnvironmentUtility intercepts the builder and sets the Environment to testing
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
                    .EnableSensitiveDataLogging()
            );


            // Add OpenTelemetry and configure it to use Azure Monitor.
            builder.Services.AddOpenTelemetry().UseAzureMonitor();

        }


        builder.Services.AddDefaultIdentity<AppUser>(options =>
            options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // After the AddDefaultIdentity line, add this configuration:
        builder.Services.ConfigureApplicationCookie(options =>
        {
            // This makes it so the [Authorize] routes will redirect to to the login page with the correct url
            options.LoginPath = "/login";
        });

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

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddMemoryCache();

        // Register all repositories with one extension method
        builder.Services.AddRepositories();

        builder.Services.Configure<ApplicationInsightsSettings>(options =>
        {
            var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            if (connectionString == null)
            {
                throw new InvalidOperationException("ApplicationInsights ConnectionString is not configured.");
            }

            options.ConnectionString = connectionString;
            options.EnableSendBeacon = true;
        });

        builder.Services.AddScoped<UserHistoryProcessor>();

        var app = builder.Build();



        
        // Initialize the static Read class with the service provider
        Create.Initialize(app.Services);
        Read.Initialize(app.Services);
        Upsert.Initialize(app.Services);
        Update.Initialize(app.Services);
        // Get the IMemoryCache service from the service provider
        var memoryCache = app.Services.GetRequiredService<IMemoryCache>();
        // Initialize the CacheHelper with the memory cache
        CacheHelper.Initialize(memoryCache);
        // Add custom migrations (Full Text Search)
        CustomMigrationRunner.RunUp(app.Services);

        FilterQueryService.Initialize(builder.Configuration);


        // Apply seed data needs to be after the CRUD Initializers
        var applySeedData = builder.Configuration.GetValue<bool>("ApplySeedData");
        Console.WriteLine($"Env flag ApplySeedData: {applySeedData}");
        if (applySeedData)
        {
            SeedDataHelper.SeedDatabase(app.Services);
        }

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
            pattern: "{controller=Home}/{action=HomePage}/{id?}")
            //pattern: "{controller=RnD}/{action=RnDCreateIssue}/{id?}")
            .WithStaticAssets();

        // Map Razor Pages endpoints (for Razor Page files like .cshtml)
        app.MapRazorPages()
           .WithStaticAssets();


        // Log Razor Page requests with custom category
        app.UsePageRequestLogging();

      
        // Start the application and begin listening for incoming HTTP requests
        app.Run();
    }
}