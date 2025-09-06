using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("f89199d9-3218-4712-8362-5c21e4e741c2"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), null, "Paid Transitional Employment programs offer structured, time-limited work opportunities that provide real income, build skills, and create pathways to permanent employment for individuals experiencing homelessness. These initiatives recognize that stable employment is a critical component of housing security, while acknowledging that many unhoused individuals face significant barriers to entering the traditional workforce immediately.\n\nUrban clean-up initiatives represent one successful model, employing homeless or recently housed individuals to maintain public spaces, remove litter, abate graffiti, and beautify neighborhoods. These programs serve multiple purposes: providing meaningful work with immediate compensation, improving community environments, fostering positive interactions between homeless individuals and the broader community, and demonstrating participants' capabilities and work ethic to potential employers.\n\nBeyond urban clean-up, effective transitional employment models include: maintenance and restoration of public parks and trails; peer outreach and navigation services for other homeless individuals; food service in community kitchens; retail positions in social enterprise businesses; administrative support in nonprofit organizations; and environmental stewardship projects. The most successful programs carefully match positions to participants' existing skills and interests while providing opportunities to develop new capabilities.\n\nComprehensive programs incorporate several key elements: predictable schedules with flexible options to accommodate health needs and service appointments; graduated responsibility as participants build confidence and skills; regular compensation at fair wages, ideally with opportunities for wage progression; integrated support services including case management, housing assistance, and mental health resources; financial literacy training and banking access; job-readiness preparation such as resume building and interview skills; and explicit pathways to permanent employment through partnerships with local businesses, preferential hiring agreements, or supported job placement.\n\nTransitional employment initiatives require thoughtful design to avoid potential pitfalls such as creating dependency or perpetuating low-wage work. Programs should establish clear timelines and goals, ensure that participants receive genuine skill development rather than just busywork, maintain strong relationships with permanent employers, and provide ongoing support during transitions to unsubsidized employment.\n\nWhen properly implemented, paid transitional employment delivers significant returns on investment: participants gain income stability, work experience, and self-confidence; communities benefit from improved public spaces and reduced visible homelessness; employers access a prepared workforce; and public systems may realize cost savings through reduced reliance on emergency services, shelters, and other crisis interventions.", 1, new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("f89199d9-3218-4712-8362-5c21e4e741c2"), "Paid Transitional Employment Programs" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a1b7f6e5-8d32-47c0-b965-2f48a3b1c7d6"), null, new DateTime(2024, 10, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("a5b1f0e9-2d76-41c4-b309-6f82a7b5c1d0"), null, new DateTime(2024, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 },
                    { new Guid("a7b3f2e1-4d98-43c6-b521-8f04a9b7c3d2"), null, new DateTime(2024, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("b2c8a7f6-9e43-48d1-b076-3a59b4c2d8e7"), null, new DateTime(2024, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("b6c2a1f0-3e87-42d5-b410-7a93b8c6d2e1"), null, new DateTime(2024, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 10 },
                    { new Guid("c3d9b8a7-0f54-49e2-b187-4b60c5d3e9f8"), null, new DateTime(2024, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("c7d3b2a1-4f98-43e6-a521-8b04c9d7e3f2"), null, new DateTime(2024, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 9 },
                    { new Guid("d4e0c9b8-1a65-40f3-b298-5c71d6e4f0a9"), null, new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("d8e4c3b2-5a09-44f7-b632-9c15d0e8f4a3"), null, new DateTime(2024, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 8 },
                    { new Guid("e5f1d0c9-2b76-41a4-b309-6d82e7f5a1b0"), null, new DateTime(2024, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 8 },
                    { new Guid("e9f5d4c3-6b10-45a8-b743-0d26e1f9a5b4"), null, new DateTime(2024, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("f0a6e5d4-7c21-46b9-b854-1e37f2a0b6c5"), null, new DateTime(2024, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("f4a0e9d8-1c65-40b3-b298-5e71f6a4b0c9"), null, new DateTime(2024, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("f6a2e1d0-3c87-42b5-b410-7e93f8a6b2c1"), null, new DateTime(2024, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a1b7f6e5-8d32-47c0-b965-2f48a3b1c7d6"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a5b1f0e9-2d76-41c4-b309-6f82a7b5c1d0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a7b3f2e1-4d98-43c6-b521-8f04a9b7c3d2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b2c8a7f6-9e43-48d1-b076-3a59b4c2d8e7"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b6c2a1f0-3e87-42d5-b410-7a93b8c6d2e1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c3d9b8a7-0f54-49e2-b187-4b60c5d3e9f8"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c7d3b2a1-4f98-43e6-a521-8b04c9d7e3f2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d4e0c9b8-1a65-40f3-b298-5c71d6e4f0a9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d8e4c3b2-5a09-44f7-b632-9c15d0e8f4a3"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e5f1d0c9-2b76-41a4-b309-6d82e7f5a1b0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e9f5d4c3-6b10-45a8-b743-0d26e1f9a5b4"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f0a6e5d4-7c21-46b9-b854-1e37f2a0b6c5"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f4a0e9d8-1c65-40b3-b298-5e71f6a4b0c9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f6a2e1d0-3c87-42b5-b410-7e93f8a6b2c1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("f89199d9-3218-4712-8362-5c21e4e741c2"));
        }
    }
}
