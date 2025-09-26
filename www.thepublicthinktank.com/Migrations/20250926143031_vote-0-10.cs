using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class vote010 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_SolutionVote_VoteValue_Range",
                schema: "solutions",
                table: "SolutionVotes",
                sql: "[VoteValue] >= 0 AND [VoteValue] <= 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_IssueVote_VoteValue_Range",
                schema: "issues",
                table: "IssueVotes",
                sql: "[VoteValue] >= 0 AND [VoteValue] <= 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommentVote_VoteValue_Range",
                schema: "comments",
                table: "CommentVotes",
                sql: "[VoteValue] >= 0 AND [VoteValue] <= 10");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SolutionVote_VoteValue_Range",
                schema: "solutions",
                table: "SolutionVotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_IssueVote_VoteValue_Range",
                schema: "issues",
                table: "IssueVotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CommentVote_VoteValue_Range",
                schema: "comments",
                table: "CommentVotes");
        }
    }
}
