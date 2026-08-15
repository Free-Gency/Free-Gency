using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class seedPlansTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "plans",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "MonthlyPrice", "Name", "UpdatedAt", "UpdatedBy", "YearlyPrice" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "Essential features to get started.", true, 0m, "Free", null, null, 0m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "More projects, proposals and AI features.", true, 10m, "Premium", null, null, 100m },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, "Maximum limits and full access to advanced features.", true, 25m, "Pro", null, null, 250m }
                });

            migrationBuilder.InsertData(
                table: "planFeatures",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Feature", "IsEnabled", "Limit", "PlanId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 0, true, 2, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 1, true, 5, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 2, true, 3, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 3, true, 1, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("11111111-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 4, true, 100, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("11111111-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 5, true, 5, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("11111111-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 6, true, 3, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("11111111-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 7, true, 3, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("11111111-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 8, true, 10, new Guid("11111111-1111-1111-1111-111111111111"), null, null }
                });

            migrationBuilder.InsertData(
                table: "planFeatures",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Feature", "Limit", "PlanId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("11111111-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 9, null, new Guid("11111111-1111-1111-1111-111111111111"), null, null });

            migrationBuilder.InsertData(
                table: "planFeatures",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Feature", "IsEnabled", "Limit", "PlanId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 0, true, 10, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 1, true, 30, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 2, true, 10, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 3, true, 5, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 4, true, 2048, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 5, true, 30, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 6, true, 20, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 7, true, 20, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 8, true, 100, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("22222222-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 9, true, 10, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 0, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 1, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 2, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 3, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 4, true, 10240, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 5, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 6, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 7, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 8, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null },
                    { new Guid("33333333-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, null, 9, true, null, new Guid("33333333-3333-3333-3333-333333333333"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "planFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));
        }
    }
}
