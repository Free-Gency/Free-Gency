using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addClientNotificationSettingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientNotificationSettings",
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
                    NewMessageInApp = table.Column<bool>(type: "bit", nullable: false),
                    NewMessageEmail = table.Column<bool>(type: "bit", nullable: false),
                    ProposalReceivedInApp = table.Column<bool>(type: "bit", nullable: false),
                    ProposalReceivedEmail = table.Column<bool>(type: "bit", nullable: false),
                    MilestoneAddedInApp = table.Column<bool>(type: "bit", nullable: false),
                    MilestoneAddedEmail = table.Column<bool>(type: "bit", nullable: false),
                    WalletUpdatedInApp = table.Column<bool>(type: "bit", nullable: false),
                    WalletUpdatedEmail = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientNotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clientNotificationSettings_ClientProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "identity",
                        principalTable: "ClientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientNotificationSettings_ProfileId",
                table: "clientNotificationSettings",
                column: "ProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientNotificationSettings");
        }
    }
}
