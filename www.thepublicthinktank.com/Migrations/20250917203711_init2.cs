using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RANK",
                schema: "solutions",
                table: "Solutions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RANK",
                schema: "issues",
                table: "Issues",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RANK",
                schema: "solutions",
                table: "Solutions");

            migrationBuilder.DropColumn(
                name: "RANK",
                schema: "issues",
                table: "Issues");
        }
    }
}
