using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddMovementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MovementCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantOfId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryPattern = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_Movement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movement_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movement_MovementCategory_MovementCategoryId",
                        column: x => x.MovementCategoryId,
                        principalSchema: "public",
                        principalTable: "MovementCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movement_Movement_VariantOfId",
                        column: x => x.VariantOfId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovementEquipment",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementEquipment", x => new { x.MovementId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_MovementEquipment_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalSchema: "public",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementEquipment_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovementMuscle",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementMuscle", x => new { x.MovementId, x.MuscleId });
                    table.ForeignKey(
                        name: "FK_MovementMuscle_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovementMuscle_Muscle_MuscleId",
                        column: x => x.MuscleId,
                        principalSchema: "public",
                        principalTable: "Muscle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovementPatternTag",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pattern = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementPatternTag", x => new { x.MovementId, x.Pattern });
                    table.ForeignKey(
                        name: "FK_MovementPatternTag_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movement_DescriptionId",
                schema: "public",
                table: "Movement",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_IsDeleted",
                schema: "public",
                table: "Movement",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_MovementCategoryId",
                schema: "public",
                table: "Movement",
                column: "MovementCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_Name",
                schema: "public",
                table: "Movement",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_NameKey",
                schema: "public",
                table: "Movement",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_PrimaryPattern",
                schema: "public",
                table: "Movement",
                column: "PrimaryPattern");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_VariantOfId",
                schema: "public",
                table: "Movement",
                column: "VariantOfId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementEquipment_EquipmentId",
                schema: "public",
                table: "MovementEquipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementMuscle_MuscleId",
                schema: "public",
                table: "MovementMuscle",
                column: "MuscleId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementMuscle_Role",
                schema: "public",
                table: "MovementMuscle",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_MovementPatternTag_Pattern",
                schema: "public",
                table: "MovementPatternTag",
                column: "Pattern");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovementEquipment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MovementMuscle",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MovementPatternTag",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Movement",
                schema: "public");
        }
    }
}
