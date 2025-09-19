using Microsoft.EntityFrameworkCore.Migrations;

namespace atlas_the_public_think_tank.Migrations_NonEF
{

    public partial class AddFullTextSearch : ICustomMigration
    {
        public void Up(MigrationBuilder migrationBuilder)
        {
            // Create catalog if not exists
            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = N'AtlasFullTextCatalog')
        BEGIN
            CREATE FULLTEXT CATALOG AtlasFullTextCatalog AS DEFAULT;
        END
        ");

            // Create full-text index for Issues if not exists
            migrationBuilder.Sql(@"
        IF NOT EXISTS (
            SELECT 1
            FROM sys.fulltext_indexes i
            JOIN sys.objects o ON i.object_id = o.object_id
            WHERE o.name = 'Issues'
              AND SCHEMA_NAME(o.schema_id) = 'issues'
        )
        BEGIN
            CREATE FULLTEXT INDEX ON [issues].[Issues] ([Title], [Content])
            KEY INDEX [PK_Issues]
            ON AtlasFullTextCatalog
            WITH CHANGE_TRACKING AUTO;
        END
        ");

            // Create full-text index for Solutions if not exists
            migrationBuilder.Sql(@"
        IF NOT EXISTS (
            SELECT 1
            FROM sys.fulltext_indexes i
            JOIN sys.objects o ON i.object_id = o.object_id
            WHERE o.name = 'Solutions'
              AND SCHEMA_NAME(o.schema_id) = 'solutions'
        )
        BEGIN
            CREATE FULLTEXT INDEX ON [solutions].[Solutions] ([Title], [Content])
            KEY INDEX [PK_Solutions]         
            ON AtlasFullTextCatalog
            WITH CHANGE_TRACKING AUTO;
        END
        ");
        }

        public void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.fulltext_indexes i
                                          JOIN sys.objects o ON i.object_id = o.object_id
                                          WHERE o.name = 'Issues'
                                            AND SCHEMA_NAME(o.schema_id) = 'issues')
                               DROP FULLTEXT INDEX ON [issues].[Issues];");

            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.fulltext_indexes i
                                          JOIN sys.objects o ON i.object_id = o.object_id
                                          WHERE o.name = 'Solutions'
                                            AND SCHEMA_NAME(o.schema_id) = 'solutions')
                               DROP FULLTEXT INDEX ON [solutions].[Solutions];");

            migrationBuilder.Sql(@"IF EXISTS (SELECT * FROM sys.fulltext_catalogs
                                          WHERE name = N'AtlasFullTextCatalog')
                               DROP FULLTEXT CATALOG AtlasFullTextCatalog;");
        }
    }

}
