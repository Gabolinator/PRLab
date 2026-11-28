using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alias",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ValueKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alias", x => x.Id);
                    table.UniqueConstraint("AK_Alias_Id_ValueKey", x => new { x.Id, x.ValueKey });
                });

            migrationBuilder.CreateTable(
                name: "EquipmentAlias",
                schema: "public",
                columns: table => new
                {
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasValueKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentAlias", x => new { x.EquipmentId, x.AliasId });
                    table.ForeignKey(
                        name: "FK_EquipmentAlias_Alias_AliasId_AliasValueKey",
                        columns: x => new { x.AliasId, x.AliasValueKey },
                        principalSchema: "public",
                        principalTable: "Alias",
                        principalColumns: new[] { "Id", "ValueKey" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentAlias_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalSchema: "public",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseAlias",
                schema: "public",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasValueKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseAlias", x => new { x.ExerciseId, x.AliasId });
                    table.ForeignKey(
                        name: "FK_ExerciseAlias_Alias_AliasId_AliasValueKey",
                        columns: x => new { x.AliasId, x.AliasValueKey },
                        principalSchema: "public",
                        principalTable: "Alias",
                        principalColumns: new[] { "Id", "ValueKey" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseAlias_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "public",
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovementAlias",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasValueKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementAlias", x => new { x.MovementId, x.AliasId });
                    table.ForeignKey(
                        name: "FK_MovementAlias_Alias_AliasId_AliasValueKey",
                        columns: x => new { x.AliasId, x.AliasValueKey },
                        principalSchema: "public",
                        principalTable: "Alias",
                        principalColumns: new[] { "Id", "ValueKey" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovementAlias_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MuscleAlias",
                schema: "public",
                columns: table => new
                {
                    MuscleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasValueKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuscleAlias", x => new { x.MuscleId, x.AliasId });
                    table.ForeignKey(
                        name: "FK_MuscleAlias_Alias_AliasId_AliasValueKey",
                        columns: x => new { x.AliasId, x.AliasValueKey },
                        principalSchema: "public",
                        principalTable: "Alias",
                        principalColumns: new[] { "Id", "ValueKey" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MuscleAlias_Muscle_MuscleId",
                        column: x => x.MuscleId,
                        principalSchema: "public",
                        principalTable: "Muscle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alias_ValueKey",
                schema: "public",
                table: "Alias",
                column: "ValueKey");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentAlias_AliasId_AliasValueKey",
                schema: "public",
                table: "EquipmentAlias",
                columns: new[] { "AliasId", "AliasValueKey" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentAlias_AliasValueKey",
                schema: "public",
                table: "EquipmentAlias",
                column: "AliasValueKey");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentAlias_EquipmentId_AliasValueKey",
                schema: "public",
                table: "EquipmentAlias",
                columns: new[] { "EquipmentId", "AliasValueKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAlias_AliasId_AliasValueKey",
                schema: "public",
                table: "ExerciseAlias",
                columns: new[] { "AliasId", "AliasValueKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAlias_AliasValueKey",
                schema: "public",
                table: "ExerciseAlias",
                column: "AliasValueKey");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAlias_ExerciseId_AliasValueKey",
                schema: "public",
                table: "ExerciseAlias",
                columns: new[] { "ExerciseId", "AliasValueKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovementAlias_AliasId_AliasValueKey",
                schema: "public",
                table: "MovementAlias",
                columns: new[] { "AliasId", "AliasValueKey" });

            migrationBuilder.CreateIndex(
                name: "IX_MovementAlias_AliasValueKey",
                schema: "public",
                table: "MovementAlias",
                column: "AliasValueKey");

            migrationBuilder.CreateIndex(
                name: "IX_MovementAlias_MovementId_AliasValueKey",
                schema: "public",
                table: "MovementAlias",
                columns: new[] { "MovementId", "AliasValueKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuscleAlias_AliasId_AliasValueKey",
                schema: "public",
                table: "MuscleAlias",
                columns: new[] { "AliasId", "AliasValueKey" });

            migrationBuilder.CreateIndex(
                name: "IX_MuscleAlias_AliasValueKey",
                schema: "public",
                table: "MuscleAlias",
                column: "AliasValueKey");

            migrationBuilder.CreateIndex(
                name: "IX_MuscleAlias_MuscleId_AliasValueKey",
                schema: "public",
                table: "MuscleAlias",
                columns: new[] { "MuscleId", "AliasValueKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentAlias",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExerciseAlias",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MovementAlias",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MuscleAlias",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Alias",
                schema: "public");
        }
    }
}
