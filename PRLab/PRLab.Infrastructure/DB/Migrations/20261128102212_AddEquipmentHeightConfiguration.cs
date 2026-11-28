using Microsoft.EntityFrameworkCore.Migrations;

namespace PRLab.Infrastructure.DB.Migrations;

public partial class AddEquipmentHeightConfiguration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "SupportsHeightConfiguration",
            schema: "public",
            table: "Equipment",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        // Full reseeds can create built-in equipment with a generated ID.
        migrationBuilder.Sql(
            """
            UPDATE public."Equipment"
            SET "SupportsHeightConfiguration" = TRUE
            WHERE "Id" = '88ca7081-d8f0-45b9-a6c1-927959dbbf5a'
               OR ("DataOrigin" = 'BuiltIn' AND "NameKey" = 'plyometricbox');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "SupportsHeightConfiguration",
            schema: "public",
            table: "Equipment");
}
