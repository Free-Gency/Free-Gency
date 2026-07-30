using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameProposalPortfolioUrlToSimilarLinksUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PortfolioUrl",
                schema: "marketplace",
                table: "ProjectProposals",
                newName: "SimilarLinksUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SimilarLinksUrl",
                schema: "marketplace",
                table: "ProjectProposals",
                newName: "PortfolioUrl");
        }
    }
}
