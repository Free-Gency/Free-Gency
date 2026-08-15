using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class seedPlansTables2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "planFeatures",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000010"),
                column: "IsEnabled",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "planFeatures",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000010"),
                column: "IsEnabled",
                value: true);
        }
    }
}
