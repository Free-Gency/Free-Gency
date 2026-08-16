using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeamPlanTablesAndSeedind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "TeamPlans",
                table: "TeamPlans",
                type: "nvarchar(350)",
                maxLength: 350,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "TeamPlans",
                table: "TeamPlanFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Limit",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TeamPlans",
                table: "TeamPlans",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Description",
                value: "Perfect for small teams just getting started");

            migrationBuilder.UpdateData(
                schema: "TeamPlans",
                table: "TeamPlans",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Description", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { "Great for growing teams", 5m, "Premium", 50m });

            migrationBuilder.UpdateData(
                schema: "TeamPlans",
                table: "TeamPlans",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "Description", "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { "Perfect for large teams", 10m, "Pro", 100m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "TeamPlans",
                table: "TeamPlans");

            migrationBuilder.UpdateData(
                schema: "TeamPlans",
                table: "TeamPlanFeatures",
                keyColumn: "Id",
                keyValue: new Guid("22222222-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Limit",
                value: 30);

            migrationBuilder.UpdateData(
                schema: "TeamPlans",
                table: "TeamPlans",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { 20m, "Pro", 200m });

            migrationBuilder.UpdateData(
                schema: "TeamPlans",
                table: "TeamPlans",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "MonthlyPrice", "Name", "YearlyPrice" },
                values: new object[] { 50m, "Premium", 500m });
        }
    }
}
