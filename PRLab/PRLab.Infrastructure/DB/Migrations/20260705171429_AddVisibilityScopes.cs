using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddVisibilityScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VisibilityScope",
                schema: "public",
                table: "WorkoutBlock",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisibilityScope",
                schema: "public",
                table: "Workout",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisibilityScope",
                schema: "public",
                table: "MovementCategory",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisibilityScope",
                schema: "public",
                table: "Movement",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisibilityScope",
                schema: "public",
                table: "Exercise",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisibilityScope",
                schema: "public",
                table: "Equipment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlock_VisibilityScope",
                schema: "public",
                table: "WorkoutBlock",
                column: "VisibilityScope");

            migrationBuilder.CreateIndex(
                name: "IX_Workout_VisibilityScope",
                schema: "public",
                table: "Workout",
                column: "VisibilityScope");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_VisibilityScope",
                schema: "public",
                table: "MovementCategory",
                column: "VisibilityScope");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_VisibilityScope",
                schema: "public",
                table: "Movement",
                column: "VisibilityScope");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_VisibilityScope",
                schema: "public",
                table: "Exercise",
                column: "VisibilityScope");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_VisibilityScope",
                schema: "public",
                table: "Equipment",
                column: "VisibilityScope");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkoutBlock_VisibilityScope",
                schema: "public",
                table: "WorkoutBlock");

            migrationBuilder.DropIndex(
                name: "IX_Workout_VisibilityScope",
                schema: "public",
                table: "Workout");

            migrationBuilder.DropIndex(
                name: "IX_MovementCategory_VisibilityScope",
                schema: "public",
                table: "MovementCategory");

            migrationBuilder.DropIndex(
                name: "IX_Movement_VisibilityScope",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_VisibilityScope",
                schema: "public",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_VisibilityScope",
                schema: "public",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "VisibilityScope",
                schema: "public",
                table: "WorkoutBlock");

            migrationBuilder.DropColumn(
                name: "VisibilityScope",
                schema: "public",
                table: "Workout");

            migrationBuilder.DropColumn(
                name: "VisibilityScope",
                schema: "public",
                table: "MovementCategory");

            migrationBuilder.DropColumn(
                name: "VisibilityScope",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropColumn(
                name: "VisibilityScope",
                schema: "public",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "VisibilityScope",
                schema: "public",
                table: "Equipment");
        }
    }
}
