using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Repositories;
using PRLab.Infrastructure.DB.Repositories.Entity;

namespace PRLab.Tests.InfrastructureTests.Repository;

public sealed class MuscleRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistMuscle()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Elbow flexor.")
        );

        var createdMuscle = await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        createdMuscle.Id.Should().Be(muscle.Id);

        var exists = await repo.ExistsAsync(
            muscle.Id,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenMuscleIsNull()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        Muscle muscle = null!;

        var act = async () => await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMuscle_WithDescription()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var descriptionText = "Elbow flexor.";

        var muscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New(descriptionText)
        );

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        var foundMuscle = await repo.GetByIdAsync(
            muscle.Id,
            CancellationToken.None
        );

        foundMuscle.Should().NotBeNull();
        foundMuscle!.Id.Should().Be(muscle.Id);
        foundMuscle.Description.GetContent().Should().Be(FormatingUtilities.NormalizeDescriptionContent(descriptionText));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var id = new MuscleId(Guid.Empty);

        var act = async () => await repo.GetByIdAsync(
            id,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnOnlyNonDeletedMuscles()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var visibleMuscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Visible muscle.")
        );

        var deletedMuscle = Muscle.New(
            "Triceps",
            "Triceps brachii",
            BodySection.UpperBody,
            Description.New("Deleted muscle.")
        );
        
        deletedMuscle.Delete();

        await repo.CreateAsync(
            visibleMuscle,
            CancellationToken.None
        );

        await repo.CreateAsync(
            deletedMuscle,
            CancellationToken.None
        );

        var muscles = await repo.ListAsync(CancellationToken.None);

        muscles.Should().ContainSingle();
        muscles.First().Id.Should().Be(visibleMuscle.Id);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenMuscleIsDeleted()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Elbow flexor.")
        );

        muscle.Delete();

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        var exists = await repo.ExistsAsync(
            muscle.Id,
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task AllExistAsync_ShouldReturnTrue_WhenAllMusclesExist()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var firstMuscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("First muscle.")
        );

        var secondMuscle = Muscle.New(
            "Triceps",
            "Triceps brachii",
            BodySection.UpperBody,
            Description.New("Second muscle.")
        );

        await repo.CreateAsync(
            firstMuscle,
            CancellationToken.None
        );

        await repo.CreateAsync(
            secondMuscle,
            CancellationToken.None
        );

        var allExist = await repo.AllExistAsync(
            [
                firstMuscle.Id,
                secondMuscle.Id
            ],
            CancellationToken.None
        );

        allExist.Should().BeTrue();
    }

    [Fact]
    public async Task AllExistAsync_ShouldReturnFalse_WhenOneMuscleDoesNotExist()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("First muscle.")
        );

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        var allExist = await repo.AllExistAsync(
            [
                muscle.Id,
                MuscleId.New()
            ],
            CancellationToken.None
        );

        allExist.Should().BeFalse();
    }

    [Fact]
    public async Task AllExistAsync_ShouldReturnTrue_WhenIdsCollectionIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var allExist = await repo.AllExistAsync(
            [],
            CancellationToken.None
        );

        allExist.Should().BeTrue();
    }

    [Fact]
    public async Task NameExistsAsync_ShouldReturnTrue_WhenNameKeyExists()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps Brachii",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Elbow flexor.")
        );

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        var exists = await repo.NameExistsAsync(
            "  biceps-brachii  ",
            null,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task NameExistsAsync_ShouldReturnFalse_WhenExistingMuscleIsExcluded()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps Brachii",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Elbow flexor.")
        );

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        var exists = await repo.NameExistsAsync(
            "Biceps Brachii",
            muscle.Id,
            CancellationToken.None
        );

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAntagonistsAsync_ShouldAddRequestedAntagonists()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Main muscle.")
        );

        var antagonist = Muscle.New(
            "Triceps",
            "Triceps brachii",
            BodySection.UpperBody,
            Description.New("Antagonist muscle.")
        );

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        await repo.CreateAsync(
            antagonist,
            CancellationToken.None
        );

        var updatedMuscle = await repo.UpdateAntagonistsAsync(
            muscle.Id,
            [antagonist.Id],
            CancellationToken.None
        );

        updatedMuscle.Antagonists.Should().ContainSingle();
        updatedMuscle.Antagonists.First().AntagonistMuscleId.Should().Be(antagonist.Id);
    }

    [Fact]
    public async Task UpdateAntagonistsAsync_ShouldRemoveOldAntagonists()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Main muscle.")
        );

        var antagonist = Muscle.New(
            "Triceps",
            "Triceps brachii",
            BodySection.UpperBody,
            Description.New("Antagonist muscle.")
        );

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        await repo.CreateAsync(
            antagonist,
            CancellationToken.None
        );

        await repo.UpdateAntagonistsAsync(
            muscle.Id,
            [antagonist.Id],
            CancellationToken.None
        );

        var updatedMuscle = await repo.UpdateAntagonistsAsync(
            muscle.Id,
            [],
            CancellationToken.None
        );

        updatedMuscle.Antagonists.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAntagonistsAsync_ShouldThrow_WhenMuscleIsOwnAntagonist()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Main muscle.")
        );

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        var act = async () => await repo.UpdateAntagonistsAsync(
            muscle.Id,
            [muscle.Id],
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAntagonistsAsync_ShouldThrow_WhenOneAntagonistDoesNotExist()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new MuscleRepository(database.Db);

        var muscle = Muscle.New(
            "Biceps",
            "Biceps brachii",
            BodySection.UpperBody,
            Description.New("Main muscle.")
        );

        await repo.CreateAsync(
            muscle,
            CancellationToken.None
        );

        var act = async () => await repo.UpdateAntagonistsAsync(
            muscle.Id,
            [MuscleId.New()],
            CancellationToken.None
        );

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}