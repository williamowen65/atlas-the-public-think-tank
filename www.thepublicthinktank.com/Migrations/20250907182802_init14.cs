using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("681601f2-ed2c-4188-9221-879efa33cc67"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("a2c7d49e-5f38-41b6-9e76-8c429d5b1f83"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Let users toggle what affects their feed: recency, variety, depth, etc.\n\nSocial media platforms should implement transparent algorithm settings that allow users to have direct control over what content appears in their feeds. This solution would enable individuals to customize their experience based on personal preferences and values, rather than being subject to black-box algorithms optimized solely for engagement metrics.\n\nKey features would include:\n\n- Adjustable content preferences: Users could set sliders for content recency vs. relevance, content diversity, topic depth, and the balance between content from connections versus broader sources\n\n- Explicit content filters: Clear options to filter sensitive topics, controversial content, or specific categories based on personal comfort levels\n\n- Algorithm transparency documentation: Plain-language explanations of how the algorithm works and what factors influence content selection\n\n- Usage insights: Data visualizations showing users how their content consumption patterns affect what they see\n\nThis approach would return agency to users, reduce algorithm-driven echo chambers, and build trust through transparency. Platforms implementing such settings would differentiate themselves as more ethical and user-centric, potentially attracting individuals concerned about digital wellbeing and algorithmic manipulation.", 1, new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("681601f2-ed2c-4188-9221-879efa33cc67"), "Transparent Algorithm Settings" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("a2c7d49e-5f38-41b6-9e76-8c429d5b1f83"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("681601f2-ed2c-4188-9221-879efa33cc67"));
        }
    }
}
