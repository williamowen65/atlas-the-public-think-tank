using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("3d3a2d73-23e5-4622-9a82-46d0e6d84d26"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("e7d9c6b2-3a5f-4182-9e08-51bd72af46c3"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Let users filter feed by emotional tone — e.g., calm, curious, motivated.\n\nSocial media platforms should implement a 'Mood Bubbles' feature that allows users to curate their content experience based on desired emotional states. Unlike traditional content filters that focus on topics or sources, this solution would categorize content by its likely emotional impact on viewers.\n\nKey features would include:\n\n- Emotional tone filtering: Users could select from emotional states like 'calm', 'curious', 'motivated', 'inspired', 'joyful', or 'reflective' to match their current needs or desired mood\n\n- AI-powered content analysis: Content would be analyzed for emotional tone using natural language processing and image recognition to identify its likely emotional impact\n\n- Personalized calibration: The system would learn individual user responses to content over time, recognizing that different people may react differently to the same content\n\n- Mood scheduling: Users could set different emotional preferences for different times of day (e.g., 'motivated' in the morning, 'calm' in the evening)\n\nThis approach would transform social media from a source of unpredictable emotional stimuli to a tool for intentional emotional well-being. It acknowledges that content consumption affects mental state and gives users agency in managing this impact. Platforms adopting this feature would position themselves as leaders in supporting digital wellness and emotional intelligence.", 1, new DateTime(2024, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("3d3a2d73-23e5-4622-9a82-46d0e6d84d26"), "Mood Bubbles" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[] { new Guid("2f8a7d5e-4b9c-48e1-a036-7c53f4e8d12b"), null, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e7d9c6b2-3a5f-4182-9e08-51bd72af46c3"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 6 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("2f8a7d5e-4b9c-48e1-a036-7c53f4e8d12b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("e7d9c6b2-3a5f-4182-9e08-51bd72af46c3"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("3d3a2d73-23e5-4622-9a82-46d0e6d84d26"));
        }
    }
}
