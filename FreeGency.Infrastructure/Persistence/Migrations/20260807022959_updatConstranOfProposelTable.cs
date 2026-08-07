using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updatConstranOfProposelTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectProposals_ProjectId_TeamId",
                schema: "marketplace",
                table: "ProjectProposals");

            migrationBuilder.DropIndex(
                name: "IX_ProjectProposals_ProjectId_UserId",
                schema: "marketplace",
                table: "ProjectProposals");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProposals_ProjectId_TeamId",
                schema: "marketplace",
                table: "ProjectProposals",
                columns: new[] { "ProjectId", "TeamId" },
                unique: true,
                filter: "[ApplicantType] = 'Team'");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProposals_ProjectId_UserId",
                schema: "marketplace",
                table: "ProjectProposals",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[ApplicantType] = 'User'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectProposals_ProjectId_TeamId",
                schema: "marketplace",
                table: "ProjectProposals");

            migrationBuilder.DropIndex(
                name: "IX_ProjectProposals_ProjectId_UserId",
                schema: "marketplace",
                table: "ProjectProposals");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProposals_ProjectId_TeamId",
                schema: "marketplace",
                table: "ProjectProposals",
                columns: new[] { "ProjectId", "TeamId" },
                unique: true,
                filter: "[TeamId] IS NOT NULL AND [TeamId] <> '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProposals_ProjectId_UserId",
                schema: "marketplace",
                table: "ProjectProposals",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [UserId] <> '00000000-0000-0000-0000-000000000000'");
        }
    }
}
