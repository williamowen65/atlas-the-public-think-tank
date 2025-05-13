using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class langupdatetoissues2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "issues");

            migrationBuilder.RenameTable(
                name: "UserVotes",
                schema: "forums",
                newName: "UserVotes",
                newSchema: "issues");

            migrationBuilder.RenameTable(
                name: "Solutions",
                schema: "forums",
                newName: "Solutions",
                newSchema: "issues");

            migrationBuilder.RenameTable(
                name: "Scopes",
                schema: "forums",
                newName: "Scopes",
                newSchema: "issues");

            migrationBuilder.RenameTable(
                name: "IssuesCategories",
                schema: "forums",
                newName: "IssuesCategories",
                newSchema: "issues");

            migrationBuilder.RenameTable(
                name: "Issues",
                schema: "forums",
                newName: "Issues",
                newSchema: "issues");

            migrationBuilder.RenameTable(
                name: "Comments",
                schema: "forums",
                newName: "Comments",
                newSchema: "issues");

            migrationBuilder.RenameTable(
                name: "Categories",
                schema: "forums",
                newName: "Categories",
                newSchema: "issues");

            migrationBuilder.RenameTable(
                name: "BlockedContent",
                schema: "forums",
                newName: "BlockedContent",
                newSchema: "issues");

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: 1,
                column: "Content",
                value: "A issue to discuss practical solutions to climate change at individual and policy levels.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "forums");

            migrationBuilder.RenameTable(
                name: "UserVotes",
                schema: "issues",
                newName: "UserVotes",
                newSchema: "forums");

            migrationBuilder.RenameTable(
                name: "Solutions",
                schema: "issues",
                newName: "Solutions",
                newSchema: "forums");

            migrationBuilder.RenameTable(
                name: "Scopes",
                schema: "issues",
                newName: "Scopes",
                newSchema: "forums");

            migrationBuilder.RenameTable(
                name: "IssuesCategories",
                schema: "issues",
                newName: "IssuesCategories",
                newSchema: "forums");

            migrationBuilder.RenameTable(
                name: "Issues",
                schema: "issues",
                newName: "Issues",
                newSchema: "forums");

            migrationBuilder.RenameTable(
                name: "Comments",
                schema: "issues",
                newName: "Comments",
                newSchema: "forums");

            migrationBuilder.RenameTable(
                name: "Categories",
                schema: "issues",
                newName: "Categories",
                newSchema: "forums");

            migrationBuilder.RenameTable(
                name: "BlockedContent",
                schema: "issues",
                newName: "BlockedContent",
                newSchema: "forums");

            migrationBuilder.UpdateData(
                schema: "forums",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: 1,
                column: "Content",
                value: "A forum to discuss practical solutions to climate change at individual and policy levels.");
        }
    }
}
