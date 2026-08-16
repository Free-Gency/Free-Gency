using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamSubscriptions_Users_UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamSubscriptions_Users_UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamSubscriptions_Users_UserId",
                schema: "TeamPlans",
                table: "TeamSubscriptions");

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
    }
}
