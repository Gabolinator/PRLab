using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Repositories;

namespace PRLab.Tests.InfrastructureTests.Repository;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingUser_ShouldReturnUser()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var user = CreateUser(
            Guid.NewGuid(),
            "Existing User");

        database.Db.Users.Add(user);
        await database.Db.SaveChangesAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.GetByIdAsync(
            user.Id,
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Name);
    }

    [Fact]
    public async Task GetByIdAsync_InexistentId_ShouldReturnNull()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.GetByIdAsync(
            UserId.New(),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ShouldThrowArgumentException()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        Func<Task> act = async () =>
        {
            await repo.GetByIdAsync(
                UserId.FromGuid(Guid.Empty),
                CancellationToken.None);
        };

        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*Id cannot be empty. Provide a valid id*");
    }

    [Fact]
    public async Task GetByIdAsync_DeletedUser_ShouldReturnNull()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var user = CreateUser(
            Guid.NewGuid(),
            "Deleted User");
        
        user.MarkDeleted();

        database.Db.Users.Add(user);
        await database.Db.SaveChangesAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.GetByIdAsync(
            user.Id,
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ExistingUser_ShouldReturnUser()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var user = CreateUser(
            Guid.NewGuid(),
            "Gabriel");

        database.Db.Users.Add(user);
        await database.Db.SaveChangesAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.GetByNameAsync(
            "Gabriel",
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Name.Should().Be("Gabriel");
    }

    [Fact]
    public async Task GetByNameAsync_InexistentName_ShouldReturnNull()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.GetByNameAsync(
            "Unknown User",
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task GetByNameAsync_EmptyName_ShouldThrowArgumentException(
        string name)
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        Func<Task> act = async () =>
        {
            await repo.GetByNameAsync(
                name,
                CancellationToken.None);
        };

        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*User name cannot be empty*");
    }

    [Fact]
    public async Task ListAsync_WithUsers_ShouldReturnActiveUsers()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var firstUser = CreateUser(
            Guid.NewGuid(),
            "Alice");

        var secondUser = CreateUser(
            Guid.NewGuid(),
            "Bob");

        firstUser.Id.Should().NotBe(secondUser.Id);
        
        database.Db.Users.AddRange(
            firstUser,
            secondUser);

        await database.Db.SaveChangesAsync();

        database.Db.ChangeTracker.Clear();

        var repo = new UserRepository(database.Db);

        var result = await repo.ListAsync(
            CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(user => user.Id == firstUser.Id);
        result.Should().Contain(user => user.Id == secondUser.Id);
    }

    [Fact]
    public async Task ListAsync_WithDeletedUser_ShouldExcludeDeletedUser()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var activeUser = CreateUser(
            Guid.NewGuid(),
            "Active User");

        var deletedUser = CreateUser(
            Guid.NewGuid(),
            "Deleted User");

        deletedUser.MarkDeleted();

        database.Db.Users.AddRange(
            activeUser,
            deletedUser);
        
        await database.Db.SaveChangesAsync();

        database.Db.ChangeTracker.Clear();
        
        var repo = new UserRepository(database.Db);

        var result = await repo.ListAsync(
            CancellationToken.None);

        result.Should().ContainSingle();
        result.Single().Id.Should().Be(activeUser.Id);
    }

    [Fact]
    public async Task ListAsync_WithoutUsers_ShouldReturnEmptyCollection()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.ListAsync(
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ValidUser_ShouldPersistAndReturnUser()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        var user = CreateUser(
            Guid.NewGuid(),
            "Created User");

        var result = await repo.CreateAsync(
            user,
            CancellationToken.None);

        result.Should().BeSameAs(user);

        var persistedUser = await database.Db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                persistedUser => persistedUser.Id == user.Id);

        persistedUser.Should().NotBeNull();
        persistedUser!.Name.Should().Be("Created User");
    }

    [Fact]
    public async Task CreateAsync_NullUser_ShouldThrowArgumentNullException()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        Func<Task> act = async () =>
        {
            await repo.CreateAsync(
                null!,
                CancellationToken.None);
        };

        await act.Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExistsAsync_ExistingUser_ShouldReturnTrue()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var user = CreateUser(
            Guid.NewGuid(),
            "Existing User");

        database.Db.Users.Add(user);
        await database.Db.SaveChangesAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.ExistsAsync(
            user.Id,
            CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_InexistentUser_ShouldReturnFalse()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.ExistsAsync(
            UserId.New(),
            CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_DeletedUser_ShouldReturnFalse()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var user = CreateUser(
            Guid.NewGuid(),
            "Deleted User");

        user.MarkDeleted();

        database.Db.Users.Add(user);
        await database.Db.SaveChangesAsync();

        var repo = new UserRepository(database.Db);

        var result = await repo.ExistsAsync(
            user.Id,
            CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_EmptyId_ShouldThrowArgumentException()
    {
        await using var database =
            await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(database.Db);

        Func<Task> act = async () =>
        {
            await repo.ExistsAsync(
                UserId.FromGuid(Guid.Empty),
                CancellationToken.None);
        };

        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*Id cannot be empty. Provide a valid id*");
    }

    private static User CreateUser(
        Guid id,
        string name)
    {
        return PredefinedUsers.Development.Create(
            id,
            name);
    }
}