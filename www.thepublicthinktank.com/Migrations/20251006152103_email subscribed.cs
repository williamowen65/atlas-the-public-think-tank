using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class emailsubscribed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SubscribedToEmail",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscribedToEmail",
                table: "AspNetUsers");
        }
    }
}
