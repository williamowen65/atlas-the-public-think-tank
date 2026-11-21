using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionedContentComposition : Migration
    {
        // Existing installations may already have the supplemental index. Explicit
        // database setup recreates it after the schema migrations finish.
        private static void DropBlockFullTextIndex(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID(N'dbo.BlockRows'))
                    DROP FULLTEXT INDEX ON dbo.BlockRows;
                """, suppressTransaction: true);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropBlockFullTextIndex(migrationBuilder);

            migrationBuilder.AddColumn<string>(
                name: "Composition",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompositionSearchText",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Latin1_General_100_CI_AI_SC");

            migrationBuilder.AlterColumn<string>(
                name: "SearchText",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "CONVERT(nvarchar(max), CASE [Kind]\r\n    WHEN N'sectioned' THEN COALESCE([CompositionSearchText], N'')\r\n    WHEN N'markdown' THEN COALESCE([Markdown], N'')\r\n    WHEN N'image' THEN CONCAT([AltText], N' ', [Caption])\r\n    WHEN N'video' THEN CONCAT([Caption], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'link-preview' THEN CONCAT([Title], N' ', [Description], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'chart-reference' THEN COALESCE([Title], N'')\r\n    ELSE N'' END)",
                stored: true,
                collation: "Latin1_General_100_CI_AI_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "CONVERT(nvarchar(max), CASE [Kind]\r\n    WHEN N'markdown' THEN COALESCE([Markdown], N'')\r\n    WHEN N'image' THEN CONCAT([AltText], N' ', [Caption])\r\n    WHEN N'video' THEN CONCAT([Caption], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'link-preview' THEN CONCAT([Title], N' ', [Description], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'chart-reference' THEN COALESCE([Title], N'')\r\n    ELSE N'' END)",
                oldStored: true,
                oldCollation: "Latin1_General_100_CI_AI_SC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropBlockFullTextIndex(migrationBuilder);

            migrationBuilder.AlterColumn<string>(
                name: "SearchText",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "CONVERT(nvarchar(max), CASE [Kind]\r\n    WHEN N'markdown' THEN COALESCE([Markdown], N'')\r\n    WHEN N'image' THEN CONCAT([AltText], N' ', [Caption])\r\n    WHEN N'video' THEN CONCAT([Caption], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'link-preview' THEN CONCAT([Title], N' ', [Description], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'chart-reference' THEN COALESCE([Title], N'')\r\n    ELSE N'' END)",
                stored: true,
                collation: "Latin1_General_100_CI_AI_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "CONVERT(nvarchar(max), CASE [Kind]\r\n    WHEN N'sectioned' THEN COALESCE([CompositionSearchText], N'')\r\n    WHEN N'markdown' THEN COALESCE([Markdown], N'')\r\n    WHEN N'image' THEN CONCAT([AltText], N' ', [Caption])\r\n    WHEN N'video' THEN CONCAT([Caption], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'link-preview' THEN CONCAT([Title], N' ', [Description], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'chart-reference' THEN COALESCE([Title], N'')\r\n    ELSE N'' END)",
                oldStored: true,
                oldCollation: "Latin1_General_100_CI_AI_SC");

            migrationBuilder.DropColumn(
                name: "Composition",
                table: "BlockRows");

            migrationBuilder.DropColumn(
                name: "CompositionSearchText",
                table: "BlockRows");
        }
    }
}
