using Microsoft.EntityFrameworkCore.Migrations;

namespace atlas_the_public_think_tank.Migrations_NonEF
{
    public partial class Issue_CanPublish_Constraint : ICustomMigration
    {
        public void Up(MigrationBuilder migrationBuilder)
        {
            // Issue logic (existing)
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[issues].[fn_Issue_CanPublish]', N'FN') IS NULL
                BEGIN
                    EXEC('
                        CREATE FUNCTION [issues].[fn_Issue_CanPublish](@IssueID UNIQUEIDENTIFIER)
                        RETURNS BIT
                        AS
                        BEGIN
                            DECLARE @ParentIssueID UNIQUEIDENTIFIER;
                            DECLARE @ParentSolutionID UNIQUEIDENTIFIER;

                            SELECT 
                                @ParentIssueID = [ParentIssueID],
                                @ParentSolutionID = [ParentSolutionID]
                            FROM [issues].[Issues]
                            WHERE [IssueID] = @IssueID;

                            -- If has a parent issue, it must be Published (1)
                            IF @ParentIssueID IS NOT NULL AND NOT EXISTS (
                                SELECT 1 FROM [issues].[Issues] p
                                WHERE p.[IssueID] = @ParentIssueID AND p.[ContentStatus] = 1
                            )
                                RETURN 0;

                            -- If has a parent solution, it must be Published (1)
                            IF @ParentSolutionID IS NOT NULL AND NOT EXISTS (
                                SELECT 1 FROM [solutions].[Solutions] s
                                WHERE s.[SolutionID] = @ParentSolutionID AND s.[ContentStatus] = 1
                            )
                                RETURN 0;

                            RETURN 1;
                        END
                    ')
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.check_constraints cc
                    JOIN sys.tables t ON t.object_id = cc.parent_object_id
                    JOIN sys.schemas s ON s.schema_id = t.schema_id
                    WHERE cc.name = N'CK_Issues_PublishRequiresPublishedParent'
                      AND t.name = N'Issues'
                      AND s.name = N'issues'
                )
                BEGIN
                    ALTER TABLE [issues].[Issues]
                    ADD CONSTRAINT [CK_Issues_PublishRequiresPublishedParent]
                    CHECK (([ContentStatus] <> 1) OR ([issues].[fn_Issue_CanPublish]([IssueID]) = 1));
                END
            ");

            // Solution logic (new)
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[solutions].[fn_Solution_CanPublish]', N'FN') IS NULL
                BEGIN
                    EXEC('
                        CREATE FUNCTION [solutions].[fn_Solution_CanPublish](@SolutionID UNIQUEIDENTIFIER)
                        RETURNS BIT
                        AS
                        BEGIN
                            DECLARE @ParentIssueID UNIQUEIDENTIFIER;

                            SELECT 
                                @ParentIssueID = [ParentIssueID]
                            FROM [solutions].[Solutions]
                            WHERE [SolutionID] = @SolutionID;

                            -- If has a parent issue, it must be Published (1)
                            IF @ParentIssueID IS NOT NULL AND NOT EXISTS (
                                SELECT 1 FROM [issues].[Issues] p
                                WHERE p.[IssueID] = @ParentIssueID AND p.[ContentStatus] = 1
                            )
                                RETURN 0;

                            RETURN 1;
                        END
                    ')
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.check_constraints cc
                    JOIN sys.tables t ON t.object_id = cc.parent_object_id
                    JOIN sys.schemas s ON s.schema_id = t.schema_id
                    WHERE cc.name = N'CK_Solutions_PublishRequiresPublishedParent'
                      AND t.name = N'Solutions'
                      AND s.name = N'solutions'
                )
                BEGIN
                    ALTER TABLE [solutions].[Solutions]
                    ADD CONSTRAINT [CK_Solutions_PublishRequiresPublishedParent]
                    CHECK (([ContentStatus] <> 1) OR ([solutions].[fn_Solution_CanPublish]([SolutionID]) = 1));
                END
            ");
        }

        public void Down(MigrationBuilder migrationBuilder)
        {
            // Issue logic (existing)
            migrationBuilder.DropCheckConstraint(
                name: "CK_Issues_PublishRequiresPublishedParent",
                table: "Issues",
                schema: "issues"
            );

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[issues].[fn_Issue_CanPublish]', N'FN') IS NOT NULL
                DROP FUNCTION [issues].[fn_Issue_CanPublish];
            ");

            // Solution logic (new)
            migrationBuilder.DropCheckConstraint(
                name: "CK_Solutions_PublishRequiresPublishedParent",
                table: "Solutions",
                schema: "solutions"
            );

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[solutions].[fn_Solution_CanPublish]', N'FN') IS NOT NULL
                DROP FUNCTION [solutions].[fn_Solution_CanPublish];
            ");
        }
    }
}