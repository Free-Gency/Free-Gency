using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanTablesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"),
                column: "Limit",
                value: 3);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"),
                column: "Limit",
                value: 3);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"),
                column: "Limit",
                value: 2);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"),
                column: "Limit",
                value: 5);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000006"),
                column: "Limit",
                value: 10);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 2);

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
                columns: new[] { "IsEnabled", "Limit" },
                values: new object[] { true, 5 });

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"),
                column: "Limit",
                value: 20);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"),
                column: "Limit",
                value: 10);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"),
                column: "Limit",
                value: 10);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"),
                column: "Limit",
                value: 30);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"),
                column: "Limit",
                value: 50);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 15);

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
                keyValue: new Guid("33333333-0000-0000-0000-000000000001"),
                column: "Limit",
                value: 150);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000002"),
                column: "Limit",
                value: 30);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000003"),
                column: "Limit",
                value: 25);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000004"),
                column: "Limit",
                value: 30);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000005"),
                column: "Limit",
                value: 100);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000006"),
                column: "Limit",
                value: 150);

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

            migrationBuilder.InsertData(
                table: "planFeatures",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Feature", "IsEnabled", "Limit", "PlanId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 10, false, null, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 10, true, 5, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 10, true, 30, new Guid("33333333-3333-3333-3333-333333333333"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000011"));

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"),
                column: "Limit",
                value: 2);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"),
                column: "Limit",
                value: 5);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"),
                column: "Limit",
                value: 1);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"),
                column: "Limit",
                value: 100);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000006"),
                column: "Limit",
                value: 5);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 3);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000009"),
                column: "Limit",
                value: 10);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000010"),
                columns: new[] { "IsEnabled", "Limit" },
                values: new object[] { false, null });

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"),
                column: "Limit",
                value: 10);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"),
                column: "Limit",
                value: 30);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"),
                column: "Limit",
                value: 5);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"),
                column: "Limit",
                value: 2048);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"),
                column: "Limit",
                value: 30);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                column: "Limit",
                value: 20);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"),
                column: "Limit",
                value: 100);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000010"),
                column: "Limit",
                value: 10);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000001"),
                column: "Limit",
                value: null);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000002"),
                column: "Limit",
                value: null);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000003"),
                column: "Limit",
                value: null);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000004"),
                column: "Limit",
                value: null);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000005"),
                column: "Limit",
                value: 10240);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000006"),
                column: "Limit",
                value: null);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000007"),
                column: "Limit",
                value: null);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000008"),
                column: "Limit",
                value: null);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000009"),
                column: "Limit",
                value: null);

            migrationBuilder.UpdateData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000010"),
                column: "Limit",
                value: null);
        }
    }
}
