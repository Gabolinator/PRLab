using Microsoft.EntityFrameworkCore.Migrations;

namespace PRLab.Infrastructure.DB.Migrations;

public partial class AddEquipmentConfigurationTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int[]>(
            name: "SupportedConfigurationTypes",
            schema: "public",
            table: "Equipment",
            type: "integer[]",
            nullable: false,
            defaultValue: Array.Empty<int>());

        migrationBuilder.Sql(
            """
            UPDATE public."Equipment"
            SET "SupportedConfigurationTypes" = ARRAY[1]
            WHERE "SupportsHeightConfiguration";

            UPDATE public."Equipment"
            SET "SupportedConfigurationTypes" = array_append("SupportedConfigurationTypes", 2)
            WHERE "Id" = '0feb109b-93e9-4fd2-a9e4-72fa337b5485'
               OR ("DataOrigin" = 'BuiltIn' AND "NameKey" = 'jumprope');

            UPDATE public."Equipment"
            SET "SupportedConfigurationTypes" = array_append("SupportedConfigurationTypes", 4)
            WHERE "Id" = 'aa5314b2-5815-489f-a7a5-328bc01541a2'
               OR ("DataOrigin" = 'BuiltIn' AND "NameKey" = 'resistanceband');
            """);

        migrationBuilder.DropColumn(
            name: "SupportsHeightConfiguration",
            schema: "public",
            table: "Equipment");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "SupportsHeightConfiguration",
            schema: "public",
            table: "Equipment",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql(
            """
            UPDATE public."Equipment"
            SET "SupportsHeightConfiguration" = TRUE
            WHERE 1 = ANY("SupportedConfigurationTypes");
            """);

        migrationBuilder.DropColumn(
            name: "SupportedConfigurationTypes",
            schema: "public",
            table: "Equipment");
    }
}
