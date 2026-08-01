using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddMuscleFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MuscleFunction",
                schema: "public",
                columns: table => new
                {
                    MuscleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Function = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuscleFunction", x => new { x.MuscleId, x.Function });
                    table.ForeignKey(
                        name: "FK_MuscleFunction_Muscle_MuscleId",
                        column: x => x.MuscleId,
                        principalSchema: "public",
                        principalTable: "Muscle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MuscleFunction_Function_Role",
                schema: "public",
                table: "MuscleFunction",
                columns: new[] { "Function", "Role" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MuscleFunction",
                schema: "public");
        }
    }
}
