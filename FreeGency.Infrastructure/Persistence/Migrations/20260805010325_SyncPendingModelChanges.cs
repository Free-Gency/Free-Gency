using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TeamLeads may already be widened; only alter when still nvarchar(200).
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = N'portfolio'
                      AND t.name = N'PortfolioProjects'
                      AND c.name = N'TeamLeads'
                      AND c.max_length = 400
                )
                BEGIN
                    DECLARE @df nvarchar(max);
                    SELECT @df = QUOTENAME(d.name)
                    FROM sys.default_constraints d
                    INNER JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = N'portfolio' AND t.name = N'PortfolioProjects' AND c.name = N'TeamLeads';

                    IF @df IS NOT NULL
                        EXEC(N'ALTER TABLE [portfolio].[PortfolioProjects] DROP CONSTRAINT ' + @df + N';');

                    ALTER TABLE [portfolio].[PortfolioProjects] ALTER COLUMN [TeamLeads] nvarchar(2000) NULL;
                END
                """);

            // Table may already exist from an earlier manual/partial apply.
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[teams].[TeamFeedbacks]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [teams].[TeamFeedbacks] (
                        [Id] uniqueidentifier NOT NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                        [CreatedBy] nvarchar(256) NOT NULL DEFAULT N'system',
                        [UpdatedAt] datetime2 NULL,
                        [UpdatedBy] nvarchar(256) NULL,
                        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
                        [DeletedAt] datetime2 NULL,
                        [DeletedBy] nvarchar(256) NULL,
                        [TeamId] uniqueidentifier NOT NULL,
                        [ReviewerUserId] uniqueidentifier NOT NULL,
                        [Rating] int NOT NULL,
                        [Comment] nvarchar(500) NULL,
                        CONSTRAINT [PK_TeamFeedbacks] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_TeamFeedbacks_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [teams].[Teams] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_TeamFeedbacks_Users_ReviewerUserId] FOREIGN KEY ([ReviewerUserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_TeamFeedbacks_ReviewerUserId]
                        ON [teams].[TeamFeedbacks] ([ReviewerUserId]);

                    CREATE INDEX [IX_TeamFeedbacks_TeamId_CreatedAt]
                        ON [teams].[TeamFeedbacks] ([TeamId], [CreatedAt]);

                    CREATE UNIQUE INDEX [IX_TeamFeedbacks_TeamId_ReviewerUserId]
                        ON [teams].[TeamFeedbacks] ([TeamId], [ReviewerUserId]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[teams].[TeamFeedbacks]', N'U') IS NOT NULL
                    DROP TABLE [teams].[TeamFeedbacks];
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = N'portfolio'
                      AND t.name = N'PortfolioProjects'
                      AND c.name = N'TeamLeads'
                      AND c.max_length = 4000
                )
                BEGIN
                    ALTER TABLE [portfolio].[PortfolioProjects] ALTER COLUMN [TeamLeads] nvarchar(200) NULL;
                END
                """);
        }
    }
}
