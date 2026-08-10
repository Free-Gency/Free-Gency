using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModeratedTextColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModeratedText",
                schema: "teams",
                table: "TeamFeedbacks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeratedText",
                schema: "chat",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeratedText",
                schema: "identity",
                table: "DeveloperFeedbacks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModeratedText",
                schema: "teams",
                table: "TeamFeedbacks");

            migrationBuilder.DropColumn(
                name: "ModeratedText",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ModeratedText",
                schema: "identity",
                table: "DeveloperFeedbacks");
        }
    }
}
