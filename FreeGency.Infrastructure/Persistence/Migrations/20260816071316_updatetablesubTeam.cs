using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatetablesubTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TeamSubscriptions_UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamSubscriptions_Users_UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamSubscriptions_Users_UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_TeamSubscriptions_UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions");
        }
    }
}
