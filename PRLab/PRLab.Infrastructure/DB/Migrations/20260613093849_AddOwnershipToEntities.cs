using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnershipToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                schema: "public",
                table: "MovementCategory",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                schema: "public",
                table: "MovementCategory",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                schema: "public",
                table: "Movement",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                schema: "public",
                table: "Movement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                schema: "public",
                table: "Exercise",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                schema: "public",
                table: "Exercise",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataOrigin",
                schema: "public",
                table: "Equipment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                schema: "public",
                table: "Equipment",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_DataOrigin",
                schema: "public",
                table: "MovementCategory",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_OwnerUserId",
                schema: "public",
                table: "MovementCategory",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_DataOrigin",
                schema: "public",
                table: "Movement",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_OwnerUserId",
                schema: "public",
                table: "Movement",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_DataOrigin",
                schema: "public",
                table: "Exercise",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_OwnerUserId",
                schema: "public",
                table: "Exercise",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_DataOrigin",
                schema: "public",
                table: "Equipment",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_OwnerUserId",
                schema: "public",
                table: "Equipment",
                column: "OwnerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MovementCategory_DataOrigin",
                schema: "public",
                table: "MovementCategory");

            migrationBuilder.DropIndex(
                name: "IX_MovementCategory_OwnerUserId",
                schema: "public",
                table: "MovementCategory");

            migrationBuilder.DropIndex(
                name: "IX_Movement_DataOrigin",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropIndex(
                name: "IX_Movement_OwnerUserId",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_DataOrigin",
                schema: "public",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_OwnerUserId",
                schema: "public",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_DataOrigin",
                schema: "public",
                table: "Equipment");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_OwnerUserId",
                schema: "public",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                schema: "public",
                table: "MovementCategory");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "public",
                table: "MovementCategory");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "public",
                table: "Movement");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                schema: "public",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "public",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "DataOrigin",
                schema: "public",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "public",
                table: "Equipment");
        }
    }
}
