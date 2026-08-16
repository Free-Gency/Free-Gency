using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyAITokenPool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TokensUsed",
                table: "usageRecords",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "AllowedTokens",
                table: "plans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 5000);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000008"),
                column: "Limit",
                value: 2000);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000009"),
                column: "Limit",
                value: 2500);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000010"),
                column: "Limit",
                value: 3500);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 5000);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000008"),
                column: "Limit",
                value: 2000);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"),
                column: "Limit",
                value: 2500);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000010"),
                column: "Limit",
                value: 3500);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000011"),
                column: "Limit",
                value: 10000);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 5000);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000008"),
                column: "Limit",
                value: 2000);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000009"),
                column: "Limit",
                value: 2500);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000010"),
                column: "Limit",
                value: 3500);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000011"),
                column: "Limit",
                value: 10000);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "AllowedTokens",
                value: 50000L);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "AllowedTokens",
                value: 300000L);

            migrationBuilder.UpdateData(
                table: "plans",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "AllowedTokens",
                value: 1000000000L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokensUsed",
                table: "usageRecords");

            migrationBuilder.DropColumn(
                name: "AllowedTokens",
                table: "plans");

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 2);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000008"),
                column: "Limit",
                value: 3);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000009"),
                column: "Limit",
                value: 20);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000010"),
                column: "Limit",
                value: 5);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 15);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000008"),
                column: "Limit",
                value: 20);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"),
                column: "Limit",
                value: 200);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000010"),
                column: "Limit",
                value: 30);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000011"),
                column: "Limit",
                value: 5);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 50);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000008"),
                column: "Limit",
                value: 100);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000009"),
                column: "Limit",
                value: 1000);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000010"),
                column: "Limit",
                value: 150);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000011"),
                column: "Limit",
                value: 30);
        }
    }
}
