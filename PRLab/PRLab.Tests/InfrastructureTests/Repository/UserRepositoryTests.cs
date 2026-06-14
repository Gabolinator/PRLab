using FluentAssertions;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Repositories;

namespace PRLab.Tests.InfrastructureTests.Repository;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotImplementedException()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();

        var repo = new UserRepository(
            database.Db,
            null!
        );

        var act = async () => await repo.GetByIdAsync(
            UserId.New(),
            CancellationToken.None
        );

        await act.Should().ThrowAsync<NotImplementedException>();
    }
}