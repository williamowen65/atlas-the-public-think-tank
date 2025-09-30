using atlas_the_public_think_tank.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace atlas_the_public_think_tank.Migrations_NonEF
{
    public class CustomMigrationRunner
    {
        private readonly IServiceProvider serviceProvider;

        public CustomMigrationRunner(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void RunUp()
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrationBuilder = new MigrationBuilder(context.Database.ProviderName);

            var migrations = new ICustomMigration[]
            {
                new AddFullTextSearch(),
                new Issue_CanPublish_Constraint()
            };

            foreach (var migration in migrations)
            {
                migration.Up(migrationBuilder);
            }

            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            if (env.IsEnvironment("Testing"))
            {
                context.Database.Migrate();
            }

            // Generate SQL for ALL operations (not only SqlOperation) and execute them
            var sqlGenerator = context.GetService<IMigrationsSqlGenerator>();
            var commands = sqlGenerator.Generate(migrationBuilder.Operations, context.Model);

            IDbContextTransaction? tx = null;
            try
            {
                tx = context.Database.BeginTransaction();

                foreach (var command in commands)
                {
                    if (command.TransactionSuppressed)
                    {
                        tx.Commit();
                        context.Database.ExecuteSqlRaw(command.CommandText);
                        tx = context.Database.BeginTransaction();
                    }
                    else
                    {
                        context.Database.ExecuteSqlRaw(command.CommandText);
                    }
                }

                tx.Commit();
            }
            finally
            {
                tx?.Dispose();
            }
        }

        public void RunDown(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrationBuilder = new MigrationBuilder(context.Database.ProviderName);

            var migrations = new ICustomMigration[]
            {
                new AddFullTextSearch(),
                new Issue_CanPublish_Constraint()
            };

            foreach (var migration in migrations)
            {
                migration.Down(migrationBuilder);
            }

            var sqlGenerator = context.GetService<IMigrationsSqlGenerator>();
            var commands = sqlGenerator.Generate(migrationBuilder.Operations, context.Model);

            IDbContextTransaction? tx = null;
            try
            {
                tx = context.Database.BeginTransaction();

                foreach (var command in commands)
                {
                    if (command.TransactionSuppressed)
                    {
                        tx.Commit();
                        context.Database.ExecuteSqlRaw(command.CommandText);
                        tx = context.Database.BeginTransaction();
                    }
                    else
                    {
                        context.Database.ExecuteSqlRaw(command.CommandText);
                    }
                }

                tx.Commit();
            }
            finally
            {
                tx?.Dispose();
            }
        }
    }
}