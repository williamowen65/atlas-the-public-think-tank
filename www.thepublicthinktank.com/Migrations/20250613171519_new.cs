using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Solutions",
                schema: "solutions",
                newName: "Solutions");

            migrationBuilder.RenameTable(
                name: "Issues",
                schema: "issues",
                newName: "Issues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Solutions",
                newName: "Solutions",
                newSchema: "solutions");

            migrationBuilder.RenameTable(
                name: "Issues",
                newName: "Issues",
                newSchema: "issues");
        }
    }
}
