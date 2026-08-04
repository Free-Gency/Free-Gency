using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class adddevelopernotificationSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "developerNotificationSettings",
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
                    ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessagesInApp = table.Column<bool>(type: "bit", nullable: false),
                    MessagesEmail = table.Column<bool>(type: "bit", nullable: false),
                    ProjectsInApp = table.Column<bool>(type: "bit", nullable: false),
                    ProjectsEmail = table.Column<bool>(type: "bit", nullable: false),
                    MilestonesInApp = table.Column<bool>(type: "bit", nullable: false),
                    MilestonesEmail = table.Column<bool>(type: "bit", nullable: false),
                    WalletInApp = table.Column<bool>(type: "bit", nullable: false),
                    WalletEmail = table.Column<bool>(type: "bit", nullable: false),
                    TeamsInApp = table.Column<bool>(type: "bit", nullable: false),
                    TeamsEmail = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_developerNotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_developerNotificationSettings_DeveloperProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "identity",
                        principalTable: "DeveloperProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_developerNotificationSettings_ProfileId",
                table: "developerNotificationSettings",
                column: "ProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "developerNotificationSettings");
        }
    }
}
