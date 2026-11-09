using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountPinActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobeBrowsers");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSeenAt",
                table: "GlobeVisitors",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "GlobeVisitors");

            migrationBuilder.CreateTable(
                name: "GlobeBrowsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    VisitorId = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobeBrowsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobeBrowsers_GlobeVisitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "GlobeVisitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlobeBrowsers_VisitorId",
                table: "GlobeBrowsers",
                column: "VisitorId");
        }
    }
}
