using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddMovementCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovementCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BaseMovementCategory = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovementCategory_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_BaseMovementCategory",
                schema: "public",
                table: "MovementCategory",
                column: "BaseMovementCategory");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_DescriptionId",
                schema: "public",
                table: "MovementCategory",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_IsDeleted",
                schema: "public",
                table: "MovementCategory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_Name",
                schema: "public",
                table: "MovementCategory",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_NameKey",
                schema: "public",
                table: "MovementCategory",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovementCategory",
                schema: "public");
        }
    }
}
