using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteDiscoveryReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "NodeRows",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                collation: "Latin1_General_100_CI_AI_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Latin1_General_100_CI_AI_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Markdown",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Latin1_General_100_CI_AI_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Latin1_General_100_CI_AI_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Latin1_General_100_CI_AI_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Latin1_General_100_CI_AI_SC",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DiscoveryRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PageVersion = table.Column<long>(type: "bigint", nullable: false),
                    DetailVersion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscoveryRevisions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DiscoveryRevisions",
                columns: new[] { "Id", "DetailVersion", "PageVersion" },
                values: new object[] { 1, 0L, 0L });

            migrationBuilder.CreateIndex(
                name: "IX_VoteRows_TargetType_TargetId",
                table: "VoteRows",
                columns: new[] { "TargetType", "TargetId" })
                .Annotation("SqlServer:Include", new[] { "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeReactionRows_NodeId_LifecycleState_ReactionDefinitionId",
                table: "NodeReactionRows",
                columns: new[] { "NodeId", "LifecycleState", "ReactionDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_NodeId_Status_VisibilityRestoredAt",
                table: "ModerationCaseRows",
                columns: new[] { "NodeId", "Status", "VisibilityRestoredAt" })
                .Annotation("SqlServer:Include", new[] { "PublicReason", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommentRows_TargetKind_TargetId",
                table: "CommentRows",
                columns: new[] { "TargetKind", "TargetId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscoveryRevisions");

            migrationBuilder.DropIndex(
                name: "IX_VoteRows_TargetType_TargetId",
                table: "VoteRows");

            migrationBuilder.DropIndex(
                name: "IX_NodeReactionRows_NodeId_LifecycleState_ReactionDefinitionId",
                table: "NodeReactionRows");

            migrationBuilder.DropIndex(
                name: "IX_ModerationCaseRows_NodeId_Status_VisibilityRestoredAt",
                table: "ModerationCaseRows");

            migrationBuilder.DropIndex(
                name: "IX_CommentRows_TargetKind_TargetId",
                table: "CommentRows");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "NodeRows",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldCollation: "Latin1_General_100_CI_AI_SC");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AI_SC");

            migrationBuilder.AlterColumn<string>(
                name: "Markdown",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AI_SC");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AI_SC");

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AI_SC");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "BlockRows",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Latin1_General_100_CI_AI_SC");
        }
    }
}
