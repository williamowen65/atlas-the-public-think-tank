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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "BlockRows");
        }
    }
}
