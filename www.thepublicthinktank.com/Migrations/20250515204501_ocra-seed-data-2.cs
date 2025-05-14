using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class ocraseeddata2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: 5,
                column: "ParentIssueID",
                value: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: 5,
                column: "ParentIssueID",
                value: null);
        }
    }
}
