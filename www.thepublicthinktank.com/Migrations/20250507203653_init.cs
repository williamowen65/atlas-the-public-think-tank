using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "forums");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlockedContent",
                schema: "forums",
                columns: table => new
                {
                    BlockedContentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReasonID = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedContent", x => x.BlockedContentID);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "forums",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                schema: "forums",
                columns: table => new
                {
                    ScopeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScopeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.ScopeID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Forums",
                schema: "forums",
                columns: table => new
                {
                    ForumID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScopeID = table.Column<int>(type: "int", nullable: false),
                    ParentForumID = table.Column<int>(type: "int", nullable: true),
                    BlockedContentID = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Solutions",
                schema: "forums",
                columns: table => new
                {
                    SolutionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForumID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedContentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solutions", x => x.SolutionID);
                    table.ForeignKey(
                        name: "FK_Solutions_AspNetUsers_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solutions_BlockedContent_BlockedContentID",
                        column: x => x.BlockedContentID,
                        principalSchema: "forums",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solutions_Forums_ForumID",
                        column: x => x.ForumID,
                        principalSchema: "forums",
                        principalTable: "Forums",
                        principalColumn: "ForumID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "forums",
                columns: table => new
                {
                    CommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForumID = table.Column<int>(type: "int", nullable: true),
                    ForumSolutionID = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentCommentID = table.Column<int>(type: "int", nullable: true),
                    BlockedContentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_BlockedContent_BlockedContentID",
                        column: x => x.BlockedContentID,
                        principalSchema: "forums",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentID",
                        column: x => x.ParentCommentID,
                        principalSchema: "forums",
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Forums_ForumID",
                        column: x => x.ForumID,
                        principalSchema: "forums",
                        principalTable: "Forums",
                        principalColumn: "ForumID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Solutions_ForumSolutionID",
                        column: x => x.ForumSolutionID,
                        principalSchema: "forums",
                        principalTable: "Solutions",
                        principalColumn: "SolutionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserHistory",
                schema: "users",
                columns: table => new
                {
                    UserHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ForumID = table.Column<int>(type: "int", nullable: true),
                    ForumSolutionID = table.Column<int>(type: "int", nullable: true),
                    CommentID = table.Column<int>(type: "int", nullable: true),
                    UserVote = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistory", x => x.UserHistoryID);
                    table.ForeignKey(
                        name: "FK_UserHistory_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHistory_Comments_CommentID",
                        column: x => x.CommentID,
                        principalSchema: "forums",
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHistory_Forums_ForumID",
                        column: x => x.ForumID,
                        principalSchema: "forums",
                        principalTable: "Forums",
                        principalColumn: "ForumID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHistory_Solutions_ForumSolutionID",
                        column: x => x.ForumSolutionID,
                        principalSchema: "forums",
                        principalTable: "Solutions",
                        principalColumn: "SolutionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserVotes",
                schema: "forums",
                columns: table => new
                {
                    ForumID = table.Column<int>(type: "int", nullable: false),
                    ForumSolutionID = table.Column<int>(type: "int", nullable: false),
                    CommentID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Vote = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVotes", x => new { x.UserID, x.ForumID, x.ForumSolutionID, x.CommentID });
                    table.ForeignKey(
                        name: "FK_UserVotes_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserVotes_Comments_CommentID",
                        column: x => x.CommentID,
                        principalSchema: "forums",
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserVotes_Forums_ForumID",
                        column: x => x.ForumID,
                        principalSchema: "forums",
                        principalTable: "Forums",
                        principalColumn: "ForumID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserVotes_Solutions_ForumSolutionID",
                        column: x => x.ForumSolutionID,
                        principalSchema: "forums",
                        principalTable: "Solutions",
                        principalColumn: "SolutionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorID",
                schema: "forums",
                table: "Comments",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BlockedContentID",
                schema: "forums",
                table: "Comments",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ForumID",
                schema: "forums",
                table: "Comments",
                column: "ForumID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ForumSolutionID",
                schema: "forums",
                table: "Comments",
                column: "ForumSolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentID",
                schema: "forums",
                table: "Comments",
                column: "ParentCommentID");

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

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_AuthorID",
                schema: "forums",
                table: "Solutions",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_BlockedContentID",
                schema: "forums",
                table: "Solutions",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_ForumID",
                schema: "forums",
                table: "Solutions",
                column: "ForumID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_CommentID",
                schema: "users",
                table: "UserHistory",
                column: "CommentID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_ForumID",
                schema: "users",
                table: "UserHistory",
                column: "ForumID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_ForumSolutionID",
                schema: "users",
                table: "UserHistory",
                column: "ForumSolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_UserID",
                schema: "users",
                table: "UserHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_CommentID",
                schema: "forums",
                table: "UserVotes",
                column: "CommentID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_ForumID",
                schema: "forums",
                table: "UserVotes",
                column: "ForumID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_ForumSolutionID",
                schema: "forums",
                table: "UserVotes",
                column: "ForumSolutionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ForumsCategories",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "UserHistory",
                schema: "users");

            migrationBuilder.DropTable(
                name: "UserVotes",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "Comments",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "Solutions",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "Forums",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BlockedContent",
                schema: "forums");

            migrationBuilder.DropTable(
                name: "Scopes",
                schema: "forums");
        }
    }
}
