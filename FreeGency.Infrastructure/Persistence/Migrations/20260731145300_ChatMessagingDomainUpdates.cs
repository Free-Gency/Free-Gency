using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChatMessagingDomainUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Text",
                schema: "chat",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                schema: "chat",
                table: "Messages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Text");

            migrationBuilder.AddColumn<Guid>(
                name: "PlanVersionId",
                schema: "chat",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                schema: "chat",
                table: "ChatRooms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceProposalRoomId",
                schema: "chat",
                table: "ChatRooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "chat",
                table: "ChatRooms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<bool>(
                name: "CanSend",
                schema: "chat",
                table: "ChatRoomMembers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleLabel",
                schema: "chat",
                table: "ChatRoomMembers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PlanVersionId",
                schema: "chat",
                table: "Messages",
                column: "PlanVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRooms_SourceProposalRoomId",
                schema: "chat",
                table: "ChatRooms",
                column: "SourceProposalRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_ChatRooms_SourceProposalRoomId",
                schema: "chat",
                table: "ChatRooms",
                column: "SourceProposalRoomId",
                principalSchema: "chat",
                principalTable: "ChatRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MilestonePlanVersions_PlanVersionId",
                schema: "chat",
                table: "Messages",
                column: "PlanVersionId",
                principalSchema: "marketplace",
                principalTable: "MilestonePlanVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_ChatRooms_SourceProposalRoomId",
                schema: "chat",
                table: "ChatRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MilestonePlanVersions_PlanVersionId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_PlanVersionId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_ChatRooms_SourceProposalRoomId",
                schema: "chat",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "MessageType",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "PlanVersionId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                schema: "chat",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "SourceProposalRoomId",
                schema: "chat",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "chat",
                table: "ChatRooms");

            migrationBuilder.DropColumn(
                name: "CanSend",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "RoleLabel",
                schema: "chat",
                table: "ChatRoomMembers");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                schema: "chat",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
