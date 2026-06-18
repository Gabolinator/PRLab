using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddLaterabilityMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransitionAfterBlock_Seconds",
                schema: "public",
                table: "ExerciseBlock",
                newName: "TransitionAfterStep_Seconds");

            migrationBuilder.AddColumn<string>(
                name: "Laterality",
                schema: "public",
                table: "Movement",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RestBetweenReps_RestPolicy",
                schema: "public",
                table: "ExerciseBlock",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransitionAfterStep_RestPolicy",
                schema: "public",
                table: "ExerciseBlock",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Movement_Laterality",
                schema: "public",
                table: "Movement",
                column: "Laterality");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Movement_Laterality",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropColumn(
                name: "Laterality",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropColumn(
                name: "RestBetweenReps_RestPolicy",
                schema: "public",
                table: "ExerciseBlock");

            migrationBuilder.DropColumn(
                name: "TransitionAfterStep_RestPolicy",
                schema: "public",
                table: "ExerciseBlock");

            migrationBuilder.RenameColumn(
                name: "TransitionAfterStep_Seconds",
                schema: "public",
                table: "ExerciseBlock",
                newName: "TransitionAfterBlock_Seconds");
        }
    }
}
