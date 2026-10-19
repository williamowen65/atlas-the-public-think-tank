using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVoteActionQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VoteActionQuotaRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteActionQuotaRows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoteActionQuotaRows_ParticipantId_OccurredAt",
                table: "VoteActionQuotaRows",
                columns: new[] { "ParticipantId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoteActionQuotaRows");
        }
    }
}
