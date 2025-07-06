using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models;
using System;
using System.Linq;

namespace CloudTests.TestingSetup
{
    public class SqliteTestFixture : IDisposable
    {
        private readonly SqliteConnection _connection;

        public SqliteTestFixture()
        {

            Console.WriteLine("Setting up E2E tests with SQLite");

            // Create and open an in-memory SQLite connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Build the service provider
            var services = new ServiceCollection();

            // Add Identity services
            services.AddIdentity<AppUser, IdentityRole<Guid>>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add a DbContext using SQLite
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            ServiceProvider = services.BuildServiceProvider();

            // Create the schema and seed data
            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created
                db.Database.EnsureCreated();

                // Seed test data
                SeedTestData(db);
            }
        }

        public IServiceProvider ServiceProvider { get; }

        private void SeedTestData(ApplicationDbContext db)
        {
            // Add test users with proper password hash
            var testUser = new AppUser
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserName = "testuser@example.com",
                NormalizedUserName = "TESTUSER@EXAMPLE.COM",
                Email = "testuser@example.com",
                NormalizedEmail = "TESTUSER@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = false
            };

            // Hash the password
            var passwordHasher = new PasswordHasher<AppUser>();
            testUser.PasswordHash = passwordHasher.HashPassword(testUser, "Password123!");

            db.Users.Add(testUser);

            // Add test scopes
            var globalScope = new Scope
            {
                ScopeID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ScopeName = "Global"
            };
            db.Scopes.Add(globalScope);

            // Add test categories
            var category = new Category
            {
                CategoryID = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CategoryName = "Environment"
            };
            db.Categories.Add(category);

            // Add test issues
            var issue = new Issue
            {
                IssueID = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Title = "Test Issue",
                Content = "This is a test issue for testing solutions",
                CreatedAt = DateTime.Now,
                AuthorID = testUser.Id,
                ScopeID = globalScope.ScopeID,
                ContentStatus = ContentStatus.Published
            };
            db.Issues.Add(issue);

            var issue2 = new Issue
            {
                IssueID = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Title = "Test Another Issue",
                Content = "This is a another test issue for testing solutions",
                CreatedAt = DateTime.Now,
                AuthorID = testUser.Id,
                ScopeID = globalScope.ScopeID,
                ContentStatus = ContentStatus.Published
            };
            db.Issues.Add(issue2);

            // Add test issue category
            var issueCategory = new IssueCategory
            {
                IssueID = issue.IssueID,
                CategoryID = category.CategoryID
            };
            db.IssueCategories.Add(issueCategory);

            db.SaveChanges();

            Console.WriteLine("SQLite test database seeded successfully");
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}