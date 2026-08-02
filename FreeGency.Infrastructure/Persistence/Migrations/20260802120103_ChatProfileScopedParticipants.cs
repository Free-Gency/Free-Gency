using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChatProfileScopedParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomMembers_Users_UserId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderUserId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatRoomMembers",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomMembers_ChatRoomId_UserId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomMembers_UserId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderUserId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientProfileId",
                schema: "chat",
                table: "ChatRoomMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperProfileId",
                schema: "chat",
                table: "ChatRoomMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SenderClientProfileId",
                schema: "chat",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SenderDeveloperProfileId",
                schema: "chat",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: true);

            // Members: Client role → ClientProfile; otherwise DeveloperProfile
            migrationBuilder.Sql("""
                UPDATE m
                SET ClientProfileId = cp.Id
                FROM chat.ChatRoomMembers m
                INNER JOIN identity.ClientProfiles cp ON cp.UserId = m.UserId
                WHERE m.RoleLabel LIKE N'%Client%';

                UPDATE m
                SET DeveloperProfileId = dp.Id
                FROM chat.ChatRoomMembers m
                INNER JOIN identity.DeveloperProfiles dp ON dp.UserId = m.UserId
                WHERE m.ClientProfileId IS NULL
                  AND (m.RoleLabel IS NULL OR m.RoleLabel NOT LIKE N'%Client%');
                """);

            // Messages: prefer room membership role; fallback client project ownership, else developer
            migrationBuilder.Sql("""
                UPDATE msg
                SET SenderClientProfileId = cp.Id
                FROM chat.Messages msg
                INNER JOIN chat.ChatRoomMembers m
                    ON m.ChatRoomId = msg.ChatRoomId AND m.UserId = msg.SenderUserId
                INNER JOIN identity.ClientProfiles cp ON cp.Id = m.ClientProfileId
                WHERE msg.SenderUserId IS NOT NULL
                  AND m.ClientProfileId IS NOT NULL;

                UPDATE msg
                SET SenderDeveloperProfileId = dp.Id
                FROM chat.Messages msg
                INNER JOIN chat.ChatRoomMembers m
                    ON m.ChatRoomId = msg.ChatRoomId AND m.UserId = msg.SenderUserId
                INNER JOIN identity.DeveloperProfiles dp ON dp.Id = m.DeveloperProfileId
                WHERE msg.SenderUserId IS NOT NULL
                  AND msg.SenderClientProfileId IS NULL
                  AND m.DeveloperProfileId IS NOT NULL;

                UPDATE msg
                SET SenderClientProfileId = cp.Id
                FROM chat.Messages msg
                INNER JOIN chat.ChatRooms r ON r.Id = msg.ChatRoomId
                INNER JOIN marketplace.Projects p ON p.Id = COALESCE(r.ProjectId, (SELECT TOP 1 pp.ProjectId FROM marketplace.ProjectProposals pp WHERE pp.Id = r.ProposalId))
                INNER JOIN identity.ClientProfiles cp ON cp.UserId = p.ClientId
                WHERE msg.SenderUserId IS NOT NULL
                  AND msg.SenderClientProfileId IS NULL
                  AND msg.SenderDeveloperProfileId IS NULL
                  AND msg.SenderUserId = p.ClientId;

                UPDATE msg
                SET SenderDeveloperProfileId = dp.Id
                FROM chat.Messages msg
                INNER JOIN identity.DeveloperProfiles dp ON dp.UserId = msg.SenderUserId
                WHERE msg.SenderUserId IS NOT NULL
                  AND msg.SenderClientProfileId IS NULL
                  AND msg.SenderDeveloperProfileId IS NULL;
                """);

            // Drop members that could not be mapped to a profile
            migrationBuilder.Sql("""
                DELETE FROM chat.ChatRoomMembers
                WHERE ClientProfileId IS NULL AND DeveloperProfileId IS NULL;
                """);

            // Clear unmapped senders rather than violate XOR / FK
            migrationBuilder.Sql("""
                UPDATE chat.Messages
                SET SenderUserId = NULL
                WHERE SenderUserId IS NOT NULL
                  AND SenderClientProfileId IS NULL
                  AND SenderDeveloperProfileId IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "SenderUserId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatRoomMembers",
                schema: "chat",
                table: "ChatRoomMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderClientProfileId",
                schema: "chat",
                table: "Messages",
                column: "SenderClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderDeveloperProfileId",
                schema: "chat",
                table: "Messages",
                column: "SenderDeveloperProfileId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Messages_SenderProfileScope",
                schema: "chat",
                table: "Messages",
                sql: "(SenderClientProfileId IS NULL AND SenderDeveloperProfileId IS NULL) OR (SenderClientProfileId IS NOT NULL AND SenderDeveloperProfileId IS NULL) OR (SenderClientProfileId IS NULL AND SenderDeveloperProfileId IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_ChatRoomId_ClientProfileId",
                schema: "chat",
                table: "ChatRoomMembers",
                columns: new[] { "ChatRoomId", "ClientProfileId" },
                unique: true,
                filter: "[ClientProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_ChatRoomId_DeveloperProfileId",
                schema: "chat",
                table: "ChatRoomMembers",
                columns: new[] { "ChatRoomId", "DeveloperProfileId" },
                unique: true,
                filter: "[DeveloperProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_ClientProfileId",
                schema: "chat",
                table: "ChatRoomMembers",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_DeveloperProfileId",
                schema: "chat",
                table: "ChatRoomMembers",
                column: "DeveloperProfileId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChatRoomMembers_ProfileScope",
                schema: "chat",
                table: "ChatRoomMembers",
                sql: "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomMembers_ClientProfiles_ClientProfileId",
                schema: "chat",
                table: "ChatRoomMembers",
                column: "ClientProfileId",
                principalSchema: "identity",
                principalTable: "ClientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomMembers_DeveloperProfiles_DeveloperProfileId",
                schema: "chat",
                table: "ChatRoomMembers",
                column: "DeveloperProfileId",
                principalSchema: "identity",
                principalTable: "DeveloperProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ClientProfiles_SenderClientProfileId",
                schema: "chat",
                table: "Messages",
                column: "SenderClientProfileId",
                principalSchema: "identity",
                principalTable: "ClientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_DeveloperProfiles_SenderDeveloperProfileId",
                schema: "chat",
                table: "Messages",
                column: "SenderDeveloperProfileId",
                principalSchema: "identity",
                principalTable: "DeveloperProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomMembers_ClientProfiles_ClientProfileId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomMembers_DeveloperProfiles_DeveloperProfileId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ClientProfiles_SenderClientProfileId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_DeveloperProfiles_SenderDeveloperProfileId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderClientProfileId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderDeveloperProfileId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Messages_SenderProfileScope",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatRoomMembers",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomMembers_ChatRoomId_ClientProfileId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomMembers_ChatRoomId_DeveloperProfileId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomMembers_ClientProfileId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomMembers_DeveloperProfileId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChatRoomMembers_ProfileScope",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "chat",
                table: "ChatRoomMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SenderUserId",
                schema: "chat",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE m
                SET UserId = cp.UserId
                FROM chat.ChatRoomMembers m
                INNER JOIN identity.ClientProfiles cp ON cp.Id = m.ClientProfileId
                WHERE m.ClientProfileId IS NOT NULL;

                UPDATE m
                SET UserId = dp.UserId
                FROM chat.ChatRoomMembers m
                INNER JOIN identity.DeveloperProfiles dp ON dp.Id = m.DeveloperProfileId
                WHERE m.DeveloperProfileId IS NOT NULL;

                UPDATE msg
                SET SenderUserId = cp.UserId
                FROM chat.Messages msg
                INNER JOIN identity.ClientProfiles cp ON cp.Id = msg.SenderClientProfileId
                WHERE msg.SenderClientProfileId IS NOT NULL;

                UPDATE msg
                SET SenderUserId = dp.UserId
                FROM chat.Messages msg
                INNER JOIN identity.DeveloperProfiles dp ON dp.Id = msg.SenderDeveloperProfileId
                WHERE msg.SenderDeveloperProfileId IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                DELETE FROM chat.ChatRoomMembers WHERE UserId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "chat",
                table: "ChatRoomMembers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "SenderClientProfileId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SenderDeveloperProfileId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ClientProfileId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "DeveloperProfileId",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatRoomMembers",
                schema: "chat",
                table: "ChatRoomMembers",
                columns: new[] { "Id", "ChatRoomId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_ChatRoomId_UserId",
                schema: "chat",
                table: "ChatRoomMembers",
                columns: new[] { "ChatRoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMembers_UserId",
                schema: "chat",
                table: "ChatRoomMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderUserId",
                schema: "chat",
                table: "Messages",
                column: "SenderUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomMembers_Users_UserId",
                schema: "chat",
                table: "ChatRoomMembers",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderUserId",
                schema: "chat",
                table: "Messages",
                column: "SenderUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
