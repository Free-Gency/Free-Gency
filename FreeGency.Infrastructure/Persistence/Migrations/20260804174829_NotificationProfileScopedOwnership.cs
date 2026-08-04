using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NotificationProfileScopedOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.EnsureSchema(name: "core");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notifications",
                newSchema: "core");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientProfileId",
                schema: "core",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperProfileId",
                schema: "core",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            // Map by active profile mode first
            migrationBuilder.Sql("""
                UPDATE n
                SET ClientProfileId = cp.Id
                FROM core.Notifications n
                INNER JOIN [identity].Users u ON u.Id = n.UserId
                INNER JOIN [identity].ClientProfiles cp ON cp.UserId = n.UserId
                WHERE u.ActiveProfileMode = N'Client';

                UPDATE n
                SET DeveloperProfileId = dp.Id
                FROM core.Notifications n
                INNER JOIN [identity].Users u ON u.Id = n.UserId
                INNER JOIN [identity].DeveloperProfiles dp ON dp.UserId = n.UserId
                WHERE u.ActiveProfileMode = N'Developer';
                """);

            // Fallback: only one profile exists
            migrationBuilder.Sql("""
                UPDATE n
                SET ClientProfileId = cp.Id
                FROM core.Notifications n
                INNER JOIN [identity].ClientProfiles cp ON cp.UserId = n.UserId
                WHERE n.ClientProfileId IS NULL
                  AND n.DeveloperProfileId IS NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM [identity].DeveloperProfiles dp WHERE dp.UserId = n.UserId);

                UPDATE n
                SET DeveloperProfileId = dp.Id
                FROM core.Notifications n
                INNER JOIN [identity].DeveloperProfiles dp ON dp.UserId = n.UserId
                WHERE n.ClientProfileId IS NULL
                  AND n.DeveloperProfileId IS NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM [identity].ClientProfiles cp WHERE cp.UserId = n.UserId);
                """);

            // Both profiles, mode unknown: client-facing types → ClientProfile, else DeveloperProfile
            migrationBuilder.Sql("""
                UPDATE n
                SET ClientProfileId = cp.Id
                FROM core.Notifications n
                INNER JOIN [identity].ClientProfiles cp ON cp.UserId = n.UserId
                WHERE n.ClientProfileId IS NULL
                  AND n.DeveloperProfileId IS NULL
                  AND n.[Type] IN (1, 10, 16, 17); -- NewProposal, EscrowLocked, ReviewReminder, ProjectPublished

                UPDATE n
                SET DeveloperProfileId = dp.Id
                FROM core.Notifications n
                INNER JOIN [identity].DeveloperProfiles dp ON dp.UserId = n.UserId
                WHERE n.ClientProfileId IS NULL
                  AND n.DeveloperProfileId IS NULL;
                """);

            migrationBuilder.Sql("""
                DELETE FROM core.Notifications
                WHERE ClientProfileId IS NULL AND DeveloperProfileId IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "core",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ClientProfileId",
                schema: "core",
                table: "Notifications",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ClientProfileId_IsRead",
                schema: "core",
                table: "Notifications",
                columns: new[] { "ClientProfileId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DeveloperProfileId",
                schema: "core",
                table: "Notifications",
                column: "DeveloperProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DeveloperProfileId_IsRead",
                schema: "core",
                table: "Notifications",
                columns: new[] { "DeveloperProfileId", "IsRead" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notifications_ProfileScope",
                schema: "core",
                table: "Notifications",
                sql: "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ClientProfiles_ClientProfileId",
                schema: "core",
                table: "Notifications",
                column: "ClientProfileId",
                principalSchema: "identity",
                principalTable: "ClientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_DeveloperProfiles_DeveloperProfileId",
                schema: "core",
                table: "Notifications",
                column: "DeveloperProfileId",
                principalSchema: "identity",
                principalTable: "DeveloperProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ClientProfiles_ClientProfileId",
                schema: "core",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_DeveloperProfiles_DeveloperProfileId",
                schema: "core",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ClientProfileId",
                schema: "core",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ClientProfileId_IsRead",
                schema: "core",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_DeveloperProfileId",
                schema: "core",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_DeveloperProfileId_IsRead",
                schema: "core",
                table: "Notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Notifications_ProfileScope",
                schema: "core",
                table: "Notifications");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "core",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE n
                SET UserId = cp.UserId
                FROM core.Notifications n
                INNER JOIN [identity].ClientProfiles cp ON cp.Id = n.ClientProfileId
                WHERE n.ClientProfileId IS NOT NULL;

                UPDATE n
                SET UserId = dp.UserId
                FROM core.Notifications n
                INNER JOIN [identity].DeveloperProfiles dp ON dp.Id = n.DeveloperProfileId
                WHERE n.DeveloperProfileId IS NOT NULL
                  AND n.UserId IS NULL;
                """);

            migrationBuilder.Sql("""
                DELETE FROM core.Notifications WHERE UserId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "core",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "ClientProfileId",
                schema: "core",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeveloperProfileId",
                schema: "core",
                table: "Notifications");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "core",
                newName: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
