using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Applies schema that orphan migration files claimed but EF never ran
    /// (no Designer / [Migration] attribute): Team.Cover, ChatRoom.Logo,
    /// and non-unique TeamCategories.TeamId index.
    /// </summary>
    public partial class FixMissingTeamCoverChatLogoAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'teams.Teams', N'Cover') IS NULL
                BEGIN
                    ALTER TABLE [teams].[Teams]
                    ADD [Cover] nvarchar(500) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'chat.ChatRooms', N'Logo') IS NULL
                BEGIN
                    ALTER TABLE [chat].[ChatRooms]
                    ADD [Logo] nvarchar(500) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes i
                    INNER JOIN sys.tables t ON i.object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = N'teams'
                      AND t.name = N'TeamCategories'
                      AND i.name = N'IX_TeamCategories_TeamId'
                      AND i.is_unique = 1
                )
                BEGIN
                    DROP INDEX [IX_TeamCategories_TeamId] ON [teams].[TeamCategories];

                    CREATE INDEX [IX_TeamCategories_TeamId]
                        ON [teams].[TeamCategories] ([TeamId]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes i
                    INNER JOIN sys.tables t ON i.object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = N'teams'
                      AND t.name = N'TeamCategories'
                      AND i.name = N'IX_TeamCategories_TeamId'
                      AND i.is_unique = 0
                )
                BEGIN
                    DROP INDEX [IX_TeamCategories_TeamId] ON [teams].[TeamCategories];

                    CREATE UNIQUE INDEX [IX_TeamCategories_TeamId]
                        ON [teams].[TeamCategories] ([TeamId]);
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'chat.ChatRooms', N'Logo') IS NOT NULL
                    ALTER TABLE [chat].[ChatRooms] DROP COLUMN [Logo];
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'teams.Teams', N'Cover') IS NOT NULL
                    ALTER TABLE [teams].[Teams] DROP COLUMN [Cover];
                """);
        }
    }
}
