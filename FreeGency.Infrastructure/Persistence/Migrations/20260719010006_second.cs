using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedUserId",
                schema: "marketplace",
                table: "Projects",
                type: "uniqueidentifier",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "AssignedUserId",
                schema: "marketplace",
                table: "Projects",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
