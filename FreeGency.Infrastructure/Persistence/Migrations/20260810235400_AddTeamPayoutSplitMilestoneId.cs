using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamPayoutSplitMilestoneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_TeamPayoutSplits_TeamId'
                      AND object_id = OBJECT_ID(N'finance.TeamPayoutSplits'))
                BEGIN
                    DROP INDEX [IX_TeamPayoutSplits_TeamId] ON [finance].[TeamPayoutSplits];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('finance.TeamPayoutSplits', 'MilestoneId') IS NULL
                BEGIN
                    ALTER TABLE [finance].[TeamPayoutSplits]
                    ADD [MilestoneId] uniqueidentifier NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_TeamPayoutSplits_MilestoneId'
                      AND object_id = OBJECT_ID(N'finance.TeamPayoutSplits'))
                BEGIN
                    CREATE INDEX [IX_TeamPayoutSplits_MilestoneId]
                        ON [finance].[TeamPayoutSplits] ([MilestoneId]);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_TeamPayoutSplits_TeamId_ProjectId_MilestoneId_UserId'
                      AND object_id = OBJECT_ID(N'finance.TeamPayoutSplits'))
                BEGIN
                    CREATE INDEX [IX_TeamPayoutSplits_TeamId_ProjectId_MilestoneId_UserId]
                        ON [finance].[TeamPayoutSplits] ([TeamId], [ProjectId], [MilestoneId], [UserId]);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = N'FK_TeamPayoutSplits_Milestones_MilestoneId'
                      AND parent_object_id = OBJECT_ID(N'finance.TeamPayoutSplits'))
                BEGIN
                    ALTER TABLE [finance].[TeamPayoutSplits] WITH CHECK
                    ADD CONSTRAINT [FK_TeamPayoutSplits_Milestones_MilestoneId]
                        FOREIGN KEY ([MilestoneId])
                        REFERENCES [marketplace].[Milestones] ([Id])
                        ON DELETE NO ACTION;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = N'FK_TeamPayoutSplits_Milestones_MilestoneId'
                      AND parent_object_id = OBJECT_ID(N'finance.TeamPayoutSplits'))
                BEGIN
                    ALTER TABLE [finance].[TeamPayoutSplits]
                    DROP CONSTRAINT [FK_TeamPayoutSplits_Milestones_MilestoneId];
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_TeamPayoutSplits_TeamId_ProjectId_MilestoneId_UserId'
                      AND object_id = OBJECT_ID(N'finance.TeamPayoutSplits'))
                BEGIN
                    DROP INDEX [IX_TeamPayoutSplits_TeamId_ProjectId_MilestoneId_UserId]
                        ON [finance].[TeamPayoutSplits];
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_TeamPayoutSplits_MilestoneId'
                      AND object_id = OBJECT_ID(N'finance.TeamPayoutSplits'))
                BEGIN
                    DROP INDEX [IX_TeamPayoutSplits_MilestoneId]
                        ON [finance].[TeamPayoutSplits];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('finance.TeamPayoutSplits', 'MilestoneId') IS NOT NULL
                BEGIN
                    ALTER TABLE [finance].[TeamPayoutSplits] DROP COLUMN [MilestoneId];
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_TeamPayoutSplits_TeamId'
                      AND object_id = OBJECT_ID(N'finance.TeamPayoutSplits'))
                BEGIN
                    CREATE INDEX [IX_TeamPayoutSplits_TeamId]
                        ON [finance].[TeamPayoutSplits] ([TeamId]);
                END
                """);
        }
    }
}
