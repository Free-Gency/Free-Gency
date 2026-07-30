using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProposalMilestoneNegotiationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy Accept ≈ discussion start under the new flow (Hire = Accept Plan).
            migrationBuilder.Sql(
                """
                UPDATE [marketplace].[ProjectProposals]
                SET [Status] = N'InDiscussion'
                WHERE [Status] = N'Accepted';
                """);

            migrationBuilder.AddColumn<string>(
                name: "Approach",
                schema: "marketplace",
                table: "ProjectProposals",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PortfolioUrl",
                schema: "marketplace",
                table: "ProjectProposals",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProposedTimeline",
                schema: "marketplace",
                table: "ProjectProposals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                schema: "marketplace",
                table: "ProjectProposals",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                schema: "marketplace",
                table: "Milestones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFunded",
                schema: "marketplace",
                table: "Milestones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MilestonePlanVersions",
                schema: "marketplace",
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
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Proposed"),
                    ChangeComment = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProposedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestonePlanVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilestonePlanVersions_ProjectProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalSchema: "marketplace",
                        principalTable: "ProjectProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilestonePlanVersions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "marketplace",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MilestonePlanItems",
                schema: "marketplace",
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
                    PlanVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefinitionOfDone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ChangeTag = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestonePlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilestonePlanItems_MilestonePlanVersions_PlanVersionId",
                        column: x => x.PlanVersionId,
                        principalSchema: "marketplace",
                        principalTable: "MilestonePlanVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MilestonePlanItems_PlanVersionId",
                schema: "marketplace",
                table: "MilestonePlanItems",
                column: "PlanVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestonePlanVersions_ProjectId_Version",
                schema: "marketplace",
                table: "MilestonePlanVersions",
                columns: new[] { "ProjectId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MilestonePlanVersions_ProposalId",
                schema: "marketplace",
                table: "MilestonePlanVersions",
                column: "ProposalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MilestonePlanItems",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "MilestonePlanVersions",
                schema: "marketplace");

            migrationBuilder.DropColumn(
                name: "Approach",
                schema: "marketplace",
                table: "ProjectProposals");

            migrationBuilder.DropColumn(
                name: "PortfolioUrl",
                schema: "marketplace",
                table: "ProjectProposals");

            migrationBuilder.DropColumn(
                name: "ProposedTimeline",
                schema: "marketplace",
                table: "ProjectProposals");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                schema: "marketplace",
                table: "ProjectProposals");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "marketplace",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "IsFunded",
                schema: "marketplace",
                table: "Milestones");
        }
    }
}
