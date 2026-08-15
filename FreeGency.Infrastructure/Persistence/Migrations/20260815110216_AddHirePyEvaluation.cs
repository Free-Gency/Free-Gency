using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHirePyEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DecisionReason",
                schema: "ai",
                table: "HirePySessions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MilestoneSummary",
                schema: "ai",
                table: "HirePySessions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecommendationCompletedAt",
                schema: "ai",
                table: "HirePySessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SelectedBudget",
                schema: "ai",
                table: "HirePySessions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedCandidateName",
                schema: "ai",
                table: "HirePySessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedFreelancerUserId",
                schema: "ai",
                table: "HirePySessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedProposalId",
                schema: "ai",
                table: "HirePySessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedTimeline",
                schema: "ai",
                table: "HirePySessions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HirePyEvaluations",
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
                    FreelancerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RankingPosition = table.Column<int>(type: "int", nullable: false),
                    RankingScore = table.Column<int>(type: "int", nullable: false),
                    TechnicalScore = table.Column<int>(type: "int", nullable: false),
                    RequirementsScore = table.Column<int>(type: "int", nullable: false),
                    ArchitectureScore = table.Column<int>(type: "int", nullable: false),
                    ImplementationScore = table.Column<int>(type: "int", nullable: false),
                    MilestoneScore = table.Column<int>(type: "int", nullable: false),
                    TimelineScore = table.Column<int>(type: "int", nullable: false),
                    BudgetScore = table.Column<int>(type: "int", nullable: false),
                    CommunicationScore = table.Column<int>(type: "int", nullable: false),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    StrengthsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcernsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RisksJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    MilestoneSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProposedBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProposedTimeline = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HirePyEvaluations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HirePyEvaluations_HirePySessionId",
                schema: "ai",
                table: "HirePyEvaluations",
                column: "HirePySessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HirePyEvaluations",
                schema: "ai");

            migrationBuilder.DropColumn(
                name: "DecisionReason",
                schema: "ai",
                table: "HirePySessions");

            migrationBuilder.DropColumn(
                name: "MilestoneSummary",
                schema: "ai",
                table: "HirePySessions");

            migrationBuilder.DropColumn(
                name: "RecommendationCompletedAt",
                schema: "ai",
                table: "HirePySessions");

            migrationBuilder.DropColumn(
                name: "SelectedBudget",
                schema: "ai",
                table: "HirePySessions");

            migrationBuilder.DropColumn(
                name: "SelectedCandidateName",
                schema: "ai",
                table: "HirePySessions");

            migrationBuilder.DropColumn(
                name: "SelectedFreelancerUserId",
                schema: "ai",
                table: "HirePySessions");

            migrationBuilder.DropColumn(
                name: "SelectedProposalId",
                schema: "ai",
                table: "HirePySessions");

            migrationBuilder.DropColumn(
                name: "SelectedTimeline",
                schema: "ai",
                table: "HirePySessions");
        }
    }
}
