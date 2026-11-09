using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobeVisitorHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobeVisitors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    FirstSeenAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobeVisitors", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobeBrowsers");

            migrationBuilder.DropTable(
                name: "GlobeVisitors");
        }
    }
}
