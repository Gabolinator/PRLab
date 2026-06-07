using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMovementEquipmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovementEquipment",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "MovementEquipmentRequirement",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementEquipmentRequirement", x => new { x.MovementId, x.EquipmentId, x.GroupKey, x.Kind });
                    table.ForeignKey(
                        name: "FK_MovementEquipmentRequirement_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalSchema: "public",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementEquipmentRequirement_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovementEquipmentRequirement_EquipmentId",
                schema: "public",
                table: "MovementEquipmentRequirement",
                column: "EquipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovementEquipmentRequirement",
                schema: "public");

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

            migrationBuilder.CreateIndex(
                name: "IX_MovementEquipment_EquipmentId",
                schema: "public",
                table: "MovementEquipment",
                column: "EquipmentId");
        }
    }
}
