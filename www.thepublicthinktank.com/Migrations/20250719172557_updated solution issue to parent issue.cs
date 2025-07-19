using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class updatedsolutionissuetoparentissue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Issues_IssueID",
                table: "Solutions");

            migrationBuilder.RenameColumn(
                name: "IssueID",
                table: "Solutions",
                newName: "ParentIssueID");

            migrationBuilder.RenameIndex(
                name: "IX_Solutions_IssueID",
                table: "Solutions",
                newName: "IX_Solutions_ParentIssueID");

            migrationBuilder.AddForeignKey(
                name: "FK_Solutions_Issues_ParentIssueID",
                table: "Solutions",
                column: "ParentIssueID",
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Issues_ParentIssueID",
                table: "Solutions");

            migrationBuilder.RenameColumn(
                name: "ParentIssueID",
                table: "Solutions",
                newName: "IssueID");

            migrationBuilder.RenameIndex(
                name: "IX_Solutions_ParentIssueID",
                table: "Solutions",
                newName: "IX_Solutions_IssueID");

            migrationBuilder.AddForeignKey(
                name: "FK_Solutions_Issues_IssueID",
                table: "Solutions",
                column: "IssueID",
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
