using FluentAssertions;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Repositories;
using PRLab.Infrastructure.DB.Repositories.Entity;

namespace PRLab.Tests.InfrastructureTests.Repository;

public sealed class DescriptionRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistDescription()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        var descriptionText = "Used for pulling exercises.";
        var description = Description.New(descriptionText);

        var createdDescription = await repo.CreateAsync(
            description,
            CancellationToken.None
        );

        createdDescription.Id.Should().Be(description.Id);

        var exists = await repo.ExistsByIdAsync(
            description.Id,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDescriptionIsNull()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        Description description = null!;

        var act = async () => await repo.CreateAsync(
            description,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDescription_WithTranslations()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        var englishContent = "English content.";
        var frenchContent = "French content.";

        var description = Description.New(
            englishContent,
            LocalizationHelper.Language.EN
        );

        description.ChangeContent(
            frenchContent,
            LocalizationHelper.Language.FR
        );

        await repo.CreateAsync(
            description,
            CancellationToken.None
        );

        var foundDescription = await repo.GetByIdAsync(
            description.Id,
            CancellationToken.None
        );

        foundDescription.Should().NotBeNull();
        foundDescription!.Id.Should().Be(description.Id);
        foundDescription.GetContent(LocalizationHelper.Language.EN).Should().Be(englishContent);
        foundDescription.GetContent(LocalizationHelper.Language.FR).Should().Be(frenchContent);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        var id = new DescriptionId(Guid.Empty);

        var act = async () => await repo.GetByIdAsync(
            id,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllDescriptions()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        var firstDescription = Description.New("First description.");
        var secondDescription = Description.New("Second description.");

        await repo.CreateAsync(
            firstDescription,
            CancellationToken.None
        );

        await repo.CreateAsync(
            secondDescription,
            CancellationToken.None
        );

        var descriptions = await repo.ListAsync(CancellationToken.None);

        descriptions.Should().HaveCount(2);
        descriptions.Select(description => description.Id)
            .Should()
            .Contain([
                firstDescription.Id,
                secondDescription.Id
            ]);
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnDescriptionCount()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        await repo.CreateAsync(
            Description.New("First description."),
            CancellationToken.None
        );

        await repo.CreateAsync(
            Description.New("Second description."),
            CancellationToken.None
        );

        var count = await repo.GetCountAsync(CancellationToken.None);

        count.Should().Be(2);
    }

    [Fact]
    public async Task ExistsByContentAsync_ShouldReturnTrue_WhenContentExists()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        var descriptionText = "Used for pulling exercises.";

        await repo.CreateAsync(
            Description.New(descriptionText),
            CancellationToken.None
        );

        var exists = await repo.ExistsByContentAsync(
            descriptionText,
            CancellationToken.None
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByContentAsync_ShouldThrow_WhenContentIsEmpty()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        var content = "   ";

        var act = async () => await repo.ExistsByContentAsync(
            content,
            CancellationToken.None
        );

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDescription()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new DescriptionRepository(
            database.Db,
            null!
        );

        var description = Description.New("Original content.");

        await repo.CreateAsync(
            description,
            CancellationToken.None
        );

        description.ChangeContent("Updated content.");

        var updatedDescription = await repo.UpdateAsync(
            description,
            CancellationToken.None
        );

        updatedDescription.GetContent().Should().Be("Updated content.");

        var foundDescription = await repo.GetByIdAsync(
            description.Id,
            CancellationToken.None
        );

        foundDescription!.GetContent().Should().Be("Updated content.");
    }
}