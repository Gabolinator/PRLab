using FluentAssertions;
using PRLab.Domain.Model.Entity;
using PRLab.Infrastructure.DB.Repositories;
using PRLab.Infrastructure.DB.Repositories.Entity;

namespace PRLab.Tests.InfrastructureTests.Repository;

public sealed class EquipmentRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistEquipment()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        var name = "Pull-up Bar";
        var descriptionText = "Used for pull-up exercises.";
        var description = Description.New(descriptionText);

        var equipment = Equipment.NewBuiltIn(
            name,
            description
        );

        var createdEquipment = await repo.CreateAsync(
            equipment,
            CancellationToken.None
        );

        createdEquipment.Id.Should().Be(equipment.Id);

        var exists = await repo.ExistsAsync(
            equipment.Id,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEquipmentIsNull()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        Equipment equipment = null!;

        var act = async () => await repo.CreateAsync(
            equipment,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEquipment_WithDescriptionTranslations()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        var name = "Pull-up Bar";
        var descriptionText = "Used for pull-up exercises.";
        var frenchDescriptionText = "Utilisé pour les tractions.";

        var description = Description.New(descriptionText);
        description.ChangeContent(
            frenchDescriptionText,
            PRLab.Domain.Utilities.LocalizationHelper.Language.FR
        );

        var equipment = Equipment.NewBuiltIn(
            name,
            description
        );

        await repo.CreateAsync(
            equipment,
            CancellationToken.None
        );

        var foundEquipment = await repo.GetByIdAsync(
            equipment.Id,
            CancellationToken.None
        );

        foundEquipment.Should().NotBeNull();
        foundEquipment!.Id.Should().Be(equipment.Id);
        foundEquipment.Description.Should().NotBeNull();
        foundEquipment.Description.GetContent().Should().Be(descriptionText);
        foundEquipment.Description.GetContent(PRLab.Domain.Utilities.LocalizationHelper.Language.FR)
            .Should()
            .Be(frenchDescriptionText);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        var id = new PRLab.Domain.Value.Identifier.EquipmentId(Guid.Empty);

        var act = async () => await repo.GetByIdAsync(
            id,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnEquipment_CaseInsensitiveAndTrimmed()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        var name = "Pull-up Bar";
        var searchName = "  pull-UP bar  ";

        var equipment = Equipment.NewBuiltIn(
            name,
            Description.New("Used for pull-up exercises.")
        );

        await repo.CreateAsync(
            equipment,
            CancellationToken.None
        );

        var foundEquipment = await repo.GetByNameAsync(
            searchName,
            CancellationToken.None
        );

        foundEquipment.Should().NotBeNull();
        foundEquipment!.Id.Should().Be(equipment.Id);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllEquipment()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        var firstEquipment = Equipment.NewBuiltIn(
            "Pull-up Bar",
            Description.New("Used for pull-ups.")
        );

        var secondEquipment = Equipment.NewBuiltIn(
            "Barbell",
            Description.New("Used for loaded exercises.")
        );

        await repo.CreateAsync(
            firstEquipment,
            CancellationToken.None
        );

        await repo.CreateAsync(
            secondEquipment,
            CancellationToken.None
        );

        var equipments = await repo.ListAsync(CancellationToken.None);

        equipments.Should().HaveCount(2);
        equipments.Select(equipment => equipment.Id)
            .Should()
            .Contain([
                firstEquipment.Id,
                secondEquipment.Id
            ]);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnEquipmentCount()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        await repo.CreateAsync(
            Equipment.NewBuiltIn(
                "Pull-up Bar",
                Description.New("Used for pull-ups.")
            ),
            CancellationToken.None
        );

        await repo.CreateAsync(
            Equipment.NewBuiltIn(
                "Barbell",
                Description.New("Used for loaded exercises.")
            ),
            CancellationToken.None
        );

        var count = await repo.CountAsync(CancellationToken.None);

        count.Should().Be(2);
    }

    [Fact]
    public async Task NameExistsAsync_ShouldReturnTrue_WhenNameKeyExists()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        var equipment = Equipment.NewBuiltIn(
            "Pull-up Bar",
            Description.New("Used for pull-ups.")
        );

        await repo.CreateAsync(
            equipment,
            CancellationToken.None
        );

        var exists = await repo.NameExistsAsync(
            "  pull up bar  ",
            null,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task NameExistsAsync_ShouldReturnFalse_WhenExistingEquipmentIsExcluded()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new EquipmentRepository(
            database.Db,
            null!
        );

        var equipment = Equipment.NewBuiltIn(
            "Pull-up Bar",
            Description.New("Used for pull-ups.")
        );

        await repo.CreateAsync(
            equipment,
            CancellationToken.None
        );

        var exists = await repo.NameExistsAsync(
            "Pull-up Bar",
            equipment.Id,
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }
}