using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixLedgerEntryProjectRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntries_Projects_ProjectId1",
                schema: "finance",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_ProjectId1",
                schema: "finance",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "ProjectId1",
                schema: "finance",
                table: "LedgerEntries");

            migrationBuilder.AddColumn<bool>(
                name: "IsReadOnly",
                schema: "chat",
                table: "ChatRooms",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReadOnly",
                schema: "chat",
                table: "ChatRooms");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId1",
                schema: "finance",
                table: "LedgerEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_ProjectId1",
                schema: "finance",
                table: "LedgerEntries",
                column: "ProjectId1");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntries_Projects_ProjectId1",
                schema: "finance",
                table: "LedgerEntries",
                column: "ProjectId1",
                principalSchema: "marketplace",
                principalTable: "Projects",
                principalColumn: "Id");
        }
    }
}
