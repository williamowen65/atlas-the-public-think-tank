using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Issues",
                schema: "issue",
                newName: "Issues",
                newSchema: "issues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "issue");

            migrationBuilder.RenameTable(
                name: "Issues",
                schema: "issues",
                newName: "Issues",
                newSchema: "issue");
        }
    }
}
