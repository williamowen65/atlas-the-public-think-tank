
using atlas_the_public_think_tank.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace atlas_the_public_think_tank.Migrations_NonEF
{
    public static class CustomMigrationRunner
    {
        public static void RunUp(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrationBuilder = new MigrationBuilder(context.Database.ProviderName);

            // Add your custom migrations here
            var migrations = new ICustomMigration[]
            {
                new AddFullTextSearch()
                // Add more custom migrations as needed
            };

            foreach (var migration in migrations)
            {
                migration.Up(migrationBuilder);
            }

            // Confirms the database is created first
            // This needs to be called for the testing environment.
            // It is called by the TestEnvironment constructor, but not soon enough. 
            // The migration needs to be applied at runtime. Not after the app has been built.
            

            // Replace the selected line with the following
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            if (env.IsEnvironment("Testing"))
            {
                context.Database.Migrate();
            }

            // Apply SQL commands to the database
            foreach (var command in migrationBuilder.Operations.OfType<SqlOperation>())
            {
                context.Database.ExecuteSqlRaw(command.Sql);
            }
        }

        public static void RunDown(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrationBuilder = new MigrationBuilder(context.Database.ProviderName);

            var migrations = new ICustomMigration[]
            {
                new AddFullTextSearch()
                // Add more custom migrations as needed
            };

            foreach (var migration in migrations)
            {
                migration.Down(migrationBuilder);
            }

            foreach (var command in migrationBuilder.Operations.OfType<SqlOperation>())
            {
                context.Database.ExecuteSqlRaw(command.Sql);
            }
        }
    }
}