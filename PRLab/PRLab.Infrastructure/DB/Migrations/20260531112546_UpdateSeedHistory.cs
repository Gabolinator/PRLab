using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SeedHistory",
                schema: "public",
                table: "SeedHistory");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "SeedHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Version",
                schema: "public",
                table: "SeedHistory",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SeedHistory",
                schema: "public",
                table: "SeedHistory",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SeedHistory_Name_Version",
                schema: "public",
                table: "SeedHistory",
                columns: new[] { "Name", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SeedHistory",
                schema: "public",
                table: "SeedHistory");

            migrationBuilder.DropIndex(
                name: "IX_SeedHistory_Name_Version",
                schema: "public",
                table: "SeedHistory");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "public",
                table: "SeedHistory");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "public",
                table: "SeedHistory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SeedHistory",
                schema: "public",
                table: "SeedHistory",
                column: "Name");
        }
    }
}
