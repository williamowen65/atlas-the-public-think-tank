using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeCreationQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NodeCreationQuotaRows",
                columns: table => new
                {
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeCreationQuotaRows", x => x.NodeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeCreationQuotaRows_ParticipantId_CreatedAt",
                table: "NodeCreationQuotaRows",
                columns: new[] { "ParticipantId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeCreationQuotaRows");
        }
    }
}
