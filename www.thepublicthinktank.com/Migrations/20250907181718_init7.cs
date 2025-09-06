using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("77f65326-607d-4c65-bff1-424bae316eab"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("c5d2e3f2-b1a0-47c9-8d67-5f2e3b4a1d90"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), null, "Microgrants for Unhoused Entrepreneurs or Gig Workers represents an innovative approach to addressing homelessness by fostering economic self-sufficiency through small-scale entrepreneurship and gig economy participation. This solution recognizes that many individuals experiencing homelessness possess valuable skills, creativity, and drive that can be leveraged to generate income when traditional employment paths may be inaccessible.\n\nThe program provides modest financial grants (typically $500-$5,000) directly to qualifying unhoused individuals with viable business ideas or who need equipment and resources to participate in gig economy opportunities. These funds can be used for essential business expenses: purchasing equipment or tools, securing necessary licenses or certifications, accessing digital technology, acquiring inventory, or covering transportation costs to work sites.\n\nBeyond financial support, a comprehensive microgrant program includes complementary resources that maximize success potential: business skills training tailored to different entrepreneurial ventures; mentorship from established entrepreneurs in similar fields; assistance with digital literacy and online platform navigation; simplified accounting tools for financial management; and connections to community markets, online platforms, or local businesses where goods or services can be sold.\n\nImplementation requires thoughtful design: low-barrier application processes that don't exclude those without formal documentation; tiered funding levels that allow for growth as businesses develop; flexibility in eligible expenses to accommodate diverse business models; and staged disbursement tied to business milestones rather than rigid timelines.\n\nThe benefits extend beyond immediate income generation. Participants develop transferable skills, build credit and financial history, establish professional networks, and gain confidence and dignity through meaningful work. The resulting financial stability helps secure and maintain housing, while successful microenterprises can potentially grow to employ others facing similar circumstances, creating a positive ripple effect in communities.", 1, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("77f65326-607d-4c65-bff1-424bae316eab"), "Microgrants for Unhoused Entrepreneurs or Gig Workers" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[] { new Guid("d6e7f8a9-b0c1-47d2-93e4-5f8c7b6a9d0e"), null, new DateTime(2024, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d2e3f2-b1a0-47c9-8d67-5f2e3b4a1d90"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d6e7f8a9-b0c1-47d2-93e4-5f8c7b6a9d0e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("c5d2e3f2-b1a0-47c9-8d67-5f2e3b4a1d90"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("77f65326-607d-4c65-bff1-424bae316eab"));
        }
    }
}
