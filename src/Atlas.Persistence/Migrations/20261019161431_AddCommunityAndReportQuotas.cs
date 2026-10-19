using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityAndReportQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityCreationQuotaRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityCreationQuotaRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportSubmissionQuotaRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSubmissionQuotaRows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCreationQuotaRows_ParticipantId_OccurredAt",
                table: "CommunityCreationQuotaRows",
                columns: new[] { "ParticipantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubmissionQuotaRows_ParticipantId_OccurredAt",
                table: "ReportSubmissionQuotaRows",
                columns: new[] { "ParticipantId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityCreationQuotaRows");

            migrationBuilder.DropTable(
                name: "ReportSubmissionQuotaRows");
        }
    }
}
