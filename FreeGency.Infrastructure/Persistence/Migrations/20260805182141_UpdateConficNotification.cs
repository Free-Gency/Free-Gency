using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConficNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Notifications_ProfileScope",
                schema: "core",
                table: "Notifications");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notifications_ProfileScope",
                schema: "core",
                table: "Notifications",
                sql: "NOT (ClientProfileId IS NOT NULL AND DeveloperProfileId IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Notifications_ProfileScope",
                schema: "core",
                table: "Notifications");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notifications_ProfileScope",
                schema: "core",
                table: "Notifications",
                sql: "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");
        }
    }
}
