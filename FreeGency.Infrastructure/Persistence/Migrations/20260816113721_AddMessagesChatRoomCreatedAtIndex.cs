using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagesChatRoomCreatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatRoomId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatRoomId_CreatedAt",
                schema: "chat",
                table: "Messages",
                columns: new[] { "ChatRoomId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatRoomId_CreatedAt",
                schema: "chat",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatRoomId",
                schema: "chat",
                table: "Messages",
                column: "ChatRoomId");
        }
    }
}
