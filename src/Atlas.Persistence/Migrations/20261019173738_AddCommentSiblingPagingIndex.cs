using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentSiblingPagingIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommentRows_TargetKind_TargetId",
                table: "CommentRows");

            migrationBuilder.CreateIndex(
                name: "IX_CommentRows_SiblingPage",
                table: "CommentRows",
                columns: new[] { "TargetKind", "TargetId", "ParentCommentId", "CreatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommentRows_SiblingPage",
                table: "CommentRows");

            migrationBuilder.CreateIndex(
                name: "IX_CommentRows_TargetKind_TargetId",
                table: "CommentRows",
                columns: new[] { "TargetKind", "TargetId" });
        }
    }
}
