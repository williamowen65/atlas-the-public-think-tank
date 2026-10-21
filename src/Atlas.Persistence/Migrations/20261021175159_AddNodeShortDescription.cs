using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeShortDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "NodeRows",
                type: "nvarchar(280)",
                maxLength: 280,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "NodeRows");
        }
    }
}
