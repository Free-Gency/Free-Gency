using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class seedingTeamPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "TeamPlans",
                table: "TeamPlans",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "MonthlyPrice", "Name", "UpdatedAt", "UpdatedBy", "YearlyPrice" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", null, null, 0m, "Free", null, null, 0m },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", null, null, 20m, "Pro", null, null, 200m },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", null, null, 50m, "Premium", null, null, 500m }
                });

            migrationBuilder.InsertData(
                schema: "TeamPlans",
                table: "TeamPlanFeatures",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Feature", "IsEnabled", "Limit", "TeamPlanId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", null, null, "CreateProposal", true, 5, new Guid("11111111-1111-1111-1111-111111111111"), null, null },
                    { new Guid("22222222-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", null, null, "CreateProposal", true, 30, new Guid("22222222-2222-2222-2222-222222222222"), null, null },
                    { new Guid("33333333-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", null, null, "CreateProposal", true, 70, new Guid("33333333-3333-3333-3333-333333333333"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "TeamPlans",
                table: "TeamPlanFeatures",
                keyColumn: "Id",
                keyValue: new Guid("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                schema: "TeamPlans",
                table: "TeamPlanFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                schema: "TeamPlans",
                table: "TeamPlanFeatures",
                keyColumn: "Id",
                keyValue: new Guid("33333333-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                schema: "TeamPlans",
                table: "TeamPlans",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                schema: "TeamPlans",
                table: "TeamPlans",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                schema: "TeamPlans",
                table: "TeamPlans",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));
        }
    }
}
