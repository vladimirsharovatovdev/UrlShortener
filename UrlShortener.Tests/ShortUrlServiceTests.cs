using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Data.Services;
using Xunit;

namespace UrlShortener.Tests;

[Collection(PostgresCollection.Name)]
public class ShortUrlServiceTests(PostgresFixture fixture) : IAsyncLifetime
{
    public ValueTask InitializeAsync() => new(fixture.ResetAsync());

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task SetupDb(string longUrl, string shortUrl)
    {
        await using var dbContext = fixture.CreateContext();

        var url = new Url
        {
            LongUrl = longUrl,
            ShortUrl = shortUrl
        };

        var manager = new Manager
        {
            Name = "Abobius",
            PasswordHash = "134897123907dhwedh",
            AssociatedUrls = new List<Url> { url }
        };

        dbContext.Add(url);
        dbContext.Add(manager);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<int> GetRedirectCount(string shortUrl)
    {
        await using var dbContext = fixture.CreateContext();
        return await dbContext.Urls
            .AsNoTracking()
            .Where(x => x.ShortUrl == shortUrl)
            .Select(x => x.RedirectCount)
            .SingleAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetLongUrlReturnsLongUrl()
    {
        const string shortUrl = "1237biba";
        const string longUrl = "asdfsdf7a6sd09f8a0sd98fa09sd8f2342fdesadf";

        await SetupDb(longUrl, shortUrl);

        await using var dbContext = fixture.CreateContext();
        var shortUrlService = new ShortUrlService(dbContext);
        var actualLongUrl = await shortUrlService.GetLongUrl(shortUrl);

        Assert.Equal(longUrl, actualLongUrl);
    }

    [Fact]
    public async Task GetLongUrlReturnsNull()
    {
        const string shortUrl = "1237biba";
        const string longUrl = "asdfsdf7a6sd09f8a0sd98fa09sd8f2342fdesadf";

        await SetupDb(longUrl, shortUrl);

        await using var dbContext = fixture.CreateContext();
        var shortUrlService = new ShortUrlService(dbContext);
        var actualLongUrl = await shortUrlService.GetLongUrl("129djv");

        Assert.Null(actualLongUrl);
    }

    [Fact]
    public async Task GetLongUrlIncrementsRedirectCountOnEveryCall()
    {
        const string shortUrl = "1237biba";
        await SetupDb("https://example.com", shortUrl);

        await using var dbContext = fixture.CreateContext();
        var shortUrlService = new ShortUrlService(dbContext);

        await shortUrlService.GetLongUrl(shortUrl);
        await shortUrlService.GetLongUrl(shortUrl);
        await shortUrlService.GetLongUrl(shortUrl);

        Assert.Equal(3, await GetRedirectCount(shortUrl));
    }

    [Fact]
    public async Task GetLongUrlCalculatesRedirectCountWhenConcurrent()
    {
        const string shortUrl = "1237biba";
        const string longUrl = "asdfsdf7a6sd09f8a0sd98fa09sd8f2342fdesadf";

        await SetupDb(longUrl, shortUrl);

        // Like real requests: every call gets its own DbContext (and connection).
        await Parallel.ForEachAsync(
            Enumerable.Range(0, 1000),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = 20,
                CancellationToken = TestContext.Current.CancellationToken
            },
            async (_, _) =>
            {
                await using var dbContext = fixture.CreateContext();
                var shortUrlService = new ShortUrlService(dbContext);
                await shortUrlService.GetLongUrl(shortUrl);
            });

        Assert.Equal(1000, await GetRedirectCount(shortUrl));
    }

    [Theory]
    [InlineData("x' OR '1'='1")]
    [InlineData("abc'; DROP TABLE urls;--")]
    public async Task GetLongUrlIsNotVulnerableToSqlInjection(string maliciousShortUrl)
    {
        await SetupDb("https://example.com/first", "first");

        await using var dbContext = fixture.CreateContext();
        var shortUrlService = new ShortUrlService(dbContext);

        var result = await shortUrlService.GetLongUrl(maliciousShortUrl);

        Assert.Null(result);
        // The table still exists and no row was touched.
        Assert.Equal(0, await GetRedirectCount("first"));
    }
}
