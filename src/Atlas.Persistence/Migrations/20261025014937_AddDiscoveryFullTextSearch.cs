using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscoveryFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "CONVERT(nvarchar(max), CASE [Kind]\r\n    WHEN N'markdown' THEN COALESCE([Markdown], N'')\r\n    WHEN N'image' THEN CONCAT([AltText], N' ', [Caption])\r\n    WHEN N'video' THEN CONCAT([Caption], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'link-preview' THEN CONCAT([Title], N' ', [Description], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'chart-reference' THEN COALESCE([Title], N'')\r\n    ELSE N'' END)",
                stored: true,
                collation: "Latin1_General_100_CI_AI_SC");

            // Full-text DDL must run outside a transaction, after the indexed column exists.
            migrationBuilder.Sql("""
                -- No GO: EF executes this batch with suppressTransaction: true.
                IF COALESCE(FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'), 0) <> 1
                    THROW 51000, 'Atlas search requires SQL Server Full-Text Search. Install the feature before applying this migration.', 1;
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_languages WHERE lcid = 1033)
                    THROW 51000, 'Atlas search requires English full-text language resources (LCID 1033).', 1;
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'AtlasDiscovery')
                    EXEC(N'CREATE FULLTEXT CATALOG AtlasDiscovery WITH ACCENT_SENSITIVITY = OFF');
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.NodeRows'))
                    EXEC(N'CREATE FULLTEXT INDEX ON dbo.NodeRows
                        (Title LANGUAGE 1033, ShortDescription LANGUAGE 1033)
                        KEY INDEX PK_NodeRows ON AtlasDiscovery
                        WITH CHANGE_TRACKING AUTO, STOPLIST = SYSTEM');
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.BlockRows'))
                    EXEC(N'CREATE FULLTEXT INDEX ON dbo.BlockRows (SearchText LANGUAGE 1033)
                        KEY INDEX PK_BlockRows ON AtlasDiscovery
                        WITH CHANGE_TRACKING AUTO, STOPLIST = SYSTEM');
                """, suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove full-text dependencies before dropping the computed column.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.BlockRows'))
                    DROP FULLTEXT INDEX ON dbo.BlockRows;
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.NodeRows'))
                    DROP FULLTEXT INDEX ON dbo.NodeRows;
                IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'AtlasDiscovery')
                    DROP FULLTEXT CATALOG AtlasDiscovery;
                """, suppressTransaction: true);

            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "BlockRows");
        }
    }
}
