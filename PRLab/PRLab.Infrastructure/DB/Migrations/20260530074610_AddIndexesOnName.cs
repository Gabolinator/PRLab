using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesOnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Muscle_Name",
                schema: "public",
                table: "Muscle");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_Name",
                schema: "public",
                table: "Equipment");

            migrationBuilder.RenameColumn(
                name: "Audit_IsDeleted",
                schema: "public",
                table: "Muscle",
                newName: "IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_Muscle_Audit_IsDeleted",
                schema: "public",
                table: "Muscle",
                newName: "IX_Muscle_IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Audit_IsDeleted",
                schema: "public",
                table: "Equipment",
                newName: "IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_Equipment_Audit_IsDeleted",
                schema: "public",
                table: "Equipment",
                newName: "IX_Equipment_IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_Name",
                schema: "public",
                table: "Muscle",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Name",
                schema: "public",
                table: "Equipment",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Muscle_Name",
                schema: "public",
                table: "Muscle");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_Name",
                schema: "public",
                table: "Equipment");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Muscle",
                newName: "Audit_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_Muscle_IsDeleted",
                schema: "public",
                table: "Muscle",
                newName: "IX_Muscle_Audit_IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Equipment",
                newName: "Audit_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_Equipment_IsDeleted",
                schema: "public",
                table: "Equipment",
                newName: "IX_Equipment_Audit_IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_Name",
                schema: "public",
                table: "Muscle",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Name",
                schema: "public",
                table: "Equipment",
                column: "Name");
        }
    }
}
