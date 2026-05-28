using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Descriptions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultLanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Audit_IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DescriptionTranslations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DescriptionTranslations_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Audit_IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Descriptions_DefaultLanguageCode",
                schema: "public",
                table: "Descriptions",
                column: "DefaultLanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionTranslations_DescriptionId",
                schema: "public",
                table: "DescriptionTranslations",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionTranslations_DescriptionId_LanguageCode",
                schema: "public",
                table: "DescriptionTranslations",
                columns: new[] { "DescriptionId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionTranslations_LanguageCode_Content",
                schema: "public",
                table: "DescriptionTranslations",
                columns: new[] { "LanguageCode", "Content" });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Audit_IsDeleted",
                schema: "public",
                table: "Equipment",
                column: "Audit_IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_DescriptionId",
                schema: "public",
                table: "Equipment",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Name",
                schema: "public",
                table: "Equipment",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Audit_IsDeleted",
                schema: "public",
                table: "Users",
                column: "Audit_IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                schema: "public",
                table: "Users",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                schema: "public",
                table: "Users",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DescriptionTranslations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Equipment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Descriptions",
                schema: "public");
        }
    }
}
