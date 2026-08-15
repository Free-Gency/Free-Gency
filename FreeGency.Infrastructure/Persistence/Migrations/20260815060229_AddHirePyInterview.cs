using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHirePyInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHirePyBot",
                schema: "identity",
                table: "DeveloperProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "HirePyInterviews",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    HirePySessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FreelancerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FreelancerDeveloperProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Started"),
                    TurnCount = table.Column<int>(type: "int", nullable: false),
                    LastUserMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    ConcludedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HirePyInterviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeveloperProfiles_IsHirePyBot",
                schema: "identity",
                table: "DeveloperProfiles",
                column: "IsHirePyBot",
                unique: true,
                filter: "[IsHirePyBot] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_HirePyInterviews_ChatRoomId",
                schema: "ai",
                table: "HirePyInterviews",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_HirePyInterviews_ProjectProposalId",
                schema: "ai",
                table: "HirePyInterviews",
                column: "ProjectProposalId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HirePyInterviews_Status",
                schema: "ai",
                table: "HirePyInterviews",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HirePyInterviews",
                schema: "ai");

            migrationBuilder.DropIndex(
                name: "IX_DeveloperProfiles_IsHirePyBot",
                schema: "identity",
                table: "DeveloperProfiles");

            migrationBuilder.DropColumn(
                name: "IsHirePyBot",
                schema: "identity",
                table: "DeveloperProfiles");
        }
    }
}
