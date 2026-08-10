using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectInvitations",
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
                    ClientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InviteeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InviteeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InviteeTeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChatRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectInvitations", x => x.Id);
                    table.CheckConstraint("CK_ProjectInvitations_InviteeScope", "(InviteeType = 'User' AND InviteeUserId IS NOT NULL AND InviteeTeamId IS NULL) OR (InviteeType = 'Team' AND InviteeTeamId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalSchema: "chat",
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_ProjectProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalSchema: "marketplace",
                        principalTable: "ProjectProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "marketplace",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Teams_InviteeTeamId",
                        column: x => x.InviteeTeamId,
                        principalSchema: "teams",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Users_ClientUserId",
                        column: x => x.ClientUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectInvitations_Users_InviteeUserId",
                        column: x => x.InviteeUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ChatRoomId",
                schema: "marketplace",
                table: "ProjectInvitations",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ClientUserId",
                schema: "marketplace",
                table: "ProjectInvitations",
                column: "ClientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_InviteeTeamId",
                schema: "marketplace",
                table: "ProjectInvitations",
                column: "InviteeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_InviteeUserId",
                schema: "marketplace",
                table: "ProjectInvitations",
                column: "InviteeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ProjectId_InviteeTeamId",
                schema: "marketplace",
                table: "ProjectInvitations",
                columns: new[] { "ProjectId", "InviteeTeamId" },
                unique: true,
                filter: "[InviteeType] = 'Team' AND [Status] = 'Pending' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ProjectId_InviteeUserId",
                schema: "marketplace",
                table: "ProjectInvitations",
                columns: new[] { "ProjectId", "InviteeUserId" },
                unique: true,
                filter: "[InviteeType] = 'User' AND [Status] = 'Pending' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_ProposalId",
                schema: "marketplace",
                table: "ProjectInvitations",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectInvitations_Status",
                schema: "marketplace",
                table: "ProjectInvitations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectInvitations",
                schema: "marketplace");
        }
    }
}
