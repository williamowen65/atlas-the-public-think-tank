using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class langupdatetoissues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Forums_ForumID",
                schema: "forums",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Solutions_ForumSolutionID",
                schema: "forums",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Forums_ForumID",
                schema: "forums",
                table: "Solutions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHistory_Forums_ForumID",
                schema: "users",
                table: "UserHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHistory_Solutions_ForumSolutionID",
                schema: "users",
                table: "UserHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVotes_Forums_ForumID",
                schema: "forums",
                table: "UserVotes");

            migrationBuilder.DropTable(
                name: "ForumsCategories",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "Forums",
                schema: "forums");

            migrationBuilder.RenameColumn(
                name: "ForumSolutionID",
                schema: "forums",
                table: "UserVotes",
                newName: "IssueSolutionID");

            migrationBuilder.RenameColumn(
                name: "ForumID",
                schema: "forums",
                table: "UserVotes",
                newName: "IssueID");

            migrationBuilder.RenameIndex(
                name: "IX_UserVotes_ForumID",
                schema: "forums",
                table: "UserVotes",
                newName: "IX_UserVotes_IssueID");

            migrationBuilder.RenameColumn(
                name: "ForumSolutionID",
                schema: "users",
                table: "UserHistory",
                newName: "IssueSolutionID");

            migrationBuilder.RenameColumn(
                name: "ForumID",
                schema: "users",
                table: "UserHistory",
                newName: "IssueID");

            migrationBuilder.RenameIndex(
                name: "IX_UserHistory_ForumSolutionID",
                schema: "users",
                table: "UserHistory",
                newName: "IX_UserHistory_IssueSolutionID");

            migrationBuilder.RenameIndex(
                name: "IX_UserHistory_ForumID",
                schema: "users",
                table: "UserHistory",
                newName: "IX_UserHistory_IssueID");

            migrationBuilder.RenameColumn(
                name: "ForumID",
                schema: "forums",
                table: "Solutions",
                newName: "IssueID");

            migrationBuilder.RenameIndex(
                name: "IX_Solutions_ForumID",
                schema: "forums",
                table: "Solutions",
                newName: "IX_Solutions_IssueID");

            migrationBuilder.RenameColumn(
                name: "ForumSolutionID",
                schema: "forums",
                table: "Comments",
                newName: "IssueSolutionID");

            migrationBuilder.RenameColumn(
                name: "ForumID",
                schema: "forums",
                table: "Comments",
                newName: "IssueID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ForumSolutionID",
                schema: "forums",
                table: "Comments",
                newName: "IX_Comments_IssueSolutionID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ForumID",
                schema: "forums",
                table: "Comments",
                newName: "IX_Comments_IssueID");

            migrationBuilder.CreateTable(
                name: "Issues",
                schema: "forums",
                columns: table => new
                {
                    IssueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScopeID = table.Column<int>(type: "int", nullable: false),
                    ParentIssueID = table.Column<int>(type: "int", nullable: true),
                    BlockedContentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.IssueID);
                    table.ForeignKey(
                        name: "FK_Issues_AspNetUsers_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_BlockedContent_BlockedContentID",
                        column: x => x.BlockedContentID,
                        principalSchema: "forums",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Issues_ParentIssueID",
                        column: x => x.ParentIssueID,
                        principalSchema: "forums",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Scopes_ScopeID",
                        column: x => x.ScopeID,
                        principalSchema: "forums",
                        principalTable: "Scopes",
                        principalColumn: "ScopeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssuesCategories",
                schema: "forums",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    IssueID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuesCategories", x => new { x.CategoryID, x.IssueID });
                    table.ForeignKey(
                        name: "FK_IssuesCategories_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalSchema: "forums",
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssuesCategories_Issues_IssueID",
                        column: x => x.IssueID,
                        principalSchema: "forums",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "forums",
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { 1, "1a61454c-5b83-4aab-8661-96d6dffbee30", null, "A forum to discuss practical solutions to climate change at individual and policy levels.", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 1, "Climate Change Solutions" },
                    { 2, "1a61454c-5b83-4aab-8661-96d6dffbee30", null, "Discussion on modern urban planning approaches for sustainable and livable cities.", 1, new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 2, "Urban Planning Innovations" },
                    { 3, "1a61454c-5b83-4aab-8661-96d6dffbe31", null, "Strategies for transitioning to renewable energy sources at community and national levels.", 1, new DateTime(2024, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, "Renewable Energy Transition" }
                });

            migrationBuilder.InsertData(
                schema: "forums",
                table: "IssuesCategories",
                columns: new[] { "CategoryID", "IssueID" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 2, 2 },
                    { 4, 2 },
                    { 5, 2 },
                    { 8, 1 },
                    { 1, 3 },
                    { 2, 3 },
                    { 4, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_AuthorID",
                schema: "forums",
                table: "Issues",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_BlockedContentID",
                schema: "forums",
                table: "Issues",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentIssueID",
                schema: "forums",
                table: "Issues",
                column: "ParentIssueID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ScopeID",
                schema: "forums",
                table: "Issues",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuesCategories_IssueID",
                schema: "forums",
                table: "IssuesCategories",
                column: "IssueID");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Issues_IssueID",
                schema: "forums",
                table: "Comments",
                column: "IssueID",
                principalSchema: "forums",
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Solutions_IssueSolutionID",
                schema: "forums",
                table: "Comments",
                column: "IssueSolutionID",
                principalSchema: "forums",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Solutions_Issues_IssueID",
                schema: "forums",
                table: "Solutions",
                column: "IssueID",
                principalSchema: "forums",
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHistory_Issues_IssueID",
                schema: "users",
                table: "UserHistory",
                column: "IssueID",
                principalSchema: "forums",
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHistory_Solutions_IssueSolutionID",
                schema: "users",
                table: "UserHistory",
                column: "IssueSolutionID",
                principalSchema: "forums",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVotes_Issues_IssueID",
                schema: "forums",
                table: "UserVotes",
                column: "IssueID",
                principalSchema: "forums",
                principalTable: "Issues",
                principalColumn: "IssueID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Issues_IssueID",
                schema: "forums",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Solutions_IssueSolutionID",
                schema: "forums",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Issues_IssueID",
                schema: "forums",
                table: "Solutions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHistory_Issues_IssueID",
                schema: "users",
                table: "UserHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHistory_Solutions_IssueSolutionID",
                schema: "users",
                table: "UserHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVotes_Issues_IssueID",
                schema: "forums",
                table: "UserVotes");

            migrationBuilder.DropTable(
                name: "IssuesCategories",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "Issues",
                schema: "forums");

            migrationBuilder.RenameColumn(
                name: "IssueSolutionID",
                schema: "forums",
                table: "UserVotes",
                newName: "ForumSolutionID");

            migrationBuilder.RenameColumn(
                name: "IssueID",
                schema: "forums",
                table: "UserVotes",
                newName: "ForumID");

            migrationBuilder.RenameIndex(
                name: "IX_UserVotes_IssueID",
                schema: "forums",
                table: "UserVotes",
                newName: "IX_UserVotes_ForumID");

            migrationBuilder.RenameColumn(
                name: "IssueSolutionID",
                schema: "users",
                table: "UserHistory",
                newName: "ForumSolutionID");

            migrationBuilder.RenameColumn(
                name: "IssueID",
                schema: "users",
                table: "UserHistory",
                newName: "ForumID");

            migrationBuilder.RenameIndex(
                name: "IX_UserHistory_IssueSolutionID",
                schema: "users",
                table: "UserHistory",
                newName: "IX_UserHistory_ForumSolutionID");

            migrationBuilder.RenameIndex(
                name: "IX_UserHistory_IssueID",
                schema: "users",
                table: "UserHistory",
                newName: "IX_UserHistory_ForumID");

            migrationBuilder.RenameColumn(
                name: "IssueID",
                schema: "forums",
                table: "Solutions",
                newName: "ForumID");

            migrationBuilder.RenameIndex(
                name: "IX_Solutions_IssueID",
                schema: "forums",
                table: "Solutions",
                newName: "IX_Solutions_ForumID");

            migrationBuilder.RenameColumn(
                name: "IssueSolutionID",
                schema: "forums",
                table: "Comments",
                newName: "ForumSolutionID");

            migrationBuilder.RenameColumn(
                name: "IssueID",
                schema: "forums",
                table: "Comments",
                newName: "ForumID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_IssueSolutionID",
                schema: "forums",
                table: "Comments",
                newName: "IX_Comments_ForumSolutionID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_IssueID",
                schema: "forums",
                table: "Comments",
                newName: "IX_Comments_ForumID");

            migrationBuilder.CreateTable(
                name: "Forums",
                schema: "forums",
                columns: table => new
                {
                    ForumID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BlockedContentID = table.Column<int>(type: "int", nullable: true),
                    ParentForumID = table.Column<int>(type: "int", nullable: true),
                    ScopeID = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forums", x => x.ForumID);
                    table.ForeignKey(
                        name: "FK_Forums_AspNetUsers_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Forums_BlockedContent_BlockedContentID",
                        column: x => x.BlockedContentID,
                        principalSchema: "forums",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Forums_Forums_ParentForumID",
                        column: x => x.ParentForumID,
                        principalSchema: "forums",
                        principalTable: "Forums",
                        principalColumn: "ForumID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Forums_Scopes_ScopeID",
                        column: x => x.ScopeID,
                        principalSchema: "forums",
                        principalTable: "Scopes",
                        principalColumn: "ScopeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ForumsCategories",
                schema: "forums",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    ForumID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumsCategories", x => new { x.CategoryID, x.ForumID });
                    table.ForeignKey(
                        name: "FK_ForumsCategories_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalSchema: "forums",
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForumsCategories_Forums_ForumID",
                        column: x => x.ForumID,
                        principalSchema: "forums",
                        principalTable: "Forums",
                        principalColumn: "ForumID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "forums",
                table: "Forums",
                columns: new[] { "ForumID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentForumID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { 1, "1a61454c-5b83-4aab-8661-96d6dffbee30", null, "A forum to discuss practical solutions to climate change at individual and policy levels.", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 1, "Climate Change Solutions" },
                    { 2, "1a61454c-5b83-4aab-8661-96d6dffbee30", null, "Discussion on modern urban planning approaches for sustainable and livable cities.", 1, new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 2, "Urban Planning Innovations" },
                    { 3, "1a61454c-5b83-4aab-8661-96d6dffbe31", null, "Strategies for transitioning to renewable energy sources at community and national levels.", 1, new DateTime(2024, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, "Renewable Energy Transition" }
                });

            migrationBuilder.InsertData(
                schema: "forums",
                table: "ForumsCategories",
                columns: new[] { "CategoryID", "ForumID" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 2, 2 },
                    { 4, 2 },
                    { 5, 2 },
                    { 8, 1 },
                    { 1, 3 },
                    { 2, 3 },
                    { 4, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Forums_AuthorID",
                schema: "forums",
                table: "Forums",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Forums_BlockedContentID",
                schema: "forums",
                table: "Forums",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Forums_ParentForumID",
                schema: "forums",
                table: "Forums",
                column: "ParentForumID");

            migrationBuilder.CreateIndex(
                name: "IX_Forums_ScopeID",
                schema: "forums",
                table: "Forums",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_ForumsCategories_ForumID",
                schema: "forums",
                table: "ForumsCategories",
                column: "ForumID");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Forums_ForumID",
                schema: "forums",
                table: "Comments",
                column: "ForumID",
                principalSchema: "forums",
                principalTable: "Forums",
                principalColumn: "ForumID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Solutions_ForumSolutionID",
                schema: "forums",
                table: "Comments",
                column: "ForumSolutionID",
                principalSchema: "forums",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Solutions_Forums_ForumID",
                schema: "forums",
                table: "Solutions",
                column: "ForumID",
                principalSchema: "forums",
                principalTable: "Forums",
                principalColumn: "ForumID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHistory_Forums_ForumID",
                schema: "users",
                table: "UserHistory",
                column: "ForumID",
                principalSchema: "forums",
                principalTable: "Forums",
                principalColumn: "ForumID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHistory_Solutions_ForumSolutionID",
                schema: "users",
                table: "UserHistory",
                column: "ForumSolutionID",
                principalSchema: "forums",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVotes_Forums_ForumID",
                schema: "forums",
                table: "UserVotes",
                column: "ForumID",
                principalSchema: "forums",
                principalTable: "Forums",
                principalColumn: "ForumID");
        }
    }
}
