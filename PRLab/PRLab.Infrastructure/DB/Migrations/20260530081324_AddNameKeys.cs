using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddNameKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameKey",
                schema: "public",
                table: "Muscle",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameKey",
                schema: "public",
                table: "Equipment",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_NameKey",
                schema: "public",
                table: "Muscle",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_NameKey",
                schema: "public",
                table: "Equipment",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Muscle_NameKey",
                schema: "public",
                table: "Muscle");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_NameKey",
                schema: "public",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "NameKey",
                schema: "public",
                table: "Muscle");

            migrationBuilder.DropColumn(
                name: "NameKey",
                schema: "public",
                table: "Equipment");
        }
    }
}
