using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModerationMutedUntil",
                schema: "identity",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationNote",
                schema: "teams",
                table: "TeamFeedbacks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                schema: "teams",
                table: "TeamFeedbacks",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Visible");

            migrationBuilder.AddColumn<string>(
                name: "ModerationNote",
                schema: "chat",
                table: "Messages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                schema: "chat",
                table: "Messages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Visible");

            migrationBuilder.AddColumn<string>(
                name: "ModerationNote",
                schema: "identity",
                table: "DeveloperFeedbacks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                schema: "identity",
                table: "DeveloperFeedbacks",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Visible");

            migrationBuilder.CreateTable(
                name: "ModerationCases",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categories = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Confidence = table.Column<float>(type: "real", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "AutoResolved"),
                    UserMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdminSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdminNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationCases_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserModerationStrikes",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModerationCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrimaryCategory = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModerationStrikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserModerationStrikes_ModerationCases_ModerationCaseId",
                        column: x => x.ModerationCaseId,
                        principalSchema: "core",
                        principalTable: "ModerationCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserModerationStrikes_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCases_SourceType_SourceId",
                schema: "core",
                table: "ModerationCases",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCases_Status_CreatedAt",
                schema: "core",
                table: "ModerationCases",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCases_UserId",
                schema: "core",
                table: "ModerationCases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModerationStrikes_ModerationCaseId",
                schema: "core",
                table: "UserModerationStrikes",
                column: "ModerationCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModerationStrikes_UserId_CreatedAt",
                schema: "core",
                table: "UserModerationStrikes",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserModerationStrikes",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ModerationCases",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "ModerationMutedUntil",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModerationNote",
                schema: "teams",
                table: "TeamFeedbacks");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                schema: "teams",
                table: "TeamFeedbacks");

            migrationBuilder.DropColumn(
                name: "ModerationNote",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ModerationNote",
                schema: "identity",
                table: "DeveloperFeedbacks");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                schema: "identity",
                table: "DeveloperFeedbacks");
        }
    }
}
