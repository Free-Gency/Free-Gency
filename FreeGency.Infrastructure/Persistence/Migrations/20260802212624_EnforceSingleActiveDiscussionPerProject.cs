using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleActiveDiscussionPerProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // One InDiscussion per project — extras go back to Viewed (same as Close Discussion).
            migrationBuilder.Sql(
                """
                ;WITH ranked AS (
                    SELECT
                        [Id],
                        ROW_NUMBER() OVER (
                            PARTITION BY [ProjectId]
                            ORDER BY
                                COALESCE([ResponseAt], [UpdatedAt], [AppliedAt], [CreatedAt]) DESC,
                                [AppliedAt] DESC,
                                [Id] DESC
                        ) AS rn
                    FROM [marketplace].[ProjectProposals]
                    WHERE [Status] = N'InDiscussion'
                      AND [IsDeleted] = 0
                )
                UPDATE p
                SET
                    p.[Status] = N'Viewed',
                    p.[UpdatedAt] = SYSUTCDATETIME(),
                    p.[UpdatedBy] = N'EnforceSingleActiveDiscussionPerProject'
                FROM [marketplace].[ProjectProposals] AS p
                INNER JOIN ranked AS r ON r.[Id] = p.[Id]
                WHERE r.rn > 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible data fix — cannot restore which extras were InDiscussion.
        }
    }
}
