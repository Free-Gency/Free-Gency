using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProfileScopedJunctionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInterests_Users_UserId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkills_Users_UserId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSpecialties_Specialties_SpecialtyId",
                table: "UserSpecialties");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSpecialties_Users_UserId",
                table: "UserSpecialties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSkills",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropIndex(
                name: "IX_UserSkills_UserId_SkillId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserInterests",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropIndex(
                name: "IX_UserInterests_UserId_CategoryId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropIndex(
                name: "IX_UserSpecialties_UserId",
                table: "UserSpecialties");

            migrationBuilder.RenameTable(
                name: "UserSpecialties",
                newName: "UserSpecialties",
                newSchema: "identity");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientProfileId",
                schema: "identity",
                table: "UserSkills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperProfileId",
                schema: "identity",
                table: "UserSkills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientProfileId",
                schema: "identity",
                table: "UserInterests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperProfileId",
                schema: "identity",
                table: "UserInterests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientProfileId",
                schema: "identity",
                table: "UserSpecialties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperProfileId",
                schema: "identity",
                table: "UserSpecialties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE ui
                SET ui.ClientProfileId = cp.Id
                FROM [identity].[UserInterests] ui
                INNER JOIN [identity].[ClientProfiles] cp ON cp.UserId = ui.UserId
                WHERE ui.ClientProfileId IS NULL AND ui.DeveloperProfileId IS NULL;

                UPDATE ui
                SET ui.DeveloperProfileId = dp.Id
                FROM [identity].[UserInterests] ui
                INNER JOIN [identity].[DeveloperProfiles] dp ON dp.UserId = ui.UserId
                WHERE ui.ClientProfileId IS NULL AND ui.DeveloperProfileId IS NULL;

                UPDATE us
                SET us.ClientProfileId = cp.Id
                FROM [identity].[UserSkills] us
                INNER JOIN [identity].[ClientProfiles] cp ON cp.UserId = us.UserId
                WHERE us.ClientProfileId IS NULL AND us.DeveloperProfileId IS NULL;

                UPDATE us
                SET us.DeveloperProfileId = dp.Id
                FROM [identity].[UserSkills] us
                INNER JOIN [identity].[DeveloperProfiles] dp ON dp.UserId = us.UserId
                WHERE us.ClientProfileId IS NULL AND us.DeveloperProfileId IS NULL;

                UPDATE us
                SET us.ClientProfileId = cp.Id
                FROM [identity].[UserSpecialties] us
                INNER JOIN [identity].[ClientProfiles] cp ON cp.UserId = us.UserId
                WHERE us.ClientProfileId IS NULL AND us.DeveloperProfileId IS NULL;

                UPDATE us
                SET us.DeveloperProfileId = dp.Id
                FROM [identity].[UserSpecialties] us
                INNER JOIN [identity].[DeveloperProfiles] dp ON dp.UserId = us.UserId
                WHERE us.ClientProfileId IS NULL AND us.DeveloperProfileId IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSkills",
                schema: "identity",
                table: "UserSkills",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserInterests",
                schema: "identity",
                table: "UserInterests",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_DeveloperProfiles_Id",
                schema: "identity",
                table: "DeveloperProfiles",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ClientProfiles_Id",
                schema: "identity",
                table: "ClientProfiles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_ClientProfileId_SkillId",
                schema: "identity",
                table: "UserSkills",
                columns: new[] { "ClientProfileId", "SkillId" },
                unique: true,
                filter: "[ClientProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_DeveloperProfileId_SkillId",
                schema: "identity",
                table: "UserSkills",
                columns: new[] { "DeveloperProfileId", "SkillId" },
                unique: true,
                filter: "[DeveloperProfileId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserSkills_ProfileScope",
                schema: "identity",
                table: "UserSkills",
                sql: "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_ClientProfileId_CategoryId",
                schema: "identity",
                table: "UserInterests",
                columns: new[] { "ClientProfileId", "CategoryId" },
                unique: true,
                filter: "[ClientProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_DeveloperProfileId_CategoryId",
                schema: "identity",
                table: "UserInterests",
                columns: new[] { "DeveloperProfileId", "CategoryId" },
                unique: true,
                filter: "[DeveloperProfileId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserInterests_ProfileScope",
                schema: "identity",
                table: "UserInterests",
                sql: "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_UserSpecialties_ClientProfileId_SpecialtyId",
                schema: "identity",
                table: "UserSpecialties",
                columns: new[] { "ClientProfileId", "SpecialtyId" },
                unique: true,
                filter: "[ClientProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSpecialties_DeveloperProfileId_SpecialtyId",
                schema: "identity",
                table: "UserSpecialties",
                columns: new[] { "DeveloperProfileId", "SpecialtyId" },
                unique: true,
                filter: "[DeveloperProfileId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserSpecialties_ProfileScope",
                schema: "identity",
                table: "UserSpecialties",
                sql: "(ClientProfileId IS NOT NULL AND DeveloperProfileId IS NULL) OR (ClientProfileId IS NULL AND DeveloperProfileId IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInterests_ClientProfiles_ClientProfileId",
                schema: "identity",
                table: "UserInterests",
                column: "ClientProfileId",
                principalSchema: "identity",
                principalTable: "ClientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserInterests_DeveloperProfiles_DeveloperProfileId",
                schema: "identity",
                table: "UserInterests",
                column: "DeveloperProfileId",
                principalSchema: "identity",
                principalTable: "DeveloperProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkills_ClientProfiles_ClientProfileId",
                schema: "identity",
                table: "UserSkills",
                column: "ClientProfileId",
                principalSchema: "identity",
                principalTable: "ClientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkills_DeveloperProfiles_DeveloperProfileId",
                schema: "identity",
                table: "UserSkills",
                column: "DeveloperProfileId",
                principalSchema: "identity",
                principalTable: "DeveloperProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSpecialties_ClientProfiles_ClientProfileId",
                schema: "identity",
                table: "UserSpecialties",
                column: "ClientProfileId",
                principalSchema: "identity",
                principalTable: "ClientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSpecialties_DeveloperProfiles_DeveloperProfileId",
                schema: "identity",
                table: "UserSpecialties",
                column: "DeveloperProfileId",
                principalSchema: "identity",
                principalTable: "DeveloperProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSpecialties_Specialties_SpecialtyId",
                schema: "identity",
                table: "UserSpecialties",
                column: "SpecialtyId",
                principalSchema: "catalog",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInterests_ClientProfiles_ClientProfileId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserInterests_DeveloperProfiles_DeveloperProfileId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkills_ClientProfiles_ClientProfileId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkills_DeveloperProfiles_DeveloperProfileId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSpecialties_ClientProfiles_ClientProfileId",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSpecialties_DeveloperProfiles_DeveloperProfileId",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSpecialties_Specialties_SpecialtyId",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSkills",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropIndex(
                name: "IX_UserSkills_ClientProfileId_SkillId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropIndex(
                name: "IX_UserSkills_DeveloperProfileId_SkillId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserSkills_ProfileScope",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserInterests",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropIndex(
                name: "IX_UserInterests_ClientProfileId_CategoryId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropIndex(
                name: "IX_UserInterests_DeveloperProfileId_CategoryId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserInterests_ProfileScope",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_DeveloperProfiles_Id",
                schema: "identity",
                table: "DeveloperProfiles");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ClientProfiles_Id",
                schema: "identity",
                table: "ClientProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserSpecialties_ClientProfileId_SpecialtyId",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.DropIndex(
                name: "IX_UserSpecialties_DeveloperProfileId_SpecialtyId",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserSpecialties_ProfileScope",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.DropColumn(
                name: "ClientProfileId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "DeveloperProfileId",
                schema: "identity",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "ClientProfileId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropColumn(
                name: "DeveloperProfileId",
                schema: "identity",
                table: "UserInterests");

            migrationBuilder.DropColumn(
                name: "ClientProfileId",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.DropColumn(
                name: "DeveloperProfileId",
                schema: "identity",
                table: "UserSpecialties");

            migrationBuilder.RenameTable(
                name: "UserSpecialties",
                schema: "identity",
                newName: "UserSpecialties");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "identity",
                table: "UserSkills",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "identity",
                table: "UserInterests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "UserSpecialties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSkills",
                schema: "identity",
                table: "UserSkills",
                columns: new[] { "Id", "UserId", "SkillId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserInterests",
                schema: "identity",
                table: "UserInterests",
                columns: new[] { "Id", "UserId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_UserId_SkillId",
                schema: "identity",
                table: "UserSkills",
                columns: new[] { "UserId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UserId_CategoryId",
                schema: "identity",
                table: "UserInterests",
                columns: new[] { "UserId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSpecialties_UserId",
                table: "UserSpecialties",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInterests_Users_UserId",
                schema: "identity",
                table: "UserInterests",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkills_Users_UserId",
                schema: "identity",
                table: "UserSkills",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSpecialties_Specialties_SpecialtyId",
                table: "UserSpecialties",
                column: "SpecialtyId",
                principalSchema: "catalog",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSpecialties_Users_UserId",
                table: "UserSpecialties",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
