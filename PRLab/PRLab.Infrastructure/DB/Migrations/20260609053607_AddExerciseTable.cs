using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercise",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_Exercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercise_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseBlock",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Target_Value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Target_TargetType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    LoadTarget_Value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    LoadTarget_Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    LoadTarget_Unit = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RestBetweenReps_Seconds = table.Column<int>(type: "integer", nullable: true),
                    TransitionAfterBlock_Seconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_EccentricSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_BottomPauseSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_ConcentricSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_TopPauseSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_EccentricIntent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ExecutionDetails_BottomIntent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ExecutionDetails_ConcentricIntent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ExecutionDetails_TopIntent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ExecutionDetails_Intent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseBlock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseBlock_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "public",
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseBlock_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_DescriptionId",
                schema: "public",
                table: "Exercise",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_IsDeleted",
                schema: "public",
                table: "Exercise",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_Name",
                schema: "public",
                table: "Exercise",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_NameKey",
                schema: "public",
                table: "Exercise",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBlock_ExerciseId",
                schema: "public",
                table: "ExerciseBlock",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBlock_ExerciseId_Sequence",
                schema: "public",
                table: "ExerciseBlock",
                columns: new[] { "ExerciseId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBlock_MovementId",
                schema: "public",
                table: "ExerciseBlock",
                column: "MovementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseBlock",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Exercise",
                schema: "public");
        }
    }
}
