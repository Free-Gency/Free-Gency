using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHiringAgentRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAgentGenerated",
                schema: "chat",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "HiringAgentCandidates",
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
                    HiringAgentRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InviteeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InviteeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InviteeTeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuggestionScore = table.Column<float>(type: "real", nullable: false),
                    RankOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Suggested"),
                    InvitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChatRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LatestPlanVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiscussionScore = table.Column<float>(type: "real", nullable: true),
                    DiscussionNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AgentMessageCount = table.Column<int>(type: "int", nullable: false),
                    LastAgentMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringAgentCandidates", x => x.Id);
                    table.CheckConstraint("CK_HiringAgentCandidates_InviteeScope", "(InviteeType = 'User' AND InviteeUserId IS NOT NULL AND InviteeTeamId IS NULL) OR (InviteeType = 'Team' AND InviteeTeamId IS NOT NULL AND InviteeUserId IS NULL)");
                    table.ForeignKey(
                        name: "FK_HiringAgentCandidates_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalSchema: "chat",
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HiringAgentCandidates_MilestonePlanVersions_LatestPlanVersionId",
                        column: x => x.LatestPlanVersionId,
                        principalSchema: "marketplace",
                        principalTable: "MilestonePlanVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HiringAgentCandidates_ProjectInvitations_InvitationId",
                        column: x => x.InvitationId,
                        principalSchema: "marketplace",
                        principalTable: "ProjectInvitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HiringAgentCandidates_ProjectProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalSchema: "marketplace",
                        principalTable: "ProjectProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "HiringAgentRuns",
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
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Queued"),
                    TopK = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    InviteDeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscussionDeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecommendedProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecommendedPlanVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecommendedCandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReportJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReportReadyAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringAgentRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiringAgentRuns_HiringAgentCandidates_RecommendedCandidateId",
                        column: x => x.RecommendedCandidateId,
                        principalSchema: "marketplace",
                        principalTable: "HiringAgentCandidates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HiringAgentRuns_MilestonePlanVersions_RecommendedPlanVersionId",
                        column: x => x.RecommendedPlanVersionId,
                        principalSchema: "marketplace",
                        principalTable: "MilestonePlanVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HiringAgentRuns_ProjectProposals_RecommendedProposalId",
                        column: x => x.RecommendedProposalId,
                        principalSchema: "marketplace",
                        principalTable: "ProjectProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HiringAgentRuns_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "marketplace",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringAgentRuns_Users_ClientUserId",
                        column: x => x.ClientUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentCandidates_ChatRoomId",
                schema: "marketplace",
                table: "HiringAgentCandidates",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentCandidates_HiringAgentRunId",
                schema: "marketplace",
                table: "HiringAgentCandidates",
                column: "HiringAgentRunId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentCandidates_InvitationId",
                schema: "marketplace",
                table: "HiringAgentCandidates",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentCandidates_LatestPlanVersionId",
                schema: "marketplace",
                table: "HiringAgentCandidates",
                column: "LatestPlanVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentCandidates_ProposalId",
                schema: "marketplace",
                table: "HiringAgentCandidates",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentCandidates_Status",
                schema: "marketplace",
                table: "HiringAgentCandidates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentRuns_ClientUserId",
                schema: "marketplace",
                table: "HiringAgentRuns",
                column: "ClientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentRuns_ProjectId",
                schema: "marketplace",
                table: "HiringAgentRuns",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentRuns_ProjectId_Status",
                schema: "marketplace",
                table: "HiringAgentRuns",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentRuns_RecommendedCandidateId",
                schema: "marketplace",
                table: "HiringAgentRuns",
                column: "RecommendedCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentRuns_RecommendedPlanVersionId",
                schema: "marketplace",
                table: "HiringAgentRuns",
                column: "RecommendedPlanVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentRuns_RecommendedProposalId",
                schema: "marketplace",
                table: "HiringAgentRuns",
                column: "RecommendedProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringAgentRuns_Status",
                schema: "marketplace",
                table: "HiringAgentRuns",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_HiringAgentCandidates_HiringAgentRuns_HiringAgentRunId",
                schema: "marketplace",
                table: "HiringAgentCandidates",
                column: "HiringAgentRunId",
                principalSchema: "marketplace",
                principalTable: "HiringAgentRuns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HiringAgentCandidates_HiringAgentRuns_HiringAgentRunId",
                schema: "marketplace",
                table: "HiringAgentCandidates");

            migrationBuilder.DropTable(
                name: "HiringAgentRuns",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "HiringAgentCandidates",
                schema: "marketplace");

            migrationBuilder.DropColumn(
                name: "IsAgentGenerated",
                schema: "chat",
                table: "Messages");
        }
    }
}
