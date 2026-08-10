using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageMilestoneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Column may already exist in local DBs that applied the change outside migrations.
            migrationBuilder.Sql("""
                IF COL_LENGTH('chat.Messages', 'MilestoneId') IS NULL
                BEGIN
                    ALTER TABLE [chat].[Messages] ADD [MilestoneId] uniqueidentifier NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Messages_MilestoneId'
                      AND object_id = OBJECT_ID(N'chat.Messages'))
                BEGIN
                    CREATE INDEX [IX_Messages_MilestoneId]
                        ON [chat].[Messages] ([MilestoneId]);
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = N'FK_Messages_Milestones_MilestoneId'
                      AND parent_object_id = OBJECT_ID(N'chat.Messages'))
                BEGIN
                    ALTER TABLE [chat].[Messages] WITH CHECK
                    ADD CONSTRAINT [FK_Messages_Milestones_MilestoneId]
                        FOREIGN KEY ([MilestoneId])
                        REFERENCES [marketplace].[Milestones] ([Id]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = N'FK_Messages_Milestones_MilestoneId'
                      AND parent_object_id = OBJECT_ID(N'chat.Messages'))
                BEGIN
                    ALTER TABLE [chat].[Messages]
                    DROP CONSTRAINT [FK_Messages_Milestones_MilestoneId];
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Messages_MilestoneId'
                      AND object_id = OBJECT_ID(N'chat.Messages'))
                BEGIN
                    DROP INDEX [IX_Messages_MilestoneId] ON [chat].[Messages];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('chat.Messages', 'MilestoneId') IS NOT NULL
                BEGIN
                    ALTER TABLE [chat].[Messages] DROP COLUMN [MilestoneId];
                END
                """);
        }
    }
}
