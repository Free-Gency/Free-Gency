using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioCaseStudyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Challenge",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DurationLabel",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrototypeUrl",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solution",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamLeads",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestimonialAuthorAvatarUrl",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestimonialAuthorName",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestimonialAuthorTitle",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestimonialQuote",
                schema: "portfolio",
                table: "PortfolioProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PortfolioMetrics",
                schema: "portfolio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortfolioProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioMetrics_PortfolioProjects_PortfolioProjectId",
                        column: x => x.PortfolioProjectId,
                        principalSchema: "portfolio",
                        principalTable: "PortfolioProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioRoadmapSteps",
                schema: "portfolio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortfolioProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDone = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioRoadmapSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioRoadmapSteps_PortfolioProjects_PortfolioProjectId",
                        column: x => x.PortfolioProjectId,
                        principalSchema: "portfolio",
                        principalTable: "PortfolioProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioMetrics_PortfolioProjectId_SortOrder",
                schema: "portfolio",
                table: "PortfolioMetrics",
                columns: new[] { "PortfolioProjectId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioRoadmapSteps_PortfolioProjectId_SortOrder",
                schema: "portfolio",
                table: "PortfolioRoadmapSteps",
                columns: new[] { "PortfolioProjectId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioMetrics",
                schema: "portfolio");

            migrationBuilder.DropTable(
                name: "PortfolioRoadmapSteps",
                schema: "portfolio");

            migrationBuilder.DropColumn(
                name: "Challenge",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "DurationLabel",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "Industry",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "PrototypeUrl",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "Solution",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "TeamLeads",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "TestimonialAuthorAvatarUrl",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "TestimonialAuthorName",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "TestimonialAuthorTitle",
                schema: "portfolio",
                table: "PortfolioProjects");

            migrationBuilder.DropColumn(
                name: "TestimonialQuote",
                schema: "portfolio",
                table: "PortfolioProjects");
        }
    }
}
