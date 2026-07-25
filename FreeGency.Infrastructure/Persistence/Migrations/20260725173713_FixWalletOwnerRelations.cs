using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixWalletOwnerRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_OwnerType_OwnerId_Currency",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerTeamId",
                schema: "finance",
                table: "Wallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                schema: "finance",
                table: "Wallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE finance.Wallets
                SET OwnerUserId = OwnerId
                WHERE OwnerType = N'User';

                UPDATE finance.Wallets
                SET OwnerTeamId = OwnerId
                WHERE OwnerType = N'Team';
                """);

            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_OwnerTeamId",
                schema: "finance",
                table: "Wallets",
                column: "OwnerTeamId",
                unique: true,
                filter: "[OwnerTeamId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_OwnerUserId",
                schema: "finance",
                table: "Wallets",
                column: "OwnerUserId",
                unique: true,
                filter: "[OwnerUserId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Wallets_OwnerScope",
                schema: "finance",
                table: "Wallets",
                sql: "(OwnerType = N'User' AND OwnerUserId IS NOT NULL AND OwnerTeamId IS NULL) OR (OwnerType = N'Team' AND OwnerTeamId IS NOT NULL AND OwnerUserId IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Teams_OwnerTeamId",
                schema: "finance",
                table: "Wallets",
                column: "OwnerTeamId",
                principalSchema: "teams",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Users_OwnerUserId",
                schema: "finance",
                table: "Wallets",
                column: "OwnerUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Teams_OwnerTeamId",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_OwnerUserId",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_OwnerTeamId",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_OwnerUserId",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Wallets_OwnerScope",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                schema: "finance",
                table: "Wallets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE finance.Wallets
                SET OwnerId = COALESCE(OwnerUserId, OwnerTeamId, '00000000-0000-0000-0000-000000000000');
                """);

            migrationBuilder.DropColumn(
                name: "OwnerTeamId",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "finance",
                table: "Wallets");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_OwnerType_OwnerId_Currency",
                schema: "finance",
                table: "Wallets",
                columns: new[] { "OwnerType", "OwnerId", "Currency" },
                unique: true);
        }
    }
}
