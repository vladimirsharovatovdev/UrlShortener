using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Data.Services;
using Xunit;

namespace UrlShortener.Tests;

public class ShortUrlServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly UsDbContext _dbContext;

    public ShortUrlServiceTests()
    {
        // Открываем соединение и держим его открытым весь тест
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<UsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new UsDbContext(options);
        _dbContext.Database.EnsureCreated(); // создаёт схему на основе моделей
    }

    private async Task SetupDb(string longUrl, string shortUrl)
    {
        var url = new Url
        {
            LongUrl = longUrl,
            ShortUrl = shortUrl
        };
        
        var manager = new Manager
        {
            Name = "Abobius",
            PasswordHash = "134897123907dhwedh",
            AssociatedUrls = new List<Url>(){url}
        };
        
        _dbContext.Add(url);
        _dbContext.Add(manager);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task GetLongUrlReturnsLongUrl()
    {
        const string shortUrl = "1237biba";
        const string longUrl = "asdfsdf7a6sd09f8a0sd98fa09sd8f2342fdesadf";
        
        await SetupDb(longUrl, shortUrl);

        var shortUrlService = new ShortUrlService(_dbContext);
        var actualLongUrl = await shortUrlService.GetLongUrl(shortUrl);
        
        Assert.Equal(longUrl, actualLongUrl);
    }
    
    [Fact]
    public async Task GetLongUrlReturnsNull()
    {
        const string shortUrl = "1237biba";
        const string longUrl = "asdfsdf7a6sd09f8a0sd98fa09sd8f2342fdesadf";
        
        await SetupDb(longUrl, shortUrl);

        var shortUrlService = new ShortUrlService(_dbContext);
        var actualLongUrl = await shortUrlService.GetLongUrl("129djv");
        
        Assert.Null(actualLongUrl);
    }
    
    [Fact]
    public async Task GetLongUrlCalculatesRedirectCountWhenConcurrent()
    {
        const string shortUrl = "1237biba";
        const string longUrl = "asdfsdf7a6sd09f8a0sd98fa09sd8f2342fdesadf";
        
        await SetupDb(longUrl, shortUrl);

        var shortUrlService = new ShortUrlService(_dbContext);
        
        var tasks = Enumerable.Range(0, 1000)
            .Select(async _ => await shortUrlService.GetLongUrl(shortUrl));

        await Task.WhenAll(tasks);

        var longUrlEntity = await _dbContext.Urls.FirstAsync();
        Assert.Equal(1000, longUrlEntity.RedirectCount);
    }
    
    public void Dispose()
    {
        _connection.Close();
        _dbContext.Dispose();
    }
}