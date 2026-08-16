using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MilestonePlanVersionUniquePerProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MilestonePlanVersions_ProjectId_Version",
                schema: "marketplace",
                table: "MilestonePlanVersions");

            migrationBuilder.DropIndex(
                name: "IX_MilestonePlanVersions_ProposalId",
                schema: "marketplace",
                table: "MilestonePlanVersions");

            migrationBuilder.CreateIndex(
                name: "IX_MilestonePlanVersions_ProjectId",
                schema: "marketplace",
                table: "MilestonePlanVersions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestonePlanVersions_ProposalId_Version",
                schema: "marketplace",
                table: "MilestonePlanVersions",
                columns: new[] { "ProposalId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MilestonePlanVersions_ProjectId",
                schema: "marketplace",
                table: "MilestonePlanVersions");

            migrationBuilder.DropIndex(
                name: "IX_MilestonePlanVersions_ProposalId_Version",
                schema: "marketplace",
                table: "MilestonePlanVersions");

            migrationBuilder.CreateIndex(
                name: "IX_MilestonePlanVersions_ProposalId",
                schema: "marketplace",
                table: "MilestonePlanVersions",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_MilestonePlanVersions_ProjectId_Version",
                schema: "marketplace",
                table: "MilestonePlanVersions",
                columns: new[] { "ProjectId", "Version" },
                unique: true);
        }
    }
}
