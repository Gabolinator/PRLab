using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddMuscleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Muscle",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LatinName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BodySection = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
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
                    table.PrimaryKey("PK_Muscle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Muscle_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MuscleAntagonist",
                schema: "public",
                columns: table => new
                {
                    MuscleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AntagonistMuscleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuscleAntagonist", x => new { x.MuscleId, x.AntagonistMuscleId });
                    table.ForeignKey(
                        name: "FK_MuscleAntagonist_Muscle_AntagonistMuscleId",
                        column: x => x.AntagonistMuscleId,
                        principalSchema: "public",
                        principalTable: "Muscle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MuscleAntagonist_Muscle_MuscleId",
                        column: x => x.MuscleId,
                        principalSchema: "public",
                        principalTable: "Muscle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_Audit_IsDeleted",
                schema: "public",
                table: "Muscle",
                column: "Audit_IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_BodySection",
                schema: "public",
                table: "Muscle",
                column: "BodySection");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_DescriptionId",
                schema: "public",
                table: "Muscle",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_LatinName",
                schema: "public",
                table: "Muscle",
                column: "LatinName");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_Name",
                schema: "public",
                table: "Muscle",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MuscleAntagonist_AntagonistMuscleId",
                schema: "public",
                table: "MuscleAntagonist",
                column: "AntagonistMuscleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MuscleAntagonist",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Muscle",
                schema: "public");
        }
    }
}
