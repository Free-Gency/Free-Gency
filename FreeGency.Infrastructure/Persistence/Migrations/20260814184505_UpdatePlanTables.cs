using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlanTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "usageRecords",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "usageRecords",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "usageRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "usageRecords",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "usageRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "usageRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "usageRecords",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "subscriptions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "subscriptions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "subscriptions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "subscriptions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "plans",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "plans",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "plans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "plans",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "plans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "plans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "plans",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "planFeatures",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "planFeatures",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "planFeatures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "planFeatures",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "planFeatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "planFeatures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "planFeatures",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "usageRecords");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "usageRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "usageRecords");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "usageRecords");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "usageRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "usageRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "usageRecords");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "plans");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "planFeatures");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "planFeatures");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "planFeatures");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "planFeatures");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "planFeatures");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "planFeatures");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "planFeatures");
        }
    }
}
