using System.Text.Json;
using System.Text.Json.Serialization;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.UserService;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

namespace PRLab.Tests.InfrastructureTests;

public sealed class EquipmentSeedAliasMappingTests
{
    [Fact]
    public void ShippedAliasesMapToTheirOwningEquipmentAndExportAsNestedValues()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "SeedData");
        var json = File.ReadAllText(Path.Combine(directory, "equipment.seed.json"));
        var serializerOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var seedEntries = JsonSerializer.Deserialize<EquipmentSeedJsonDto[]>(json, serializerOptions)!;
        var factory = new JsonEquipmentSeedFactory(new TestUserProvider(), new TestSeedConfig(directory));
        (string Name, string[] Aliases)[] expectedEntries =
        [
            ("Assault Bike", ["Air Bike", "AssaultBike"]),
            ("Barbell", ["BB", "Bar"]),
            ("C2 Bike", ["BikeErg", "Bike Erg", "Concept2 BikeErg", "Concept2 Bike Erg"]),
            ("Dumbbell", ["DB", "Dumb Bell"]),
            ("Flat Bench", ["Flat Weight Bench"]),
            ("Floor Line", []),
            ("Glute Ham Developer", ["GHD", "Glute-Ham Developer"]),
            ("Gymnastic Rings", ["Gym Rings", "Rings"]),
            ("Jump Rope", ["Skipping Rope", "Speed Rope"]),
            ("Kettlebell", ["KB", "Kettle Bell"]),
            ("Parallel Bars", ["P-Bars", "Parallel Bar"]),
            ("Plyometric Box", ["Plyo Box", "Box"]),
            ("Pull-up Bar", ["Pull Up Bar", "Chin-up Bar", "Chin Up Bar"]),
            ("Resistance Band", ["Band", "Exercise Band"]),
            ("Row Erg", ["RowErg", "Rower", "C2 Rower", "Concept2 Rower"]),
            ("Sandbag", ["Sand Bag"]),
            ("Ski Erg", ["SkiErg", "C2 SkiErg", "Concept2 SkiErg"]),
            ("Squat Rack", ["Squat Stand"]),
            ("Wall", []),
            ("Wall Ball", ["Wallball"]),
            ("Medicine Ball", ["Med Ball", "Medball", "Medicineball"])
        ];

        Assert.Equal(expectedEntries.Length, seedEntries.Length);
        foreach (var expectedEntry in expectedEntries)
        {
            var seedDto = Assert.Single(seedEntries, entry => entry.Name == expectedEntry.Name);
            var expectedAliases =
                expectedEntry.Aliases
                    .DistinctBy(Alias.GetValueKey)
                    .OrderBy(Alias.GetValueKey, StringComparer.Ordinal)
                    .ToArray();
            var seedAliasesInKeyOrder = seedDto.Aliases.OrderBy(Alias.GetValueKey, StringComparer.Ordinal);
            Assert.Equal(expectedAliases, seedAliasesInKeyOrder);

            var seedItem = factory.ToSeedItem(SeedExecutionOptions.Default, seedDto);
            var equipment = seedItem.Entity;
            Assert.Equal(seedDto.Action, seedItem.Action);
            Assert.Equal(seedDto.Id, equipment.Id.Value);
            Assert.Equal(seedDto.Aliases, equipment.Aliases.Select(relation => relation.Alias.Value));
            Assert.All(equipment.Aliases, relation => Assert.Equal(equipment.Id, relation.EquipmentId));

            var exported = EquipmentSeedJsonDto.FromEquipment(equipment);
            var exportedJson = JsonSerializer.SerializeToElement(exported, serializerOptions);
            var exportedAliases = exportedJson.GetProperty(nameof(EquipmentSeedJsonDto.Aliases));
            Assert.Equal(expectedAliases, exportedAliases.EnumerateArray().Select(alias => alias.GetString()));
        }
    }

    [Fact]
    public void RejectsDuplicateNormalizedAliasesWithinOneEquipmentEntry()
    {
        var factory = new JsonEquipmentSeedFactory(new TestUserProvider(), new TestSeedConfig(string.Empty));
        var seedDto = new EquipmentSeedJsonDto { Name = "Wall Ball", Aliases = ["Med Ball", " medball "] };

        var exception = Assert.Throws<InvalidOperationException>(() => factory.Validate(seedDto));

        Assert.Contains("duplicate alias", exception.Message);
    }

    [Fact]
    public void RejectsInvalidAliasValues()
    {
        var factory = new JsonEquipmentSeedFactory(new TestUserProvider(), new TestSeedConfig(string.Empty));
        string[] invalidAliases = [null!, string.Empty, " ", "--", new string('x', Alias.MaxValueLength + 1)];

        foreach (var invalidAlias in invalidAliases)
        {
            var seedDto = new EquipmentSeedJsonDto { Name = "Dumbbell", Aliases = [invalidAlias] };
            var exception = Assert.Throws<InvalidOperationException>(() => factory.Validate(seedDto));

            Assert.Contains("invalid alias", exception.Message);
        }
    }

    [Fact]
    public void AllowsOmittedAliasesAndTheSameAliasOnDifferentEquipmentEntries()
    {
        var factory = new JsonEquipmentSeedFactory(new TestUserProvider(), new TestSeedConfig(string.Empty));
        var bench = JsonSerializer.Deserialize<EquipmentSeedJsonDto>("""{"Name":"Bench"}""")!;
        var dumbbell = new EquipmentSeedJsonDto { Name = "Dumbbell", Aliases = ["Weight"] };
        var barbell = new EquipmentSeedJsonDto { Name = "Barbell", Aliases = ["Weight"] };

        var benchItem = factory.ToSeedItem(SeedExecutionOptions.Default, bench);
        var dumbbellItem = factory.ToSeedItem(SeedExecutionOptions.Default, dumbbell);
        var barbellItem = factory.ToSeedItem(SeedExecutionOptions.Default, barbell);

        Assert.Empty(bench.Aliases);
        Assert.Empty(benchItem.Entity.Aliases);
        Assert.Equal("Weight", Assert.Single(dumbbellItem.Entity.Aliases).Alias.Value);
        Assert.Equal("Weight", Assert.Single(barbellItem.Entity.Aliases).Alias.Value);
    }

    private sealed record TestSeedConfig(string SeedFileDirectory) : ISeedingConfig
    {
        public SeedingSource Source => SeedingSource.JsonFiles;
    }

    private sealed class TestUserProvider : ISystemUserProvider
    {
        private readonly User user = User.Existing(UserId.New(), "Seed mapping tests", UserRole.Admin);

        public User GetSystemAdminUser(string? name = null) => user;
    }
}
