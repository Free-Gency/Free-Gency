using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations;

/// <summary>
/// Teams can have multiple categories; drop the old unique-on-TeamId constraint.
/// </summary>
public class AllowMultipleTeamCategories : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamCategories_TeamId",
            schema: "teams",
            table: "TeamCategories");

        migrationBuilder.CreateIndex(
            name: "IX_TeamCategories_TeamId",
            schema: "teams",
            table: "TeamCategories",
            column: "TeamId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamCategories_TeamId",
            schema: "teams",
            table: "TeamCategories");

        migrationBuilder.CreateIndex(
            name: "IX_TeamCategories_TeamId",
            schema: "teams",
            table: "TeamCategories",
            column: "TeamId",
            unique: true);
    }
}
