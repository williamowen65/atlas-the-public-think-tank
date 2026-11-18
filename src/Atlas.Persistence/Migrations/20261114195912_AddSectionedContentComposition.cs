using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionedContentComposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            migrationBuilder.DropColumn(
                name: "Composition",
                table: "BlockRows");

            migrationBuilder.DropColumn(
                name: "CompositionSearchText",
                table: "BlockRows");

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
        }
    }
}
