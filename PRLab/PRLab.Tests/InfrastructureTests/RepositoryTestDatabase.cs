using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Tests.InfrastructureTests;

public sealed class RepositoryTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    public PRLabPgDBContext Db { get; }

    private RepositoryTestDatabase(
        SqliteConnection connection,
        PRLabPgDBContext db)
    {
        this.connection = connection;
        Db = db;
    }

    public static async Task<RepositoryTestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PRLabPgDBContext>()
            .UseSqlite(connection)
            .Options;

        var db = new PRLabPgDBContext(options);

        await db.Database.EnsureCreatedAsync();

        return new RepositoryTestDatabase(
            connection,
            db
        );
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await connection.DisposeAsync();
    }
}