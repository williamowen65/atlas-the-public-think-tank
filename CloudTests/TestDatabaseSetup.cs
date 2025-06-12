using atlas_the_public_think_tank.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.EntityFrameworkCore.InMemory; // Add this for InMemory database support

using System;
using atlas_the_public_think_tank.Models;

namespace CloudTests    
{
    public class TestDatabaseSetup
    {
        public static WebApplicationFactory<atlas_the_public_think_tank.Program> CreateWebApplicationFactory()
        {
            return new WebApplicationFactory<atlas_the_public_think_tank.Program>()
                .WithWebHostBuilder(builder =>
                {
                    // Setting the ASPNETCORE_ENVIRONMENT variable to Testing
                    builder.UseEnvironment("Testing");

                    builder.ConfigureServices(services =>
                    {
                        // Find the DbContext registration and replace it
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add a new DbContext using a test connection string
                        services.AddDbContext<ApplicationDbContext>(options =>
                        {
                            // Use a unique database name for each test run to avoid conflicts
                            var databaseName = $"ThinkTankTest_{Guid.NewGuid()}";
                            options.UseInMemoryDatabase(databaseName); // Requires Microsoft.EntityFrameworkCore namespace
                        });

                        // Create and seed the database
                        var sp = services.BuildServiceProvider();
                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                            // Ensure database is created
                            Boolean isDbCreated = db.Database.EnsureCreated();

                            // Seed test data
                            SeedTestData(db);
                        }
                    });
                });
        }

        private static void SeedTestData(ApplicationDbContext db)
        {
            //// Add test users
            var testUser = new AppUser
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                EmailConfirmed = true
            };
            //db.Users.Add(testUser);

            // Add test scopes
            //var globalScope = new Scope
            //{
            //    ScopeID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            //    ScopeName = "Global"
            //};
            //db.Scopes.Add(globalScope);

            //// Add test categories
            //var category = new Category
            //{
            //    CategoryID = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            //    CategoryName = "Environment"
            //};
            //db.Categories.Add(category);

            //// Add test issues
            //var issue = new Issue
            //{
            //    IssueID = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            //    Title = "Climate Change",
            //    Content = "We need solutions for climate change",
            //    CreatedAt = DateTime.Now,
            //    AuthorID = testUser.Id,
            //    ScopeID = globalScope.ScopeID
            //};
            //db.Issues.Add(issue);

            db.SaveChanges();
        }
    }
}