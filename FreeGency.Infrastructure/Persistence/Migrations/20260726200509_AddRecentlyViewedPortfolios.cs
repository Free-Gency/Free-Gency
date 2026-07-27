using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecentlyViewedPortfolios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecentlyViewedPortfolios",
                schema: "portfolio",
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
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortfolioProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentlyViewedPortfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecentlyViewedPortfolios_PortfolioProjects_PortfolioProjectId",
                        column: x => x.PortfolioProjectId,
                        principalSchema: "portfolio",
                        principalTable: "PortfolioProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecentlyViewedPortfolios_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewedPortfolios_PortfolioProjectId",
                schema: "portfolio",
                table: "RecentlyViewedPortfolios",
                column: "PortfolioProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewedPortfolios_UserId_PortfolioProjectId",
                schema: "portfolio",
                table: "RecentlyViewedPortfolios",
                columns: new[] { "UserId", "PortfolioProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewedPortfolios_UserId_ViewedAt",
                schema: "portfolio",
                table: "RecentlyViewedPortfolios",
                columns: new[] { "UserId", "ViewedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecentlyViewedPortfolios",
                schema: "portfolio");
        }
    }
}
