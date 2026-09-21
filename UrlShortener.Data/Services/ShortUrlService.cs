using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Data.Services;

public class ShortUrlService : IShortUrlService
{
    private readonly UsDbContext _dbContext;

    public ShortUrlService(UsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetLongUrl(string shortUrl)
    {
        await _dbContext.Database.OpenConnectionAsync();
        await using var command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = $@"
    UPDATE urls 
    SET redirect_count = redirect_count + 1
    WHERE short_url = '{shortUrl}'
    RETURNING long_url";

        return (string?)await command.ExecuteScalarAsync();
    }

    public async Task<List<UrlDto>> GetUrls(int managerId)
    {
        var managerUrls = await _dbContext.Urls
            .Where(x => x.ManagerId == managerId)
            .ToUrlDto()
            .OrderBy(x => x.Id)
            .ToListAsync();
        return managerUrls;
    }

    public async Task<UrlDto?> GetSingleUrl(int urlId, int managerId)
    {
        var url = await _dbContext.Urls
            .ToUrlDto()
            .FirstOrDefaultAsync(x => x.Id == urlId && x.ManagerId == managerId);
        return url;
    }

    public async Task<List<UrlDto>> CreateUrl(List<CreateUrlModel> models, int managerId)
    {
        var shortUrlsToCheck = models
            .Select(x => x.ShortUrl)
            .Where(x => !String.IsNullOrEmpty(x))
            .ToList();

        if (shortUrlsToCheck.Count != 0)
        {
            bool shortUrlIsNotUnique = await _dbContext.Urls
                .AnyAsync(x => shortUrlsToCheck.Contains(x.ShortUrl));
            if (shortUrlIsNotUnique)
            {
                return [];
            }
        }

        if (models.Any(x => String.IsNullOrEmpty(x.LongUrl)))
        {
            return [];
        }

        var urls = models
            .Select(x => new Url
            {
                LongUrl = x.LongUrl,
                ShortUrl = x.ShortUrl,
                ManagerId = managerId
            })
            .ToList();

        _dbContext.AddRange(urls);
        await _dbContext.SaveChangesAsync();

        foreach (var url in urls)
        {
            if (String.IsNullOrEmpty(url.ShortUrl))
            {
                var hashedCounter = Base62.Encode(CounterHasher.Scramble(url.Id));
                url.ShortUrl = hashedCounter;
            }
        }

        await _dbContext.SaveChangesAsync();

        return urls.Select(x => x.ToUrlDto()).ToList();
    }

    public async Task<bool> DeleteUrl(int id, int managerId)
    {
        var url = await _dbContext.Urls.FirstOrDefaultAsync(x =>
            x.Id == id && x.ManagerId == managerId);
        if (url == null)
            return false;

        _dbContext.Remove(url);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<UrlDto?> ChangeUrl(int id, ChangeUrlModel model, int managerId)
    {
        var url = await _dbContext.Urls.FirstOrDefaultAsync(x =>
            x.Id == id && x.ManagerId == managerId);

        if (url == null)
        {
            return null;
        }

        ArgumentException.ThrowIfNullOrEmpty(model.NewLongUrl);

        url.LongUrl = model.NewLongUrl;

        if (String.IsNullOrEmpty(model.NewShortUrl))
        {
            var hashedCounter = Base62.Encode(CounterHasher.Scramble(id));
            url.ShortUrl = hashedCounter;
        }
        else
        {
            bool shortUrlIsNotUnique =
                await _dbContext.Urls.AnyAsync(x => x.ShortUrl == model.NewShortUrl && x.Id != id);
            if (!shortUrlIsNotUnique)
            {
                url.ShortUrl = model.NewShortUrl;
            }
            else
            {
                throw new ArgumentException("model.NewShortUrl should be unique", nameof(model));
            }
        }

        await _dbContext.SaveChangesAsync();

        return url.ToUrlDto();
    }
}

public interface IShortUrlService
{
    Task<string?> GetLongUrl(string shortUrl);
    Task<List<UrlDto>> CreateUrl(List<CreateUrlModel> models, int managerId);
}

public enum ResponseStatus
{
    Success,
    BadRequest,
    NotFound
}

public class CreateUrlModel
{
    public string LongUrl { get; set; }
    public string? ShortUrl { get; set; }
}

public class ChangeUrlModel
{
    public string NewLongUrl { get; set; }
    public string? NewShortUrl { get; set; }
}

public class UrlDto
{
    public int Id { get; set; }
    public string LongUrl { get; set; }
    public string ShortUrl { get; set; }
    public int ManagerId { get; set; }
    public int RedirectCount { get; set; }
}

public static class UrlDtoExtensions
{
    public static IQueryable<UrlDto> ToUrlDto(this IQueryable<Url> urls)
    {
        return urls.Select(x => new UrlDto
        {
            Id = x.Id,
            LongUrl = x.LongUrl,
            ShortUrl = x.ShortUrl,
            ManagerId = x.ManagerId,
            RedirectCount = x.RedirectCount
        });
    }

    public static UrlDto ToUrlDto(this Url url)
    {
        return new UrlDto
        {
            Id = url.Id,
            LongUrl = url.LongUrl,
            ShortUrl = url.ShortUrl,
            ManagerId = url.ManagerId,
            RedirectCount = url.RedirectCount
        };
    }
}