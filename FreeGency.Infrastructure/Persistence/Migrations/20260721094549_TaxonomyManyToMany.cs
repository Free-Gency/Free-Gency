using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TaxonomyManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Specialties_SpecialtyId",
                schema: "marketplace",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Categories_CategoryId",
                schema: "catalog",
                table: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Specialties_CategoryId",
                schema: "catalog",
                table: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SpecialtyId",
                schema: "marketplace",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "catalog",
                table: "Specialties");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                schema: "marketplace",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsReadOnly",
                schema: "chat",
                table: "ChatRooms");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId1",
                schema: "finance",
                table: "LedgerEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategorySpecialties",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialtyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySpecialties", x => new { x.Id, x.CategoryId, x.SpecialtyId });
                    table.ForeignKey(
                        name: "FK_CategorySpecialties_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "catalog",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategorySpecialties_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalSchema: "catalog",
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategorySpecialty",
                schema: "catalog",
                columns: table => new
                {
                    CategoriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialtiesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySpecialty", x => new { x.CategoriesId, x.SpecialtiesId });
                    table.ForeignKey(
                        name: "FK_CategorySpecialty_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalSchema: "catalog",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategorySpecialty_Specialties_SpecialtiesId",
                        column: x => x.SpecialtiesId,
                        principalSchema: "catalog",
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MilestoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChatRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_ChatRooms_ChatRoomId",
                        column: x => x.ChatRoomId,
                        principalSchema: "chat",
                        principalTable: "ChatRooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "chat",
                        principalTable: "Messages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalSchema: "marketplace",
                        principalTable: "Milestones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_ProjectProposals_ProjectProposalId",
                        column: x => x.ProjectProposalId,
                        principalSchema: "marketplace",
                        principalTable: "ProjectProposals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "marketplace",
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "teams",
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSpecialties",
                schema: "marketplace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialtyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSpecialties", x => new { x.Id, x.ProjectId, x.SpecialtyId });
                    table.ForeignKey(
                        name: "FK_ProjectSpecialties_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "marketplace",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSpecialties_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalSchema: "catalog",
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpecialtySkills",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialtyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialtySkills", x => new { x.Id, x.SpecialtyId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_SpecialtySkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "catalog",
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpecialtySkills_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalSchema: "catalog",
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamSpecialties",
                schema: "teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialtyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, defaultValue: "system"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSpecialties", x => new { x.Id, x.TeamId, x.SpecialtyId });
                    table.ForeignKey(
                        name: "FK_TeamSpecialties_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalSchema: "catalog",
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamSpecialties_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "teams",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamCategories_TeamId",
                schema: "teams",
                table: "TeamCategories",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_ProjectId1",
                schema: "finance",
                table: "LedgerEntries",
                column: "ProjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySpecialties_CategoryId_SpecialtyId",
                schema: "catalog",
                table: "CategorySpecialties",
                columns: new[] { "CategoryId", "SpecialtyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategorySpecialties_SpecialtyId",
                schema: "catalog",
                table: "CategorySpecialties",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySpecialty_SpecialtiesId",
                schema: "catalog",
                table: "CategorySpecialty",
                column: "SpecialtiesId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ChatRoomId",
                table: "Notifications",
                column: "ChatRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_MessageId",
                table: "Notifications",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_MilestoneId",
                table: "Notifications",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ProjectId",
                table: "Notifications",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ProjectProposalId",
                table: "Notifications",
                column: "ProjectProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TeamId",
                table: "Notifications",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSpecialties_ProjectId_SpecialtyId",
                schema: "marketplace",
                table: "ProjectSpecialties",
                columns: new[] { "ProjectId", "SpecialtyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSpecialties_SpecialtyId",
                schema: "marketplace",
                table: "ProjectSpecialties",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialtySkills_SkillId",
                schema: "catalog",
                table: "SpecialtySkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialtySkills_SpecialtyId_SkillId",
                schema: "catalog",
                table: "SpecialtySkills",
                columns: new[] { "SpecialtyId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamSpecialties_SpecialtyId",
                schema: "teams",
                table: "TeamSpecialties",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSpecialties_TeamId_SpecialtyId",
                schema: "teams",
                table: "TeamSpecialties",
                columns: new[] { "TeamId", "SpecialtyId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntries_Projects_ProjectId1",
                schema: "finance",
                table: "LedgerEntries",
                column: "ProjectId1",
                principalSchema: "marketplace",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntries_Projects_ProjectId1",
                schema: "finance",
                table: "LedgerEntries");

            migrationBuilder.DropTable(
                name: "CategorySpecialties",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CategorySpecialty",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProjectSpecialties",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "SpecialtySkills",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "TeamSpecialties",
                schema: "teams");

            migrationBuilder.DropIndex(
                name: "IX_TeamCategories_TeamId",
                schema: "teams",
                table: "TeamCategories");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_ProjectId1",
                schema: "finance",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "ProjectId1",
                schema: "finance",
                table: "LedgerEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                schema: "catalog",
                table: "Specialties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SpecialtyId",
                schema: "marketplace",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReadOnly",
                schema: "chat",
                table: "ChatRooms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_CategoryId",
                schema: "catalog",
                table: "Specialties",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SpecialtyId",
                schema: "marketplace",
                table: "Projects",
                column: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Specialties_SpecialtyId",
                schema: "marketplace",
                table: "Projects",
                column: "SpecialtyId",
                principalSchema: "catalog",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Categories_CategoryId",
                schema: "catalog",
                table: "Specialties",
                column: "CategoryId",
                principalSchema: "catalog",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
