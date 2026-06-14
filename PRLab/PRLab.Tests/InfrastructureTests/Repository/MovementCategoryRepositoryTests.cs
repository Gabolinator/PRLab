using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Repositories;
using PRLab.Infrastructure.DB.Repositories.Entity;

namespace PRLab.Tests.InfrastructureTests.Repository;

using FluentAssertions;

public sealed class MovementCategoryRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistMovementCategory()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var movementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        var createdMovementCategory = await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        createdMovementCategory.Id.Should().Be(movementCategory.Id);

        var exists = await repo.ExistsAsync(
            movementCategory.Id,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenMovementCategoryIsNull()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        MovementCategory movementCategory = null!;

        var act = async () => await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMovementCategory_WithDescriptionTranslations()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var name = "Cable Machine";
        var englishDescriptionText = "Cable-based resistance movements.";
        var frenchDescriptionText = "Mouvements de résistance avec câble.";

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            englishDescriptionText,
            BaseMovementCategory.Resistance
        );

        movementCategory.ChangeDescription(
            frenchDescriptionText,
            LocalizationHelper.Language.FR
        );

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var foundMovementCategory = await repo.GetByIdAsync(
            movementCategory.Id,
            CancellationToken.None
        );

        foundMovementCategory.Should().NotBeNull();
        foundMovementCategory!.Id.Should().Be(movementCategory.Id);
        foundMovementCategory.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        foundMovementCategory.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        foundMovementCategory.BaseMovementCategory.Should().Be(BaseMovementCategory.Resistance);
        foundMovementCategory.Description.GetContent(LocalizationHelper.Language.EN).Should().Be(englishDescriptionText);
        foundMovementCategory.Description.GetContent(LocalizationHelper.Language.FR).Should().Be(frenchDescriptionText);
        foundMovementCategory.Ownership.Should().NotBeNull();
        foundMovementCategory.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
        foundMovementCategory.Ownership.OwnerUserId.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMovementCategoryIsDeleted()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var movementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        ((IAudited)movementCategory).MarkDeleted();

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var foundMovementCategory = await repo.GetByIdAsync(
            movementCategory.Id,
            CancellationToken.None
        );

        foundMovementCategory.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var id = new MovementCategoryId(Guid.Empty);

        var act = async () => await repo.GetByIdAsync(
            id,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnMovementCategory_WhenNameKeyMatches()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var name = "Cable Machine";
        var searchName = "  cable-machine  ";

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var foundMovementCategory = await repo.GetByNameAsync(
            searchName,
            CancellationToken.None
        );

        foundMovementCategory.Should().NotBeNull();
        foundMovementCategory!.Id.Should().Be(movementCategory.Id);
        foundMovementCategory.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(searchName));
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_WhenMovementCategoryIsDeleted()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var name = "Cable Machine";

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        ((IAudited)movementCategory).MarkDeleted();

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var foundMovementCategory = await repo.GetByNameAsync(
            name,
            CancellationToken.None
        );

        foundMovementCategory.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldThrow_WhenNameIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var name = "   ";

        var act = async () => await repo.GetByNameAsync(
            name,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnOnlyNonDeletedMovementCategories()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var visibleMovementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        var deletedMovementCategory = MovementCategory.NewBuiltIn(
            "Old Category",
            "Deleted category.",
            BaseMovementCategory.Hybrid
        );

        ((IAudited)deletedMovementCategory).MarkDeleted();

        await repo.CreateAsync(
            visibleMovementCategory,
            CancellationToken.None
        );

        await repo.CreateAsync(
            deletedMovementCategory,
            CancellationToken.None
        );

        var movementCategories = await repo.ListAsync(CancellationToken.None);

        movementCategories.Should().ContainSingle();
        movementCategories.First().Id.Should().Be(visibleMovementCategory.Id);
    }

    [Fact]
    public async Task ListByBaseCategoryAsync_ShouldReturnOnlyMatchingBaseCategory()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var cableMachineCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        var weightliftingCategory = MovementCategory.NewBuiltIn(
            "Weightlifting",
            "Loaded resistance movements.",
            BaseMovementCategory.Resistance
        );

        var ergMachineCategory = MovementCategory.NewBuiltIn(
            "Erg Machine",
            "Machine-based cardio movements.",
            BaseMovementCategory.Cardio
        );

        await repo.CreateAsync(
            cableMachineCategory,
            CancellationToken.None
        );

        await repo.CreateAsync(
            weightliftingCategory,
            CancellationToken.None
        );

        await repo.CreateAsync(
            ergMachineCategory,
            CancellationToken.None
        );

        var movementCategories = await repo.ListByBaseCategoryAsync(
            BaseMovementCategory.Resistance,
            CancellationToken.None
        );

        movementCategories.Should().HaveCount(2);
        movementCategories.Select(movementCategory => movementCategory.Id)
            .Should()
            .Contain([
                cableMachineCategory.Id,
                weightliftingCategory.Id
            ]);

        movementCategories.Select(movementCategory => movementCategory.Id)
            .Should()
            .NotContain(ergMachineCategory.Id);
    }

    [Fact]
    public async Task ListByBaseCategoryAsync_ShouldNotReturnDeletedMovementCategories()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var visibleMovementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        var deletedMovementCategory = MovementCategory.NewBuiltIn(
            "Weightlifting",
            "Deleted resistance movements.",
            BaseMovementCategory.Resistance
        );

        ((IAudited)deletedMovementCategory).MarkDeleted();

        await repo.CreateAsync(
            visibleMovementCategory,
            CancellationToken.None
        );

        await repo.CreateAsync(
            deletedMovementCategory,
            CancellationToken.None
        );

        var movementCategories = await repo.ListByBaseCategoryAsync(
            BaseMovementCategory.Resistance,
            CancellationToken.None
        );

        movementCategories.Should().ContainSingle();
        movementCategories.First().Id.Should().Be(visibleMovementCategory.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistMovementCategoryChanges()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var movementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var updatedDescriptionText = "Updated description.";

        movementCategory.ChangeDescription(
            updatedDescriptionText,
            LocalizationHelper.Language.EN
        );

        await repo.UpdateAsync(
            movementCategory,
            CancellationToken.None
        );

        var foundMovementCategory = await repo.GetByIdAsync(
            movementCategory.Id,
            CancellationToken.None
        );

        foundMovementCategory.Should().NotBeNull();
        foundMovementCategory!.Description.GetContent(LocalizationHelper.Language.EN)
            .Should()
            .Be(updatedDescriptionText);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenMovementCategoryIsNull()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        MovementCategory movementCategory = null!;

        var act = async () => await repo.UpdateAsync(
            movementCategory,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenIdIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var movementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        typeof(MovementCategory)
            .GetProperty(nameof(MovementCategory.Id))!
            .SetValue(
                movementCategory,
                new MovementCategoryId(Guid.Empty)
            );

        var act = async () => await repo.UpdateAsync(
            movementCategory,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenMovementCategoryExists()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var movementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var exists = await repo.ExistsAsync(
            movementCategory.Id,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenMovementCategoryIsDeleted()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var movementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        ((IAudited)movementCategory).MarkDeleted();

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var exists = await repo.ExistsAsync(
            movementCategory.Id,
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ShouldThrow_WhenIdIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var id = new MovementCategoryId(Guid.Empty);

        var act = async () => await repo.ExistsAsync(
            id,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task NameExistsAsync_ShouldReturnTrue_WhenNameKeyExists()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var movementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var exists = await repo.NameExistsAsync(
            "  cable-machine  ",
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task NameExistsAsync_ShouldReturnFalse_WhenMovementCategoryIsDeleted()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var name = "Cable Machine";

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        ((IAudited)movementCategory).MarkDeleted();

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var exists = await repo.NameExistsAsync(
            name,
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task NameExistsAsync_ShouldThrow_WhenNameIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var name = "   ";

        var act = async () => await repo.NameExistsAsync(
            name,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task NameExistsAsync_WithExcludedId_ShouldReturnFalse_WhenExistingMovementCategoryIsExcluded()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var movementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        await repo.CreateAsync(
            movementCategory,
            CancellationToken.None
        );

        var exists = await repo.NameExistsAsync(
            "Cable Machine",
            movementCategory.Id,
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task NameExistsAsync_WithExcludedId_ShouldReturnTrue_WhenDifferentMovementCategoryHasSameName()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var excludedMovementCategory = MovementCategory.NewBuiltIn(
            "Weightlifting",
            "Loaded resistance movements.",
            BaseMovementCategory.Resistance
        );

        var existingMovementCategory = MovementCategory.NewBuiltIn(
            "Cable Machine",
            "Cable-based resistance movements.",
            BaseMovementCategory.Resistance
        );

        await repo.CreateAsync(
            excludedMovementCategory,
            CancellationToken.None
        );

        await repo.CreateAsync(
            existingMovementCategory,
            CancellationToken.None
        );

        var exists = await repo.NameExistsAsync(
            "Cable Machine",
            excludedMovementCategory.Id,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task NameExistsAsync_WithExcludedId_ShouldThrow_WhenNameIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MovementCategoryRepository(database.Db);

        var name = "   ";

        var act = async () => await repo.NameExistsAsync(
            name,
            MovementCategoryId.New(),
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }
}