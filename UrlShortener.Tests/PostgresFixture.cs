using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using UrlShortener.Data;
using Xunit;

namespace UrlShortener.Tests;

/// <summary>
/// One throw-away PostgreSQL container shared by every test in the "Postgres" collection.
/// The real EF Core migrations are applied to it, so tests run against the same schema
/// (including the xmin row version) and the same SQL dialect as production. Requires Docker.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    // Keep the major version in line with the PostgreSQL used in production.
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>Creates a new context configured like the application (Npgsql + snake_case).</summary>
    public UsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UsDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new UsDbContext(options);
    }

    /// <summary>Empties all tables so every test starts from a clean database.</summary>
    public async Task ResetAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE urls, managers, refresh_tokens RESTART IDENTITY CASCADE");
    }
}

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
