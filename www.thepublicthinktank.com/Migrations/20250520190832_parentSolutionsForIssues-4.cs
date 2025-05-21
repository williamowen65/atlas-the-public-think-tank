using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class parentSolutionsForIssues4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentVotes_AspNetUsers_UserID",
                schema: "comments",
                table: "CommentVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentVotes_Comments_CommentID",
                schema: "comments",
                table: "CommentVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueVotes_AspNetUsers_UserID",
                schema: "issues",
                table: "IssueVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueVotes_Issues_IssueID",
                schema: "issues",
                table: "IssueVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SolutionVotes_AspNetUsers_UserID",
                schema: "solutions",
                table: "SolutionVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SolutionVotes_Solutions_SolutionID",
                schema: "solutions",
                table: "SolutionVotes");

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                schema: "solutions",
                table: "SolutionVotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                schema: "issues",
                table: "IssueVotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                schema: "comments",
                table: "CommentVotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolutionVotes_AppUserId",
                schema: "solutions",
                table: "SolutionVotes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueVotes_AppUserId",
                schema: "issues",
                table: "IssueVotes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentVotes_AppUserId",
                schema: "comments",
                table: "CommentVotes",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentVotes_AspNetUsers_AppUserId",
                schema: "comments",
                table: "CommentVotes",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentVotes_AspNetUsers_UserID",
                schema: "comments",
                table: "CommentVotes",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentVotes_Comments_CommentID",
                schema: "comments",
                table: "CommentVotes",
                column: "CommentID",
                principalSchema: "comments",
                principalTable: "Comments",
                principalColumn: "CommentID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueVotes_AspNetUsers_AppUserId",
                schema: "issues",
                table: "IssueVotes",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueVotes_AspNetUsers_UserID",
                schema: "issues",
                table: "IssueVotes",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueVotes_Issues_IssueID",
                schema: "issues",
                table: "IssueVotes",
                column: "IssueID",
                principalSchema: "issues",
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionVotes_AspNetUsers_AppUserId",
                schema: "solutions",
                table: "SolutionVotes",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionVotes_AspNetUsers_UserID",
                schema: "solutions",
                table: "SolutionVotes",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionVotes_Solutions_SolutionID",
                schema: "solutions",
                table: "SolutionVotes",
                column: "SolutionID",
                principalSchema: "solutions",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentVotes_AspNetUsers_AppUserId",
                schema: "comments",
                table: "CommentVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentVotes_AspNetUsers_UserID",
                schema: "comments",
                table: "CommentVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentVotes_Comments_CommentID",
                schema: "comments",
                table: "CommentVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueVotes_AspNetUsers_AppUserId",
                schema: "issues",
                table: "IssueVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueVotes_AspNetUsers_UserID",
                schema: "issues",
                table: "IssueVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_IssueVotes_Issues_IssueID",
                schema: "issues",
                table: "IssueVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SolutionVotes_AspNetUsers_AppUserId",
                schema: "solutions",
                table: "SolutionVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SolutionVotes_AspNetUsers_UserID",
                schema: "solutions",
                table: "SolutionVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SolutionVotes_Solutions_SolutionID",
                schema: "solutions",
                table: "SolutionVotes");

            migrationBuilder.DropIndex(
                name: "IX_SolutionVotes_AppUserId",
                schema: "solutions",
                table: "SolutionVotes");

            migrationBuilder.DropIndex(
                name: "IX_IssueVotes_AppUserId",
                schema: "issues",
                table: "IssueVotes");

            migrationBuilder.DropIndex(
                name: "IX_CommentVotes_AppUserId",
                schema: "comments",
                table: "CommentVotes");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                schema: "solutions",
                table: "SolutionVotes");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                schema: "issues",
                table: "IssueVotes");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                schema: "comments",
                table: "CommentVotes");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentVotes_AspNetUsers_UserID",
                schema: "comments",
                table: "CommentVotes",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentVotes_Comments_CommentID",
                schema: "comments",
                table: "CommentVotes",
                column: "CommentID",
                principalSchema: "comments",
                principalTable: "Comments",
                principalColumn: "CommentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueVotes_AspNetUsers_UserID",
                schema: "issues",
                table: "IssueVotes",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IssueVotes_Issues_IssueID",
                schema: "issues",
                table: "IssueVotes",
                column: "IssueID",
                principalSchema: "issues",
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionVotes_AspNetUsers_UserID",
                schema: "solutions",
                table: "SolutionVotes",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionVotes_Solutions_SolutionID",
                schema: "solutions",
                table: "SolutionVotes",
                column: "SolutionID",
                principalSchema: "solutions",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
