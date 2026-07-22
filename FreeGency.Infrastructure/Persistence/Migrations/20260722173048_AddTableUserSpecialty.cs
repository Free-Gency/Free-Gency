using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreeGency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTableUserSpecialty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSpecialties",
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
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialtyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSpecialties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSpecialties_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalSchema: "catalog",
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSpecialties_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSpecialties_SpecialtyId",
                table: "UserSpecialties",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSpecialties_UserId",
                table: "UserSpecialties",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSpecialties");
        }
    }
}
