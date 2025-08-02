using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"),
                column: "Content",
                value: "Atlas: The Public Think Tank represents a paradigm shift in how social media platforms function. While traditional platforms prioritize engagement metrics and advertising revenue, Atlas focuses on collaborative problem-solving and thoughtful discourse.\n\nKey innovations include:\n\n- Nuanced voting system: Instead of simplistic likes/dislikes, Atlas employs a 0-10 scale that encourages thoughtful evaluation of content quality and relevance\n\n- Issue-solution framework: Content is organized around problems and their potential solutions, creating natural context for constructive discussion\n\n- Transparency by design: Algorithm settings are fully adjustable by users, giving people control over what they see and why\n\n- Community-driven development: The platform itself is treated as an evolving project that users can help improve\n\nAtlas addresses many core problems with current social media: the amplification of divisive content, lack of nuance in discussions, and the prioritization of engagement over user wellbeing. By creating a space specifically designed for collaborative thinking and problem-solving, Atlas demonstrates that social platforms can be reimagined to better serve human needs.\n\nThis solution doesn't just critique existing social media—it offers a concrete alternative that shows how technology can be harnessed to connect people in more meaningful, productive ways.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"),
                column: "Content",
                value: "Why Atlas can become a leader in social media");
        }
    }
}
