using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class ocraseeddata3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: 5,
                columns: new[] { "Content", "Title" },
                values: new object[] { "The Southern Resident orca population has dropped from 88 individuals in 2010 to just 74 as of late 2024. This decline is attributed to a combination of factors, including reduced prey availability, pollution, and vessel traffic.", "Decline of the Southern Resident orca population" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: 5,
                columns: new[] { "Content", "Title" },
                values: new object[] { "This decline is attributed to a combination of factors, including reduced prey availability, pollution, and vessel traffic.", "The Southern Resident orca population has dropped from 88 individuals in 2010 to just 74 as of late 2024" });
        }
    }
}
