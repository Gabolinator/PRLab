using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkTargetsToMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultWorkTargetType",
                schema: "public",
                table: "Movement",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MovementAllowedWorkTarget",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementAllowedWorkTarget", x => new { x.MovementId, x.TargetType });
                    table.ForeignKey(
                        name: "FK_MovementAllowedWorkTarget_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movement_DefaultWorkTargetType",
                schema: "public",
                table: "Movement",
                column: "DefaultWorkTargetType");

            migrationBuilder.CreateIndex(
                name: "IX_MovementAllowedWorkTarget_TargetType",
                schema: "public",
                table: "MovementAllowedWorkTarget",
                column: "TargetType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovementAllowedWorkTarget",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Movement_DefaultWorkTargetType",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropColumn(
                name: "DefaultWorkTargetType",
                schema: "public",
                table: "Movement");
        }
    }
}
